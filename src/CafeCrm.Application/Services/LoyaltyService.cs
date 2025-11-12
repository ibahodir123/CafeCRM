using CafeCrm.Domain.Enums;
using CafeCrm.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace CafeCrm.Application.Services;

public class LoyaltyService
{
    private readonly ILoyaltyAccountRepository _loyaltyAccounts;
    private readonly ILogger<LoyaltyService> _logger;

    public LoyaltyService(ILoyaltyAccountRepository loyaltyAccounts, ILogger<LoyaltyService> logger)
    {
        _loyaltyAccounts = loyaltyAccounts;
        _logger = logger;
    }

    public async Task AccrueAsync(Guid customerId, decimal amount, CancellationToken cancellationToken = default)
    {
        var account = await _loyaltyAccounts.GetOrCreateAsync(customerId, cancellationToken);
        account.Balance += amount;
        account.LastActivityAt = DateTime.UtcNow;
        account.Tier = CalculateTier(account.Balance);
        await _loyaltyAccounts.UpdateAsync(account, cancellationToken);
        _logger.LogInformation("Accrued {Amount} points for customer {Customer}", amount, customerId);
    }

    private static LoyaltyTier CalculateTier(decimal balance)
    {
        if (balance >= 5000) return LoyaltyTier.Platinum;
        if (balance >= 2500) return LoyaltyTier.Gold;
        if (balance >= 1000) return LoyaltyTier.Silver;
        return LoyaltyTier.Standard;
    }
}
