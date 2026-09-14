namespace RonSijm.EF.Markdown.Tests.TestModels;

public class TestEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public Guid? ExternalId { get; set; }
}

public class TestEntityWithLongKey
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class TestEntityWithGuidKey
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class TestEntityWithNullableInt
{
    public int Id { get; set; }
    public int? NullableValue { get; set; }
    public string? NullableString { get; set; }
}

public enum TestStatus
{
    Pending,
    Active,
    Completed
}

public class TestEntityWithEnum
{
    public int Id { get; set; }
    public TestStatus Status { get; set; }
}

public class TestEntityWithDateTimeOffset
{
    public int Id { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

public class TestEntityWithNumericTypes
{
    public int Id { get; set; }
    public short ShortValue { get; set; }
    public byte ByteValue { get; set; }
    public double DoubleValue { get; set; }
    public float FloatValue { get; set; }
}

public class TestEntityWithDateTypes
{
    public int Id { get; set; }
    public DateOnly DateOnlyValue { get; set; }
    public TimeOnly TimeOnlyValue { get; set; }
    public TimeSpan TimeSpanValue { get; set; }
}

public class TestEntityWithNullableTypes
{
    public int Id { get; set; }
    public int? NullableInt { get; set; }
    public long? NullableLong { get; set; }
    public decimal? NullableDecimal { get; set; }
    public DateTime? NullableDateTime { get; set; }
    public bool? NullableBool { get; set; }
    public TestStatus? NullableEnum { get; set; }
}

