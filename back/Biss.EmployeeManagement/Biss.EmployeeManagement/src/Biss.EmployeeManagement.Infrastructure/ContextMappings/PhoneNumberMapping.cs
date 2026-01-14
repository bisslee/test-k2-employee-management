using Biss.EmployeeManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Biss.EmployeeManagement.Infrastructure.ContextMappings
{
    public class PhoneNumberMapping : IEntityTypeConfiguration<PhoneNumber>
    {
        public void Configure(EntityTypeBuilder<PhoneNumber> builder)
        {
            builder.ToTable("PhoneNumbers");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            
            // Configuração das propriedades
            builder.Property(x => x.Number)
                .IsRequired()
                .HasMaxLength(20);
            
            builder.Property(x => x.Type)
                .HasMaxLength(20);
            
            // Configuração dos relacionamentos
            builder.HasOne(x => x.Employee)
                .WithMany(x => x.PhoneNumbers)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Configuração de índices para performance
            builder.HasIndex(x => x.EmployeeId);
            builder.HasIndex(x => x.Number);
        }
    }
}
