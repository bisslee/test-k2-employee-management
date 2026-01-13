using FluentAssertions;
using Biss.EmployeeManagement.Application.Helpers;
using Biss.EmployeeManagement.Application.Queries.Employees.GetEmployees;
using Biss.EmployeeManagement.Domain.Entities;
using Biss.EmployeeManagement.Domain.Entities.Enums;
using Biss.EmployeeManagement.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Biss.EmployeeManagement.Tests.Application.Queries.Employees
{
    public class GetEmployeesHandlerTests : BaseTest
    {
        private readonly Mock<ILogger<GetEmployeesHandler>> LoggerMock;
        private readonly Mock<IReadRepository<Employee>> ReadRepositoryMock;
        private readonly IResponseBuilder ResponseBuilder;
        private readonly GetEmployeesHandler Handler;

        public GetEmployeesHandlerTests()
        {
            LoggerMock = new Mock<ILogger<GetEmployeesHandler>>();
            ReadRepositoryMock = new Mock<IReadRepository<Employee>>();
            ResponseBuilder = new ResponseBuilder();
            Handler = new GetEmployeesHandler(
                ReadRepositoryMock.Object,
                LoggerMock.Object,
                ResponseBuilder
            );
        }

        private GetEmployeesRequest CreateValidRequest()
        {
            return new GetEmployeesRequest
            {
                Page = 1,
                Offset = 20
            };
        }

        private List<Employee> CreateEmployees(int count)
        {
            var employees = new List<Employee>();
            for (int i = 0; i < count; i++)
            {
                employees.Add(new Employee
                {
                    Id = Guid.NewGuid(),
                    FirstName = $"João{i}",
                    LastName = "Silva",
                    Email = $"joao{i}@biss.com.br",
                    Document = $"1234567890{i}",
                    BirthDate = new DateTime(1990, 1, 1),
                    Role = EmployeeRole.Manager,
                    IsActive = true,
                    Status = DataStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                });
            }
            return employees;
        }

        [Fact]
        public async Task Handle_Should_Return_Employees_Successfully()
        {
            // Arrange
            var request = CreateValidRequest();
            var employees = CreateEmployees(5);
            var totalItems = 10;

            ReadRepositoryMock.Setup(r => r.FindWithPagination(
                It.IsAny<System.Linq.Expressions.Expression<Func<Employee, bool>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ReturnsAsync((employees, totalItems));

            // Act
            var result = await Handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.StatusCode.Should().Be(200);
            result.Data.Should().NotBeNull();
            result.Data!.Response!.Should().HaveCount(5);
            result.Metadata.Should().NotBeNull();
            result.Metadata!.TotalItems.Should().Be(totalItems);
        }

        [Fact]
        public async Task Handle_Should_Return_Empty_List_When_No_Employees_Found()
        {
            // Arrange
            var request = CreateValidRequest();
            var emptyList = new List<Employee>();
            var totalItems = 0;

            ReadRepositoryMock.Setup(r => r.FindWithPagination(
                It.IsAny<System.Linq.Expressions.Expression<Func<Employee, bool>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ReturnsAsync((emptyList, totalItems));

            // Act
            var result = await Handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.StatusCode.Should().Be(200);
            result.Data.Should().NotBeNull();
            result.Data!.Response!.Should().BeEmpty();
            result.Metadata!.TotalItems.Should().Be(0);
        }

        [Fact]
        public async Task Handle_Should_Filter_By_Email()
        {
            // Arrange
            var request = CreateValidRequest();
            request.Email = "joao@biss.com.br";
            var employees = CreateEmployees(2);
            employees[0].Email = "joao@biss.com.br";
            employees[1].Email = "maria@biss.com.br";

            ReadRepositoryMock.Setup(r => r.FindWithPagination(
                It.IsAny<System.Linq.Expressions.Expression<Func<Employee, bool>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ReturnsAsync((employees.Where(e => e.Email.Contains("joao@biss.com.br")).ToList(), 1));

            // Act
            var result = await Handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            ReadRepositoryMock.Verify(r => r.FindWithPagination(
                It.IsAny<System.Linq.Expressions.Expression<Func<Employee, bool>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
        }
    }
}
