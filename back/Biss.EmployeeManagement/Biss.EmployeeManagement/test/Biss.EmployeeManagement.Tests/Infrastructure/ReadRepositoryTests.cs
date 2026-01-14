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
    public class ReadRepositoryTests : BaseTest
    {
        private readonly AppDbContext Context;
        private readonly ReadRepository<Employee> Repository;
        private readonly Faker Faker;

        public ReadRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            Faker = new Faker("pt_BR");
            Context = new AppDbContext(options);

            var logger = new Mock<ILogger<ReadRepository<Employee>>>();
            Repository = new ReadRepository<Employee>(Context, logger.Object);

            SeedData();
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
                Email = Faker.Internet.Email(provider: "example.com"),
                Document = Faker.Person.Cpf(),
                BirthDate = Faker.Date.Past(Faker.Random.Int(18, 65)),
                Role = Faker.PickRandom<EmployeeRole>(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
                Status = Faker.PickRandom<DataStatus>(),
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

        private void SeedData()
        {
            for (int i = 0; i < 10; i++)
            {
                Context.Employees.Add(CreateEmployee());
                Context.SaveChanges();
            }
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllEmployees()
        {
            var employees = await Repository.Find(x => x.Id != Guid.Empty);
            Assert.Equal(10, employees.Count);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnEmployee()
        {
            var firstEmployee = Context.Employees.FirstOrDefault();
            Assert.NotNull(firstEmployee);
            
            var result = await Repository.GetByIdAsync(firstEmployee.Id);
            Assert.NotNull(result);
            Assert.Equal(firstEmployee.Id, result.Id);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNullWhenIdDoesNotExist()
        {
            var result = await Repository.Find(x => x.Id == Guid.Empty);
            Assert.Empty(result);
        }

        [Fact]
        public async Task Find_ReturnsMatchingEmployeesWhenPredicateIsValid()
        {
            var employees = await Repository.Find(e => e.Email.Contains("example.com"));
            Assert.NotEmpty(employees);
            Assert.Equal(10, employees.Count);
        }

        [Fact]
        public async Task Find_ReturnsEmptyListWhenNoMatchesFound()
        {
            var employees = await Repository.Find(e => e.Email.Contains("nonexistent.com"));
            Assert.Empty(employees);
        }

        [Fact]
        public async Task FindWithPagination_ReturnsCorrectPageData()
        {
            (List<Employee> pagedResult, int total) = await Repository
                .FindWithPagination(e => true, page: 1, pageSize: 2);

            Assert.Equal(2, pagedResult.Count);
            Assert.Equal(10, total);
        }
    }
}
