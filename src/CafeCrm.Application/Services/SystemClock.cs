using CafeCrm.Application.Abstractions;

namespace CafeCrm.Application.Services;

internal sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
