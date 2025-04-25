using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using UsersContacts.Interfaces;

namespace UsersContacts.Persistence
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly DbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(DbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public async Task UpdateAsync(T entity)
        {
            var entry = _context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }
            entry.State = EntityState.Modified;
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await Task.CompletedTask;
        }

        public async Task<(IEnumerable<T> Data, int TotalCount)> GetPagedAsync(
            Expression<Func<T, bool>>? filter = null,
            string? search = null,
            int? pageSize = null,
            int? pageNumber = null)
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x => EF.Property<string>(x, "Name").Contains(search) ||
                                       EF.Property<string>(x, "Phone").Contains(search) ||
                                       EF.Property<string>(x, "Address").Contains(search));
            }

            int totalCount = await query.CountAsync();
            int skip = ((pageNumber ?? 1) - 1) * (pageSize ?? 5);
            int take = pageSize ?? 5;

            var data = await query
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return (data, totalCount);
        }
    }
}