using CafeCrm.Domain.Entities;

namespace CafeCrm.Domain.Repositories;

public interface ILoyaltyAccountRepository
{
    Task<LoyaltyAccount> GetOrCreateAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task UpdateAsync(LoyaltyAccount account, CancellationToken cancellationToken = default);
}
