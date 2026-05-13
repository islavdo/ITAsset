using Microsoft.EntityFrameworkCore;

namespace ITAssetAccounting.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly DbContext _context;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(DbContext context)
    {
        _context = context;
    }

    public IRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        var type = typeof(TEntity);
        if (!_repositories.TryGetValue(type, out var repo))
        {
            repo = new Repository<TEntity>(_context);
            _repositories[type] = repo;
        }

        return (IRepository<TEntity>)repo;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
