using CafeCrm.Domain.Enums;

namespace CafeCrm.Domain.Entities;

public class Reservation : AuditableEntity
{
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public DateTime ReservationTime { get; set; }
    public int PartySize { get; set; }
    public string? TablePreference { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.New;
    public string? Notes { get; set; }
}
