using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TrackIT.Core.Entities;
using TrackIT.Core.Interfaces;
using TrackIT.Data.Context;

namespace TrackIT.Data.Repositories
{
    public class GenericRepository<T>(AppDbContext context) : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly AppDbContext _context = context;

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        // --- THE FIX ---
        public async Task<IReadOnlyList<T>> GetAsync(
            Expression<Func<T, bool>>? filter = null,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _context.Set<T>();

            // 1. Apply Includes (Joins)
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            // 2. Apply Filter (Where)
            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query.ToListAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
        }

        public async Task UpdateAsync(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(T entity)
        {
            var property = entity.GetType().GetProperty("IsDeleted");

            if (property != null && property.PropertyType == typeof(bool))
            {
                // SOFT DELETE: Set IsDeleted = true
                property.SetValue(entity, true);
                await UpdateAsync(entity);
            }
            else
            {
                // HARD DELETE: If it doesn't support soft delete, remove it really
                _context.Set<T>().Remove(entity);
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Set<T>().AnyAsync(e => e.Id == id);
        }
    }
}