using RonSijm.EF.Markdown.Tests.TestModels;

namespace RonSijm.EF.Markdown.Tests;

public class MarkdownFileFormatTests : IClassFixture<TestFixture>, IDisposable
{
    private readonly TestFixture _fixture;
    private readonly string _testDir;

    public MarkdownFileFormatTests(TestFixture fixture)
    {
        _fixture = fixture;
        _testDir = _fixture.CreateTestDirectory();
    }

    [Fact]
    public void GeneratedFile_HasCorrectHeader()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        context.TestEntities.Add(new TestEntity { Name = "Test", Price = 10m, Quantity = 1, CreatedAt = DateTime.UtcNow, IsActive = true });
        context.SaveChanges();

        // Act
        var filePath = Path.Combine(_testDir, "TestEntity.md");
        var content = File.ReadAllText(filePath);

        // Assert
        Assert.StartsWith("# TestEntity", content);
    }

    [Fact]
    public void GeneratedFile_HasTableHeaders()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        context.TestEntities.Add(new TestEntity { Name = "Test", Price = 10m, Quantity = 1, CreatedAt = DateTime.UtcNow, IsActive = true });
        context.SaveChanges();

        // Act
        var filePath = Path.Combine(_testDir, "TestEntity.md");
        var content = File.ReadAllText(filePath);

        // Assert
        Assert.Contains("| Id |", content);
        Assert.Contains("| Name |", content);
        Assert.Contains("| Price |", content);
    }

    [Fact]
    public void GeneratedFile_HasSeparatorRow()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        context.TestEntities.Add(new TestEntity { Name = "Test", Price = 10m, Quantity = 1, CreatedAt = DateTime.UtcNow, IsActive = true });
        context.SaveChanges();

        // Act
        var filePath = Path.Combine(_testDir, "TestEntity.md");
        var lines = File.ReadAllLines(filePath);

        // Assert - Find separator row (contains only |, -, and spaces)
        var separatorLine = lines.FirstOrDefault(l => l.Contains("|---"));
        Assert.NotNull(separatorLine);
    }

    [Fact]
    public void CustomFileExtension_CreatesCorrectFile()
    {
        // Arrange
        using var context = _fixture.CreateContextWithExtension(_testDir, ".markdown");
        context.Database.EnsureCreated();

        context.TestEntities.Add(new TestEntity { Name = "Test", Price = 10m, Quantity = 1, CreatedAt = DateTime.UtcNow, IsActive = true });
        context.SaveChanges();

        // Act & Assert
        var filePath = Path.Combine(_testDir, "TestEntity.markdown");
        Assert.True(File.Exists(filePath));
    }

    [Fact]
    public void MultipleEntityTypes_CreateSeparateFiles()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        context.TestEntities.Add(new TestEntity { Name = "Test", Price = 10m, Quantity = 1, CreatedAt = DateTime.UtcNow, IsActive = true });
        context.LongKeyEntities.Add(new TestEntityWithLongKey { Name = "Long" });
        context.GuidKeyEntities.Add(new TestEntityWithGuidKey { Name = "Guid" });
        context.SaveChanges();

        // Assert
        Assert.True(File.Exists(Path.Combine(_testDir, "TestEntity.md")));
        Assert.True(File.Exists(Path.Combine(_testDir, "TestEntityWithLongKey.md")));
        Assert.True(File.Exists(Path.Combine(_testDir, "TestEntityWithGuidKey.md")));
    }

    [Fact]
    public void ParseExistingFile_WithExtraWhitespace_Works()
    {
        // Arrange - Create file with extra whitespace
        Directory.CreateDirectory(_testDir);
        var filePath = Path.Combine(_testDir, "TestEntity.md");
        var content = @"# TestEntity

| Id | Name | Price | Quantity | CreatedAt | IsActive | ExternalId |
|---|---|---|---|---|---|---|
|  1  |  Spaced Entry  |  50.00  |  25  |  2024-01-01T00:00:00Z  |  true  |    |
";
        File.WriteAllText(filePath, content);

        // Act
        using var context = _fixture.CreateContext(_testDir);
        var entities = context.TestEntities.ToList();

        // Assert
        Assert.Single(entities);
        Assert.Equal("Spaced Entry", entities[0].Name.Trim());
    }

    [Fact]
    public void ParseExistingFile_WithMissingOptionalColumns_Works()
    {
        // Arrange - Create file without ExternalId column
        Directory.CreateDirectory(_testDir);
        var filePath = Path.Combine(_testDir, "TestEntity.md");
        var content = @"# TestEntity

| Id | Name | Price | Quantity | CreatedAt | IsActive |
|---|---|---|---|---|---|
| 1 | No External | 50.00 | 25 | 2024-01-01T00:00:00Z | true |
";
        File.WriteAllText(filePath, content);

        // Act
        using var context = _fixture.CreateContext(_testDir);
        var entities = context.TestEntities.ToList();

        // Assert
        Assert.Single(entities);
        Assert.Null(entities[0].ExternalId);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            try { Directory.Delete(_testDir, true); } catch { }
        }
    }
}

