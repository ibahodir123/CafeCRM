using CafeCrm.Domain.Entities;

namespace CafeCrm.Domain.Repositories;

public interface ICustomerRepository
{
    Task<IReadOnlyList<Customer>> GetRecentAsync(int take, CancellationToken cancellationToken = default);
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Customer> AddAsync(Customer customer, CancellationToken cancellationToken = default);
    Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Customer>> SearchAsync(string query, CancellationToken cancellationToken = default);
}
