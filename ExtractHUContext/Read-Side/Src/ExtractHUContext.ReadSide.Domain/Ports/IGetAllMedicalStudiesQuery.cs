using ExtractHUContext.ReadSide.Domain.ReadModels;

namespace ExtractHUContext.ReadSide.Domain.Ports;

public interface IGetAllMedicalStudiesQuery
{
    Task<IEnumerable<MedicalStudyReadModel>> Execute();
}
