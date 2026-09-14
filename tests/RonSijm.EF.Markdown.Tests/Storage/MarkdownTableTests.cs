using RonSijm.EF.Markdown.Tests.TestModels;

namespace RonSijm.EF.Markdown.Tests.Storage;

public class MarkdownTableTests : IClassFixture<TestFixture>, IDisposable
{
    private readonly TestFixture _fixture;
    private readonly string _testDir;

    public MarkdownTableTests(TestFixture fixture)
    {
        _fixture = fixture;
        _testDir = _fixture.CreateTestDirectory();
    }

    [Fact]
    public void Create_AddsEntityToTable()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        // Act
        context.TestEntities.Add(new TestEntity
        {
            Name = "Test",
            Price = 10m,
            Quantity = 5,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
        context.SaveChanges();

        // Assert
        var entities = context.TestEntities.ToList();
        Assert.Single(entities);
        Assert.Equal("Test", entities[0].Name);
    }

    [Fact]
    public void Update_ModifiesExistingEntity()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        var entity = new TestEntity
        {
            Name = "Original",
            Price = 10m,
            Quantity = 5,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        context.TestEntities.Add(entity);
        context.SaveChanges();

        // Act
        entity.Name = "Updated";
        entity.Price = 20m;
        context.SaveChanges();

        // Assert - use new context to verify persistence
        using var context2 = _fixture.CreateContext(_testDir);
        var loaded = context2.TestEntities.ToList().First();
        Assert.Equal("Updated", loaded.Name);
        Assert.Equal(20m, loaded.Price);
    }

    [Fact]
    public void Delete_RemovesEntityFromTable()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        var entity = new TestEntity
        {
            Name = "ToDelete",
            Price = 10m,
            Quantity = 5,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        context.TestEntities.Add(entity);
        context.SaveChanges();

        // Act
        context.TestEntities.Remove(entity);
        context.SaveChanges();

        // Assert
        using var context2 = _fixture.CreateContext(_testDir);
        var entities = context2.TestEntities.ToList();
        Assert.Empty(entities);
    }

    [Fact]
    public void Load_ReadsExistingMarkdownFile()
    {
        // Arrange - Create file manually
        Directory.CreateDirectory(_testDir);
        var filePath = Path.Combine(_testDir, "TestEntity.md");
        var content = @"# TestEntity

| Id | Name | Price | Quantity | CreatedAt | IsActive | ExternalId |
|---|---|---|---|---|---|---|
| 1 | Manual Entry | 50.00 | 25 | 2024-01-01T00:00:00Z | true |  |
";
        File.WriteAllText(filePath, content);

        // Act
        using var context = _fixture.CreateContext(_testDir);
        var entities = context.TestEntities.ToList();

        // Assert
        Assert.Single(entities);
        Assert.Equal("Manual Entry", entities[0].Name);
        Assert.Equal(50.00m, entities[0].Price);
        Assert.Equal(25, entities[0].Quantity);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            try { Directory.Delete(_testDir, true); } catch { }
        }
    }
}

