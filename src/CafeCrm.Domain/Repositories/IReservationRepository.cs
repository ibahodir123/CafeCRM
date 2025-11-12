using CafeCrm.Domain.Entities;
using CafeCrm.Domain.Enums;

namespace CafeCrm.Domain.Repositories;

public interface IReservationRepository
{
    Task<IReadOnlyList<Reservation>> GetUpcomingAsync(DateTime fromUtc, int take, CancellationToken cancellationToken = default);
    Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Reservation> AddAsync(Reservation reservation, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(Guid reservationId, ReservationStatus status, CancellationToken cancellationToken = default);
}
