using FluentAssertions;
using Biss.EmployeeManagement.Domain.Entities;
using Biss.EmployeeManagement.Domain.Entities.Enums;
using Biss.EmployeeManagement.Domain.Exceptions;
using Biss.EmployeeManagement.Domain.Repositories;
using Biss.EmployeeManagement.Domain.Specifications.Employees;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Biss.EmployeeManagement.Tests.Domain.Specifications.Employees
{
    public class EmployeeDocumentMustBeUniqueSpecificationTests : BaseTest
    {
        private readonly Mock<IReadRepository<Employee>> RepositoryMock;

        public EmployeeDocumentMustBeUniqueSpecificationTests()
        {
            RepositoryMock = new Mock<IReadRepository<Employee>>();
        }

        private Employee CreateEmployee(string document)
        {
            return new Employee
            {
                Id = Guid.NewGuid(),
                FirstName = "João",
                LastName = "Silva",
                Email = "joao@biss.com.br",
                Document = document,
                BirthDate = new DateTime(1990, 1, 1),
                Role = EmployeeRole.Manager,
                IsActive = true,
                Status = DataStatus.Active
            };
        }

        [Fact]
        public async Task IsSatisfiedByAsync_Should_Return_True_When_Document_Is_Unique()
        {
            // Arrange
            var employee = CreateEmployee("12345678909");
            var specification = new EmployeeDocumentMustBeUniqueSpecification(RepositoryMock.Object);

            RepositoryMock.Setup(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<Func<Employee, bool>>>()))
                .ReturnsAsync((List<Employee>?)null);

            // Act
            var result = await specification.IsSatisfiedByAsync(employee);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsSatisfiedByAsync_Should_Return_True_When_Document_Is_Empty()
        {
            // Arrange
            var employee = CreateEmployee("");
            var specification = new EmployeeDocumentMustBeUniqueSpecification(RepositoryMock.Object);

            // Act
            var result = await specification.IsSatisfiedByAsync(employee);

            // Assert
            result.Should().BeTrue();
            RepositoryMock.Verify(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<Func<Employee, bool>>>()), Times.Never);
        }

        [Fact]
        public async Task IsSatisfiedByAsync_Should_Throw_Exception_When_Document_Already_Exists()
        {
            // Arrange
            var employee = CreateEmployee("12345678909");
            var existingEmployee = CreateEmployee("12345678909");
            existingEmployee.Id = Guid.NewGuid();

            var specification = new EmployeeDocumentMustBeUniqueSpecification(RepositoryMock.Object);

            RepositoryMock.Setup(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<Func<Employee, bool>>>()))
                .ReturnsAsync(new List<Employee> { existingEmployee });

            // Act & Assert
            await Assert.ThrowsAsync<EmployeeDocumentAlreadyExistsException>(
                () => specification.IsSatisfiedByAsync(employee));
        }

        [Fact]
        public async Task IsSatisfiedByAsync_Should_Return_True_When_Document_Exists_But_Is_Excluded()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var employee = CreateEmployee("12345678909");
            employee.Id = employeeId;
            var existingEmployee = CreateEmployee("12345678909");
            existingEmployee.Id = employeeId;

            var specification = new EmployeeDocumentMustBeUniqueSpecification(RepositoryMock.Object, employeeId);

            RepositoryMock.Setup(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<Func<Employee, bool>>>()))
                .ReturnsAsync(new List<Employee> { existingEmployee });

            // Act
            var result = await specification.IsSatisfiedByAsync(employee);

            // Assert
            result.Should().BeTrue();
        }
    }
}
