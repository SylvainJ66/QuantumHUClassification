using ExtractHUContext.WriteSide.Infrastructure.Persistence.Ef;
using ExtractHUContext.WriteSide.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExtractHUContext.WriteSide.Infrastructure.Persistence;

public class QuantumHUDbContext : DbContext
{
    public QuantumHUDbContext(DbContextOptions<QuantumHUDbContext> options)
        : base(options)
    {
    }

    public DbSet<QuantumGreetingEf> QuantumGreetings => Set<QuantumGreetingEf>();
    public DbSet<MedicalStudyEf> MedicalStudies => Set<MedicalStudyEf>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("quantum_hu_context");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(QuantumHUDbContext).Assembly);
    }
}
