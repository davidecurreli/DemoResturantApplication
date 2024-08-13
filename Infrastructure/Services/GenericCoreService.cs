using Domain;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Services;

/// <summary>
/// <para>Generic Core Service for the main CRUD operations on a TEntity type</para>
/// 
/// <para>All generic services should inherit from this class to be implemented in Infrastructure/Services</para>
/// 
/// <para>Creating a specialyzed service is only necessary when the required result is not achievable with the generic methods</para>
/// </summary>
/// <typeparam name="TRepository"></typeparam>
/// <typeparam name="TEntity"></typeparam>
public class GenericCoreService<TRepository, TEntity>(IGenericRepository<TEntity> repository) 
    : IGenericCoreService<TEntity> where TRepository : class where TEntity : BaseEntity
{
    /// <summary>
    /// Insert an element to the respective IGenericRepository TEntity
    /// </summary>
    /// <param name="entity">Generic Domain.Entity</param>
    /// <param name="handleEx"></param>
    /// <returns>The newly created entity</returns>
    public virtual async Task<TEntity> Insert(TEntity entity)
    {
        try
        {
            entity.CreatedOn = DateTime.Now;
            entity.UpdatedOn = DateTime.Now;

            repository.Insert(entity);
            await repository.SaveAsync();

            return entity;
        }
        catch (Exception e) { throw; }
    }

    /// <summary>
    /// Main TEntity Getter. Get all elements of the selected TEntity
    /// </summary>
    /// <returns>A list of all TEntity elements available</returns>
    public virtual async Task<IReadOnlyList<TEntity>> GetListAsync()
    {
        return await repository.GetListAsync();
    }

    /// <summary>
    /// Main TEntity with predicate Getter. Get all elements of the selected TEntity that correspond with the given filter
    /// </summary>
    /// <returns>A list of all TEntity elements available that correspond with the given filter</returns>
    public IQueryable<TEntity> GetQueryable(
        Expression<Func<TEntity, bool>>? predicate = null,
        string? includes = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null)
    {
        var query = repository.GetQueryable();

        if (predicate is not null)
            query = query.Where(predicate);

        if (includes is not null)
        {
            foreach (var includeProperty in includes.Split(","))
                query = query.Include(includeProperty.Trim());
        }

        try
        {
            if (orderBy is not null)
                return orderBy(query);
            else
                return query;
        }
        catch (Exception e) { throw; }
    }

    /// <summary>
    /// Get a TEntity element by Id
    /// </summary>
    /// <param name="id">Int - The Id of the TEntity</param>
    /// <returns></returns>
    public virtual async Task<TEntity?> GetById(int id)
    {
        return await repository.GetByIdAsync(id);
    }

    /// <summary>
    /// Update an element in the respective IGenericRepository TEntity
    /// </summary>
    /// <param name="entity">>Generic Domain.Entity</param>
    /// <returns>>The updated Domain.Entity element</returns>
    public virtual int Update(TEntity entity)
    {
        entity.UpdatedOn = DateTime.Now;

        try { return repository.Update(entity); }
        catch (Exception e) { throw; }
    }

    /// <summary>
    /// (Soft) Delete a TEntity element
    /// </summary>
    /// <param name="entity">Generic Domain.Entity</param>
    /// <param name="handleEx"></param>
    /// <param name="hardDelete"></param>
    /// <returns>The successfully deleted Domain.Entity element</returns>
    public virtual bool Delete(TEntity entity)
    {
        try
        {
            if (entity == null)
                return false;

            repository.Delete(entity);

            return true;
        }
        catch (Exception e)
        {
            throw;
        }
    }

    /// <summary>
    /// Delete a TEntity element given its Id
    /// </summary>
    /// <param name="id">Int - The Id of the entity to be deleted</param>
    /// <returns>A delition confirmation bool</returns>
    public virtual async Task<bool> Delete(int id)
    {
        if (id == 0)
            return false;

        TEntity? existingEntity = await GetById(id);

        if (existingEntity is null)
            return false;

        repository.Delete(existingEntity);

        return true;
    }
}