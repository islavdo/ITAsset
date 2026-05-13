using Microsoft.EntityFrameworkCore;

namespace ITAssetAccounting.Infrastructure.Repositories;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    protected readonly DbContext Context;
    protected readonly DbSet<TEntity> Set;

    public Repository(DbContext context)
    {
        Context = context;
        Set = context.Set<TEntity>();
    }

    public IQueryable<TEntity> Query() => Set.AsQueryable();

    public async Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default) =>
        await Set.FindAsync(new[] { id }, cancellationToken);

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) =>
        await Set.AddAsync(entity, cancellationToken);

    public void Update(TEntity entity) => Set.Update(entity);

    public void Delete(TEntity entity) => Set.Remove(entity);
}
