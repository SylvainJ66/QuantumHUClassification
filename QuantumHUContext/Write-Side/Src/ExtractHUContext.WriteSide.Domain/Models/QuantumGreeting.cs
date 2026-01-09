using ExtractHUContext.WriteSide.Domain.Models.Snapshots;
using SharedKernel.Domain;

namespace ExtractHUContext.WriteSide.Domain.Models;

public class QuantumGreeting
{
    private readonly Guid _id;
    private readonly string _message;
    private readonly DateTime _createdAt;

    public QuantumGreeting(Guid id, string message, DateTime createdAt)
    {
        _id = id;
        _message = message;
        _createdAt = createdAt;
    }

    public static Result<QuantumGreeting> Create(
        Guid id,
        string message,
        IDateTimeProvider dateTimeProvider)
    {
        if (string.IsNullOrWhiteSpace(message))
            return Result.Failure<QuantumGreeting>("Message cannot be empty");

        if (message.Length > 500)
            return Result.Failure<QuantumGreeting>("Message cannot exceed 500 characters");

        var greeting = new QuantumGreeting(id, message, dateTimeProvider.Now);
        return Result.Success(greeting);
    }

    public QuantumGreetingSnapshot ToSnapshot()
    {
        return new QuantumGreetingSnapshot(
            _id,
            _message,
            _createdAt
        );
    }

    public static QuantumGreeting FromSnapshot(QuantumGreetingSnapshot snapshot)
    {
        return new QuantumGreeting(
            snapshot.Id,
            snapshot.Message,
            snapshot.CreatedAt
        );
    }
}
