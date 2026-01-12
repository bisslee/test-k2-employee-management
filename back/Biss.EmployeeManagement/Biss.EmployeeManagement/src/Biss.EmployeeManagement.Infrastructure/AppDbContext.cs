using Biss.EmployeeManagement.Domain.Entities;
using Biss.EmployeeManagement.Infrastructure.ContextHelpers;
using Microsoft.EntityFrameworkCore;

namespace Biss.EmployeeManagement.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // dentro de .\src\Biss.EmployeeManagement.api
        // dotnet ef migrations add InitialBiss.EmployeeManagementMigrations -p ..\Biss.EmployeeManagement.Infrastructure
        // dotnet ef database update 

        // acrescentar as entidades aqui
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Address> Addresses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração de tipos de dados
            var configureDataTypes = new ConfigureDataTypes();

            // Inserção de dados iniciais
            var insertData = new InsertDataValues();

      

        }
    }
}
