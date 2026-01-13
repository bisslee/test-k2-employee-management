using System;
using System.Collections.Generic;
using System.Linq;

namespace Biss.EmployeeManagement.Domain.Entities.Response
{
    public class ErrorResponse
    {
        public string ErrorCode { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? TraceId { get; set; }
        public string? CorrelationId { get; set; }
        public List<ValidationError>? ValidationErrors { get; set; }

        public ErrorResponse()
        {
        }

        public ErrorResponse(string errorCode, string message)
        {
            ErrorCode = errorCode;
            Message = message;
        }

        public ErrorResponse(string errorCode, string message, List<ValidationError> validationErrors)
        {
            ErrorCode = errorCode;
            Message = message;
            ValidationErrors = validationErrors;
        }
    }

    public class ValidationError
    {
        public string Field { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public object? Value { get; set; }

        public ValidationError()
        {
        }

        public ValidationError(string field, string message, object? value = null)
        {
            Field = field;
            Message = message;
            Value = value;
        }
    }
}
