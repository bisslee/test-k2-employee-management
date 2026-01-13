using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Biss.EmployeeManagement.Application.Commands.Auth.Login;
using Biss.EmployeeManagement.Application.Helpers;
using Biss.EmployeeManagement.Application.Services;
using Biss.EmployeeManagement.Domain.Entities;
using Biss.EmployeeManagement.Domain.Entities.Enums;
using Biss.EmployeeManagement.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace Biss.EmployeeManagement.Tests.Application.Commands.Auth
{
    public class LoginHandlerTests : BaseTest
    {
        private readonly Mock<ILogger<LoginHandler>> LoggerMock;
        private readonly Mock<IReadRepository<Employee>> ReadRepositoryMock;
        private readonly Mock<IValidator<LoginRequest>> ValidatorMock;
        private readonly Mock<IJwtTokenService> JwtTokenServiceMock;
        private readonly IResponseBuilder ResponseBuilder;
        private readonly LoginHandler Handler;

        public LoginHandlerTests()
        {
            LoggerMock = new Mock<ILogger<LoginHandler>>();
            ReadRepositoryMock = new Mock<IReadRepository<Employee>>();
            ValidatorMock = new Mock<IValidator<LoginRequest>>();
            JwtTokenServiceMock = new Mock<IJwtTokenService>();
            ResponseBuilder = new ResponseBuilder();
            Handler = new LoginHandler(
                LoggerMock.Object,
                ReadRepositoryMock.Object,
                ValidatorMock.Object,
                ResponseBuilder,
                JwtTokenServiceMock.Object
            );
        }

        private LoginRequest CreateValidRequest()
        {
            return new LoginRequest
            {
                Email = "joao.silva@biss.com.br",
                Password = "Test123!@#"
            };
        }

        private Employee CreateEmployee(string password)
        {
            return new Employee
            {
                Id = Guid.NewGuid(),
                FirstName = "João",
                LastName = "Silva",
                Email = "joao.silva@biss.com.br",
                Document = "12345678909",
                BirthDate = new DateTime(1990, 1, 1),
                Role = EmployeeRole.Manager,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                IsActive = true,
                Status = DataStatus.Active
            };
        }

        [Fact]
        public async Task Handle_Should_Login_Successfully()
        {
            // Arrange
            var request = CreateValidRequest();
            var employee = CreateEmployee(request.Password);

            ValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            ReadRepositoryMock.Setup(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<Func<Employee, bool>>>()))
                .ReturnsAsync(new List<Employee> { employee });

            JwtTokenServiceMock.Setup(j => j.GenerateTokenAsync(It.IsAny<Employee>()))
                .ReturnsAsync("test-jwt-token");

            JwtTokenServiceMock.Setup(j => j.GetTokenExpirationMinutes())
                .Returns(60);

            // Act
            var response = await Handler.Handle(request, CancellationToken.None);

            // Assert
            response.Success.Should().BeTrue();
            response.StatusCode.Should().Be(200);
            response.Data.Should().NotBeNull();
            response.Data!.Response!.Employee.Should().NotBeNull();
            response.Data.Response.Employee!.Email.Should().Be(request.Email);
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
        public async Task Handle_Should_Return_Unauthorized_When_Employee_Not_Found()
        {
            // Arrange
            var request = CreateValidRequest();

            ValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            ReadRepositoryMock.Setup(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<Func<Employee, bool>>>()))
                .ReturnsAsync((List<Employee>?)null);

            // Act
            var response = await Handler.Handle(request, CancellationToken.None);

            // Assert
            response.Success.Should().BeFalse();
            response.StatusCode.Should().Be(401);
            response.Data.Should().BeNull();
            response.Error.Should().NotBeNull();
            response.Error!.Message.Should().Contain("Invalid email or password");
        }

        [Fact]
        public async Task Handle_Should_Return_Unauthorized_When_Password_Is_Invalid()
        {
            // Arrange
            var request = CreateValidRequest();
            var employee = CreateEmployee("WrongPassword123!");

            ValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            ReadRepositoryMock.Setup(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<Func<Employee, bool>>>()))
                .ReturnsAsync(new List<Employee> { employee });

            // Act
            var response = await Handler.Handle(request, CancellationToken.None);

            // Assert
            response.Success.Should().BeFalse();
            response.StatusCode.Should().Be(401);
            response.Data.Should().BeNull();
            response.Error.Should().NotBeNull();
            response.Error!.Message.Should().Contain("Invalid email or password");
        }

        [Fact]
        public async Task Handle_Should_Throw_Exception_When_Internal_Error_Occurs()
        {
            // Arrange
            var request = CreateValidRequest();

            ValidatorMock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            ReadRepositoryMock.Setup(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<Func<Employee, bool>>>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => Handler.Handle(request, CancellationToken.None));

            exception.Message.Should().Be("Database error");
        }
    }
}
