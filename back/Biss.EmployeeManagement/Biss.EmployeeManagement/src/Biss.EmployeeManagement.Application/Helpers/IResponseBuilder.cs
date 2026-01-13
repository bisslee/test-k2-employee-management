using Biss.EmployeeManagement.Domain.Entities.Response;
using System.Collections.Generic;

namespace Biss.EmployeeManagement.Application.Helpers
{
    public interface IResponseBuilder
    {
        TResponse BuildSuccessResponse<TResponse, T>(T data, string? message = null, int statusCode = 200)
            where TResponse : ApiResponse<T>, new();
        TResponse BuildErrorResponse<TResponse, T>(string message, IEnumerable<string>? errors = null, int statusCode = 500)
            where TResponse : ApiResponse<T>, new();
        TResponse BuildNotFoundResponse<TResponse, T>(string? message = null)
            where TResponse : ApiResponse<T>, new();
        TResponse BuildValidationErrorResponse<TResponse, T>(string? message, IEnumerable<string> errors)
            where TResponse : ApiResponse<T>, new();
    }
}
