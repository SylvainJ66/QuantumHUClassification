using ExtractHUContext.ReadSide.Domain.Ports;
using ExtractHUContext.ReadSide.Domain.ReadModels;

namespace ExtractHUContext.ReadSide.Domain.QueryHandlers.GetAllQuantumGreetings;

public static class GetAllQuantumGreetingsHandler
{
    public static Task<IEnumerable<QuantumGreetingReadModel>> Handle(
        IGetAllQuantumGreetingsQuery queryImpl) 
        => queryImpl.Execute();
}
