using AutoMapper;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Biss.EmployeeManagement.Application.Commands.Employees.ChangeEmployee;
using Biss.EmployeeManagement.Application.Helpers;
using Biss.EmployeeManagement.Domain.Entities;
using Biss.EmployeeManagement.Domain.Entities.Enums;
using Biss.EmployeeManagement.Domain.Exceptions;
using Biss.EmployeeManagement.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Biss.EmployeeManagement.Tests.Application.Commands.Employees
{
    public class ChangeEmployeeHandlerTests : BaseTest
    {
        private readonly Mock<ILogger<ChangeEmployeeHandler>> LoggerMock;
        private readonly Mock<IWriteRepository<Employee>> WriteRepositoryMock;
        private readonly Mock<IReadRepository<Employee>> ReadRepositoryMock;
        private readonly Mock<IValidator<ChangeEmployeeRequest>> ValidatorMock;
        private readonly Mock<IMapper> MapperMock;
        private readonly IResponseBuilder ResponseBuilder;
        private readonly ChangeEmployeeHandler Handler;

        public ChangeEmployeeHandlerTests()
        {
            LoggerMock = new Mock<ILogger<ChangeEmployeeHandler>>();
            WriteRepositoryMock = new Mock<IWriteRepository<Employee>>();
            ReadRepositoryMock = new Mock<IReadRepository<Employee>>();
            ValidatorMock = new Mock<IValidator<ChangeEmployeeRequest>>();
            MapperMock = new Mock<IMapper>();
            ResponseBuilder = new ResponseBuilder();
            Handler = new ChangeEmployeeHandler(
                LoggerMock.Object,
                WriteRepositoryMock.Object,
                ReadRepositoryMock.Object,
                ValidatorMock.Object,
                MapperMock.Object,
                ResponseBuilder
            );
        }

        private ChangeEmployeeRequest CreateValidRequest()
        {
            return new ChangeEmployeeRequest
            {
                Id = Guid.NewGuid(),
                FirstName = "João",
                LastName = "Silva",
                Email = "joao.silva@biss.com.br",
                Document = "12345678909",
                BirthDate = new DateTime(1990, 1, 1),
                Role = EmployeeRole.Manager,
                PhoneNumbers = new List<PhoneNumberRequest>
                {
                    new PhoneNumberRequest { Number = "11999999999", Type = "Mobile" }
                }
            };
        }

        private Employee CreateEmployee(Guid id, string email, string document)
        {
            return new Employee
            {
                Id = id,
                FirstName = "João",
                LastName = "Silva",
                Email = email,
                Document = document,
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
        public async Task Handle_Should_Update_Employee_Successfully()
        {
            // Arrange
            var request = CreateValidRequest();
            var existingEmployee = CreateEmployee(request.Id, request.Email, request.Document);

            ValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            ReadRepositoryMock.Setup(r => r.GetByIdAsync(request.Id))
                .ReturnsAsync(existingEmployee);

            ReadRepositoryMock.Setup(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<Func<Employee, bool>>>()))
                .ReturnsAsync(new List<Employee>());

            WriteRepositoryMock.Setup(r => r.Update(It.IsAny<Employee>()))
                .ReturnsAsync(true);

            MapperMock.Setup(m => m.Map(It.IsAny<ChangeEmployeeRequest>(), It.IsAny<Employee>()))
                .Callback<ChangeEmployeeRequest, Employee>((req, emp) =>
                {
                    emp.FirstName = req.FirstName;
                    emp.LastName = req.LastName;
                    emp.Email = req.Email;
                });

            // Act
            var result = await Handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.StatusCode.Should().Be(200);
            WriteRepositoryMock.Verify(r => r.Update(It.IsAny<Employee>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_Return_ValidationError_When_Validation_Fails()
        {
            // Arrange
            var request = CreateValidRequest();
            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("Email", "Email is required")
            };

            ValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(validationFailures));

            // Act
            var result = await Handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            WriteRepositoryMock.Verify(r => r.Update(It.IsAny<Employee>()), Times.Never);
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
        public async Task Handle_Should_Return_Error_When_Email_Already_Exists()
        {
            // Arrange
            var request = CreateValidRequest();
            var existingEmployee = CreateEmployee(request.Id, "old@email.com", request.Document);
            var otherEmployeeId = Guid.NewGuid();
            var otherEmployee = CreateEmployee(otherEmployeeId, request.Email, "98765432100");

            ValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            ReadRepositoryMock.Setup(r => r.GetByIdAsync(request.Id))
                .ReturnsAsync(existingEmployee);

            // Setup para retornar o outro employee quando buscar por email
            // A specification verifica o email e como o ExcludeEmployeeId é request.Id,
            // e o otherEmployee tem ID diferente, deve lançar exceção que será capturada pelo handler
            ReadRepositoryMock.SetupSequence(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<Func<Employee, bool>>>()))
                .ReturnsAsync(new List<Employee> { otherEmployee }) // Primeira chamada: email (encontra outro employee)
                .ReturnsAsync(new List<Employee>()); // Segunda chamada: document

            MapperMock.Setup(m => m.Map(It.IsAny<ChangeEmployeeRequest>(), It.IsAny<Employee>()))
                .Callback<ChangeEmployeeRequest, Employee>((req, emp) =>
                {
                    emp.Email = req.Email;
                    emp.Document = req.Document;
                });

            // Act
            var result = await Handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Error.Should().NotBeNull();
            result.Error!.Message.Should().Contain("already exists");
        }

        [Fact]
        public async Task Handle_Should_Update_Password_When_Provided()
        {
            // Arrange
            var request = CreateValidRequest();
            request.Password = "NewPassword123!";
            var existingEmployee = CreateEmployee(request.Id, request.Email, request.Document);
            var originalPasswordHash = existingEmployee.PasswordHash;

            ValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            ReadRepositoryMock.Setup(r => r.GetByIdAsync(request.Id))
                .ReturnsAsync(existingEmployee);

            ReadRepositoryMock.Setup(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<Func<Employee, bool>>>()))
                .ReturnsAsync(new List<Employee>());

            WriteRepositoryMock.Setup(r => r.Update(It.IsAny<Employee>()))
                .ReturnsAsync(true);

            MapperMock.Setup(m => m.Map(It.IsAny<ChangeEmployeeRequest>(), It.IsAny<Employee>()))
                .Callback<ChangeEmployeeRequest, Employee>((req, emp) =>
                {
                    emp.FirstName = req.FirstName;
                });

            // Act
            var result = await Handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            existingEmployee.PasswordHash.Should().NotBe(originalPasswordHash);
        }
    }
}
