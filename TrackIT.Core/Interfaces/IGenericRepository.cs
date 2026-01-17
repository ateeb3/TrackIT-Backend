using System.Linq.Expressions;
using TrackIT.Core.Entities;

namespace TrackIT.Core.Interfaces
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(int id);
        Task<IReadOnlyList<T>> GetAllAsync();

        // --- NEW POWERFUL METHOD ---
        // Allows: repo.GetAsync(x => x.Price > 100, x => x.Category, x => x.Vendor);
        Task<IReadOnlyList<T>> GetAsync(
            Expression<Func<T, bool>>? filter = null,
            params Expression<Func<T, object>>[] includes);

        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<bool> ExistsAsync(int id);
    }
}