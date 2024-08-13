using System.Linq.Expressions;

namespace Infrastructure.Services;

public interface IGenericCoreService<T>
{
    #region CREATE
    /// <summary>
    /// Insert an element to the respective IGenericRepository TEntity
    /// </summary>
    /// <param name="entity">Generic Domain.BaseEntity</param>
    /// <returns>The Id of the newly created entity</returns>
    Task<T> Insert(T entity);
    #endregion

    #region READ
    /// <summary>
    /// Main TEntity Getter. Get all elements of the selected TEntity
    /// </summary>
    /// <returns>A list of all TEntity elements available</returns>
    Task<IReadOnlyList<T>> GetListAsync();

    /// <summary>
    /// IQueryable version of the TEntity getter
    /// </summary>
    /// <returns>Returns an IQueryable item for LINQ data extraction</returns>
    IQueryable<T> GetQueryable(Expression<Func<T, bool>>? predicate = null, string? includes = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);

    /// <summary>
    /// Get a TEntity element by Id
    /// </summary>
    /// <param name="id">Int - The Id of the TEntity</param>
    /// <returns>TEntity element with the selected Id or null</returns>
    Task<T?> GetById(int id);
    #endregion

    #region UPDATE
    /// <summary>
    /// Update an element in the respective IGenericRepository TEntity
    /// </summary>
    /// <param name="entity">>Generic Domain.BaseEntity</param>
    /// <returns>>The updated Domain.BaseEntity element</returns>
    int Update(T entitiy);
    #endregion

    #region DELETE
    /// <summary>
    /// (Soft) Delete a TEntity element
    /// </summary>
    /// <param name="entity">Generic Domain.BaseEntity</param>
    /// <returns>The successfully deleted Domain.BaseEntity element</returns>
    bool Delete(T entity);

    /// <summary>
    /// (Soft) Delete a TEntity element given its Id
    /// </summary>
    /// <param name="id">Int - The Id of the entity to be soft deleted</param>
    /// <returns>The successfully deleted Domain.BaseEntity element</returns>
    Task<bool> Delete(int id);
    #endregion
}
