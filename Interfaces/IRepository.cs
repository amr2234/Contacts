using System.Linq.Expressions;

namespace UsersContacts.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<(IEnumerable<T> Data, int TotalCount)> GetPagedAsync(
            Expression<Func<T, bool>>? filter = null,
            string? search = null,
            int? pageSize = null,
            int? pageNumber = null);
    }
}