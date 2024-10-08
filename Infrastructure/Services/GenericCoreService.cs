using System.Linq.Expressions;
using Domain;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class GenericCoreService<TRepository, TEntity>(IGenericRepository<TEntity> repository) : IGenericCoreService<TEntity>
    where TRepository : class
    where TEntity : BaseEntity
{
    protected readonly IGenericRepository<TEntity> repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public virtual async Task<TEntity> Insert(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        try
        {
            entity.CreatedOn = DateTime.Now;
            entity.UpdatedOn = DateTime.Now;

            repository.Insert(entity);
            await repository.SaveAsync();

            return entity;
        }
        catch (Exception e)
        {
            // Should log the exception here
            throw;
        }
    }

    public virtual async Task<IReadOnlyList<TEntity>> GetListAsync()
    {
        return await repository.GetListAsync();
    }

    public virtual IQueryable<TEntity> GetQueryable(
        Expression<Func<TEntity, bool>>? predicate = null,
        string? includes = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null)
    {
        var query = repository.GetQueryable();

        if (predicate != null)
            query = query.Where(predicate);

        if (!string.IsNullOrWhiteSpace(includes))
        {
            foreach (var includeProperty in includes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty.Trim());
            }
        }

        return orderBy != null ? orderBy(query) : query;
    }

    public virtual async Task<TEntity?> GetById(int id)
    {
        return await repository.GetByIdAsync(id);
    }

    public virtual int Update(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        entity.UpdatedOn = DateTime.Now;

        try
        {
            return repository.Update(entity);
        }
        catch (Exception e)
        {
            // Should log the exception here
            throw;
        }
    }

    public virtual bool Delete(TEntity entity)
    {
        if (entity == null)
            return false;

        try
        {
            repository.Delete(entity);
            return true;
        }
        catch (Exception e)
        {
            // Should log the exception here
            return false;
        }
    }

    public virtual async Task<bool> Delete(int id)
    {
        if (id == 0)
            return false;

        TEntity? existingEntity = await GetById(id);

        if (existingEntity is null)
            return false;

        return Delete(existingEntity);
    }
}