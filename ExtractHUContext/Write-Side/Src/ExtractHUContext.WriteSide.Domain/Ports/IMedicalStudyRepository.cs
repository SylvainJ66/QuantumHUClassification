using ExtractHUContext.WriteSide.Domain.Models;

namespace ExtractHUContext.WriteSide.Domain.Ports;

public interface IMedicalStudyRepository
{
    Task Save(MedicalStudy study);
    Task<MedicalStudy?> GetById(Guid id);
}
