using ExtractHUContext.WriteSide.Domain.Models;

namespace ExtractHUContext.WriteSide.Domain.Ports;

public interface IQuantumGreetingRepository
{
    Task SaveAsync(QuantumGreeting greeting);
    Task<QuantumGreeting?> GetByIdAsync(Guid id);
}
