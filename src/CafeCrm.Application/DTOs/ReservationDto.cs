using CafeCrm.Domain.Enums;

namespace CafeCrm.Application.DTOs;

public record ReservationDto(
    Guid Id,
    string CustomerName,
    DateTime ReservationTime,
    int PartySize,
    ReservationStatus Status,
    string? Notes);
