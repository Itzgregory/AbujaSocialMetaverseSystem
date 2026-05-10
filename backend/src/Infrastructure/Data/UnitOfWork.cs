using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AbujaSocialMetaverse.Infrastructure.Data;

public interface IUnitOfWork : IAsyncDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    bool HasActiveTransaction { get; }
    DbSet<T> Set<T>() where T : class;
}

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;
    private TransactionState _state = TransactionState.None;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public DbSet<T> Set<T>() where T : class => _context.Set<T>();

    public bool HasActiveTransaction => _state == TransactionState.Active;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditFields();
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_state == TransactionState.Active)
            throw new InvalidOperationException(
                "A transaction is already active. Commit or rollback before starting a new one.");

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        _state = TransactionState.Active;
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_state != TransactionState.Active || _transaction is null)
            throw new InvalidOperationException("No active transaction to commit.");

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _transaction.CommitAsync(cancellationToken);
            _state = TransactionState.Committed;
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_state != TransactionState.Active || _transaction is null)
            return;

        try
        {
            await _transaction.RollbackAsync(cancellationToken);
            _state = TransactionState.RolledBack;
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    private void SetAuditFields()
    {
        var now = DateTimeOffset.UtcNow;
        var entries = _context.ChangeTracker.Entries<IAuditableEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case Microsoft.EntityFrameworkCore.EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                    break;
                case Microsoft.EntityFrameworkCore.EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    break;
            }
        }
    }

    private async Task DisposeTransactionAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_state == TransactionState.Active)
            await RollbackTransactionAsync();

        await DisposeTransactionAsync();
        GC.SuppressFinalize(this);
    }

    private enum TransactionState
    {
        None,
        Active,
        Committed,
        RolledBack
    }
}