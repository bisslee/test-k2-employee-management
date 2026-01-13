using Biss.EmployeeManagement.Application.Commands.Employees.AddEmployee;
using Biss.EmployeeManagement.Application.Commands.Employees.ChangeEmployee;
using Biss.EmployeeManagement.Application.Commands.Employees.RemoveEmployee;
using Biss.EmployeeManagement.Application.Queries.Employees.GetEmployee;
using Biss.EmployeeManagement.Application.Queries.Employees.GetEmployeeByKey;
using Biss.EmployeeManagement.Application.Queries.Employees.GetEmployees;
using Biss.EmployeeManagement.Api.Helper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Biss.EmployeeManagement.Api.Controllers
{
    /// <summary>
    /// Controller responsável por gerenciar operações relacionadas a funcionários
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [SwaggerTag("Gerenciamento de Funcionários")]
    [Authorize]
    public class EmployeeController : BaseControllerHandle
    {
        private readonly IMediator Mediator;
        private readonly ILogger<EmployeeController> Logger;

        public EmployeeController(
            IMediator mediator,
            ILogger<EmployeeController> logger) : base(logger)
        {
            Mediator = mediator;
            Logger = logger;
        }

        /// <summary>
        /// Lista funcionários com paginação e filtros opcionais
        /// </summary>
        /// <param name="request">Parâmetros de consulta incluindo filtros e paginação</param>
        /// <returns>Lista paginada de funcionários</returns>
        /// <response code="200">Lista de funcionários retornada com sucesso</response>
        /// <response code="206">Lista parcial de funcionários (paginação)</response>
        /// <response code="204">Nenhum funcionário encontrado</response>
        /// <response code="400">Parâmetros de consulta inválidos</response>
        /// <response code="401">Não autorizado</response>
        /// <response code="500">Erro interno do servidor</response>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Listar funcionários",
            Description = "Retorna uma lista paginada de funcionários com filtros opcionais.",
            OperationId = "GetEmployees",
            Tags = new[] { "Employees" }
        )]
        [SwaggerResponse(200, "Lista de funcionários retornada com sucesso", typeof(GetEmployeesResponse))]
        [SwaggerResponse(206, "Lista parcial de funcionários (paginação)", typeof(GetEmployeesResponse))]
        [SwaggerResponse(204, "Nenhum funcionário encontrado")]
        [SwaggerResponse(400, "Parâmetros de consulta inválidos", typeof(GetEmployeesResponse))]
        [SwaggerResponse(401, "Não autorizado")]
        [SwaggerResponse(500, "Erro interno do servidor", typeof(GetEmployeesResponse))]
        public async Task<ActionResult> Get([FromQuery] GetEmployeesRequest request)
        {
            Logger.LogInformation("GET /api/v1/employee - Starting employee list request");
            
            try
            {
                var response = await Mediator.Send(request);
                Logger.LogInformation("GET /api/v1/employee - Successfully processed employee list request. StatusCode: {StatusCode}", 
                    response.StatusCode);
                return HandleResponse(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "GET /api/v1/employee - Unexpected error occurred while processing employee list request");
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Obtém um funcionário específico pelo ID
        /// </summary>
        /// <param name="id">ID único do funcionário (GUID)</param>
        /// <returns>Dados do funcionário solicitado</returns>
        /// <response code="200">Funcionário encontrado e retornado com sucesso</response>
        /// <response code="400">ID inválido</response>
        /// <response code="401">Não autorizado</response>
        /// <response code="404">Funcionário não encontrado</response>
        /// <response code="500">Erro interno do servidor</response>
        [HttpGet("{id:guid}")]
        [SwaggerOperation(
            Summary = "Obter funcionário por ID",
            Description = "Retorna os dados de um funcionário específico baseado no ID fornecido.",
            OperationId = "GetEmployeeById",
            Tags = new[] { "Employees" }
        )]
        [SwaggerResponse(200, "Funcionário encontrado e retornado com sucesso", typeof(GetEmployeeByKeyResponse))]
        [SwaggerResponse(400, "ID inválido", typeof(GetEmployeeByKeyResponse))]
        [SwaggerResponse(401, "Não autorizado")]
        [SwaggerResponse(404, "Funcionário não encontrado", typeof(GetEmployeeByKeyResponse))]
        [SwaggerResponse(500, "Erro interno do servidor", typeof(GetEmployeeByKeyResponse))]
        public async Task<ActionResult> GetByKey(Guid id)
        {
            Logger.LogInformation("GET /api/v1/employee/{EmployeeId} - Starting get employee by key request", id);
            
            var request = new GetEmployeeByKeyRequest()
            {
                Id = id
            };
            try
            {
                var response = await Mediator.Send(request);
                Logger.LogInformation("GET /api/v1/employee/{EmployeeId} - Successfully processed get employee by key request. StatusCode: {StatusCode}", 
                    id, response.StatusCode);
                return HandleResponse(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "GET /api/v1/employee/{EmployeeId} - Unexpected error occurred while processing get employee by key request", id);
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Cria um novo funcionário
        /// </summary>
        /// <param name="request">Dados do funcionário a ser criado</param>
        /// <returns>Funcionário criado com ID gerado</returns>
        /// <response code="201">Funcionário criado com sucesso</response>
        /// <response code="400">Dados inválidos ou funcionário já existe</response>
        /// <response code="401">Não autorizado</response>
        /// <response code="409">Conflito - email ou documento já existem</response>
        /// <response code="422">Entidade não processável</response>
        /// <response code="500">Erro interno do servidor</response>
        [HttpPost]
        [SwaggerOperation(
            Summary = "Criar novo funcionário",
            Description = "Cria um novo funcionário. Valida se email e documento são únicos. Requer pelo menos 2 telefones.",
            OperationId = "CreateEmployee",
            Tags = new[] { "Employees" }
        )]
        [SwaggerResponse(201, "Funcionário criado com sucesso", typeof(AddEmployeeResponse))]
        [SwaggerResponse(400, "Dados inválidos", typeof(AddEmployeeResponse))]
        [SwaggerResponse(401, "Não autorizado")]
        [SwaggerResponse(409, "Conflito - email ou documento já existem", typeof(AddEmployeeResponse))]
        [SwaggerResponse(422, "Entidade não processável", typeof(AddEmployeeResponse))]
        [SwaggerResponse(500, "Erro interno do servidor", typeof(AddEmployeeResponse))]
        public async Task<ActionResult> Add([FromBody] AddEmployeeRequest request)
        {
            Logger.LogInformation("POST /api/v1/employee - Starting add employee request. Email: {Email}, Document: {Document}", 
                request.Email, request.Document);
            
            try
            {
                var response = await Mediator.Send(request);
                Logger.LogInformation("POST /api/v1/employee - Successfully processed add employee request. StatusCode: {StatusCode}, EmployeeId: {EmployeeId}", 
                    response.StatusCode, response.Data?.Response?.Id);
                return HandleResponse(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "POST /api/v1/employee - Unexpected error occurred while processing add employee request. Email: {Email}", 
                    request.Email);
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Atualiza um funcionário existente
        /// </summary>
        /// <param name="id">ID do funcionário a ser atualizado</param>
        /// <param name="request">Novos dados do funcionário</param>
        /// <returns>Funcionário atualizado</returns>
        /// <response code="200">Funcionário atualizado com sucesso</response>
        /// <response code="400">Dados inválidos</response>
        /// <response code="401">Não autorizado</response>
        /// <response code="404">Funcionário não encontrado</response>
        /// <response code="409">Conflito - email ou documento já existem</response>
        /// <response code="422">Entidade não processável</response>
        /// <response code="500">Erro interno do servidor</response>
        [HttpPut("{id:guid}")]
        [SwaggerOperation(
            Summary = "Atualizar funcionário",
            Description = "Atualiza os dados de um funcionário existente. Valida se email e documento são únicos (exceto para o próprio funcionário).",
            OperationId = "UpdateEmployee",
            Tags = new[] { "Employees" }
        )]
        [SwaggerResponse(200, "Funcionário atualizado com sucesso", typeof(ChangeEmployeeResponse))]
        [SwaggerResponse(400, "Dados inválidos", typeof(ChangeEmployeeResponse))]
        [SwaggerResponse(401, "Não autorizado")]
        [SwaggerResponse(404, "Funcionário não encontrado", typeof(ChangeEmployeeResponse))]
        [SwaggerResponse(409, "Conflito - email ou documento já existem", typeof(ChangeEmployeeResponse))]
        [SwaggerResponse(422, "Entidade não processável", typeof(ChangeEmployeeResponse))]
        [SwaggerResponse(500, "Erro interno do servidor", typeof(ChangeEmployeeResponse))]
        public async Task<ActionResult> Change(Guid id, [FromBody] ChangeEmployeeRequest request)
        {
            Logger.LogInformation("PUT /api/v1/employee/{EmployeeId} - Starting change employee request. Email: {Email}, Document: {Document}", 
                id, request.Email, request.Document);
            
            try
            {
                request.Id = id;
                var response = await Mediator.Send(request);
                Logger.LogInformation("PUT /api/v1/employee/{EmployeeId} - Successfully processed change employee request. StatusCode: {StatusCode}", 
                    id, response.StatusCode);
                return HandleResponse(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "PUT /api/v1/employee/{EmployeeId} - Unexpected error occurred while processing change employee request. Email: {Email}", 
                    id, request.Email);
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Remove um funcionário
        /// </summary>
        /// <param name="id">ID do funcionário a ser removido</param>
        /// <returns>Confirmação da remoção</returns>
        /// <response code="200">Funcionário removido com sucesso</response>
        /// <response code="204">Funcionário não encontrado (já removido)</response>
        /// <response code="400">ID inválido</response>
        /// <response code="401">Não autorizado</response>
        /// <response code="404">Funcionário não encontrado</response>
        /// <response code="500">Erro interno do servidor</response>
        [HttpDelete("{id:guid}")]
        [SwaggerOperation(
            Summary = "Remover funcionário",
            Description = "Remove um funcionário específico baseado no ID fornecido. A remoção é lógica (soft delete).",
            OperationId = "DeleteEmployee",
            Tags = new[] { "Employees" }
        )]
        [SwaggerResponse(200, "Funcionário removido com sucesso", typeof(RemoveEmployeeResponse))]
        [SwaggerResponse(204, "Funcionário não encontrado (já removido)")]
        [SwaggerResponse(400, "ID inválido", typeof(RemoveEmployeeResponse))]
        [SwaggerResponse(401, "Não autorizado")]
        [SwaggerResponse(404, "Funcionário não encontrado", typeof(RemoveEmployeeResponse))]
        [SwaggerResponse(500, "Erro interno do servidor", typeof(RemoveEmployeeResponse))]
        public async Task<ActionResult> Remove(Guid id)
        {
            Logger.LogInformation("DELETE /api/v1/employee/{EmployeeId} - Starting remove employee request", id);
            
            try
            {
                var request = new RemoveEmployeeRequest()
                {
                    Id = id
                };

                var response = await Mediator.Send(request);
                Logger.LogInformation("DELETE /api/v1/employee/{EmployeeId} - Successfully processed remove employee request. StatusCode: {StatusCode}", 
                    id, response.StatusCode);
                return HandleResponse(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "DELETE /api/v1/employee/{EmployeeId} - Unexpected error occurred while processing remove employee request", id);
                return HandleException(ex);
            }
        }
    }
}
