using ExtractHUContext.WriteSide.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExtractHUContext.WriteSide.Infrastructure.Persistence.Configurations;

public class QuantumGreetingConfiguration : IEntityTypeConfiguration<QuantumGreetingEf>
{
    public void Configure(EntityTypeBuilder<QuantumGreetingEf> builder)
    {
        builder.ToTable("Quantum_Greetings");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Message)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(g => g.CreatedAt)
            .IsRequired();
    }
}
