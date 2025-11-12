namespace CafeCrm.Domain.Entities;

/// <summary>
/// Base entity with immutable identifier and audit timestamps.
/// </summary>
public abstract class AuditableEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
