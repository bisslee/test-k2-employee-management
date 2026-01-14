using Biss.EmployeeManagement.Domain.Entities;
using Biss.EmployeeManagement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace Biss.EmployeeManagement.Infrastructure.Repositories
{
    public class ReadRepository<TEntity> : IReadRepository<TEntity> where TEntity : class
    {
        protected readonly AppDbContext Context;
        private readonly ILogger<ReadRepository<TEntity>> _logger;

        public ReadRepository(
            AppDbContext context,
            ILogger<ReadRepository<TEntity>> logger)
        {
            Context = context;
            _logger = logger;
        }

        public async Task<List<TEntity>> Find(Expression<Func<TEntity, bool>> predicate)
        {
            _logger.LogDebug("Executing Find query with predicate");
            IQueryable<TEntity> query = Context.Set<TEntity>()
                .AsNoTracking() // Otimização: não tracking para consultas
                .Where(predicate);
            
            // Incluir PhoneNumbers se for Employee
            if (typeof(TEntity) == typeof(Employee))
            {
                query = ((IQueryable<Employee>)query).Include(e => e.PhoneNumbers).Cast<TEntity>();
            }
            
            return await query.ToListAsync();
        }

        public async Task<(List<TEntity>, int)> FindWithPagination
        (
            Expression<Func<TEntity, bool>> predicate,
            int page,
            int pageSize,
            string? fieldName = null,
            string? order = null
        )
        {
            _logger.LogDebug("Executing FindWithPagination. Page: {Page}, PageSize: {PageSize}, FieldName: {FieldName}, Order: {Order}", 
                page, pageSize, fieldName, order);

            // Otimização: Executar count e data em paralelo
            IQueryable<TEntity> query = Context.Set<TEntity>()
                .AsNoTracking() // Otimização: não tracking para consultas
                .Where(predicate);
            
            // Incluir PhoneNumbers se for Employee
            if (typeof(TEntity) == typeof(Employee))
            {
                query = ((IQueryable<Employee>)query).Include(e => e.PhoneNumbers).Cast<TEntity>();
            }

            // Executar count primeiro (DbContext não é thread-safe)
            var total = await query.CountAsync();
            
            if (!string.IsNullOrEmpty(fieldName))
            {
                var ordering = fieldName +
                    (string.Equals(order, "desc", StringComparison.OrdinalIgnoreCase)
                        ? " descending" : "");
                query = query.OrderBy(ordering);
            }

            // Executar data após o count
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogDebug("FindWithPagination completed. Total: {Total}, Returned: {Returned}", total, data.Count);

            return (data, total);
        }

        public async Task<TEntity?> GetByIdAsync(Guid id)
        {
            _logger.LogDebug("Executing GetByIdAsync for ID: {Id}", id);
            IQueryable<TEntity> query = Context.Set<TEntity>()
                .AsNoTracking() // Otimização: não tracking para consultas
                .Where(e => EF.Property<Guid>(e, "Id") == id);
            
            // Incluir PhoneNumbers se for Employee
            if (typeof(TEntity) == typeof(Employee))
            {
                query = ((IQueryable<Employee>)query).Include(e => e.PhoneNumbers).Cast<TEntity>();
            }
            
            return await query.FirstOrDefaultAsync();
        }
    }
}
