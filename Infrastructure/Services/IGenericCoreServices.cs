using System.Linq.Expressions;

namespace Infrastructure.Services;

public interface IGenericCoreService<T>
{
    Task<T> Insert(T entity);
    Task<IReadOnlyList<T>> GetListAsync();
    IQueryable<T> GetQueryable(Expression<Func<T, bool>>? predicate = null, string? includes = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);
    Task<T?> GetById(int id);
    int Update(T entitiy);
    bool Delete(T entity);
    Task<bool> Delete(int id);
}