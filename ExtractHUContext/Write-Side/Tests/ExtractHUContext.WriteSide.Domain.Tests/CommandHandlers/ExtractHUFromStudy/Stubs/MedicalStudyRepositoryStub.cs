using ExtractHUContext.WriteSide.Domain.Models;
using ExtractHUContext.WriteSide.Domain.Ports;

namespace ExtractHUContext.WriteSide.Domain.Tests.CommandHandlers.ExtractHUFromStudy.Stubs;

public class MedicalStudyRepositoryStub : IMedicalStudyRepository
{
    public List<MedicalStudy> SavedStudies { get; } = [];
    private readonly Dictionary<Guid, MedicalStudy> _studies = new();

    public void AddStudy(MedicalStudy study)
    {
        var snapshot = study.ToSnapshot();
        _studies[snapshot.Id] = study;
    }

    public Task Save(MedicalStudy study)
    {
        var snapshot = study.ToSnapshot();
        var studyCopy = MedicalStudy.FromSnapshot(snapshot);
        _studies[snapshot.Id] = study;
        SavedStudies.Add(studyCopy);
        return Task.CompletedTask;
    }

    public Task<MedicalStudy?> GetById(Guid id)
    {
        _studies.TryGetValue(id, out var study);
        return Task.FromResult(study);
    }
}
