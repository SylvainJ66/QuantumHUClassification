using ExtractHUContext.ReadSide.Domain.ReadModels;

namespace ExtractHUContext.ReadSide.Domain.Ports;

public interface IGetAllQuantumGreetingsQuery
{
    Task<IEnumerable<QuantumGreetingReadModel>> Execute();
}
