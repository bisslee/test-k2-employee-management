using Biss.EmployeeManagement.Domain.Entities;
using Biss.EmployeeManagement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Biss.EmployeeManagement.Infrastructure.Repositories
{
    public class WriteRepository<TEntity> : IWriteRepository<TEntity> where TEntity : BaseEntity
    {
        protected readonly AppDbContext Context;
        private readonly ILogger<WriteRepository<TEntity>> _logger;

        public WriteRepository(
            AppDbContext context, 
            ILogger<WriteRepository<TEntity>> logger)
        {
            Context = context;
            _logger = logger;
        }

        public async Task<bool> Delete(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;

            return await Update(entity);
        }

        public async Task<bool> Update(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            Context.Set<TEntity>().Update(entity);
            return await Context.SaveChangesAsync() > 0;
        }

        public async Task<bool> Add(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            await Context.Set<TEntity>().AddAsync(entity);
            return await Context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ExecuteSql(string sql, params object[] parameters)
        {
            if (string.IsNullOrEmpty(sql))
            {
                throw new ArgumentNullException(nameof(sql));
            }

            await Context.Database.ExecuteSqlRawAsync(sql, parameters);
            return true;
        }
    }
}
