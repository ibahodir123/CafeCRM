using CafeCrm.Domain.Entities;
using CafeCrm.Domain.Repositories;
using CafeCrm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CafeCrm.Infrastructure.Repositories;

internal sealed class LoyaltyAccountRepository : ILoyaltyAccountRepository
{
    private readonly CafeCrmDbContext _dbContext;

    public LoyaltyAccountRepository(CafeCrmDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LoyaltyAccount> GetOrCreateAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var account = await _dbContext.LoyaltyAccounts.FirstOrDefaultAsync(a => a.CustomerId == customerId, cancellationToken);
        if (account != null)
        {
            return account;
        }

        account = new LoyaltyAccount { CustomerId = customerId };
        _dbContext.LoyaltyAccounts.Add(account);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return account;
    }

    public async Task UpdateAsync(LoyaltyAccount account, CancellationToken cancellationToken = default)
    {
        _dbContext.LoyaltyAccounts.Update(account);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
