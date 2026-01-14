using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Biss.EmployeeManagement.Application.Commands.Employees.RemoveEmployee;
using Biss.EmployeeManagement.Application.Helpers;
using Biss.EmployeeManagement.Domain.Entities;
using Biss.EmployeeManagement.Domain.Entities.Enums;
using Biss.EmployeeManagement.Domain.Exceptions;
using Biss.EmployeeManagement.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Biss.EmployeeManagement.Tests.Application.Commands.Employees
{
    public class RemoveEmployeeHandlerTests : BaseTest
    {
        private readonly Mock<ILogger<RemoveEmployeeHandler>> LoggerMock;
        private readonly Mock<IWriteRepository<Employee>> WriteRepositoryMock;
        private readonly Mock<IReadRepository<Employee>> ReadRepositoryMock;
        private readonly Mock<IValidator<RemoveEmployeeRequest>> ValidatorMock;
        private readonly IResponseBuilder ResponseBuilder;
        private readonly RemoveEmployeeHandler Handler;

        public RemoveEmployeeHandlerTests()
        {
            LoggerMock = new Mock<ILogger<RemoveEmployeeHandler>>();
            WriteRepositoryMock = new Mock<IWriteRepository<Employee>>();
            ReadRepositoryMock = new Mock<IReadRepository<Employee>>();
            ValidatorMock = new Mock<IValidator<RemoveEmployeeRequest>>();
            ResponseBuilder = new ResponseBuilder();
            Handler = new RemoveEmployeeHandler(
                LoggerMock.Object,
                WriteRepositoryMock.Object,
                ReadRepositoryMock.Object,
                ValidatorMock.Object,
                ResponseBuilder
            );
        }

        private RemoveEmployeeRequest CreateValidRequest()
        {
            return new RemoveEmployeeRequest
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
        public async Task Handle_Should_Remove_Employee_Successfully()
        {
            // Arrange
            var request = CreateValidRequest();
            var employee = CreateEmployee(request.Id);

            ValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            ReadRepositoryMock.Setup(r => r.GetByIdAsync(request.Id))
                .ReturnsAsync(employee);

            WriteRepositoryMock.Setup(r => r.Delete(It.IsAny<Employee>()))
                .ReturnsAsync(true);

            // Act
            var result = await Handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.StatusCode.Should().Be(200);
            WriteRepositoryMock.Verify(r => r.Delete(It.IsAny<Employee>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_Return_ValidationError_When_Validation_Fails()
        {
            // Arrange
            var request = CreateValidRequest();
            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("Id", "Id is required")
            };

            ValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(validationFailures));

            // Act
            var result = await Handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            WriteRepositoryMock.Verify(r => r.Delete(It.IsAny<Employee>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_Throw_Exception_When_Employee_Not_Found()
        {
            // Arrange
            var request = CreateValidRequest();

            ValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            ReadRepositoryMock.Setup(r => r.GetByIdAsync(request.Id))
                .ReturnsAsync((Employee?)null);

            // Act & Assert
            await Assert.ThrowsAsync<EmployeeNotFoundException>(
                () => Handler.Handle(request, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_Should_Return_Error_When_Delete_Fails()
        {
            // Arrange
            var request = CreateValidRequest();
            var employee = CreateEmployee(request.Id);

            ValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            ReadRepositoryMock.Setup(r => r.GetByIdAsync(request.Id))
                .ReturnsAsync(employee);

            WriteRepositoryMock.Setup(r => r.Delete(It.IsAny<Employee>()))
                .ReturnsAsync(false);

            // Act
            var result = await Handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(500);
        }
    }
}
