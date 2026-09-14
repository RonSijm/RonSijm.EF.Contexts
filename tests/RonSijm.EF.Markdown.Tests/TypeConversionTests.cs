using RonSijm.EF.Markdown.Tests.TestModels;

namespace RonSijm.EF.Markdown.Tests;

public class TypeConversionTests : IClassFixture<TestFixture>, IDisposable
{
    private readonly TestFixture _fixture;
    private readonly string _testDir;

    public TypeConversionTests(TestFixture fixture)
    {
        _fixture = fixture;
        _testDir = _fixture.CreateTestDirectory();
    }

    [Fact]
    public void LongKey_GeneratesAndPersists()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        var entity = new TestEntityWithLongKey { Name = "Long Key Entity" };

        // Act
        context.LongKeyEntities.Add(entity);
        context.SaveChanges();

        // Assert
        Assert.NotEqual(0L, entity.Id);

        using var context2 = _fixture.CreateContext(_testDir);
        var loaded = context2.LongKeyEntities.ToList().First();
        Assert.Equal(entity.Id, loaded.Id);
        Assert.Equal("Long Key Entity", loaded.Name);
    }

    [Fact]
    public void GuidKey_GeneratesAndPersists()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        var entity = new TestEntityWithGuidKey { Name = "Guid Key Entity" };

        // Act
        context.GuidKeyEntities.Add(entity);
        context.SaveChanges();

        // Assert
        Assert.NotEqual(Guid.Empty, entity.Id);

        using var context2 = _fixture.CreateContext(_testDir);
        var loaded = context2.GuidKeyEntities.ToList().First();
        Assert.Equal(entity.Id, loaded.Id);
        Assert.Equal("Guid Key Entity", loaded.Name);
    }

    [Fact]
    public void NullableInt_PersistsNullAndValue()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        var entityWithValue = new TestEntityWithNullableInt { NullableValue = 42, NullableString = "Has Value" };
        var entityWithNull = new TestEntityWithNullableInt { NullableValue = null, NullableString = null };

        // Act
        context.NullableEntities.AddRange(entityWithValue, entityWithNull);
        context.SaveChanges();

        // Assert
        using var context2 = _fixture.CreateContext(_testDir);
        var loaded = context2.NullableEntities.ToList().OrderBy(e => e.Id).ToList();

        Assert.Equal(42, loaded[0].NullableValue);
        Assert.Equal("Has Value", loaded[0].NullableString);
        Assert.Null(loaded[1].NullableValue);
        Assert.Null(loaded[1].NullableString);
    }

    [Fact]
    public void Enum_PersistsAndLoads()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        var entity1 = new TestEntityWithEnum { Status = TestStatus.Pending };
        var entity2 = new TestEntityWithEnum { Status = TestStatus.Active };
        var entity3 = new TestEntityWithEnum { Status = TestStatus.Completed };

        // Act
        context.EnumEntities.AddRange(entity1, entity2, entity3);
        context.SaveChanges();

        // Assert
        using var context2 = _fixture.CreateContext(_testDir);
        var loaded = context2.EnumEntities.ToList().OrderBy(e => e.Id).ToList();

        Assert.Equal(TestStatus.Pending, loaded[0].Status);
        Assert.Equal(TestStatus.Active, loaded[1].Status);
        Assert.Equal(TestStatus.Completed, loaded[2].Status);
    }

    [Fact]
    public void DateTimeOffset_PersistsAndLoads()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        var timestamp = new DateTimeOffset(2024, 6, 15, 10, 30, 0, TimeSpan.FromHours(2));
        var entity = new TestEntityWithDateTimeOffset { Timestamp = timestamp };

        // Act
        context.DateTimeOffsetEntities.Add(entity);
        context.SaveChanges();

        // Assert
        using var context2 = _fixture.CreateContext(_testDir);
        var loaded = context2.DateTimeOffsetEntities.ToList().First();
        Assert.Equal(timestamp, loaded.Timestamp);
    }

    [Fact]
    public void Decimal_PersistsWithPrecision()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        var entity = new TestEntity
        {
            Name = "Decimal Test",
            Price = 123.456789m,
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
        Assert.Equal(123.456789m, loaded.Price);
    }

    [Fact]
    public void NumericTypes_ShortByteDoubleFloat_PersistAndLoad()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        var entity = new TestEntityWithNumericTypes
        {
            ShortValue = 12345,
            ByteValue = 255,
            DoubleValue = 3.14159265358979,
            FloatValue = 2.71828f
        };

        // Act
        context.NumericEntities.Add(entity);
        context.SaveChanges();

        // Assert
        using var context2 = _fixture.CreateContext(_testDir);
        var loaded = context2.NumericEntities.ToList().First();
        Assert.Equal(12345, loaded.ShortValue);
        Assert.Equal(255, loaded.ByteValue);
        Assert.Equal(3.14159265358979, loaded.DoubleValue, 10);
        Assert.Equal(2.71828f, loaded.FloatValue, 4);
    }

    [Fact]
    public void NullValue_InMarkdownFile_ParsesCorrectly()
    {
        // Arrange - Create a markdown file with explicit (null) values
        Directory.CreateDirectory(_testDir);
        var filePath = Path.Combine(_testDir, "TestEntityWithNullableInt.md");
        var content = @"# TestEntityWithNullableInt

| Id | NullableValue | NullableString |
|---|---|---|
| 1 | (null) | (null) |
| 2 | 42 | Test String |
";
        File.WriteAllText(filePath, content);

        using var context = _fixture.CreateContext(_testDir);

        // Act
        var entities = context.NullableEntities.ToList().OrderBy(e => e.Id).ToList();

        // Assert
        Assert.Equal(2, entities.Count);
        Assert.Null(entities[0].NullableValue);
        Assert.Null(entities[0].NullableString);
        Assert.Equal(42, entities[1].NullableValue);
        Assert.Equal("Test String", entities[1].NullableString);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            try { Directory.Delete(_testDir, true); } catch { }
        }
    }
}

