namespace ExtractHUContext.ReadSide.Domain.ReadModels;

public record QuantumGreetingReadModel(
    Guid Id,
    string Message,
    DateTime CreatedAt
);
