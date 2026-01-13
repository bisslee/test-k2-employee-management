using Biss.EmployeeManagement.Domain.Entities.Response;
using Biss.EmployeeManagement.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace Biss.EmployeeManagement.Api.Middleware
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public GlobalExceptionHandlerMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlerMiddleware> logger,
            IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() 
                ?? context.Response.Headers["X-Correlation-ID"].FirstOrDefault() 
                ?? Guid.NewGuid().ToString();

            var traceId = context.TraceIdentifier;

            _logger.LogError(exception, 
                "Unhandled exception occurred. CorrelationId: {CorrelationId}, TraceId: {TraceId}, Path: {Path}, Method: {Method}", 
                correlationId, traceId, context.Request.Path, context.Request.Method);

            var errorResponse = CreateErrorResponse(exception, correlationId, traceId);
            var statusCode = GetStatusCode(exception);

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = _environment.IsDevelopment()
            });

            if (!context.Response.HasStarted && context.Response.Body.CanWrite)
            {
                try
                {
                    await context.Response.WriteAsync(jsonResponse);
                }
                catch (ObjectDisposedException)
                {
                    // Stream já foi fechado, não faz nada
                }
            }
        }

        private ErrorResponse CreateErrorResponse(Exception exception, string correlationId, string traceId)
        {
            return exception switch
            {
                DomainException domainEx => new ErrorResponse
                {
                    ErrorCode = domainEx.ErrorCode,
                    Message = domainEx.Message,
                    Details = _environment.IsDevelopment() ? exception.ToString() : null,
                    Timestamp = DateTime.UtcNow,
                    TraceId = traceId,
                    CorrelationId = correlationId
                },

                FluentValidation.ValidationException validationEx => new ErrorResponse
                {
                    ErrorCode = "VALIDATION_ERROR",
                    Message = "One or more validation errors occurred.",
                    Details = _environment.IsDevelopment() ? exception.ToString() : null,
                    Timestamp = DateTime.UtcNow,
                    TraceId = traceId,
                    CorrelationId = correlationId,
                    ValidationErrors = validationEx.Errors.Select(e => new ValidationError
                    {
                        Field = e.PropertyName,
                        Message = e.ErrorMessage,
                        Value = e.AttemptedValue
                    }).ToList()
                },

                UnauthorizedAccessException => new ErrorResponse
                {
                    ErrorCode = "UNAUTHORIZED",
                    Message = "Access denied. You are not authorized to perform this action.",
                    Details = _environment.IsDevelopment() ? exception.ToString() : null,
                    Timestamp = DateTime.UtcNow,
                    TraceId = traceId,
                    CorrelationId = correlationId
                },

                ArgumentException => new ErrorResponse
                {
                    ErrorCode = "INVALID_ARGUMENT",
                    Message = "Invalid argument provided.",
                    Details = _environment.IsDevelopment() ? exception.ToString() : null,
                    Timestamp = DateTime.UtcNow,
                    TraceId = traceId,
                    CorrelationId = correlationId
                },

                _ => new ErrorResponse
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = _environment.IsDevelopment() 
                        ? exception.Message 
                        : "An unexpected error occurred. Please try again later.",
                    Details = _environment.IsDevelopment() ? exception.ToString() : null,
                    Timestamp = DateTime.UtcNow,
                    TraceId = traceId,
                    CorrelationId = correlationId
                }
            };
        }

        private int GetStatusCode(Exception exception)
        {
            return exception switch
            {
                DomainException domainEx => domainEx.StatusCode,
                FluentValidation.ValidationException => (int)HttpStatusCode.BadRequest,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                ArgumentException => (int)HttpStatusCode.BadRequest,
                _ => (int)HttpStatusCode.InternalServerError
            };
        }
    }
}
