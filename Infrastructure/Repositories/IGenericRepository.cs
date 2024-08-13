using System.Linq.Expressions;

namespace Infrastructure.Repositories;

public interface IGenericRepository<TEntity> where TEntity : class
{
    #region CREATE
    void Insert(TEntity entity);
    #endregion

    #region READ
    Task<TEntity?> GetByIdAsync(int id);
    IQueryable<TEntity> Get(Expression<Func<TEntity, bool>> predicate);
    IQueryable<TEntity> GetQueryable();
    Task<IReadOnlyList<TEntity>> GetListAsync();
    #endregion

    #region UPDATE
    int Update(TEntity entity);
    #endregion

    #region DELETE
    void Delete(TEntity entity);
    #endregion

    #region SAVE
    Task<int> SaveAsync();
    #endregion
}
