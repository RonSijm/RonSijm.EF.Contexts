// Licensed under the MIT license.

using System.Text;
using RonSijm.EF.FileStore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace RonSijm.EF.Markdown.Storage.Internal;

/// <summary>
/// Represents a markdown table that stores entity data.
/// Inherits generic functionality from FileTableBase and provides markdown-specific parsing/writing.
/// </summary>
public class MarkdownTable : FileTableBase, IMarkdownTable
{
    // Threshold for parallel parsing (5000 rows) - datasets larger than this use parallel processing
    private const int ParallelParsingThreshold = 5000;

    /// <summary>
    /// Creates a new instance of <see cref="MarkdownTable"/>.
    /// </summary>
    public MarkdownTable(IEntityType entityType, string directoryPath, string fileExtension)
        : base(entityType, directoryPath, fileExtension)
    {
    }

    /// <inheritdoc />
    public override void Save()
    {
        var directory = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Use buffered StreamWriter for better I/O performance
        // Buffer size of 64KB provides good balance between memory and performance
        using var writer = new StreamWriter(FilePath, false, Encoding.UTF8, bufferSize: 65536);

        // Title
        writer.Write("# ");
        writer.WriteLine(GetTableName(EntityType));
        writer.WriteLine();

        // Header row
        writer.Write('|');
        foreach (var property in Properties)
        {
            writer.Write(' ');
            writer.Write(property.Name);
            writer.Write(" |");
        }
        writer.WriteLine();

        // Separator row
        writer.Write('|');
        foreach (var _ in Properties)
        {
            writer.Write("---|");
        }
        writer.WriteLine();

        // Data rows
        foreach (var row in Rows)
        {
            writer.Write('|');
            for (var i = 0; i < Properties.Length; i++)
            {
                writer.Write(' ');
                FormatValueToWriter(row[i], Properties[i], writer);
                writer.Write(" |");
            }
            writer.WriteLine();
        }
    }

    /// <inheritdoc />
    protected override void ParseFileStreaming(string filePath)
    {
        // Use streaming read instead of ReadAllLines to reduce memory allocation
        var dataStarted = false;
        var headerParsed = false;
        int[]? columnMapping = null;

        // Reusable list for cell parsing to avoid allocations
        var cellBuffer = new List<(int start, int length)>(16);

        foreach (var line in File.ReadLines(filePath))
        {
            // Use span for trimming check without allocation
            var lineSpan = line.AsSpan();
            var trimmedSpan = lineSpan.Trim();

            if (trimmedSpan.IsEmpty || trimmedSpan[0] == '#')
            {
                continue;
            }

            if (trimmedSpan[0] != '|')
            {
                continue;
            }

            // Parse header row
            if (!headerParsed)
            {
                columnMapping = ParseHeaderRowSpan(trimmedSpan);
                headerParsed = true;
                continue;
            }

            // Skip separator row - check for "---" pattern
            if (trimmedSpan.Contains("---".AsSpan(), StringComparison.Ordinal))
            {
                dataStarted = true;
                continue;
            }

            // Parse data row
            if (dataStarted && columnMapping != null)
            {
                var row = ParseDataRowSpan(trimmedSpan, columnMapping, cellBuffer);
                if (row != null)
                {
                    AddRow(row);
                }
            }
        }
    }

    /// <inheritdoc />
    protected override void ParseContent(ReadOnlySpan<char> content)
    {
        // First pass: find header and collect data line positions
        var headerParsed = false;
        var dataStarted = false;
        int[]? columnMapping = null;
        var dataLines = new List<string>(1000);

        var lineStart = 0;
        for (var i = 0; i <= content.Length; i++)
        {
            var isEndOfLine = i == content.Length || content[i] == '\n';
            if (!isEndOfLine)
            {
                continue;
            }

            var lineEnd = i;
            if (lineEnd > lineStart && lineEnd > 0 && content[lineEnd - 1] == '\r')
            {
                lineEnd--;
            }

            if (lineEnd > lineStart)
            {
                var line = content.Slice(lineStart, lineEnd - lineStart);
                var trimmedLine = line.Trim();

                if (!trimmedLine.IsEmpty && trimmedLine[0] != '#' && trimmedLine[0] == '|')
                {
                    if (!headerParsed)
                    {
                        columnMapping = ParseHeaderRowSpan(trimmedLine);
                        headerParsed = true;
                    }
                    else if (trimmedLine.Contains("---".AsSpan(), StringComparison.Ordinal))
                    {
                        dataStarted = true;
                    }
                    else if (dataStarted && columnMapping != null)
                    {
                        // Store line as string for parallel processing
                        dataLines.Add(trimmedLine.ToString());
                    }
                }
            }

            lineStart = i + 1;
        }

        if (columnMapping == null || dataLines.Count == 0)
        {
            return;
        }

        // Use parallel parsing for large datasets
        if (dataLines.Count >= ParallelParsingThreshold)
        {
            ParseRowsParallel(dataLines, columnMapping);
        }
        else
        {
            ParseRowsSequential(dataLines, columnMapping);
        }
    }

    /// <summary>
    /// Parses rows sequentially for smaller datasets.
    /// </summary>
    private void ParseRowsSequential(List<string> dataLines, int[] columnMapping)
    {
        var cellBuffer = new List<(int start, int length)>(16);

        foreach (var line in dataLines)
        {
            var row = ParseDataRowSpan(line.AsSpan(), columnMapping, cellBuffer);
            if (row != null)
            {
                AddRow(row);
            }
        }
    }

    /// <summary>
    /// Parses rows in parallel for large datasets.
    /// </summary>
    private void ParseRowsParallel(List<string> dataLines, int[] columnMapping)
    {
        // Parse rows in parallel - each thread gets its own cell buffer
        var parsedRows = new object[dataLines.Count][];

        Parallel.For(0, dataLines.Count, () => new List<(int start, int length)>(16),
            (i, _, cellBuffer) =>
            {
                var line = dataLines[i];
                parsedRows[i] = ParseDataRowSpan(line.AsSpan(), columnMapping, cellBuffer)!;
                return cellBuffer;
            },
            _ => { });

        // Add rows to list and index sequentially (maintains order)
        foreach (var row in parsedRows)
        {
            if (row != null)
            {
                AddRow(row);
            }
        }
    }

    /// <summary>
    /// Parses header row using Span to avoid allocations.
    /// </summary>
    private int[] ParseHeaderRowSpan(ReadOnlySpan<char> line)
    {
        var mapping = new int[Properties.Length];
        Array.Fill(mapping, -1);

        // Skip leading |
        if (line[0] == '|')
        {
            line = line[1..];
        }
        // Skip trailing |
        if (line[^1] == '|')
        {
            line = line[..^1];
        }

        var columnIndex = 0;
        var cellStart = 0;

        for (var i = 0; i <= line.Length; i++)
        {
            if (i == line.Length || line[i] == '|')
            {
                var cell = line[cellStart..i].Trim();

                // Match against property names
                for (var j = 0; j < Properties.Length; j++)
                {
                    if (cell.Equals(Properties[j].Name.AsSpan(), StringComparison.OrdinalIgnoreCase))
                    {
                        mapping[j] = columnIndex;
                        break;
                    }
                }

                columnIndex++;
                cellStart = i + 1;
            }
        }

        return mapping;
    }

    /// <summary>
    /// Parses data row using Span to minimize allocations.
    /// </summary>
    private object[]? ParseDataRowSpan(ReadOnlySpan<char> line, int[] columnMapping, List<(int start, int length)> cellBuffer)
    {
        cellBuffer.Clear();

        // Skip leading |
        var startIndex = 0;
        if (line[0] == '|')
        {
            startIndex = 1;
        }
        // Skip trailing |
        var endIndex = line.Length;
        if (line[^1] == '|')
        {
            endIndex = line.Length - 1;
        }

        // Find all cell boundaries
        var cellStart = startIndex;
        for (var i = startIndex; i <= endIndex; i++)
        {
            if (i == endIndex || line[i] == '|')
            {
                cellBuffer.Add((cellStart, i - cellStart));
                cellStart = i + 1;
            }
        }

        var row = new object[Properties.Length];

        for (var i = 0; i < Properties.Length; i++)
        {
            var columnIndex = columnMapping[i];
            if (columnIndex >= 0 && columnIndex < cellBuffer.Count)
            {
                var (start, length) = cellBuffer[columnIndex];
                var cellSpan = line.Slice(start, length).Trim();

                // Convert span to string only when needed for parsing
                var cellValue = cellSpan.ToString();
                row[i] = ParseValue(cellValue, i);
            }
            else
            {
                row[i] = Properties[i].ClrType.IsValueType
                    ? Activator.CreateInstance(Properties[i].ClrType)!
                    : null!;
            }
        }

        return row;
    }
}

