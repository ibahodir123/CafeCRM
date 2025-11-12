namespace CafeCrm.Infrastructure.Seed;

public interface IDatabaseInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
