using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Restaurante.Infraestructura; // For ApplicationDbContext
// First, define IBaseRepository<T> in Restaurante.Infraestructura.Repository
// This is a generic repository for basic CRUD operations


namespace Restaurante.Infraestructura.Repository
{
    public interface IBaseRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string? includeProperties = null,
            int? skip = null,
            int? take = null);

        Task<T?> GetByIdAsync(object id, string? includeProperties = null);

        Task AddAsync(T entity);

        Task UpdateAsync(T entity);

        Task DeleteAsync(T entity);

        Task<int> CountAsync(Expression<Func<T, bool>>? filter = null);
    }
}