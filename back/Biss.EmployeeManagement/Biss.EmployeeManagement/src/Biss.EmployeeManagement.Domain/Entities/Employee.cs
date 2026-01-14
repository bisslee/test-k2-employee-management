using Biss.EmployeeManagement.Domain.Entities.Enums;
using System;
using System.Collections.Generic;

namespace Biss.EmployeeManagement.Domain.Entities
{
    public class Employee : BaseEntity
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Document { get; set; } = null!;
        public DateTime BirthDate { get; set; }
        public EmployeeRole Role { get; set; }
        public string PasswordHash { get; set; } = null!; // Hash BCrypt da senha
        
        // Relacionamento com telefones (dois telefones obrigatórios)
        public List<PhoneNumber> PhoneNumbers { get; set; } = new List<PhoneNumber>();

        // Propriedades computadas
        public string FullName => $"{FirstName} {LastName}";
    }
}
