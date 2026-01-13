using Biss.EmployeeManagement.Domain.Entities;
using Biss.EmployeeManagement.Domain.Entities.Enums;
using Biss.EmployeeManagement.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Biss.EmployeeManagement.Infrastructure.Seed
{
    public class EmployeeSeedService
    {
        private readonly IWriteRepository<Employee> EmployeeRepository;
        private readonly IReadRepository<Employee> EmployeeReadRepository;
        private readonly ILogger<EmployeeSeedService> Logger;

        public EmployeeSeedService(
            IWriteRepository<Employee> employeeRepository,
            IReadRepository<Employee> employeeReadRepository,
            ILogger<EmployeeSeedService> logger)
        {
            EmployeeRepository = employeeRepository;
            EmployeeReadRepository = employeeReadRepository;
            Logger = logger;
        }

        public async Task SeedMasterUserAsync()
        {
            const string masterEmail = "admin@employee.com";
            const string masterPassword = "admin@123";

            try
            {
                Logger.LogInformation("Checking if master user already exists. Email: {Email}", masterEmail);

                // Verificar se o usuário master já existe
                var existingEmployees = await EmployeeReadRepository.Find(e => e.Email == masterEmail);
                if (existingEmployees != null && existingEmployees.Any())
                {
                    Logger.LogInformation("Master user already exists. Email: {Email}", masterEmail);
                    return;
                }

                Logger.LogInformation("Creating master user. Email: {Email}", masterEmail);

                // Criar usuário master
                var masterUser = new Employee
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Admin",
                    LastName = "Master",
                    Email = masterEmail,
                    Document = "00000000000", // Documento padrão para admin
                    BirthDate = new DateTime(1990, 1, 1),
                    Role = EmployeeRole.Director,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(masterPassword, BCrypt.Net.BCrypt.GenerateSalt()),
                    IsActive = true,
                    Status = DataStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                };

                var result = await EmployeeRepository.Add(masterUser);

                if (result)
                {
                    Logger.LogInformation("Master user created successfully. Email: {Email}, Id: {Id}", 
                        masterEmail, masterUser.Id);
                }
                else
                {
                    Logger.LogError("Failed to create master user. Email: {Email}", masterEmail);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred while seeding master user. Email: {Email}", masterEmail);
                // Não lançar exceção para não impedir a inicialização da aplicação
            }
        }
    }
}
