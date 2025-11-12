using CafeCrm.Domain.Enums;

namespace CafeCrm.Application.DTOs;

public record VisitDto(
    Guid Id,
    DateTime StartedAt,
    decimal TotalAmount,
    string? CustomerName,
    VisitSource Source,
    string? TableNumber,
    string? Notes,
    bool IsTakeaway,
    string? PosCheckId);
