using System;
using System.Text.Json.Serialization;

namespace Biss.EmployeeManagement.Domain.Entities
{
    public class PhoneNumber : BaseEntity
    {
        public Guid EmployeeId { get; set; }
        
        [JsonIgnore] // Evita referência circular na serialização JSON
        public Employee? Employee { get; set; }
        
        public string Number { get; set; } = null!;
        public string? Type { get; set; } // Ex: "Mobile", "Work", "Home"
    }
}
