using ExtractHUContext.ReadSide.Domain.Ports;
using ExtractHUContext.ReadSide.Domain.ReadModels;

namespace ExtractHUContext.ReadSide.Domain.Tests.QueryHandlers.GetAllMedicalStudies.Stubs;

public class GetAllMedicalStudiesQueryStub : IGetAllMedicalStudiesQuery
{
    private readonly List<MedicalStudyReadModel> _studies = new();

    public void AddStudy(MedicalStudyReadModel study)
    {
        _studies.Add(study);
    }

    public void AddStudies(IEnumerable<MedicalStudyReadModel> studies)
    {
        _studies.AddRange(studies);
    }

    public void Clear()
    {
        _studies.Clear();
    }

    public Task<IEnumerable<MedicalStudyReadModel>> Execute()
    {
        // Return studies ordered by UploadDate descending (like the real query)
        return Task.FromResult<IEnumerable<MedicalStudyReadModel>>(
            _studies.OrderByDescending(s => s.UploadDate).ToList());
    }
}
