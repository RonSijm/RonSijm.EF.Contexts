// Licensed under the MIT license.

using System.Globalization;
using System.Text;
using RonSijm.EF.FileStore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace RonSijm.EF.Csv.Storage.Internal;

/// <summary>
/// Represents a CSV table that stores entity data.
/// Inherits generic functionality from FileTableBase and provides CSV-specific parsing/writing.
/// </summary>
public class CsvTable : FileTableBase, ICsvTable
{
    // Threshold for parallel parsing (5000 rows) - datasets larger than this use parallel processing
    private const int ParallelParsingThreshold = 5000;

    /// <summary>
    /// Creates a new instance of <see cref="CsvTable"/>.
    /// </summary>
    public CsvTable(IEntityType entityType, string directoryPath, string fileExtension)
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

        // Header row
        for (var i = 0; i < Properties.Length; i++)
        {
            if (i > 0)
            {
                writer.Write(',');
            }
            WriteCsvValue(writer, Properties[i].Name);
        }
        writer.WriteLine();

        // Data rows
        foreach (var row in Rows)
        {
            for (var i = 0; i < Properties.Length; i++)
            {
                if (i > 0)
                {
                    writer.Write(',');
                }
                WriteCsvValueFormatted(writer, row[i], Properties[i]);
            }
            writer.WriteLine();
        }
    }

    /// <summary>
    /// Writes a CSV value, quoting if necessary.
    /// </summary>
    private static void WriteCsvValue(StreamWriter writer, string value)
    {
        if (NeedsQuoting(value))
        {
            writer.Write('"');
            // Escape any quotes by doubling them
            foreach (var c in value)
            {
                if (c == '"')
                {
                    writer.Write("\"\"");
                }
                else
                {
                    writer.Write(c);
                }
            }
            writer.Write('"');
        }
        else
        {
            writer.Write(value);
        }
    }

    /// <summary>
    /// Writes a formatted value to CSV, quoting if necessary.
    /// </summary>
    private void WriteCsvValueFormatted(StreamWriter writer, object? value, IProperty property)
    {
        if (value == null)
        {
            return; // Empty field for null
        }

        // Format the value to a string
        var formattedValue = FormatValueToString(value, property);
        WriteCsvValue(writer, formattedValue);
    }

    /// <summary>
    /// Formats a value to a string for CSV output.
    /// </summary>
    private static string FormatValueToString(object? value, IProperty property)
    {
        if (value == null)
        {
            return string.Empty;
        }

        return value switch
        {
            DateTime dt => dt.ToString("O", CultureInfo.InvariantCulture),
            DateTimeOffset dto => dto.ToString("O", CultureInfo.InvariantCulture),
            bool b => b ? "true" : "false",
            string s => s,
            int i => i.ToString(CultureInfo.InvariantCulture),
            long l => l.ToString(CultureInfo.InvariantCulture),
            double d => d.ToString(CultureInfo.InvariantCulture),
            decimal dec => dec.ToString(CultureInfo.InvariantCulture),
            Guid g => g.ToString(),
            _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty
        };
    }

    /// <summary>
    /// Determines if a value needs to be quoted in CSV.
    /// </summary>
    private static bool NeedsQuoting(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        foreach (var c in value)
        {
            if (c == ',' || c == '"' || c == '\n' || c == '\r')
            {
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc />
    protected override void ParseFileStreaming(string filePath)
    {
        var headerParsed = false;
        int[]? columnMapping = null;

        foreach (var line in File.ReadLines(filePath))
        {
            var lineSpan = line.AsSpan();
            var trimmedSpan = lineSpan.Trim();

            if (trimmedSpan.IsEmpty)
            {
                continue;
            }

            // Parse header row
            if (!headerParsed)
            {
                columnMapping = ParseHeaderRow(trimmedSpan);
                headerParsed = true;
                continue;
            }

            // Parse data row
            if (columnMapping != null)
            {
                var row = ParseDataRow(trimmedSpan, columnMapping);
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
        var headerParsed = false;
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

                if (!trimmedLine.IsEmpty)
                {
                    if (!headerParsed)
                    {
                        columnMapping = ParseHeaderRow(trimmedLine);
                        headerParsed = true;
                    }
                    else if (columnMapping != null)
                    {
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

    private void ParseRowsSequential(List<string> dataLines, int[] columnMapping)
    {
        foreach (var line in dataLines)
        {
            var row = ParseDataRow(line.AsSpan(), columnMapping);
            if (row != null)
            {
                AddRow(row);
            }
        }
    }

    private void ParseRowsParallel(List<string> dataLines, int[] columnMapping)
    {
        var parsedRows = new object[dataLines.Count][];

        Parallel.For(0, dataLines.Count, i =>
        {
            var line = dataLines[i];
            parsedRows[i] = ParseDataRow(line.AsSpan(), columnMapping)!;
        });

        foreach (var row in parsedRows)
        {
            if (row != null)
            {
                AddRow(row);
            }
        }
    }

    private int[] ParseHeaderRow(ReadOnlySpan<char> line)
    {
        var mapping = new int[Properties.Length];
        Array.Fill(mapping, -1);

        var cells = ParseCsvCells(line);
        for (var columnIndex = 0; columnIndex < cells.Count; columnIndex++)
        {
            var cell = cells[columnIndex].Trim();
            for (var j = 0; j < Properties.Length; j++)
            {
                if (cell.Equals(Properties[j].Name, StringComparison.OrdinalIgnoreCase))
                {
                    mapping[j] = columnIndex;
                    break;
                }
            }
        }

        return mapping;
    }

    private object[]? ParseDataRow(ReadOnlySpan<char> line, int[] columnMapping)
    {
        var cells = ParseCsvCells(line);
        var row = new object[Properties.Length];

        for (var i = 0; i < Properties.Length; i++)
        {
            var columnIndex = columnMapping[i];
            if (columnIndex >= 0 && columnIndex < cells.Count)
            {
                var cellValue = cells[columnIndex].Trim();
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

    /// <summary>
    /// Parses CSV cells handling quoted values.
    /// </summary>
    private static List<string> ParseCsvCells(ReadOnlySpan<char> line)
    {
        var cells = new List<string>();
        var inQuotes = false;
        var cellStart = 0;
        var sb = new StringBuilder();

        for (var i = 0; i <= line.Length; i++)
        {
            if (i == line.Length)
            {
                // End of line - add final cell
                if (inQuotes)
                {
                    sb.Append(line[cellStart..i]);
                }
                else if (sb.Length > 0)
                {
                    cells.Add(sb.ToString());
                }
                else
                {
                    cells.Add(line[cellStart..i].ToString());
                }
                break;
            }

            var c = line[i];

            if (c == '"')
            {
                if (!inQuotes)
                {
                    // Start of quoted section
                    inQuotes = true;
                    cellStart = i + 1;
                }
                else if (i + 1 < line.Length && line[i + 1] == '"')
                {
                    // Escaped quote
                    sb.Append(line[cellStart..i]);
                    sb.Append('"');
                    i++; // Skip next quote
                    cellStart = i + 1;
                }
                else
                {
                    // End of quoted section
                    sb.Append(line[cellStart..i]);
                    inQuotes = false;
                    cellStart = i + 1;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                // End of cell
                if (sb.Length > 0)
                {
                    cells.Add(sb.ToString());
                    sb.Clear();
                }
                else
                {
                    cells.Add(line[cellStart..i].ToString());
                }
                cellStart = i + 1;
            }
        }

        return cells;
    }
}
