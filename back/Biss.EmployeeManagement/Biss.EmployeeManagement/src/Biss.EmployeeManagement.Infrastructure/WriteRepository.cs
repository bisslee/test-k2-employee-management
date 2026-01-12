using Biss.EmployeeManagement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Biss.EmployeeManagement.Infrastructure
{
    public class WriteRepository<TEntity> : IWriteRepository<TEntity> where TEntity : class
    {
        private readonly AppDbContext Context;
        public WriteRepository(
            AppDbContext context
            )
        {
            Context = context;
        }

        public async Task<bool> Delete(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            Context.Set<TEntity>().Remove(entity);
            await Context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Update(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            Context.Set<TEntity>().Update(entity);
            await Context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Add(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            await Context.Set<TEntity>().AddAsync(entity);
            await Context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> Execute(string procedure)
        {
            if (string.IsNullOrEmpty(procedure))
            {
                throw new ArgumentNullException(nameof(procedure));
            }

            await Context.Database.ExecuteSqlRawAsync(procedure);
            return true;
        }
    }
}
