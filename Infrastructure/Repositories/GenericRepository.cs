using System.Linq.Expressions;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : BaseEntity
{
    protected readonly DbContextClass _dbContext;
    protected readonly DbSet<TEntity> _dbSet;

    public GenericRepository(DbContextClass context)
    {
        _dbContext = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _dbContext.Set<TEntity>();
    }

    public void Insert(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        try
        {
            _dbSet.Add(entity);
        }
        catch (Exception ex)
        {
            // Should log the exception here
            throw;
        }
    }

    public IQueryable<TEntity> Get(Expression<Func<TEntity, bool>> predicate)
    {
        return _dbSet.Where(predicate);
    }

    public IQueryable<TEntity> GetQueryable()
    {
        return _dbSet.AsQueryable();
    }

    public async Task<IReadOnlyList<TEntity>> GetListAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<TEntity?> GetByIdAsync(int id)
    {
        var idProperty = typeof(TEntity).GetProperty("Id") ?? throw new InvalidOperationException("Entity does not have an Id property");

        ParameterExpression param = Expression.Parameter(typeof(TEntity), "x");
        var body = Expression.Equal(Expression.Property(param, idProperty), Expression.Constant(id));

        var lambdaExpression = Expression.Lambda<Func<TEntity, bool>>(body, param);

        try
        {
            return await _dbSet.FirstOrDefaultAsync(lambdaExpression);
        }
        catch (Exception ex)
        {
            // Should log the exception here
            throw;
        }
    }

    public int Update(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _dbSet.Update(entity);

        bool saveFailed;
        do
        {
            try
            {
                return _dbContext.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                saveFailed = true;

                // Update the values of the entity that failed to save from the store
                ex.Entries.Single().Reload();
            }
            catch (Exception ex)
            {
                // Should log the exception here
                throw;
            }
        } while (saveFailed);

        return 0;
    }

    public void Delete(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        try
        {
            _dbSet.Remove(entity);
            _dbContext.SaveChanges();
        }
        catch (Exception ex)
        {
            // Should log the exception here
            throw;
        }
    }

    public async Task<int> SaveAsync()
    {
        try
        {
            return await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Should log the exception here
            throw;
        }
    }
}