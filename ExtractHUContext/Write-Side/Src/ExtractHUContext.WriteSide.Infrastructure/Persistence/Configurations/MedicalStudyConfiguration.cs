using ExtractHUContext.WriteSide.Infrastructure.Persistence.Ef;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExtractHUContext.WriteSide.Infrastructure.Persistence.Configurations;

public class MedicalStudyConfiguration : IEntityTypeConfiguration<MedicalStudyEf>
{
    public void Configure(EntityTypeBuilder<MedicalStudyEf> builder)
    {
        builder.ToTable("Medical_Studies");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.UploadDate)
            .IsRequired();

        builder.Property(s => s.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(s => s.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.FileSizeBytes)
            .IsRequired();

        builder.Property(s => s.StorageKey)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(s => s.ExtractionStatus)
            .IsRequired();

        builder.Property(s => s.ExtractionStartedAt)
            .IsRequired(false);

        builder.Property(s => s.ExtractionCompletedAt)
            .IsRequired(false);

        builder.Property(s => s.MeanHu)
            .IsRequired(false);

        builder.Property(s => s.MinHu)
            .IsRequired(false);

        builder.Property(s => s.MaxHu)
            .IsRequired(false);

        builder.Property(s => s.StandardDeviation)
            .IsRequired(false);

        builder.Property(s => s.VoxelCount)
            .IsRequired(false);

        builder.Property(s => s.ExtractionError)
            .IsRequired(false)
            .HasMaxLength(2000);
    }
}
