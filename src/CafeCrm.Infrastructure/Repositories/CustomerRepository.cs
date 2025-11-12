using CafeCrm.Domain.Entities;
using CafeCrm.Domain.Repositories;
using CafeCrm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CafeCrm.Infrastructure.Repositories;

internal sealed class CustomerRepository : ICustomerRepository
{
    private readonly CafeCrmDbContext _dbContext;

    public CustomerRepository(CafeCrmDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Customer> AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return customer;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Customers.FindAsync(new object?[] { id }, cancellationToken);
        if (entity is null)
        {
            return;
        }

        _dbContext.Customers.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Customers
            .Include(c => c.LoyaltyAccount)
            .Include(c => c.Visits)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Customer>> GetRecentAsync(int take, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Customers
            .AsNoTracking()
            .Include(c => c.LoyaltyAccount)
            .Include(c => c.Visits)
            .OrderByDescending(c => c.LastVisit ?? c.UpdatedAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Customer>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        query = $"%{query}%";

        return await _dbContext.Customers
            .AsNoTracking()
            .Include(c => c.LoyaltyAccount)
            .Where(c =>
                EF.Functions.Like(c.Name, query) ||
                (c.Phone != null && EF.Functions.Like(c.Phone, query)) ||
                (c.Email != null && EF.Functions.Like(c.Email, query)))
            .Take(20)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        _dbContext.Customers.Update(customer);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
