// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore.Storage;

namespace RonSijm.EF.FileStore.Storage.Internal;

/// <summary>
/// Transaction manager for file-based providers (no-op implementation).
/// </summary>
public class FileStoreTransactionManager : IDbContextTransactionManager
{
    /// <inheritdoc />
    public virtual IDbContextTransaction CurrentTransaction => null!;

    /// <inheritdoc />
    public virtual IDbContextTransaction BeginTransaction()
    {
        return new FileStoreTransaction();
    }

    /// <inheritdoc />
    public virtual Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(BeginTransaction());
    }

    /// <inheritdoc />
    public virtual void CommitTransaction()
    {
        // No-op for file-based storage
    }

    /// <inheritdoc />
    public virtual Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        CommitTransaction();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual void RollbackTransaction()
    {
        // No-op for file-based storage - rollback not supported
    }

    /// <inheritdoc />
    public virtual Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        RollbackTransaction();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual void ResetState()
    {
    }

    /// <inheritdoc />
    public virtual Task ResetStateAsync(CancellationToken cancellationToken = default)
    {
        ResetState();
        return Task.CompletedTask;
    }
}

/// <summary>
/// A no-op transaction for file-based providers.
/// </summary>
public class FileStoreTransaction : IDbContextTransaction
{
    /// <inheritdoc />
    public Guid TransactionId { get; } = Guid.NewGuid();

    /// <inheritdoc />
    public void Commit()
    {
        // No-op
    }

    /// <inheritdoc />
    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        Commit();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Rollback()
    {
        // No-op - rollback not supported for file-based storage
    }

    /// <inheritdoc />
    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        Rollback();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Dispose()
    {
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}

