using ExtractHUContext.WriteSide.Domain.Models;
using ExtractHUContext.WriteSide.Domain.Ports;

namespace ExtractHUContext.WriteSide.Domain.Tests.CommandHandlers.UploadMedicalStudy.Stubs;

public class MedicalStudyRepositoryStub : IMedicalStudyRepository
{
    public List<MedicalStudy> SavedStudies { get; } = new();

    public Task Save(MedicalStudy study)
    {
        SavedStudies.Add(study);
        return Task.CompletedTask;
    }

    public Task<MedicalStudy?> GetById(Guid id)
    {
        throw new NotImplementedException();
    }
}
