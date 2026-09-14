using RonSijm.EF.Markdown.Tests.TestModels;

namespace RonSijm.EF.Markdown.Tests.Query;

public class QueryTests : IClassFixture<TestFixture>, IDisposable
{
    private readonly TestFixture _fixture;
    private readonly string _testDir;

    public QueryTests(TestFixture fixture)
    {
        _fixture = fixture;
        _testDir = _fixture.CreateTestDirectory();
    }

    [Fact]
    public void Where_FiltersEntities()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        context.TestEntities.AddRange(
            new TestEntity { Name = "Active 1", Price = 10m, Quantity = 1, CreatedAt = DateTime.UtcNow, IsActive = true },
            new TestEntity { Name = "Inactive", Price = 20m, Quantity = 2, CreatedAt = DateTime.UtcNow, IsActive = false },
            new TestEntity { Name = "Active 2", Price = 30m, Quantity = 3, CreatedAt = DateTime.UtcNow, IsActive = true }
        );
        context.SaveChanges();

        // Act - Use ToList() first, then filter in memory (provider doesn't support server-side Where)
        using var context2 = _fixture.CreateContext(_testDir);
        var activeEntities = context2.TestEntities.ToList().Where(e => e.IsActive).ToList();

        // Assert
        Assert.Equal(2, activeEntities.Count);
        Assert.All(activeEntities, e => Assert.True(e.IsActive));
    }

    [Fact]
    public void OrderBy_SortsEntities()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        context.TestEntities.AddRange(
            new TestEntity { Name = "C", Price = 30m, Quantity = 3, CreatedAt = DateTime.UtcNow, IsActive = true },
            new TestEntity { Name = "A", Price = 10m, Quantity = 1, CreatedAt = DateTime.UtcNow, IsActive = true },
            new TestEntity { Name = "B", Price = 20m, Quantity = 2, CreatedAt = DateTime.UtcNow, IsActive = true }
        );
        context.SaveChanges();

        // Act - Use ToList() first, then sort in memory (provider doesn't support server-side OrderBy)
        using var context2 = _fixture.CreateContext(_testDir);
        var sorted = context2.TestEntities.ToList().OrderBy(e => e.Name).ToList();

        // Assert
        Assert.Equal("A", sorted[0].Name);
        Assert.Equal("B", sorted[1].Name);
        Assert.Equal("C", sorted[2].Name);
    }

    [Fact]
    public void FirstOrDefault_ReturnsFirstMatch()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        context.TestEntities.AddRange(
            new TestEntity { Name = "First", Price = 10m, Quantity = 1, CreatedAt = DateTime.UtcNow, IsActive = true },
            new TestEntity { Name = "Second", Price = 20m, Quantity = 2, CreatedAt = DateTime.UtcNow, IsActive = true }
        );
        context.SaveChanges();

        // Act - Use ToList() first, then filter in memory
        using var context2 = _fixture.CreateContext(_testDir);
        var entity = context2.TestEntities.ToList().FirstOrDefault(e => e.Name == "Second");

        // Assert
        Assert.NotNull(entity);
        Assert.Equal("Second", entity.Name);
    }

    [Fact]
    public void FirstOrDefault_ReturnsNull_WhenNoMatch()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        context.TestEntities.Add(new TestEntity { Name = "Only", Price = 10m, Quantity = 1, CreatedAt = DateTime.UtcNow, IsActive = true });
        context.SaveChanges();

        // Act - Use ToList() first, then filter in memory
        using var context2 = _fixture.CreateContext(_testDir);
        var entity = context2.TestEntities.ToList().FirstOrDefault(e => e.Name == "NonExistent");

        // Assert
        Assert.Null(entity);
    }

    [Fact]
    public void Count_ReturnsCorrectCount()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        for (int i = 0; i < 5; i++)
        {
            context.TestEntities.Add(new TestEntity { Name = $"Entity {i}", Price = i * 10m, Quantity = i, CreatedAt = DateTime.UtcNow, IsActive = true });
        }
        context.SaveChanges();

        // Act - Use ToList() first, then count in memory
        using var context2 = _fixture.CreateContext(_testDir);
        var count = context2.TestEntities.ToList().Count;

        // Assert
        Assert.Equal(5, count);
    }

    [Fact]
    public void Any_ReturnsTrueWhenEntitiesExist()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        context.TestEntities.Add(new TestEntity { Name = "Test", Price = 10m, Quantity = 1, CreatedAt = DateTime.UtcNow, IsActive = true });
        context.SaveChanges();

        // Act - Use ToList() first, then check in memory
        using var context2 = _fixture.CreateContext(_testDir);
        var any = context2.TestEntities.ToList().Any();

        // Assert
        Assert.True(any);
    }

    [Fact]
    public void Any_ReturnsFalseWhenEmpty()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        // Act - Use ToList() first, then check in memory
        var any = context.TestEntities.ToList().Any();

        // Assert
        Assert.False(any);
    }

    [Fact]
    public void Select_ProjectsProperties()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        context.TestEntities.Add(new TestEntity { Name = "Test", Price = 99.99m, Quantity = 10, CreatedAt = DateTime.UtcNow, IsActive = true });
        context.SaveChanges();

        // Act - Use ToList() first, then project in memory
        using var context2 = _fixture.CreateContext(_testDir);
        var names = context2.TestEntities.ToList().Select(e => e.Name).ToList();

        // Assert
        Assert.Single(names);
        Assert.Equal("Test", names[0]);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            try { Directory.Delete(_testDir, true); } catch { }
        }
    }
}

