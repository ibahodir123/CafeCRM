using CafeCrm.Application.DTOs;
using CafeCrm.Domain.Entities;
using CafeCrm.Domain.Enums;
using CafeCrm.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace CafeCrm.Application.Services;

public class ReservationService
{
    private readonly IReservationRepository _reservations;
    private readonly ILogger<ReservationService> _logger;

    public ReservationService(IReservationRepository reservations, ILogger<ReservationService> logger)
    {
        _reservations = reservations;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ReservationDto>> GetUpcomingAsync(int take = 20, CancellationToken cancellationToken = default)
    {
        var items = await _reservations.GetUpcomingAsync(DateTime.UtcNow, take, cancellationToken);
        return items.Select(MapToDto).ToList();
    }

    public async Task<ReservationDto> ScheduleAsync(Guid customerId, DateTime reservationTime, int partySize, string? notes, CancellationToken cancellationToken = default)
    {
        var reservation = new Reservation
        {
            CustomerId = customerId,
            ReservationTime = reservationTime,
            PartySize = partySize,
            Notes = notes
        };

        var saved = await _reservations.AddAsync(reservation, cancellationToken);
        _logger.LogInformation("Scheduled reservation {ReservationId} for {Customer}", saved.Id, customerId);
        return MapToDto(saved);
    }

    public Task UpdateStatusAsync(Guid reservationId, ReservationStatus status, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating reservation {ReservationId} with status {Status}", reservationId, status);
        return _reservations.UpdateStatusAsync(reservationId, status, cancellationToken);
    }

    private static ReservationDto MapToDto(Reservation reservation)
    {
        var name = reservation.Customer?.Name ?? "Клиент";
        return new ReservationDto(
            reservation.Id,
            name,
            reservation.ReservationTime,
            reservation.PartySize,
            reservation.Status,
            reservation.Notes);
    }
}
