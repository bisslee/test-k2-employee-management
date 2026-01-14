using FluentAssertions;
using Biss.EmployeeManagement.Domain.Entities;
using Biss.EmployeeManagement.Domain.Entities.Enums;
using Biss.EmployeeManagement.Domain.Exceptions;
using Biss.EmployeeManagement.Domain.Repositories;
using Biss.EmployeeManagement.Domain.Specifications.Employees;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Biss.EmployeeManagement.Tests.Domain.Specifications.Employees
{
    public class EmployeeMustExistSpecificationTests : BaseTest
    {
        private readonly Mock<IReadRepository<Employee>> RepositoryMock;

        public EmployeeMustExistSpecificationTests()
        {
            RepositoryMock = new Mock<IReadRepository<Employee>>();
        }

        private Employee CreateEmployee(Guid id)
        {
            return new Employee
            {
                Id = id,
                FirstName = "João",
                LastName = "Silva",
                Email = "joao@biss.com.br",
                Document = "12345678909",
                BirthDate = new DateTime(1990, 1, 1),
                Role = EmployeeRole.Manager,
                IsActive = true,
                Status = DataStatus.Active
            };
        }

        [Fact]
        public async Task IsSatisfiedByAsync_Should_Return_True_When_Employee_Exists()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var employee = CreateEmployee(employeeId);
            var specification = new EmployeeMustExistSpecification(RepositoryMock.Object);

            RepositoryMock.Setup(r => r.GetByIdAsync(employeeId))
                .ReturnsAsync(employee);

            // Act
            var result = await specification.IsSatisfiedByAsync(employeeId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsSatisfiedByAsync_Should_Throw_Exception_When_Employee_Not_Exists()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var specification = new EmployeeMustExistSpecification(RepositoryMock.Object);

            RepositoryMock.Setup(r => r.GetByIdAsync(employeeId))
                .ReturnsAsync((Employee?)null);

            // Act & Assert
            await Assert.ThrowsAsync<EmployeeNotFoundException>(
                () => specification.IsSatisfiedByAsync(employeeId));
        }

        [Fact]
        public async Task IsSatisfiedByAsync_Should_Throw_Exception_When_EmployeeId_Is_Empty()
        {
            // Arrange
            var employeeId = Guid.Empty;
            var specification = new EmployeeMustExistSpecification(RepositoryMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<EmployeeNotFoundException>(
                () => specification.IsSatisfiedByAsync(employeeId));
        }
    }
}
