// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RonSijm.EF.FileStore.ForeignKeys;

namespace RonSijm.EF.Markdown.Tests.ForeignKeys;

#region Test Models

public class Author
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<Book> Books { get; set; } = new List<Book>();
}

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int AuthorId { get; set; }
    public Author Author { get; set; } = null!;
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? CategoryId { get; set; }  // Optional FK
    public Category? Category { get; set; }
}

public class ForeignKeyTestDbContext : DbContext
{
    public ForeignKeyTestDbContext(DbContextOptions<ForeignKeyTestDbContext> options)
        : base(options)
    {
    }

    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Books)
                .WithOne(e => e.Author)
                .HasForeignKey(e => e.AuthorId)
                .IsRequired();
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Products)
                .WithOne(e => e.Category)
                .HasForeignKey(e => e.CategoryId)
                .IsRequired(false);  // Optional relationship
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
    }
}

#endregion

public class ForeignKeyValidationTests : IDisposable
{
    private readonly List<string> _testDirectories = new();

    private string CreateTestDirectory()
    {
        var testDir = Path.Combine(Path.GetTempPath(), "MarkdownEFCoreTests_FK", Guid.NewGuid().ToString());
        _testDirectories.Add(testDir);
        return testDir;
    }

    private ForeignKeyTestDbContext CreateContext(string directoryPath, bool enforceForeignKeys)
    {
        var options = new DbContextOptionsBuilder<ForeignKeyTestDbContext>()
            .UseMarkdown(directoryPath, builder =>
            {
                if (enforceForeignKeys)
                {
                    builder.EnforceForeignKeys();
                }
            })
            .ConfigureWarnings(w => w.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning))
            .Options;

        return new ForeignKeyTestDbContext(options);
    }

    public void Dispose()
    {
        foreach (var dir in _testDirectories)
        {
            if (Directory.Exists(dir))
            {
                try
                {
                    Directory.Delete(dir, recursive: true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
    }

    #region Tests with FK Validation DISABLED - Can violate constraints

    [Fact]
    public void Disabled_CanInsertChildWithNonExistentParent()
    {
        // Arrange
        var testDir = CreateTestDirectory();
        using var context = CreateContext(testDir, enforceForeignKeys: false);
        context.Database.EnsureCreated();

        // Act - Insert a book with a non-existent author ID
        var book = new Book { Title = "Orphan Book", AuthorId = 999 };
        context.Books.Add(book);

        // Assert - Should NOT throw, FK validation is disabled
        var exception = Record.Exception(() => context.SaveChanges());
        Assert.Null(exception);

        // Verify the book was actually saved
        using var context2 = CreateContext(testDir, enforceForeignKeys: false);
        var savedBook = context2.Books.ToList().First();
        Assert.Equal("Orphan Book", savedBook.Title);
        Assert.Equal(999, savedBook.AuthorId);
    }

    [Fact]
    public void Disabled_CanDeleteParentWithExistingChildren()
    {
        // Arrange
        var testDir = CreateTestDirectory();
        using var context = CreateContext(testDir, enforceForeignKeys: false);
        context.Database.EnsureCreated();

        var author = new Author { Name = "Test Author" };
        context.Authors.Add(author);
        context.SaveChanges();
        var authorId = author.Id;

        var book = new Book { Title = "Test Book", AuthorId = authorId };
        context.Books.Add(book);
        context.SaveChanges();

        // Use a fresh context to avoid EF Core's cascade delete tracking
        using var context2 = CreateContext(testDir, enforceForeignKeys: false);
        var authorToDelete = context2.Authors.ToList().First(a => a.Id == authorId);

        // Act - Delete the author while the book still references it
        context2.Authors.Remove(authorToDelete);

        // Assert - Should NOT throw, FK validation is disabled
        var exception = Record.Exception(() => context2.SaveChanges());
        Assert.Null(exception);

        // Verify the author was deleted but the book remains (orphaned)
        using var context3 = CreateContext(testDir, enforceForeignKeys: false);
        Assert.Empty(context3.Authors.ToList());
        var orphanedBook = context3.Books.ToList().First();
        Assert.Equal("Test Book", orphanedBook.Title);
        Assert.Equal(authorId, orphanedBook.AuthorId);  // Still references deleted author
    }

    [Fact]
    public void Disabled_CanUpdateForeignKeyToNonExistentParent()
    {
        // Arrange
        var testDir = CreateTestDirectory();
        using var context = CreateContext(testDir, enforceForeignKeys: false);
        context.Database.EnsureCreated();

        var author = new Author { Name = "Real Author" };
        context.Authors.Add(author);
        context.SaveChanges();

        var book = new Book { Title = "Test Book", AuthorId = author.Id };
        context.Books.Add(book);
        context.SaveChanges();

        // Act - Update the book to reference a non-existent author
        book.AuthorId = 999;

        // Assert - Should NOT throw, FK validation is disabled
        var exception = Record.Exception(() => context.SaveChanges());
        Assert.Null(exception);

        // Verify the book was updated with invalid FK
        using var context2 = CreateContext(testDir, enforceForeignKeys: false);
        var updatedBook = context2.Books.ToList().First();
        Assert.Equal(999, updatedBook.AuthorId);
    }

    #endregion

    #region Tests with FK Validation ENABLED - Should throw on violations

    [Fact]
    public void Enabled_ThrowsOnInsertChildWithNonExistentParent()
    {
        // Arrange
        var testDir = CreateTestDirectory();
        using var context = CreateContext(testDir, enforceForeignKeys: true);
        context.Database.EnsureCreated();

        // Act - Try to insert a book with a non-existent author ID
        var book = new Book { Title = "Orphan Book", AuthorId = 999 };
        context.Books.Add(book);

        // Assert - Should throw ForeignKeyValidationException
        var exception = Assert.Throws<ForeignKeyValidationException>(() => context.SaveChanges());
        Assert.Contains("Foreign key constraint violation", exception.Message);
        Assert.Contains("Book", exception.Message);
        Assert.Contains("Author", exception.Message);
        Assert.Contains("999", exception.Message);
    }

    [Fact]
    public void Enabled_ThrowsOnDeleteParentWithExistingChildren()
    {
        // Arrange
        var testDir = CreateTestDirectory();
        using var context = CreateContext(testDir, enforceForeignKeys: true);
        context.Database.EnsureCreated();

        var author = new Author { Name = "Test Author" };
        context.Authors.Add(author);
        context.SaveChanges();

        var book = new Book { Title = "Test Book", AuthorId = author.Id };
        context.Books.Add(book);
        context.SaveChanges();

        // Act - Try to delete the author while the book still references it
        context.Authors.Remove(author);

        // Assert - Should throw ForeignKeyValidationException
        var exception = Assert.Throws<ForeignKeyValidationException>(() => context.SaveChanges());
        Assert.Contains("Foreign key constraint violation", exception.Message);
        Assert.Contains("Cannot delete", exception.Message);
        Assert.Contains("Author", exception.Message);
        Assert.Contains("Book", exception.Message);
    }

    [Fact]
    public void Enabled_ThrowsOnUpdateForeignKeyToNonExistentParent()
    {
        // Arrange
        var testDir = CreateTestDirectory();
        using var context = CreateContext(testDir, enforceForeignKeys: true);
        context.Database.EnsureCreated();

        var author = new Author { Name = "Real Author" };
        context.Authors.Add(author);
        context.SaveChanges();

        var book = new Book { Title = "Test Book", AuthorId = author.Id };
        context.Books.Add(book);
        context.SaveChanges();

        // Act - Try to update the book to reference a non-existent author
        book.AuthorId = 999;

        // Assert - Should throw ForeignKeyValidationException
        var exception = Assert.Throws<ForeignKeyValidationException>(() => context.SaveChanges());
        Assert.Contains("Foreign key constraint violation", exception.Message);
        Assert.Contains("Cannot update", exception.Message);
        Assert.Contains("999", exception.Message);
    }

    #endregion

    #region Tests with FK Validation ENABLED - Valid operations should succeed

    [Fact]
    public void Enabled_AllowsInsertChildWithExistingParent()
    {
        // Arrange
        var testDir = CreateTestDirectory();
        using var context = CreateContext(testDir, enforceForeignKeys: true);
        context.Database.EnsureCreated();

        var author = new Author { Name = "Valid Author" };
        context.Authors.Add(author);
        context.SaveChanges();

        // Act - Insert a book with a valid author ID
        var book = new Book { Title = "Valid Book", AuthorId = author.Id };
        context.Books.Add(book);

        // Assert - Should NOT throw
        var exception = Record.Exception(() => context.SaveChanges());
        Assert.Null(exception);

        // Verify the book was saved
        using var context2 = CreateContext(testDir, enforceForeignKeys: true);
        var savedBook = context2.Books.ToList().First();
        Assert.Equal("Valid Book", savedBook.Title);
        Assert.Equal(author.Id, savedBook.AuthorId);
    }

    [Fact]
    public void Enabled_AllowsDeleteParentWithNoChildren()
    {
        // Arrange
        var testDir = CreateTestDirectory();
        using var context = CreateContext(testDir, enforceForeignKeys: true);
        context.Database.EnsureCreated();

        var author = new Author { Name = "Lonely Author" };
        context.Authors.Add(author);
        context.SaveChanges();

        // Act - Delete the author (no books reference it)
        context.Authors.Remove(author);

        // Assert - Should NOT throw
        var exception = Record.Exception(() => context.SaveChanges());
        Assert.Null(exception);

        // Verify the author was deleted
        using var context2 = CreateContext(testDir, enforceForeignKeys: true);
        Assert.Empty(context2.Authors.ToList());
    }

    [Fact]
    public void Enabled_AllowsDeleteParentAfterDeletingChildren()
    {
        // Arrange
        var testDir = CreateTestDirectory();
        using var context = CreateContext(testDir, enforceForeignKeys: true);
        context.Database.EnsureCreated();

        var author = new Author { Name = "Test Author" };
        context.Authors.Add(author);
        context.SaveChanges();

        var book = new Book { Title = "Test Book", AuthorId = author.Id };
        context.Books.Add(book);
        context.SaveChanges();

        // Act - Delete the book first (separate SaveChanges), then the author
        context.Books.Remove(book);
        context.SaveChanges();  // Commit the book deletion first

        context.Authors.Remove(author);

        // Assert - Should NOT throw (book was already deleted)
        var exception = Record.Exception(() => context.SaveChanges());
        Assert.Null(exception);

        // Verify both were deleted
        using var context2 = CreateContext(testDir, enforceForeignKeys: true);
        Assert.Empty(context2.Authors.ToList());
        Assert.Empty(context2.Books.ToList());
    }

    #endregion

    #region Tests for Optional Foreign Keys

    [Fact]
    public void Enabled_AllowsInsertWithNullOptionalForeignKey()
    {
        // Arrange
        var testDir = CreateTestDirectory();
        using var context = CreateContext(testDir, enforceForeignKeys: true);
        context.Database.EnsureCreated();

        // Act - Insert a product with null CategoryId (optional FK)
        var product = new Product { Name = "Uncategorized Product", CategoryId = null };
        context.Products.Add(product);

        // Assert - Should NOT throw (optional FK can be null)
        var exception = Record.Exception(() => context.SaveChanges());
        Assert.Null(exception);

        // Verify the product was saved
        using var context2 = CreateContext(testDir, enforceForeignKeys: true);
        var savedProduct = context2.Products.ToList().First();
        Assert.Equal("Uncategorized Product", savedProduct.Name);
        Assert.Null(savedProduct.CategoryId);
    }

    [Fact]
    public void Enabled_AllowsInsertWithValidOptionalForeignKey()
    {
        // Arrange
        var testDir = CreateTestDirectory();
        using var context = CreateContext(testDir, enforceForeignKeys: true);
        context.Database.EnsureCreated();

        var category = new Category { Name = "Electronics" };
        context.Categories.Add(category);
        context.SaveChanges();

        // Act - Insert a product with a valid CategoryId
        var product = new Product { Name = "Laptop", CategoryId = category.Id };
        context.Products.Add(product);

        // Assert - Should NOT throw
        var exception = Record.Exception(() => context.SaveChanges());
        Assert.Null(exception);

        // Verify the product was saved
        using var context2 = CreateContext(testDir, enforceForeignKeys: true);
        var savedProduct = context2.Products.ToList().First();
        Assert.Equal("Laptop", savedProduct.Name);
        Assert.Equal(category.Id, savedProduct.CategoryId);
    }

    [Fact]
    public void Enabled_ThrowsOnInsertWithInvalidOptionalForeignKey()
    {
        // Arrange
        var testDir = CreateTestDirectory();
        using var context = CreateContext(testDir, enforceForeignKeys: true);
        context.Database.EnsureCreated();

        // Act - Try to insert a product with a non-existent CategoryId
        var product = new Product { Name = "Invalid Product", CategoryId = 999 };
        context.Products.Add(product);

        // Assert - Should throw (even optional FK must reference existing parent if not null)
        var exception = Assert.Throws<ForeignKeyValidationException>(() => context.SaveChanges());
        Assert.Contains("Foreign key constraint violation", exception.Message);
        Assert.Contains("Product", exception.Message);
        Assert.Contains("Category", exception.Message);
    }

    #endregion
}

