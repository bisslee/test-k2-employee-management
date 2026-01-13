using Biss.EmployeeManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Reflection;

namespace Biss.EmployeeManagement.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // dentro de .\src\Biss.EmployeeManagement.Api
        // dotnet ef migrations add InitialBiss.EmployeeManagementMigrations -p ..\Biss.EmployeeManagement.Infrastructure
        // dotnet ef database update 

        // Entidades
        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<PhoneNumber> PhoneNumbers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aplicar todas as configurações de mapeamento
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Otimizações de performance
            optionsBuilder
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking) // Desabilita tracking por padrão
                .EnableSensitiveDataLogging(false) // Desabilita logs sensíveis em produção
                .EnableDetailedErrors(false); // Desabilita erros detalhados em produção
        }
    }
}
