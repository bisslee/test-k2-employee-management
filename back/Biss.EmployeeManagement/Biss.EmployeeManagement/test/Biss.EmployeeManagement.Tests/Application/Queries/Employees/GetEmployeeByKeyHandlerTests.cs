using FluentAssertions;
using Biss.EmployeeManagement.Application.Helpers;
using Biss.EmployeeManagement.Application.Queries.Employees.GetEmployeeByKey;
using Biss.EmployeeManagement.Domain.Entities;
using Biss.EmployeeManagement.Domain.Entities.Enums;
using Biss.EmployeeManagement.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Biss.EmployeeManagement.Tests.Application.Queries.Employees
{
    public class GetEmployeeByKeyHandlerTests : BaseTest
    {
        private readonly Mock<ILogger<GetEmployeeByKeyHandler>> LoggerMock;
        private readonly Mock<IReadRepository<Employee>> ReadRepositoryMock;
        private readonly IResponseBuilder ResponseBuilder;
        private readonly GetEmployeeByKeyHandler Handler;

        public GetEmployeeByKeyHandlerTests()
        {
            LoggerMock = new Mock<ILogger<GetEmployeeByKeyHandler>>();
            ReadRepositoryMock = new Mock<IReadRepository<Employee>>();
            ResponseBuilder = new ResponseBuilder();
            Handler = new GetEmployeeByKeyHandler(
                ReadRepositoryMock.Object,
                LoggerMock.Object,
                ResponseBuilder
            );
        }

        private GetEmployeeByKeyRequest CreateValidRequest()
        {
            return new GetEmployeeByKeyRequest
            {
                Id = Guid.NewGuid()
            };
        }

        private Employee CreateEmployee(Guid id)
        {
            return new Employee
            {
                Id = id,
                FirstName = "João",
                LastName = "Silva",
                Email = "joao.silva@biss.com.br",
                Document = "12345678909",
                BirthDate = new DateTime(1990, 1, 1),
                Role = EmployeeRole.Manager,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!@#"),
                IsActive = true,
                Status = DataStatus.Active,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            };
        }

        [Fact]
        public async Task Handle_Should_Return_Employee_Successfully()
        {
            // Arrange
            var request = CreateValidRequest();
            var employee = CreateEmployee(request.Id);

            ReadRepositoryMock.Setup(r => r.GetByIdAsync(request.Id))
                .ReturnsAsync(employee);

            // Act
            var result = await Handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.StatusCode.Should().Be(200);
            result.Data.Should().NotBeNull();
            result.Data!.Response!.Id.Should().Be(request.Id);
        }

        [Fact]
        public async Task Handle_Should_Return_NotFound_When_Employee_Not_Exists()
        {
            // Arrange
            var request = CreateValidRequest();

            ReadRepositoryMock.Setup(r => r.GetByIdAsync(request.Id))
                .ReturnsAsync((Employee?)null);

            // Act
            var result = await Handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(404);
            result.Data.Should().BeNull();
        }
    }
}
