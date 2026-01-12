using System.Text.Json.Serialization;


namespace Biss.EmployeeManagement.Domain.Entities.Response
{
    public class ApiDataResponse<TEntityResponse>
    {
        /// <summary>
        /// Data object for response
        /// </summary>
        [JsonPropertyName("response")]
        public TEntityResponse Response { get; set; }
        public ApiDataResponse(TEntityResponse response)
        {
            Response = response;
        }

        public ApiDataResponse()
        {

        }
    }

}
