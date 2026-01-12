using System;

namespace Biss.EmployeeManagement.Domain.Entities
{

    public class Customer
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string DocumentNumber { get; set; }
        public string Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public string Phone { get; set; }
        public Address Address { get; set; }
        public string FavoriteSport { get; set; }
        public string FavoriteClub { get; set; }
        public bool AcceptTermsUse { get; set; }
        public bool AcceptPrivacyPolicy { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool Active { get; set; } = true;
        public string Status { get; set; }
        public DateTime? InactivatedAt { get; set; }

        public Customer()
        {
            Address = new Address();
        }

    }
}
