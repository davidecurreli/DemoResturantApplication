using System.Linq.Expressions;

namespace Infrastructure.Repositories;

public interface IGenericRepository<TEntity> where TEntity : class
{
    void Insert(TEntity entity);
    IQueryable<TEntity> Get(Expression<Func<TEntity, bool>> predicate);
    IQueryable<TEntity> GetQueryable();
    Task<IReadOnlyList<TEntity>> GetListAsync();
    Task<TEntity?> GetByIdAsync(int id);
    int Update(TEntity entity);
    void Delete(TEntity entity);
    Task<int> SaveAsync();
}