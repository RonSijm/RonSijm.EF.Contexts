using RonSijm.EF.Markdown.Tests.TestModels;

namespace RonSijm.EF.Markdown.Tests.Storage;

public class MarkdownStoreTests : IClassFixture<TestFixture>, IDisposable
{
    private readonly TestFixture _fixture;
    private readonly string _testDir;

    public MarkdownStoreTests(TestFixture fixture)
    {
        _fixture = fixture;
        _testDir = _fixture.CreateTestDirectory();
    }

    [Fact]
    public void EnsureCreated_CreatesDirectory()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);

        // Act
        var created = context.Database.EnsureCreated();

        // Assert
        Assert.True(created);
        Assert.True(Directory.Exists(_testDir));
    }

    [Fact]
    public void EnsureCreated_CreatesMarkdownFiles()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);

        // Act
        context.Database.EnsureCreated();

        // Assert
        Assert.True(File.Exists(Path.Combine(_testDir, "TestEntity.md")));
    }

    [Fact]
    public void EnsureDeleted_RemovesDirectory()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();
        Assert.True(Directory.Exists(_testDir));

        // Act
        var deleted = context.Database.EnsureDeleted();

        // Assert
        Assert.True(deleted);
        Assert.False(Directory.Exists(_testDir));
    }

    [Fact]
    public void EnsureDeleted_ReturnsFalse_WhenDirectoryDoesNotExist()
    {
        // Arrange
        var nonExistentDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        using var context = _fixture.CreateContext(nonExistentDir);

        // Act
        var deleted = context.Database.EnsureDeleted();

        // Assert
        Assert.False(deleted);
    }

    [Fact]
    public void SaveChanges_PersistsDataToMarkdownFile()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        var entity = new TestEntity
        {
            Name = "Test Product",
            Price = 99.99m,
            Quantity = 10,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        // Act
        context.TestEntities.Add(entity);
        context.SaveChanges();

        // Assert
        var filePath = Path.Combine(_testDir, "TestEntity.md");
        var content = File.ReadAllText(filePath);
        Assert.Contains("Test Product", content);
        Assert.Contains("99.99", content);
    }

    [Fact]
    public void SaveChanges_GeneratesAutoIncrementId()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        var entity1 = new TestEntity { Name = "Entity 1", Price = 10m, Quantity = 1, CreatedAt = DateTime.UtcNow, IsActive = true };
        var entity2 = new TestEntity { Name = "Entity 2", Price = 20m, Quantity = 2, CreatedAt = DateTime.UtcNow, IsActive = true };

        // Act
        context.TestEntities.Add(entity1);
        context.TestEntities.Add(entity2);
        context.SaveChanges();

        // Assert
        Assert.NotEqual(0, entity1.Id);
        Assert.NotEqual(0, entity2.Id);
        Assert.NotEqual(entity1.Id, entity2.Id);
    }

    [Fact]
    public async Task EnsureCreatedAsync_CreatesDirectory()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);

        // Act
        var created = await context.Database.EnsureCreatedAsync();

        // Assert
        Assert.True(created);
        Assert.True(Directory.Exists(_testDir));
    }

    [Fact]
    public async Task EnsureDeletedAsync_RemovesDirectory()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        await context.Database.EnsureCreatedAsync();
        Assert.True(Directory.Exists(_testDir));

        // Act
        var deleted = await context.Database.EnsureDeletedAsync();

        // Assert
        Assert.True(deleted);
        Assert.False(Directory.Exists(_testDir));
    }

    [Fact]
    public async Task SaveChangesAsync_PersistsDataToMarkdownFile()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        await context.Database.EnsureCreatedAsync();

        var entity = new TestEntity
        {
            Name = "Async Test Product",
            Price = 199.99m,
            Quantity = 5,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        // Act
        context.TestEntities.Add(entity);
        await context.SaveChangesAsync();

        // Assert
        var filePath = Path.Combine(_testDir, "TestEntity.md");
        var content = await File.ReadAllTextAsync(filePath);
        Assert.Contains("Async Test Product", content);
        Assert.Contains("199.99", content);
    }

    [Fact]
    public async Task CanConnectAsync_ReturnsTrue()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        // Act
        var canConnect = await context.Database.CanConnectAsync();

        // Assert
        Assert.True(canConnect);
    }

    [Fact]
    public void CanConnect_ReturnsTrue_WhenDirectoryExists()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        // Act
        var canConnect = context.Database.CanConnect();

        // Assert
        Assert.True(canConnect);
    }

    [Fact]
    public void CanConnect_ReturnsTrue_WhenDirectoryDoesNotExist_ButCreateDirectoryIfNotExistsIsTrue()
    {
        // Arrange - By default, CreateDirectoryIfNotExists is true, so CanConnect returns true
        var nonExistentDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        using var context = _fixture.CreateContext(nonExistentDir);

        // Act
        var canConnect = context.Database.CanConnect();

        // Assert - CanConnect returns true because CreateDirectoryIfNotExists is true by default
        Assert.True(canConnect);
    }

    [Fact]
    public void DateTimeOffsetEntity_IsPersisted()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        var timestamp = DateTimeOffset.UtcNow;
        var entity = new TestEntityWithDateTimeOffset
        {
            Timestamp = timestamp
        };

        // Act
        context.DateTimeOffsetEntities.Add(entity);
        context.SaveChanges();

        // Assert
        using var context2 = _fixture.CreateContext(_testDir);
        var loaded = context2.DateTimeOffsetEntities.ToList().First();
        Assert.Equal(timestamp.Year, loaded.Timestamp.Year);
        Assert.Equal(timestamp.Month, loaded.Timestamp.Month);
        Assert.Equal(timestamp.Day, loaded.Timestamp.Day);
    }

    [Fact]
    public void EnumEntity_IsPersisted()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        var entity = new TestEntityWithEnum
        {
            Status = TestStatus.Active
        };

        // Act
        context.EnumEntities.Add(entity);
        context.SaveChanges();

        // Assert
        using var context2 = _fixture.CreateContext(_testDir);
        var loaded = context2.EnumEntities.ToList().First();
        Assert.Equal(TestStatus.Active, loaded.Status);
    }

    [Fact]
    public void EnumEntity_AllValues_ArePersisted()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        var entities = new[]
        {
            new TestEntityWithEnum { Status = TestStatus.Pending },
            new TestEntityWithEnum { Status = TestStatus.Active },
            new TestEntityWithEnum { Status = TestStatus.Completed }
        };

        // Act
        context.EnumEntities.AddRange(entities);
        context.SaveChanges();

        // Assert
        using var context2 = _fixture.CreateContext(_testDir);
        var loaded = context2.EnumEntities.ToList().OrderBy(e => e.Id).ToList();
        Assert.Equal(3, loaded.Count);
        Assert.Equal(TestStatus.Pending, loaded[0].Status);
        Assert.Equal(TestStatus.Active, loaded[1].Status);
        Assert.Equal(TestStatus.Completed, loaded[2].Status);
    }

    [Fact]
    public void DateTypeEntity_IsPersisted()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        var entity = new TestEntityWithDateTypes
        {
            DateOnlyValue = new DateOnly(2026, 2, 2),
            TimeOnlyValue = new TimeOnly(14, 30, 45),
            TimeSpanValue = TimeSpan.FromHours(2.5)
        };

        // Act
        context.DateTypeEntities.Add(entity);
        context.SaveChanges();

        // Assert
        using var context2 = _fixture.CreateContext(_testDir);
        var loaded = context2.DateTypeEntities.ToList().First();
        Assert.Equal(new DateOnly(2026, 2, 2), loaded.DateOnlyValue);
        Assert.Equal(new TimeOnly(14, 30, 45), loaded.TimeOnlyValue);
        Assert.Equal(TimeSpan.FromHours(2.5), loaded.TimeSpanValue);
    }

    [Fact]
    public void NullableTypeEntity_WithValues_IsPersisted()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        var entity = new TestEntityWithNullableTypes
        {
            NullableInt = 42,
            NullableLong = 123456789L,
            NullableDecimal = 99.99m,
            NullableDateTime = new DateTime(2026, 2, 2, 12, 0, 0, DateTimeKind.Utc),
            NullableBool = true,
            NullableEnum = TestStatus.Active
        };

        // Act
        context.NullableTypeEntities.Add(entity);
        context.SaveChanges();

        // Assert
        using var context2 = _fixture.CreateContext(_testDir);
        var loaded = context2.NullableTypeEntities.ToList().First();
        Assert.Equal(42, loaded.NullableInt);
        Assert.Equal(123456789L, loaded.NullableLong);
        Assert.Equal(99.99m, loaded.NullableDecimal);
        Assert.Equal(new DateTime(2026, 2, 2, 12, 0, 0, DateTimeKind.Utc), loaded.NullableDateTime);
        Assert.True(loaded.NullableBool);
        Assert.Equal(TestStatus.Active, loaded.NullableEnum);
    }

    [Fact]
    public void NullableTypeEntity_WithNulls_IsPersisted()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        var entity = new TestEntityWithNullableTypes
        {
            NullableInt = null,
            NullableLong = null,
            NullableDecimal = null,
            NullableDateTime = null,
            NullableBool = null,
            NullableEnum = null
        };

        // Act
        context.NullableTypeEntities.Add(entity);
        context.SaveChanges();

        // Assert
        using var context2 = _fixture.CreateContext(_testDir);
        var loaded = context2.NullableTypeEntities.ToList().First();
        Assert.Null(loaded.NullableInt);
        Assert.Null(loaded.NullableLong);
        Assert.Null(loaded.NullableDecimal);
        Assert.Null(loaded.NullableDateTime);
        Assert.Null(loaded.NullableBool);
        Assert.Null(loaded.NullableEnum);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            try { Directory.Delete(_testDir, true); } catch { }
        }
    }
}

