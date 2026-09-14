using RonSijm.EF.Markdown.Tests.TestModels;

namespace RonSijm.EF.Markdown.Tests;

public class CrudOperationsTests : IClassFixture<TestFixture>, IDisposable
{
    private readonly TestFixture _fixture;
    private readonly string _testDir;

    public CrudOperationsTests(TestFixture fixture)
    {
        _fixture = fixture;
        _testDir = _fixture.CreateTestDirectory();
    }

    [Fact]
    public void Add_MultipleEntities_AllPersisted()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        // Act
        for (int i = 0; i < 5; i++)
        {
            context.TestEntities.Add(new TestEntity
            {
                Name = $"Entity {i}",
                Price = i * 10m,
                Quantity = i,
                CreatedAt = DateTime.UtcNow,
                IsActive = i % 2 == 0
            });
        }
        context.SaveChanges();

        // Assert
        using var context2 = _fixture.CreateContext(_testDir);
        var entities = context2.TestEntities.ToList();
        Assert.Equal(5, entities.Count);
    }

    [Fact]
    public void Update_MultipleEntities_AllUpdated()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        var entities = new List<TestEntity>();
        for (int i = 0; i < 3; i++)
        {
            var entity = new TestEntity
            {
                Name = $"Original {i}",
                Price = i * 10m,
                Quantity = i,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            entities.Add(entity);
            context.TestEntities.Add(entity);
        }
        context.SaveChanges();

        // Act
        foreach (var entity in entities)
        {
            entity.Name = $"Updated {entity.Id}";
        }
        context.SaveChanges();

        // Assert
        using var context2 = _fixture.CreateContext(_testDir);
        var loaded = context2.TestEntities.ToList();
        Assert.All(loaded, e => Assert.StartsWith("Updated", e.Name));
    }

    [Fact]
    public void Delete_MultipleEntities_AllRemoved()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        var entities = new List<TestEntity>();
        for (int i = 0; i < 5; i++)
        {
            var entity = new TestEntity
            {
                Name = $"Entity {i}",
                Price = i * 10m,
                Quantity = i,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            entities.Add(entity);
            context.TestEntities.Add(entity);
        }
        context.SaveChanges();

        // Act - Delete first 3 (use the tracked entities directly)
        var toDelete = entities.Take(3).ToList();
        context.TestEntities.RemoveRange(toDelete);
        context.SaveChanges();

        // Assert
        using var context2 = _fixture.CreateContext(_testDir);
        var remaining = context2.TestEntities.ToList();
        Assert.Equal(2, remaining.Count);
    }

    [Fact]
    public void MixedOperations_AllApplied()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        var entity1 = new TestEntity { Name = "Entity 1", Price = 10m, Quantity = 1, CreatedAt = DateTime.UtcNow, IsActive = true };
        var entity2 = new TestEntity { Name = "Entity 2", Price = 20m, Quantity = 2, CreatedAt = DateTime.UtcNow, IsActive = true };
        context.TestEntities.AddRange(entity1, entity2);
        context.SaveChanges();

        // Act - Update one, delete one, add one
        entity1.Name = "Updated Entity 1";
        context.TestEntities.Remove(entity2);
        context.TestEntities.Add(new TestEntity { Name = "Entity 3", Price = 30m, Quantity = 3, CreatedAt = DateTime.UtcNow, IsActive = true });
        context.SaveChanges();

        // Assert (use ToList() first, then OrderBy() in memory)
        using var context2 = _fixture.CreateContext(_testDir);
        var entities = context2.TestEntities.ToList().OrderBy(e => e.Id).ToList();
        Assert.Equal(2, entities.Count);
        Assert.Equal("Updated Entity 1", entities[0].Name);
        Assert.Equal("Entity 3", entities[1].Name);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            try { Directory.Delete(_testDir, true); } catch { }
        }
    }
}

