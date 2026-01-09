namespace SharedKernel.Domain;

public interface IDateTimeProvider
{
    DateTime Now { get; }
}
