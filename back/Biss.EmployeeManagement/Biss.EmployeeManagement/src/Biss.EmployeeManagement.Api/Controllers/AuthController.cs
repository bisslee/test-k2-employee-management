using Biss.EmployeeManagement.Application.Commands.Auth.Login;
using Biss.EmployeeManagement.Api.Helper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Biss.EmployeeManagement.Api.Controllers
{
    /// <summary>
    /// Controller responsável por gerenciar operações de autenticação e autorização
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [SwaggerTag("Gerenciamento de Autenticação")]
    public class AuthController : BaseControllerHandle
    {
        private readonly IMediator Mediator;
        private readonly ILogger<AuthController> Logger;

        public AuthController(
            IMediator mediator,
            ILogger<AuthController> logger) : base(logger)
        {
            Mediator = mediator;
            Logger = logger;
        }

        /// <summary>
        /// Realiza login do funcionário
        /// </summary>
        /// <param name="request">Credenciais de login (Email e Senha)</param>
        /// <returns>Token de autenticação JWT e dados do funcionário</returns>
        /// <response code="200">Login realizado com sucesso</response>
        /// <response code="400">Credenciais inválidas</response>
        /// <response code="401">Não autorizado</response>
        /// <response code="500">Erro interno do servidor</response>
        [HttpPost("login")]
        [SwaggerOperation(
            Summary = "Realizar login",
            Description = "Autentica o funcionário usando email e senha, retorna token JWT de acesso.",
            OperationId = "Login",
            Tags = new[] { "Authentication" }
        )]
        [SwaggerResponse(200, "Login realizado com sucesso", typeof(LoginResponse))]
        [SwaggerResponse(400, "Credenciais inválidas", typeof(LoginResponse))]
        [SwaggerResponse(401, "Não autorizado")]
        [SwaggerResponse(500, "Erro interno do servidor", typeof(LoginResponse))]
        public async Task<ActionResult> Login([FromBody] LoginRequest request)
        {
            Logger.LogInformation("POST /api/v1/auth/login - Starting login request for user: {Email}", request.Email);
            
            try
            {
                var response = await Mediator.Send(request);
                Logger.LogInformation("POST /api/v1/auth/login - Successfully processed login request. StatusCode: {StatusCode}", 
                    response.StatusCode);
                return HandleResponse(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "POST /api/v1/auth/login - Unexpected error occurred while processing login request for user: {Email}", 
                    request.Email);
                return HandleException(ex);
            }
        }
    }
}
