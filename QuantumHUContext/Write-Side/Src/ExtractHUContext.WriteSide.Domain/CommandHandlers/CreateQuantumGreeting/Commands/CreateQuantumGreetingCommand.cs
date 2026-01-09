namespace ExtractHUContext.WriteSide.Domain.CommandHandlers.CreateQuantumGreeting.Commands;

public record CreateQuantumGreetingCommand(
    Guid GreetingId,
    string Message
);
