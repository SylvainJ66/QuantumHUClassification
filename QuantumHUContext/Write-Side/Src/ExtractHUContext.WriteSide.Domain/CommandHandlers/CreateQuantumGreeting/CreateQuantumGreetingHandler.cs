using ExtractHUContext.WriteSide.Domain.CommandHandlers.CreateQuantumGreeting.Commands;
using ExtractHUContext.WriteSide.Domain.Models;
using ExtractHUContext.WriteSide.Domain.Ports;
using SharedKernel.Domain;

namespace ExtractHUContext.WriteSide.Domain.CommandHandlers.CreateQuantumGreeting;

public static class CreateQuantumGreetingHandler
{
    public static async Task<Result> Handle(
        CreateQuantumGreetingCommand command,
        IQuantumGreetingRepository repository,
        IDateTimeProvider dateTimeProvider)
    {
        var greetingResult = QuantumGreeting.Create(
            command.GreetingId,
            command.Message,
            dateTimeProvider
        );

        if (!greetingResult.IsSuccess)
            return Result.Failure(greetingResult.Error);

        await repository.SaveAsync(greetingResult.Value);

        return Result.Success();
    }
}
