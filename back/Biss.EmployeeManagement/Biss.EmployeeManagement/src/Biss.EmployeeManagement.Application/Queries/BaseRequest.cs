namespace Biss.EmployeeManagement.Application.Queries
{
    public class BaseRequest
    {
        /// <summary>
        /// Total Itens per page
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Actual ordered page
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Field name to order
        /// 
        public string? FieldName { get; set; }

        /// <summary>
        /// order type
        /// 
        public string? Order { get; set; } = "asc";

    }
}
