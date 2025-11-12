using CafeCrm.Domain.Entities;

namespace CafeCrm.Application.Contracts;

public interface IPosClient
{
    Task ConnectAsync(CancellationToken cancellationToken = default);
    Task DisconnectAsync(CancellationToken cancellationToken = default);
    IAsyncEnumerable<PosTicket> ListenTicketsAsync(CancellationToken cancellationToken = default);
}
