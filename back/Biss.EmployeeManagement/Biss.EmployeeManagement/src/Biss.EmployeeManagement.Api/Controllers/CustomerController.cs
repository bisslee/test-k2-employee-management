using Biss.EmployeeManagement.Application.Commands.AddCustomer;
using Biss.EmployeeManagement.Application.Commands.ChangeCustomer;
using Biss.EmployeeManagement.Application.Commands.RemoveCustomer;
using Biss.EmployeeManagement.Application.Queries.GetCustomer;
using Biss.EmployeeManagement.Application.Queries.GetCustomerByKey;
using Biss.EmployeeManagement.Domain.Entities.Response;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using Biss.EmployeeManagement.Api.Helper;

namespace Biss.EmployeeManagement.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CustomerController : BaseControllerHandle
    {
        private readonly ILogger<CustomerController> Logger;
        private IMediator Mediator { get; }

        public CustomerController(
            ILogger<CustomerController> logger,
            IMediator mediator
            ) : base(logger)
        {
            Logger = logger;
            Mediator = mediator;
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Query", Description = "List Customer")]
        [SwaggerResponse(206, Type = typeof(GetCustomerResponse))]
        [SwaggerResponse(204)]
        [SwaggerResponse(400, Type = typeof(GetCustomerResponse))]
        [SwaggerResponse(401)]
        [SwaggerResponse(404)]
        [SwaggerResponse(500, Type = typeof(GetCustomerResponse))]
        public async Task<ActionResult> Get([FromQuery] GetCustomerRequest request)
        {
            try
            {
                var response = await Mediator.Send(request);
                return HandleResponse(response);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Query", Description = "Get Customer By Key")]
        [SwaggerResponse(200, Type = typeof(GetCustomerByKeyResponse))]
        [SwaggerResponse(204)]
        [SwaggerResponse(400, Type = typeof(GetCustomerByKeyResponse))]
        [SwaggerResponse(401)]
        [SwaggerResponse(404)]
        [SwaggerResponse(500, Type = typeof(GetCustomerByKeyResponse))]
        public async Task<ActionResult> GetByKey(Guid id)
        {
            var request = new GetCustomerByKeyRequest()
            {
                Id = id
            };
            try
            {
                var response = await Mediator.Send(request);
                return HandleResponse(response);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Command", Description = "Add Customer")]
        [SwaggerResponse(201, Type = typeof(AddCustomerResponse))]
        [SwaggerResponse(400, Type = typeof(AddCustomerResponse))]
        [SwaggerResponse(401)]
        [SwaggerResponse(404)]
        [SwaggerResponse(500, Type = typeof(AddCustomerResponse))]
        public async Task<ActionResult> Add([FromBody] AddCustomerRequest request)
        {
            try
            {
                var response = await Mediator.Send(request);
                return HandleResponse(response);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Command", Description = "Change Customer")]
        [SwaggerResponse(200, Type = typeof(ChangeCustomerResponse))]
        [SwaggerResponse(400, Type = typeof(ChangeCustomerResponse))]
        [SwaggerResponse(401)]
        [SwaggerResponse(404)]
        [SwaggerResponse(500, Type = typeof(ChangeCustomerResponse))]
        public async Task<ActionResult> Change(Guid id, [FromBody] ChangeCustomerRequest request)
        {
            try
            {
                request.Id = id;
                var response = await Mediator.Send(request);
                return HandleResponse(response);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Command", Description = "Remove Customer")]
        [SwaggerResponse(200, Type = typeof(RemoveCustomerResponse))]
        [SwaggerResponse(204)]
        [SwaggerResponse(400, Type = typeof(RemoveCustomerResponse))]
        [SwaggerResponse(401)]
        [SwaggerResponse(404)]
        [SwaggerResponse(500, Type = typeof(RemoveCustomerResponse))]
        public async Task<ActionResult> Remove(Guid id)
        {
            try
            {
                var request = new RemoveCustomerRequest()
                {
                    Id = id
                };

                var response = await Mediator.Send(request);
                return HandleResponse(response);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }


    }
}
