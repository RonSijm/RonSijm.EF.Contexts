using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RonSijm.EF.Markdown.Tests.TestModels;

namespace RonSijm.EF.Markdown.Tests;

public class TestFixture : IDisposable
{
    private readonly List<string> _testDirectories = new();

    public string CreateTestDirectory()
    {
        var testDir = Path.Combine(Path.GetTempPath(), "MarkdownEFCoreTests", Guid.NewGuid().ToString());
        _testDirectories.Add(testDir);
        return testDir;
    }

    public TestDbContext CreateContext(string? directoryPath = null)
    {
        directoryPath ??= CreateTestDirectory();

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseMarkdown(directoryPath)
            .ConfigureWarnings(w => w.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning))
            .Options;

        return new TestDbContext(options);
    }

    public TestDbContext CreateContextWithExtension(string directoryPath, string fileExtension)
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseMarkdown(directoryPath, b => b.UseFileExtension(fileExtension))
            .ConfigureWarnings(w => w.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning))
            .Options;

        return new TestDbContext(options);
    }

    public void Dispose()
    {
        foreach (var dir in _testDirectories)
        {
            if (Directory.Exists(dir))
            {
                try
                {
                    Directory.Delete(dir, recursive: true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
    }
}

