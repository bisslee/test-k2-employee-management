using Biss.EmployeeManagement.Domain.Entities;
using Biss.EmployeeManagement.Domain.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biss.EmployeeManagement.Infrastructure.ContextMappings
{
    public class EmployeeMapping : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.ToTable("Employees");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            
            // Configuração das propriedades
            builder.Property(x => x.FirstName)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(x => x.LastName)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(150);
            
            builder.Property(x => x.Document)
                .IsRequired()
                .HasMaxLength(20);
            
            builder.Property(x => x.PasswordHash)
                .IsRequired()
                .HasMaxLength(255);
            
            builder.Property(x => x.Role)
                .IsRequired()
                .HasConversion<int>(); // Converter enum para int
            
            // Configuração dos relacionamentos
            builder.HasMany(x => x.PhoneNumbers)
                .WithOne(x => x.Employee)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Configuração de índices para performance
            builder.HasIndex(x => x.Email).IsUnique();
            builder.HasIndex(x => x.Document).IsUnique();
            builder.HasIndex(x => x.FirstName);
            builder.HasIndex(x => x.LastName);
            builder.HasIndex(x => x.Role);
        }
    }
}
