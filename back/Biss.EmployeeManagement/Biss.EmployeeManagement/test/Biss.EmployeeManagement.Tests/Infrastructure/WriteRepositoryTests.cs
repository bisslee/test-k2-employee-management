using Bogus;
using Bogus.Extensions.Brazil;
using Biss.EmployeeManagement.Domain.Entities;
using Biss.EmployeeManagement.Domain.Entities.Enums;
using Biss.EmployeeManagement.Infrastructure;
using Biss.EmployeeManagement.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Biss.EmployeeManagement.Tests.Infrastructure
{
    public class WriteRepositoryTests : BaseTest
    {
        private readonly AppDbContext Context;
        private readonly WriteRepository<Employee> Repository;
        private readonly Faker Faker;

        public WriteRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
               .UseSqlite("Filename=:memory:")
               .Options;

            Faker = new Faker("pt_BR");
            Context = new AppDbContext(options);
            Context.Database.OpenConnection();
            Context.Database.EnsureCreated();
            
            var logger = new Mock<ILogger<WriteRepository<Employee>>>();
            Repository = new WriteRepository<Employee>(Context, logger.Object);
        }

        public override void Dispose()
        {
            Context?.Dispose();
            base.Dispose();
        }

        private Employee CreateEmployee()
        {
            var employeeId = Guid.NewGuid();
            return new Employee
            {
                Id = employeeId,
                FirstName = Faker.Name.FirstName(),
                LastName = Faker.Name.LastName(),
                Email = Faker.Internet.Email(),
                Document = Faker.Person.Cpf(),
                BirthDate = Faker.Date.Past(Faker.Random.Int(18, 65)),
                Role = Faker.PickRandom<EmployeeRole>(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
                Status = DataStatus.Active,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "testes",
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = "testes",
                PhoneNumbers = new List<PhoneNumber>
                {
                    new PhoneNumber
                    {
                        Id = Guid.NewGuid(),
                        EmployeeId = employeeId,
                        Number = Faker.Phone.PhoneNumber(),
                        Type = "Mobile",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "testes"
                    },
                    new PhoneNumber
                    {
                        Id = Guid.NewGuid(),
                        EmployeeId = employeeId,
                        Number = Faker.Phone.PhoneNumber(),
                        Type = "Home",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "testes"
                    }
                }
            };
        }

        [Fact]
        public async Task AddEmployee()
        {
            var employee = CreateEmployee();
            var result = await Repository.Add(employee);

            Assert.True(result);
            Assert.NotNull(await Context.Employees.FindAsync(employee.Id));
        }

        [Fact]
        public async Task Add_ThrowsArgumentNullExceptionWhenEntityIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => Repository.Add(null!));
        }

        [Fact]
        public async Task UpdateEmployee()
        {
            var employee = CreateEmployee();
            await Repository.Add(employee);

            var newEmail = Faker.Internet.Email();
            var newFirstName = Faker.Name.FirstName();

            employee.Email = newEmail;
            employee.FirstName = newFirstName;

            var result = await Repository.Update(employee);

            Assert.True(result);
            var updatedEmployee = await Context.Employees.FindAsync(employee.Id);
            Assert.NotNull(updatedEmployee);
            Assert.Equal(newEmail, updatedEmployee.Email);
            Assert.Equal(newFirstName, updatedEmployee.FirstName);
        }

        [Fact]
        public async Task Update_ThrowsArgumentNullExceptionWhenEntityIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => Repository.Update(null!));
        }

        [Fact]
        public async Task RemoveEmployee()
        {
            var employee = CreateEmployee();
            await Repository.Add(employee);
            var result = await Repository.Delete(employee);
            
            Assert.True(result);
            
            // Verificar se o employee foi marcado como deletado (soft delete)
            var deletedEmployee = await Context.Employees.FindAsync(employee.Id);
            Assert.NotNull(deletedEmployee);
            Assert.True(deletedEmployee.IsDeleted);
            Assert.NotNull(deletedEmployee.DeletedAt);
        }

        [Fact]
        public async Task Remove_ThrowsArgumentNullExceptionWhenEntityIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => Repository.Delete(null!));
        }

        [Fact]
        public async Task ExecuteSql_ExecutesRawSqlCommand()
        {
            var sql = "SELECT @p0 as id, @p1 as email";
            var employeeId = Guid.NewGuid();
            var email = "test@example.com";

            var parameters = new object[] { employeeId, email };

            var result = await Repository.ExecuteSql(sql, parameters);

            Assert.True(result);
        }
    }
}
