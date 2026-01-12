namespace ExtractHUContext.WriteSide.Domain.Models.Snapshots;

public record QuantumGreetingSnapshot(
    Guid Id,
    string Message,
    DateTime CreatedAt
);
