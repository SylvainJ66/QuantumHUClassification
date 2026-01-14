using SharedKernel.Domain;

namespace ExtractHUContext.WriteSide.Domain.Tests.CommandHandlers.ExtractHUFromStudy.Stubs;

public class DateTimeProviderStub : IDateTimeProvider
{
    private DateTime? _fixedNow;

    public void SetNow(DateTime dateTime)
    {
        _fixedNow = dateTime;
    }

    public DateTime Now => _fixedNow ?? DateTime.UtcNow;
}
