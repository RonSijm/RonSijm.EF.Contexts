using RonSijm.EF.Markdown.Tests.TestModels;

namespace RonSijm.EF.Markdown.Tests;

public class EdgeCaseTests : IClassFixture<TestFixture>, IDisposable
{
    private readonly TestFixture _fixture;
    private readonly string _testDir;

    public EdgeCaseTests(TestFixture fixture)
    {
        _fixture = fixture;
        _testDir = _fixture.CreateTestDirectory();
    }

    [Fact]
    public void EmptyTable_ReturnsEmptyList()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        // Act
        var entities = context.TestEntities.ToList();

        // Assert
        Assert.Empty(entities);
    }

    [Fact]
    public void SpecialCharactersInString_ArePreserved()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        var entity = new TestEntity
        {
            Name = "Test with | pipe and special chars: <>&\"'",
            Price = 10m,
            Quantity = 1,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        // Act
        context.TestEntities.Add(entity);
        context.SaveChanges();

        // Assert
        using var context2 = _fixture.CreateContext(_testDir);
        var loaded = context2.TestEntities.ToList().First();
        Assert.Equal("Test with | pipe and special chars: <>&\"'", loaded.Name);
    }

    [Fact]
    public void EmptyString_IsPreserved()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        var entity = new TestEntity
        {
            Name = "",
            Price = 10m,
            Quantity = 1,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        // Act
        context.TestEntities.Add(entity);
        context.SaveChanges();

        // Assert
        using var context2 = _fixture.CreateContext(_testDir);
        var loaded = context2.TestEntities.ToList().First();
        Assert.Equal("", loaded.Name);
    }

    [Fact]
    public void LargeDecimalValue_IsPreserved()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        var entity = new TestEntity
        {
            Name = "Large Decimal",
            Price = 999999999.999999m,
            Quantity = 1,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        // Act
        context.TestEntities.Add(entity);
        context.SaveChanges();

        // Assert
        using var context2 = _fixture.CreateContext(_testDir);
        var loaded = context2.TestEntities.ToList().First();
        Assert.Equal(999999999.999999m, loaded.Price);
    }

    [Fact]
    public void NegativeValues_ArePreserved()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        var entity = new TestEntity
        {
            Name = "Negative",
            Price = -123.45m,
            Quantity = -10,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        // Act
        context.TestEntities.Add(entity);
        context.SaveChanges();

        // Assert
        using var context2 = _fixture.CreateContext(_testDir);
        var loaded = context2.TestEntities.ToList().First();
        Assert.Equal(-123.45m, loaded.Price);
        Assert.Equal(-10, loaded.Quantity);
    }

    [Fact]
    public void DateTimeMinMax_ArePreserved()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        var minDate = DateTime.MinValue;
        var maxDate = DateTime.MaxValue;

        var entity1 = new TestEntity { Name = "Min", Price = 1m, Quantity = 1, CreatedAt = minDate, IsActive = true };
        var entity2 = new TestEntity { Name = "Max", Price = 2m, Quantity = 2, CreatedAt = maxDate, IsActive = true };

        // Act
        context.TestEntities.AddRange(entity1, entity2);
        context.SaveChanges();

        // Assert
        using var context2 = _fixture.CreateContext(_testDir);
        var loaded = context2.TestEntities.ToList().OrderBy(e => e.Id).ToList();
        Assert.Equal(minDate, loaded[0].CreatedAt);
        Assert.Equal(maxDate, loaded[1].CreatedAt);
    }

    [Fact]
    public void GuidExternalId_NullAndValue_ArePreserved()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        var guid = Guid.NewGuid();
        var entity1 = new TestEntity { Name = "With Guid", Price = 1m, Quantity = 1, CreatedAt = DateTime.UtcNow, IsActive = true, ExternalId = guid };
        var entity2 = new TestEntity { Name = "Without Guid", Price = 2m, Quantity = 2, CreatedAt = DateTime.UtcNow, IsActive = true, ExternalId = null };

        // Act
        context.TestEntities.AddRange(entity1, entity2);
        context.SaveChanges();

        // Assert
        using var context2 = _fixture.CreateContext(_testDir);
        var loaded = context2.TestEntities.ToList().OrderBy(e => e.Id).ToList();
        Assert.Equal(guid, loaded[0].ExternalId);
        Assert.Null(loaded[1].ExternalId);
    }

    [Fact]
    public void MalformedMarkdownFile_WithMissingColumns_HandlesGracefully()
    {
        // Arrange - Create a malformed markdown file with fewer columns than expected
        Directory.CreateDirectory(_testDir);
        var filePath = Path.Combine(_testDir, "TestEntity.md");
        var content = @"# TestEntity

| Id | Name |
|---|---|
| 1 | Test |
";
        File.WriteAllText(filePath, content);

        using var context = _fixture.CreateContext(_testDir);

        // Act & Assert - Should handle gracefully (may return empty or partial data)
        var entities = context.TestEntities.ToList();
        // The provider should handle this without throwing
        Assert.NotNull(entities);
    }

    [Fact]
    public void MarkdownFile_WithExtraWhitespace_ParsesCorrectly()
    {
        // Arrange - Create a markdown file with extra whitespace
        Directory.CreateDirectory(_testDir);
        var filePath = Path.Combine(_testDir, "TestEntity.md");
        var content = @"# TestEntity

| Id | Name | Price | Quantity | CreatedAt | IsActive | ExternalId |
|---|---|---|---|---|---|---|
|  1  |  Test Name  |  99.99  |  10  |  2024-01-01T00:00:00Z  |  true  |   |
";
        File.WriteAllText(filePath, content);

        using var context = _fixture.CreateContext(_testDir);

        // Act
        var entities = context.TestEntities.ToList();

        // Assert
        Assert.Single(entities);
        Assert.Equal("Test Name", entities[0].Name.Trim());
    }

    [Fact]
    public void MarkdownFile_WithEmptyDataRows_HandlesGracefully()
    {
        // Arrange - Create a markdown file with empty data rows
        Directory.CreateDirectory(_testDir);
        var filePath = Path.Combine(_testDir, "TestEntity.md");
        var content = @"# TestEntity

| Id | Name | Price | Quantity | CreatedAt | IsActive | ExternalId |
|---|---|---|---|---|---|---|
";
        File.WriteAllText(filePath, content);

        using var context = _fixture.CreateContext(_testDir);

        // Act
        var entities = context.TestEntities.ToList();

        // Assert
        Assert.Empty(entities);
    }

    [Fact]
    public void MarkdownFile_WithOnlyHeader_HandlesGracefully()
    {
        // Arrange - Create a markdown file with only header (no separator or data)
        Directory.CreateDirectory(_testDir);
        var filePath = Path.Combine(_testDir, "TestEntity.md");
        var content = @"# TestEntity
";
        File.WriteAllText(filePath, content);

        using var context = _fixture.CreateContext(_testDir);

        // Act
        var entities = context.TestEntities.ToList();

        // Assert
        Assert.Empty(entities);
    }

    [Fact]
    public void MultipleContexts_SameDirectory_WorkCorrectly()
    {
        // Arrange
        using var context1 = _fixture.CreateContext(_testDir);
        context1.Database.EnsureCreated();

        var entity = new TestEntity
        {
            Name = "Shared Entity",
            Price = 50m,
            Quantity = 5,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        context1.TestEntities.Add(entity);
        context1.SaveChanges();

        // Act - Create a second context pointing to the same directory
        using var context2 = _fixture.CreateContext(_testDir);
        var loaded = context2.TestEntities.ToList();

        // Assert
        Assert.Single(loaded);
        Assert.Equal("Shared Entity", loaded[0].Name);
    }

    [Fact]
    public void ZeroValues_ArePreserved()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        var entity = new TestEntity
        {
            Name = "Zero Values",
            Price = 0m,
            Quantity = 0,
            CreatedAt = DateTime.UtcNow,
            IsActive = false
        };

        // Act
        context.TestEntities.Add(entity);
        context.SaveChanges();

        // Assert
        using var context2 = _fixture.CreateContext(_testDir);
        var loaded = context2.TestEntities.ToList().First();
        Assert.Equal(0m, loaded.Price);
        Assert.Equal(0, loaded.Quantity);
        Assert.False(loaded.IsActive);
    }

    [Fact]
    public void MarkdownFile_WithNonPipeLines_IgnoresThoseLines()
    {
        // Arrange - Create a markdown file with lines that don't start with '|'
        Directory.CreateDirectory(_testDir);
        var filePath = Path.Combine(_testDir, "TestEntity.md");
        var content = @"# TestEntity

Some random text that should be ignored
Another line without pipe

| Id | Name | Price | Quantity | CreatedAt | IsActive | ExternalId |
|---|---|---|---|---|---|---|
This line doesn't start with pipe and should be ignored
| 1 | Test | 10 | 5 | 2024-01-01T00:00:00Z | true | |
More text to ignore
| 2 | Test2 | 20 | 10 | 2024-01-02T00:00:00Z | false | |
";
        File.WriteAllText(filePath, content);

        using var context = _fixture.CreateContext(_testDir);

        // Act
        var entities = context.TestEntities.ToList();

        // Assert - Should only have the valid data rows
        Assert.Equal(2, entities.Count);
        Assert.Equal("Test", entities.First(e => e.Id == 1).Name);
        Assert.Equal("Test2", entities.First(e => e.Id == 2).Name);
    }

    [Fact]
    public void MarkdownFile_WithCommentsAndBlankLines_ParsesCorrectly()
    {
        // Arrange - Create a markdown file with comments and blank lines
        Directory.CreateDirectory(_testDir);
        var filePath = Path.Combine(_testDir, "TestEntity.md");
        var content = @"# TestEntity

<!-- This is a comment -->

| Id | Name | Price | Quantity | CreatedAt | IsActive | ExternalId |
|---|---|---|---|---|---|---|

| 1 | Test | 10 | 5 | 2024-01-01T00:00:00Z | true | |

<!-- Another comment -->
";
        File.WriteAllText(filePath, content);

        using var context = _fixture.CreateContext(_testDir);

        // Act
        var entities = context.TestEntities.ToList();

        // Assert
        Assert.Single(entities);
        Assert.Equal("Test", entities[0].Name);
    }

    [Fact]
    public void MarkdownFile_WithOnlyNonTableContent_ReturnsEmpty()
    {
        // Arrange - Create a markdown file with no table content
        Directory.CreateDirectory(_testDir);
        var filePath = Path.Combine(_testDir, "TestEntity.md");
        var content = @"# TestEntity

This is just some text.
No table here.
Just paragraphs.
";
        File.WriteAllText(filePath, content);

        using var context = _fixture.CreateContext(_testDir);

        // Act
        var entities = context.TestEntities.ToList();

        // Assert
        Assert.Empty(entities);
    }

    [Fact]
    public void MarkdownFile_WithEmptyValueForNonNullableValueType_ReturnsDefaultValue()
    {
        // Arrange - Create a markdown file with empty value for non-nullable int (Quantity)
        Directory.CreateDirectory(_testDir);
        var filePath = Path.Combine(_testDir, "TestEntity.md");
        var content = @"# TestEntity

| Id | Name | Price | Quantity | CreatedAt | IsActive | ExternalId |
|---|---|---|---|---|---|---|
| 1 | Test | 10 | | 2024-01-01T00:00:00Z | true | |
";
        File.WriteAllText(filePath, content);

        using var context = _fixture.CreateContext(_testDir);

        // Act
        var entities = context.TestEntities.ToList();

        // Assert - Quantity should be default value (0) for int
        Assert.Single(entities);
        Assert.Equal(0, entities[0].Quantity);
    }

    [Fact]
    public void MarkdownFile_WithNullMarkerForNonNullableValueType_ReturnsDefaultValue()
    {
        // Arrange - Create a markdown file with (null) marker for non-nullable int
        Directory.CreateDirectory(_testDir);
        var filePath = Path.Combine(_testDir, "TestEntity.md");
        var content = @"# TestEntity

| Id | Name | Price | Quantity | CreatedAt | IsActive | ExternalId |
|---|---|---|---|---|---|---|
| 1 | Test | 10 | (null) | 2024-01-01T00:00:00Z | true | |
";
        File.WriteAllText(filePath, content);

        using var context = _fixture.CreateContext(_testDir);

        // Act
        var entities = context.TestEntities.ToList();

        // Assert - Quantity should be default value (0) for int
        Assert.Single(entities);
        Assert.Equal(0, entities[0].Quantity);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            try { Directory.Delete(_testDir, true); } catch { }
        }
    }
}

