using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Api.Data;
using RestaurantManagement.Api.Entities;
using RestaurantManagement.Api.Interfaces.Repositories;

namespace RestaurantManagement.Api.Repositories;

public class GenericRepository<T>(ApplicationDbContext dbContext) : IGenericRepository<T> where T : BaseEntity
{
    private readonly DbSet<T> _dbSet = dbContext.Set<T>();

    public IQueryable<T> Query() => _dbSet.AsQueryable();

    public async Task<T?> GetByIdAsync(Guid id, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;
        query = includes.Aggregate(query, (current, include) => current.Include(include));
        return await query.FirstOrDefaultAsync(entity => entity.Id == id);
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;
        query = includes.Aggregate(query, (current, include) => current.Include(include));
        return await query.FirstOrDefaultAsync(predicate);
    }

    public async Task<List<T>> ListAsync(Expression<Func<T, bool>>? predicate = null, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;
        query = includes.Aggregate(query, (current, include) => current.Include(include));
        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        return await query.ToListAsync();
    }

    public Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        return predicate is null ? _dbSet.CountAsync() : _dbSet.CountAsync(predicate);
    }

    public Task AddAsync(T entity) => _dbSet.AddAsync(entity).AsTask();

    public void Update(T entity) => _dbSet.Update(entity);

    public void Remove(T entity) => _dbSet.Remove(entity);

    public Task SaveChangesAsync() => dbContext.SaveChangesAsync();
}
