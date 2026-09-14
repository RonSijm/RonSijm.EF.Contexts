using RonSijm.EF.Markdown.Tests.TestModels;

#pragma warning disable EF1001 // Internal EF Core API usage

namespace RonSijm.EF.Markdown.Tests.Storage;

public class MarkdownTransactionManagerTests : IClassFixture<TestFixture>, IDisposable
{
    private readonly TestFixture _fixture;
    private readonly string _testDir;

    public MarkdownTransactionManagerTests(TestFixture fixture)
    {
        _fixture = fixture;
        _testDir = _fixture.CreateTestDirectory();
    }

    [Fact]
    public void BeginTransaction_ReturnsTransaction()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        // Act
        using var transaction = context.Database.BeginTransaction();

        // Assert
        Assert.NotNull(transaction);
        Assert.NotEqual(Guid.Empty, transaction.TransactionId);
    }

    [Fact]
    public async Task BeginTransactionAsync_ReturnsTransaction()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        await context.Database.EnsureCreatedAsync();

        // Act
        await using var transaction = await context.Database.BeginTransactionAsync();

        // Assert
        Assert.NotNull(transaction);
        Assert.NotEqual(Guid.Empty, transaction.TransactionId);
    }

    [Fact]
    public void CommitTransaction_DoesNotThrow()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();
        using var transaction = context.Database.BeginTransaction();

        // Act & Assert - Should not throw
        transaction.Commit();
    }

    [Fact]
    public async Task CommitTransactionAsync_DoesNotThrow()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        await context.Database.EnsureCreatedAsync();
        await using var transaction = await context.Database.BeginTransactionAsync();

        // Act & Assert - Should not throw
        await transaction.CommitAsync();
    }

    [Fact]
    public void RollbackTransaction_DoesNotThrow()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();
        using var transaction = context.Database.BeginTransaction();

        // Act & Assert - Should not throw
        transaction.Rollback();
    }

    [Fact]
    public async Task RollbackTransactionAsync_DoesNotThrow()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        await context.Database.EnsureCreatedAsync();
        await using var transaction = await context.Database.BeginTransactionAsync();

        // Act & Assert - Should not throw
        await transaction.RollbackAsync();
    }

    [Fact]
    public void Transaction_DisposeAsync_DoesNotThrow()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();
        var transaction = context.Database.BeginTransaction();

        // Act & Assert - Should not throw
        transaction.Dispose();
    }

    [Fact]
    public void CurrentTransaction_ReturnsNull()
    {
        // Arrange
        using var context = _fixture.CreateContext(_testDir);
        context.Database.EnsureCreated();

        // Act
        var currentTransaction = context.Database.CurrentTransaction;

        // Assert
        Assert.Null(currentTransaction);
    }

    [Fact]
    public void TransactionManager_CommitTransaction_DoesNotThrow()
    {
        // Arrange
        var transactionManager = new RonSijm.EF.FileStore.Storage.Internal.FileStoreTransactionManager();

        // Act & Assert - Should not throw
        var exception = Record.Exception(() => transactionManager.CommitTransaction());
        Assert.Null(exception);
    }

    [Fact]
    public async Task TransactionManager_CommitTransactionAsync_DoesNotThrow()
    {
        // Arrange
        var transactionManager = new RonSijm.EF.FileStore.Storage.Internal.FileStoreTransactionManager();

        // Act & Assert - Should not throw
        var exception = await Record.ExceptionAsync(() => transactionManager.CommitTransactionAsync());
        Assert.Null(exception);
    }

    [Fact]
    public void TransactionManager_RollbackTransaction_DoesNotThrow()
    {
        // Arrange
        var transactionManager = new RonSijm.EF.FileStore.Storage.Internal.FileStoreTransactionManager();

        // Act & Assert - Should not throw
        var exception = Record.Exception(() => transactionManager.RollbackTransaction());
        Assert.Null(exception);
    }

    [Fact]
    public async Task TransactionManager_RollbackTransactionAsync_DoesNotThrow()
    {
        // Arrange
        var transactionManager = new RonSijm.EF.FileStore.Storage.Internal.FileStoreTransactionManager();

        // Act & Assert - Should not throw
        var exception = await Record.ExceptionAsync(() => transactionManager.RollbackTransactionAsync());
        Assert.Null(exception);
    }

    [Fact]
    public void TransactionManager_ResetState_DoesNotThrow()
    {
        // Arrange
        var transactionManager = new RonSijm.EF.FileStore.Storage.Internal.FileStoreTransactionManager();

        // Act & Assert - Should not throw
        var exception = Record.Exception(() => transactionManager.ResetState());
        Assert.Null(exception);
    }

    [Fact]
    public async Task TransactionManager_ResetStateAsync_DoesNotThrow()
    {
        // Arrange
        var transactionManager = new RonSijm.EF.FileStore.Storage.Internal.FileStoreTransactionManager();

        // Act & Assert - Should not throw
        var exception = await Record.ExceptionAsync(() => transactionManager.ResetStateAsync());
        Assert.Null(exception);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            try { Directory.Delete(_testDir, true); } catch { }
        }
    }
}

