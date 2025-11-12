using CafeCrm.Domain.Entities;
using CafeCrm.Domain.Enums;
using CafeCrm.Domain.Repositories;
using CafeCrm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CafeCrm.Infrastructure.Repositories;

internal sealed class ReservationRepository : IReservationRepository
{
    private readonly CafeCrmDbContext _dbContext;

    public ReservationRepository(CafeCrmDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Reservation> AddAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        _dbContext.Reservations.Add(reservation);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return reservation;
    }

    public async Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Reservations
            .Include(r => r.Customer)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Reservation>> GetUpcomingAsync(DateTime fromUtc, int take, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Reservations
            .AsNoTracking()
            .Include(r => r.Customer)
            .Where(r => r.ReservationTime >= fromUtc.AddHours(-1))
            .OrderBy(r => r.ReservationTime)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateStatusAsync(Guid reservationId, ReservationStatus status, CancellationToken cancellationToken = default)
    {
        await _dbContext.Reservations
            .Where(r => r.Id == reservationId)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.Status, status), cancellationToken);
    }
}
