using ExtractHUContext.WriteSide.Domain.Models;

namespace ExtractHUContext.WriteSide.Domain.Ports;

public interface IQuantumGreetingRepository
{
    Task Save(QuantumGreeting greeting);
    Task<QuantumGreeting?> GetById(Guid id);
}
