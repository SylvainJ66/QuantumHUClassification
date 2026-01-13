using ExtractHUContext.ReadSide.Domain.Ports;
using ExtractHUContext.ReadSide.Domain.QueryHandlers.GetAllMedicalStudies.Queries;
using ExtractHUContext.ReadSide.Domain.ReadModels;

namespace ExtractHUContext.ReadSide.Domain.QueryHandlers.GetAllMedicalStudies;

public static class GetAllMedicalStudiesHandler
{
    public static Task<IEnumerable<MedicalStudyReadModel>> Handle(
        GetAllMedicalStudiesQuery query,
        IGetAllMedicalStudiesQuery queryImpl)
        => queryImpl.Execute();
}
