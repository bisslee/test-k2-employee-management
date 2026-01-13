using AutoMapper;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Biss.EmployeeManagement.Application.Commands.Employees.AddEmployee;
using Biss.EmployeeManagement.Application.Helpers;
using Biss.EmployeeManagement.Domain.Entities;
using Biss.EmployeeManagement.Domain.Entities.Enums;
using Biss.EmployeeManagement.Domain.Exceptions;
using Biss.EmployeeManagement.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace Biss.EmployeeManagement.Tests.Application.Commands.Employees
{
    public class AddEmployeeHandlerTests : BaseTest
    {
        private readonly Mock<ILogger<AddEmployeeHandler>> LoggerMock;
        private readonly Mock<IWriteRepository<Employee>> WriteRepositoryMock;
        private readonly Mock<IReadRepository<Employee>> ReadRepositoryMock;
        private readonly Mock<IValidator<AddEmployeeRequest>> ValidatorMock;
        private readonly Mock<IMapper> MapperMock;
        private readonly IResponseBuilder ResponseBuilder;
        private readonly AddEmployeeHandler Handler;

        public AddEmployeeHandlerTests()
        {
            LoggerMock = new Mock<ILogger<AddEmployeeHandler>>();
            WriteRepositoryMock = new Mock<IWriteRepository<Employee>>();
            ReadRepositoryMock = new Mock<IReadRepository<Employee>>();
            ValidatorMock = new Mock<IValidator<AddEmployeeRequest>>();
            MapperMock = new Mock<IMapper>();
            ResponseBuilder = new ResponseBuilder();
            Handler = new AddEmployeeHandler(
                LoggerMock.Object,
                WriteRepositoryMock.Object,
                ReadRepositoryMock.Object,
                ValidatorMock.Object,
                MapperMock.Object,
                ResponseBuilder
            );
        }

        private AddEmployeeRequest CreateValidRequest()
        {
            return new AddEmployeeRequest
            {
                FirstName = "João",
                LastName = "Silva",
                Email = "joao.silva@biss.com.br",
                Document = "12345678909",
                BirthDate = new DateTime(1990, 1, 1),
                Role = EmployeeRole.Manager,
                Password = "Test123!@#",
                PhoneNumbers = new List<PhoneNumberRequest>
                {
                    new PhoneNumberRequest { Number = "11999999999", Type = "Mobile" },
                    new PhoneNumberRequest { Number = "1133333333", Type = "Home" }
                }
            };
        }

        [Fact]
        public async Task Handle_Should_Add_Employee_Successfully()
        {
            // Arrange
            var request = CreateValidRequest();
            var employee = new Employee
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Document = request.Document,
                BirthDate = request.BirthDate,
                Role = request.Role,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            };

            ValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            ReadRepositoryMock.Setup(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<Func<Employee, bool>>>()))
                .ReturnsAsync((List<Employee>?)null);

            MapperMock.Setup(m => m.Map<Employee>(It.IsAny<AddEmployeeRequest>()))
                .Returns(employee);

            WriteRepositoryMock.Setup(r => r.Add(It.IsAny<Employee>()))
                .ReturnsAsync(true);

            // Act
            var response = await Handler.Handle(request, CancellationToken.None);

            // Assert
            response.Success.Should().BeTrue();
            response.StatusCode.Should().Be(201);
            response.Data.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_Should_Return_BadRequestWhen_Validation_Fails()
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
            var response = await Handler.Handle(request, CancellationToken.None);

            // Assert
            response.Success.Should().BeFalse();
            response.StatusCode.Should().Be(400);
            response.Data.Should().BeNull();
        }

        [Fact]
        public async Task Handle_Should_Return_BadRequest_When_Email_Already_Exists()
        {
            // Arrange
            var request = CreateValidRequest();
            var existingEmployee = new Employee { Email = request.Email, Id = Guid.NewGuid() };
            var employee = new Employee
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                Document = request.Document,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Role = request.Role,
                BirthDate = request.BirthDate,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            };

            ValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            // Primeira chamada (email) retorna existente
            ReadRepositoryMock.SetupSequence(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<Func<Employee, bool>>>()))
                .ReturnsAsync(new List<Employee> { existingEmployee });

            MapperMock.Setup(m => m.Map<Employee>(request))
                .Returns(employee);

            // Act
            var response = await Handler.Handle(request, CancellationToken.None);

            // Assert
            response.Success.Should().BeFalse();
            response.StatusCode.Should().Be(400);
            response.Data.Should().BeNull();
            response.Error.Should().NotBeNull();
            response.Error!.Message.Should().Contain("already exists");
        }

        [Fact]
        public async Task Handle_Should_Return_BadRequest_When_Document_Already_Exists()
        {
            // Arrange
            var request = CreateValidRequest();
            var existingEmployee = new Employee { Document = request.Document, Id = Guid.NewGuid() };
            var employee = new Employee
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                Document = request.Document,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Role = request.Role,
                BirthDate = request.BirthDate,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            };

            ValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            // Primeira chamada (email) retorna null, segunda (document) retorna existente
            ReadRepositoryMock.SetupSequence(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<Func<Employee, bool>>>()))
                .ReturnsAsync((List<Employee>?)null)
                .ReturnsAsync(new List<Employee> { existingEmployee });

            MapperMock.Setup(m => m.Map<Employee>(request))
                .Returns(employee);

            // Act
            var response = await Handler.Handle(request, CancellationToken.None);

            // Assert
            response.Success.Should().BeFalse();
            response.StatusCode.Should().Be(400);
            response.Data.Should().BeNull();
            response.Error.Should().NotBeNull();
            response.Error!.Message.Should().Contain("already exists");
        }

        [Fact]
        public async Task Handle_Should_Throw_Exception_When_Internal_Error_Occurs()
        {
            // Arrange
            var request = CreateValidRequest();
            var employee = new Employee
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                Document = request.Document,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Role = request.Role,
                BirthDate = request.BirthDate,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            };

            ValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            ReadRepositoryMock.Setup(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<Func<Employee, bool>>>()))
                .ReturnsAsync((List<Employee>?)null);

            MapperMock.Setup(m => m.Map<Employee>(request))
                .Returns(employee);

            WriteRepositoryMock.Setup(r => r.Add(It.IsAny<Employee>()))
                .ThrowsAsync(new Exception("Erro interno"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => Handler.Handle(request, CancellationToken.None));

            exception.Message.Should().Be("Erro interno");
        }
    }
}
