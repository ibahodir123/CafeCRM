using System.Collections.Generic;

namespace CafeCrm.Domain.Entities;

public class Customer : AuditableEntity
{
    private readonly List<Visit> _visits = new();
    private readonly List<Reservation> _reservations = new();

    public string Name { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public DateTime? BirthDate { get; private set; }
    public string? Preferences { get; private set; }

    public int VisitCount { get; private set; }
    public decimal TotalSpent { get; private set; }
    public DateTime? LastVisit { get; private set; }

    public IReadOnlyCollection<Visit> Visits => _visits.AsReadOnly();
    public IReadOnlyCollection<Reservation> Reservations => _reservations.AsReadOnly();
    public LoyaltyAccount? LoyaltyAccount { get; private set; }

    private Customer()
    {
    }

    public Customer(string name, string? phone = null, string? email = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        Phone = phone?.Trim();
        Email = email?.Trim();
    }

    public void UpdateContact(string name, string? phone, string? email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
    }

    public void UpdateProfile(DateTime? birthDate, string? preferences)
    {
        BirthDate = birthDate;
        Preferences = string.IsNullOrWhiteSpace(preferences) ? null : preferences.Trim();
    }

    public void AddVisit(Visit visit)
    {
        if (visit is null) throw new ArgumentNullException(nameof(visit));
        _visits.Add(visit);
        visit.Customer = this;
        visit.CustomerId = Id;
        VisitCount++;
        TotalSpent += visit.TotalAmount;
        LastVisit = DateTime.UtcNow;
    }

    public bool IsVip() => TotalSpent >= 10000m || VisitCount >= 10;
}
