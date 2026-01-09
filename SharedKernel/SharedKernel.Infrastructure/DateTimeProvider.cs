using SharedKernel.Domain;

namespace SharedKernel.Infrastructure;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.UtcNow;
}
