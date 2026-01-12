using Biss.EmployeeManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Biss.EmployeeManagement.Domain.Repositories
{
    public interface IReadRepository<TEntity> where TEntity : class
    {
        Task<TEntity?> GetByIdAsync(Guid id);

        Task<List<TEntity>> Find(Expression<Func<TEntity, bool>> predicate);

        Task<(List<TEntity>, int)> FindWithPagination
           (
               Expression<Func<TEntity, bool>> predicate,
               int page,
               int pageSize,
               string? fieldName = null,
               string? order = null
           );
    }

}
