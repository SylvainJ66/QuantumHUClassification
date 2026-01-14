using ExtractHUContext.WriteSide.Domain.Models;
using ExtractHUContext.WriteSide.Domain.Models.Snapshots;
using ExtractHUContext.WriteSide.Domain.Models.ValueObjects;
using ExtractHUContext.WriteSide.Domain.Ports;
using ExtractHUContext.WriteSide.Infrastructure.Persistence.Ef;

namespace ExtractHUContext.WriteSide.Infrastructure.Persistence.Repositories;

public class EfMedicalStudyRepository : IMedicalStudyRepository
{
    private readonly QuantumHUDbContext _dbContext;

    public EfMedicalStudyRepository(QuantumHUDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Save(MedicalStudy study)
    {
        var snapshot = study.ToSnapshot();

        var entity = await _dbContext.MedicalStudies
            .FindAsync(snapshot.Id);

        if (entity == null)
        {
            // Create new
            entity = new MedicalStudyEf
            {
                Id = snapshot.Id,
                UploadDate = snapshot.UploadDate,
                FileName = snapshot.FileName,
                ContentType = snapshot.ContentType,
                FileSizeBytes = snapshot.FileSizeBytes,
                StorageKey = snapshot.StorageKey,
                ExtractionStatus = (int)snapshot.ExtractionStatus,
                ExtractionStartedAt = snapshot.ExtractionStartedAt,
                ExtractionCompletedAt = snapshot.ExtractionCompletedAt,
                MeanHu = snapshot.HuStatistics?.MeanHu,
                MinHu = snapshot.HuStatistics?.MinHu,
                MaxHu = snapshot.HuStatistics?.MaxHu,
                StandardDeviation = snapshot.HuStatistics?.StandardDeviation,
                VoxelCount = snapshot.HuStatistics?.VoxelCount,
                ExtractionError = snapshot.ExtractionError
            };

            await _dbContext.MedicalStudies.AddAsync(entity);
        }
        else
        {
            // Update existing
            entity.UploadDate = snapshot.UploadDate;
            entity.FileName = snapshot.FileName;
            entity.ContentType = snapshot.ContentType;
            entity.FileSizeBytes = snapshot.FileSizeBytes;
            entity.StorageKey = snapshot.StorageKey;
            entity.ExtractionStatus = (int)snapshot.ExtractionStatus;
            entity.ExtractionStartedAt = snapshot.ExtractionStartedAt;
            entity.ExtractionCompletedAt = snapshot.ExtractionCompletedAt;
            entity.MeanHu = snapshot.HuStatistics?.MeanHu;
            entity.MinHu = snapshot.HuStatistics?.MinHu;
            entity.MaxHu = snapshot.HuStatistics?.MaxHu;
            entity.StandardDeviation = snapshot.HuStatistics?.StandardDeviation;
            entity.VoxelCount = snapshot.HuStatistics?.VoxelCount;
            entity.ExtractionError = snapshot.ExtractionError;
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task<MedicalStudy?> GetById(Guid id)
    {
        var entity = await _dbContext.MedicalStudies
            .FindAsync(id);

        if (entity == null)
            return null;

        HuStatistics? huStatistics = null;
        if (entity is
            {
                MeanHu: not null,
                MinHu: not null,
                MaxHu: not null,
                StandardDeviation: not null,
                VoxelCount: not null
            })
        {
            huStatistics = HuStatistics.Create(
                entity.MeanHu.Value,
                entity.MinHu.Value,
                entity.MaxHu.Value,
                entity.StandardDeviation.Value,
                entity.VoxelCount.Value).Value;
        }

        var snapshot = new MedicalStudySnapshot(
            entity.Id,
            entity.UploadDate,
            entity.FileName,
            entity.ContentType,
            entity.FileSizeBytes,
            entity.StorageKey,
            (ExtractionStatus)entity.ExtractionStatus,
            entity.ExtractionStartedAt,
            entity.ExtractionCompletedAt,
            huStatistics,
            entity.ExtractionError
        );

        return MedicalStudy.FromSnapshot(snapshot);
    }
}
