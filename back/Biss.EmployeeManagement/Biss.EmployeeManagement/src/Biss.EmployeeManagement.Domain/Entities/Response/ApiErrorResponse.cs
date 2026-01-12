using System;
using System.Text.Json.Serialization;

namespace Biss.EmployeeManagement.Domain.Entities.Response
{
    public class ApiErrorResponse
    {
        /// <summary>
        /// If have one more error layer 
        /// </summary>
        [JsonPropertyName("innerError")]
        public string InnerError { get; set; }

        /// <summary>
        /// Error Message
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; }


        /// <summary>
        /// Exception Stack Trace
        /// </summary>
        [JsonPropertyName("detail")]
        public string Detail { get; set; }

        public ApiErrorResponse()
        {
            Message = string.Empty;
            Detail = string.Empty;
            InnerError = string.Empty;
        }

        public ApiErrorResponse(Exception e)
        {
            Detail = e.StackTrace;
            Message = e.Message;
            InnerError = e.InnerException?.Message;
        }


    }
}
