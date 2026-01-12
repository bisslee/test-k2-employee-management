using System.Text.Json.Serialization;

namespace Biss.EmployeeManagement.Domain.Entities.Response
{
    public abstract class ApiResponse<TEntityResponse>
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; } = false;
        [JsonPropertyName("data")]
        public ApiDataResponse<TEntityResponse> Data { get; set; }

        [JsonPropertyName("metadata")]
        public ApiMetaDataResponse Metadata { get; set; }

        [JsonPropertyName("error")]
        public ApiErrorResponse Error { get; set; }

        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }
    }
}
