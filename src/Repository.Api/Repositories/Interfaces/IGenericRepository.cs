using System.Linq.Expressions;
using Repository.Api.Entities;

namespace Repository.Api.Repositories.Interfaces;

public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    IQueryable<T> Query();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    void Update(T entity);
    Task DeleteAsync(T entity); // soft delete
    Task HardDeleteAsync(T entity); // hard delete

    Task<PagedResult<T>> GetPagedAsync(QueryParameters parameters, Expression<Func<T, bool>>? filter = null);
}