using CafeCrm.Domain.Enums;

namespace CafeCrm.Domain.Entities;

public class LoyaltyAccount : AuditableEntity
{
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public decimal Balance { get; set; }
    public LoyaltyTier Tier { get; set; } = LoyaltyTier.Standard;
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;
}
