using System.Text.Json.Serialization;

namespace Biss.EmployeeManagement.Domain.Entities.Response
{
    public class ApiMetaDataResponse
    {

        /// <summary>
        /// Total items of query 
        /// </summary>
        [JsonPropertyName("totalItems")]
        public int TotalItems { get; set; }
        /// <summary>
        /// Total pages = TotalItems / offset
        /// </summary>
        [JsonPropertyName("totalPages")]
        public int TotalPages
        {
            get
            {
                var offset = (Offset == 0 ? 1 : Offset);

                int result = TotalItems / offset;

                if (TotalItems % offset > 0)
                    result++;

                return result;
            }
        }

        /// <summary>
        /// Total Itens per page
        /// </summary>
        [JsonPropertyName("offset")]
        public int Offset { get; set; }

        /// <summary>
        /// Actual ordered page
        /// </summary>
        [JsonPropertyName("page")]
        public int Page { get; set; }

        public ApiMetaDataResponse(int totalItems, int offset, int page)
        {
            TotalItems = totalItems;
            Offset = offset;
            Page = page;
        }

        public ApiMetaDataResponse()
        {
            TotalItems = 0;
            Offset = 0;
            Page = 0;
        }
    }
}
