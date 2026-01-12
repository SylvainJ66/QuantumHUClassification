using ExtractHUContext.WriteSide.Domain.Models;
using ExtractHUContext.WriteSide.Domain.Models.Snapshots;
using ExtractHUContext.WriteSide.Domain.Ports;
using ExtractHUContext.WriteSide.Infrastructure.Persistence.Ef;
using Microsoft.EntityFrameworkCore;

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
                StorageKey = snapshot.StorageKey
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
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task<MedicalStudy?> GetById(Guid id)
    {
        var entity = await _dbContext.MedicalStudies
            .FindAsync(id);

        if (entity == null)
            return null;

        var snapshot = new MedicalStudySnapshot(
            entity.Id,
            entity.UploadDate,
            entity.FileName,
            entity.ContentType,
            entity.FileSizeBytes,
            entity.StorageKey
        );

        return MedicalStudy.FromSnapshot(snapshot);
    }
}
