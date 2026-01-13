using Biss.EmployeeManagement.Domain.Entities.Response;
using Biss.EmployeeManagement.Domain.Resources;
using System.Collections.Generic;
using System.Linq;

namespace Biss.EmployeeManagement.Application.Helpers
{
    public class ResponseBuilder : IResponseBuilder
    {
        public TResponse BuildSuccessResponse<TResponse, T>(T data, string? message = null, int statusCode = 200)
            where TResponse : ApiResponse<T>, new()
        {
            var response = new TResponse
            {
                Success = true,
                StatusCode = statusCode
            };

            if (data != null)
            {
                response.Data = new ApiDataResponse<T>(data);
            }

            return response;
        }

        public TResponse BuildErrorResponse<TResponse, T>(string message, IEnumerable<string>? errors = null, int statusCode = 500)
            where TResponse : ApiResponse<T>, new()
        {
            var response = new TResponse
            {
                Success = false,
                StatusCode = statusCode,
                Error = new ApiErrorResponse
                {
                    Message = message ?? "An error occurred",
                    Detail = errors != null ? string.Join("; ", errors) : string.Empty
                }
            };

            return response;
        }

        public TResponse BuildNotFoundResponse<TResponse, T>(string? message = null)
            where TResponse : ApiResponse<T>, new()
        {
            var response = new TResponse
            {
                Success = false,
                StatusCode = 404,
                Error = new ApiErrorResponse
                {
                    Message = message ?? "Resource not found"
                }
            };

            return response;
        }

        public TResponse BuildValidationErrorResponse<TResponse, T>(string? message, IEnumerable<string> errors)
            where TResponse : ApiResponse<T>, new()
        {
            var errorList = errors?.ToList() ?? new List<string>();
            var response = new TResponse
            {
                Success = false,
                StatusCode = 400,
                Error = new ApiErrorResponse
                {
                    Message = message ?? "Validation failed",
                    Detail = string.Join("; ", errorList)
                }
            };

            return response;
        }
    }
}
