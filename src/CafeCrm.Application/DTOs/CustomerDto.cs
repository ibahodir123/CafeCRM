using CafeCrm.Domain.Enums;

namespace CafeCrm.Application.DTOs;

public record CustomerDto(
    Guid Id,
    string Name,
    string? Phone,
    string? Email,
    LoyaltyTier Tier,
    decimal LoyaltyBalance,
    DateTime? LastVisitAt,
    string? Preferences);
