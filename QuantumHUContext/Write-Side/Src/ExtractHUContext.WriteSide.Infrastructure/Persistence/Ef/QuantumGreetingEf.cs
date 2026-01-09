namespace ExtractHUContext.WriteSide.Infrastructure.Persistence.Entities;

public class QuantumGreetingEf
{
    public Guid Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
