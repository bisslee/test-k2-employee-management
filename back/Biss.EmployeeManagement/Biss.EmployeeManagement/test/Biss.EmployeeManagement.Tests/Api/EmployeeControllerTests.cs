using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Biss.EmployeeManagement.Api.Controllers;
using Biss.EmployeeManagement.Application.Commands.Employees.AddEmployee;
using Biss.EmployeeManagement.Application.Commands.Employees.ChangeEmployee;
using Biss.EmployeeManagement.Application.Commands.Employees.RemoveEmployee;
using Biss.EmployeeManagement.Application.Queries.Employees.GetEmployee;
using Biss.EmployeeManagement.Application.Queries.Employees.GetEmployeeByKey;
using Biss.EmployeeManagement.Application.Queries.Employees.GetEmployees;
using Biss.EmployeeManagement.Domain.Entities;
using Biss.EmployeeManagement.Domain.Entities.Enums;
using Biss.EmployeeManagement.Domain.Entities.Response;
using Moq;

namespace Biss.EmployeeManagement.Tests.Api
{
    public class EmployeeControllerTests : BaseTest
    {
        private readonly Mock<IMediator> MediatorMock;
        private readonly Mock<ILogger<EmployeeController>> LoggerMock;
        private readonly EmployeeController Controller;

        public EmployeeControllerTests()
        {
            MediatorMock = new Mock<IMediator>();
            LoggerMock = new Mock<ILogger<EmployeeController>>();
            Controller = new EmployeeController(
                MediatorMock.Object,
                LoggerMock.Object);

            Controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        private AddEmployeeRequest CreateValidAddRequest()
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
                PhoneNumbers = new List<Biss.EmployeeManagement.Application.Commands.Employees.AddEmployee.PhoneNumberRequest>
                {
                    new Biss.EmployeeManagement.Application.Commands.Employees.AddEmployee.PhoneNumberRequest { Number = "11999999999", Type = "Mobile" },
                    new Biss.EmployeeManagement.Application.Commands.Employees.AddEmployee.PhoneNumberRequest { Number = "1133333333", Type = "Home" }
                }
            };
        }

        private ChangeEmployeeRequest CreateValidChangeRequest()
        {
            return new ChangeEmployeeRequest
            {
                Id = Guid.NewGuid(),
                FirstName = "João",
                LastName = "Silva Atualizado",
                Email = "joao.silva.updated@biss.com.br",
                Document = "12345678909",
                BirthDate = new DateTime(1990, 1, 1),
                Role = EmployeeRole.Director,
                PhoneNumbers = new List<Biss.EmployeeManagement.Application.Commands.Employees.ChangeEmployee.PhoneNumberRequest>
                {
                    new Biss.EmployeeManagement.Application.Commands.Employees.ChangeEmployee.PhoneNumberRequest { Number = "11999999999", Type = "Mobile" },
                    new Biss.EmployeeManagement.Application.Commands.Employees.ChangeEmployee.PhoneNumberRequest { Number = "1133333333", Type = "Home" }
                }
            };
        }

        private GetEmployeesRequest CreateValidGetRequest()
        {
            return new GetEmployeesRequest
            {
                Page = 1,
                Offset = 10,
                FirstName = "João",
                Email = "joao@biss.com.br"
            };
        }

        [Fact]
        public async Task Get_Should_Return_PartialContent_When_Employees_Found()
        {
            // Arrange
            var request = CreateValidGetRequest();
            var employees = new List<Employee>
            {
                new Employee { Id = Guid.NewGuid(), FirstName = "João", Email = "joao@biss.com.br" }
            };

            var response = new GetEmployeesResponse
            {
                Success = true,
                StatusCode = 200,
                Data = new ApiDataResponse<List<Employee>>(employees),
                Metadata = new ApiMetaDataResponse(1, 10, 1)
            };

            MediatorMock.Setup(m => m.Send(It.IsAny<GetEmployeesRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await Controller.Get(request);

            // Assert
            result.Should().NotBeNull();
            var okResult = result as ObjectResult;
            okResult.Should().NotBeNull();
            // BaseControllerHandle retorna 206 (PartialContent) para listas paginadas
            okResult!.StatusCode.Should().Be(206);
        }

        [Fact]
        public async Task GetByKey_Should_Return_Ok_When_Employee_Found()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var employee = new Employee { Id = employeeId, FirstName = "João", Email = "joao@biss.com.br" };

            var response = new GetEmployeeByKeyResponse
            {
                Success = true,
                StatusCode = 200,
                Data = new ApiDataResponse<Employee>(employee)
            };

            MediatorMock.Setup(m => m.Send(It.IsAny<GetEmployeeByKeyRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await Controller.GetByKey(employeeId);

            // Assert
            result.Should().NotBeNull();
            var okResult = result as ObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Add_Should_Return_Created_When_Employee_Added()
        {
            // Arrange
            var request = CreateValidAddRequest();
            var employee = new Employee
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email
            };

            var response = new AddEmployeeResponse
            {
                Success = true,
                StatusCode = 201,
                Data = new ApiDataResponse<Employee>(employee)
            };

            MediatorMock.Setup(m => m.Send(It.IsAny<AddEmployeeRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await Controller.Add(request);

            // Assert
            result.Should().NotBeNull();
            var createdResult = result as ObjectResult;
            createdResult.Should().NotBeNull();
            createdResult!.StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task Change_Should_Return_Ok_When_Employee_Updated()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var request = CreateValidChangeRequest();
            request.Id = employeeId;

            var employee = new Employee
            {
                Id = employeeId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email
            };

            var response = new ChangeEmployeeResponse
            {
                Success = true,
                StatusCode = 200,
                Data = new ApiDataResponse<Employee>(employee)
            };

            MediatorMock.Setup(m => m.Send(It.IsAny<ChangeEmployeeRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await Controller.Change(employeeId, request);

            // Assert
            result.Should().NotBeNull();
            var okResult = result as ObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Remove_Should_Return_Ok_When_Employee_Removed()
        {
            // Arrange
            var employeeId = Guid.NewGuid();

            var response = new RemoveEmployeeResponse
            {
                Success = true,
                StatusCode = 200,
                Data = new ApiDataResponse<bool>(true)
            };

            MediatorMock.Setup(m => m.Send(It.IsAny<RemoveEmployeeRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await Controller.Remove(employeeId);

            // Assert
            result.Should().NotBeNull();
            var okResult = result as ObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);
        }
    }
}
