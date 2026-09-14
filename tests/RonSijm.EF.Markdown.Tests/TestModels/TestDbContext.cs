using Microsoft.EntityFrameworkCore;

namespace RonSijm.EF.Markdown.Tests.TestModels;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public DbSet<TestEntity> TestEntities => Set<TestEntity>();
    public DbSet<TestEntityWithLongKey> LongKeyEntities => Set<TestEntityWithLongKey>();
    public DbSet<TestEntityWithGuidKey> GuidKeyEntities => Set<TestEntityWithGuidKey>();
    public DbSet<TestEntityWithNullableInt> NullableEntities => Set<TestEntityWithNullableInt>();
    public DbSet<TestEntityWithEnum> EnumEntities => Set<TestEntityWithEnum>();
    public DbSet<TestEntityWithDateTimeOffset> DateTimeOffsetEntities => Set<TestEntityWithDateTimeOffset>();
    public DbSet<TestEntityWithNumericTypes> NumericEntities => Set<TestEntityWithNumericTypes>();
    public DbSet<TestEntityWithDateTypes> DateTypeEntities => Set<TestEntityWithDateTypes>();
    public DbSet<TestEntityWithNullableTypes> NullableTypeEntities => Set<TestEntityWithNullableTypes>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TestEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
        });

        modelBuilder.Entity<TestEntityWithLongKey>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<TestEntityWithGuidKey>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<TestEntityWithNullableInt>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<TestEntityWithEnum>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<TestEntityWithDateTimeOffset>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<TestEntityWithNumericTypes>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<TestEntityWithDateTypes>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<TestEntityWithNullableTypes>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
    }
}

