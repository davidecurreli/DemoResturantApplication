using System.Linq.Expressions;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

/// <summary>
/// Generic repository for all Domain.Entities.
/// 
/// This repository implements the IGenericRepository(TEntity) interface and is used to perform CRUD operations on the lowest level of the application (the data layer).
/// 
/// By design, this repository is not aware of the business logic and is not aware of the existence of the service layer.
/// To interact with multiple entities, do not specialize this repository, instead use the IGenericCoreService(TEntity) interface.
/// 
/// Any specialization of this repository should be done in the service layer, not in the data layer. 
/// </summary>
/// <typeparam name="TEntity">The BaseEntity(int) type</typeparam>
public class GenericRepository<TEntity>(DbContextClass context) : IGenericRepository<TEntity> where TEntity : BaseEntity
{
    protected readonly DbContextClass _dbContext = context;

    #region CREATE
    /// <summary>
    /// Insert an entity into the database
    /// </summary>
    /// <param name="entity">TEntity</param>
    public void Insert(TEntity entity)
    {
        try
        {
            _dbContext.Set<TEntity>().Add(entity);
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    #endregion

    #region READ
    /// <summary>
    /// Get an entity by id from the database
    /// </summary>
    /// <param name="predicate">Expression[Func[TEntity, bool]] predicate</param>
    /// <returns>TEntity</returns>
    public IQueryable<TEntity> Get(Expression<Func<TEntity, bool>> predicate)
    {
        return _dbContext.Set<TEntity>().Where(predicate);
    }

    /// <summary>
    /// Get an entity by id from the database
    /// </summary>
    /// <returns>IQueryable(TEntity)</returns>
    public IQueryable<TEntity> GetQueryable()
    {
        return _dbContext.Set<TEntity>().AsQueryable();
    }

    /// <summary>
    /// Get the list of entities from the database, optionally override the soft delete filter
    /// </summary>
    /// <param name="overrideSoftDelete">whether or not to retrieve even soft delete rows</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<IReadOnlyList<TEntity>> GetListAsync()
    {
        return await _dbContext.Set<TEntity>().ToListAsync();
    }

    /// <summary>
    /// Get an entity by id from the database
    /// </summary>
    /// <param name="id">int</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<TEntity?> GetByIdAsync(int id)
    {
        var idProperty = typeof(TEntity).GetProperty("Id") ?? throw new Exception("Expression parameters cannot be null");

        ParameterExpression param = Expression.Parameter(typeof(TEntity), "x");
        var body = Expression.Equal(Expression.Property(param, idProperty), Expression.Constant(id));

        var lamdaExpression = Expression.Lambda<Func<TEntity, bool>>(body, param);
        try
        {
            return await _dbContext.Set<TEntity>().FirstOrDefaultAsync(lamdaExpression);
        }
        catch (Exception ex) { throw; }
    }
    #endregion

    #region UPDATE
    /// <summary>
    /// Try to Update an entity and save changes to the database.
    /// </summary>
    /// <param name="entity"></param>
    public int Update(TEntity entity)
    {
        _dbContext.Set<TEntity>().Update(entity);

        var saveFailed = false;
        do
        {
            saveFailed = false;
            try { return _dbContext.SaveChanges(); }
            catch (DbUpdateConcurrencyException ex)
            {
                saveFailed = true;

                // Update the values of the entity that failed to save from the store
                ex.Entries.Single().Reload();
            }
            catch (Exception ex) { throw; }
        } while (saveFailed);

        return 0;
    }
    #endregion

    #region DELETE - HARD
    /// <summary>
    /// Delete an entity from the database
    /// </summary>
    /// <param name="entity"></param>
    public void Delete(TEntity entity)
    {
        try
        {
            _dbContext.Set<TEntity>().Remove(entity);
            _dbContext.SaveChanges();
        }
        catch (Exception ex) { throw; }
    }
    #endregion

    #region SAVE 
    /// <summary>
    /// Save changes to the database
    /// </summary>
    /// <returns></returns>
    public async Task<int> SaveAsync()
    {
        try
        {
            return await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex) { throw; }
    }
    #endregion
}