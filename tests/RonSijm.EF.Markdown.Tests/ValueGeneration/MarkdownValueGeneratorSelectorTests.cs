using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using RonSijm.EF.Markdown.Tests.TestModels;

namespace RonSijm.EF.Markdown.Tests.ValueGeneration;

public class MarkdownValueGeneratorSelectorTests : IClassFixture<TestFixture>, IDisposable
{
    private readonly TestFixture _fixture;
    private readonly string _testDir;

    public MarkdownValueGeneratorSelectorTests(TestFixture fixture)
    {
        _fixture = fixture;
        _testDir = _fixture.CreateTestDirectory();
    }

    [Fact]
    public void IntKey_GeneratesSequentialValues()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        // Act
        var entity1 = new TestEntity { Name = "First", Price = 10m, Quantity = 1, CreatedAt = DateTime.UtcNow, IsActive = true };
        var entity2 = new TestEntity { Name = "Second", Price = 20m, Quantity = 2, CreatedAt = DateTime.UtcNow, IsActive = true };
        var entity3 = new TestEntity { Name = "Third", Price = 30m, Quantity = 3, CreatedAt = DateTime.UtcNow, IsActive = true };

        context.TestEntities.AddRange(entity1, entity2, entity3);
        context.SaveChanges();

        // Assert - IDs should be sequential
        Assert.True(entity1.Id > 0);
        Assert.True(entity2.Id > entity1.Id);
        Assert.True(entity3.Id > entity2.Id);
    }

    [Fact]
    public void LongKey_GeneratesSequentialValues()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        // Act
        var entity1 = new TestEntityWithLongKey { Name = "First" };
        var entity2 = new TestEntityWithLongKey { Name = "Second" };

        context.LongKeyEntities.AddRange(entity1, entity2);
        context.SaveChanges();

        // Assert
        Assert.True(entity1.Id > 0L);
        Assert.True(entity2.Id > entity1.Id);
    }

    [Fact]
    public void GuidKey_GeneratesUniqueValues()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        // Act
        var entity1 = new TestEntityWithGuidKey { Name = "First" };
        var entity2 = new TestEntityWithGuidKey { Name = "Second" };

        context.GuidKeyEntities.AddRange(entity1, entity2);
        context.SaveChanges();

        // Assert
        Assert.NotEqual(Guid.Empty, entity1.Id);
        Assert.NotEqual(Guid.Empty, entity2.Id);
        Assert.NotEqual(entity1.Id, entity2.Id);
    }

    [Fact]
    public void IntKey_GeneratesNewIdForExistingData()
    {
        // Arrange - Create file with existing data
        Directory.CreateDirectory(_testDir);
        var filePath = Path.Combine(_testDir, "TestEntity.md");
        var content = @"# TestEntity

| Id | Name | Price | Quantity | CreatedAt | IsActive | ExternalId |
|---|---|---|---|---|---|---|
| 100 | Existing | 50.00 | 25 | 2024-01-01T00:00:00Z | true |  |
";
        File.WriteAllText(filePath, content);

        using var context = _fixture.CreateContext(_testDir);

        // Act
        var newEntity = new TestEntity { Name = "New", Price = 10m, Quantity = 1, CreatedAt = DateTime.UtcNow, IsActive = true };
        context.TestEntities.Add(newEntity);
        context.SaveChanges();

        // Assert - New ID should be generated (note: current implementation uses a simple counter,
        // so it may not continue from existing max ID - this is a known limitation)
        Assert.True(newEntity.Id > 0);

        // Verify both entities exist
        using var context2 = _fixture.CreateContext(_testDir);
        var entities = context2.TestEntities.ToList();
        Assert.Equal(2, entities.Count);
    }

    [Fact]
    public void ExplicitId_IsPreserved()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        // Act - Set explicit ID
        var entity = new TestEntity 
        { 
            Id = 999, 
            Name = "Explicit", 
            Price = 10m, 
            Quantity = 1, 
            CreatedAt = DateTime.UtcNow, 
            IsActive = true 
        };
        context.TestEntities.Add(entity);
        context.SaveChanges();

        // Assert
        using var context2 = _fixture.CreateContext(_testDir);
        var loaded = context2.TestEntities.ToList().First();
        Assert.Equal(999, loaded.Id);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            try { Directory.Delete(_testDir, true); } catch { }
        }
    }
}

