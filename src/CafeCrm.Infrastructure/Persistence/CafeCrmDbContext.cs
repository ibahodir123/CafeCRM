using CafeCrm.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CafeCrm.Infrastructure.Persistence;

public class CafeCrmDbContext : DbContext
{
    public CafeCrmDbContext(DbContextOptions<CafeCrmDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Visit> Visits => Set<Visit>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<LoyaltyAccount> LoyaltyAccounts => Set<LoyaltyAccount>();
    public DbSet<EventLog> EventLogs => Set<EventLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(e => e.Phone)
                .HasMaxLength(20);
            entity.Property(e => e.Email)
                .HasMaxLength(200);
            entity.Property(e => e.Preferences)
                .HasMaxLength(1000);
            entity.Property(e => e.TotalSpent)
                .HasPrecision(18, 2);
            entity.Property(e => e.LastVisit);
            entity.Property(e => e.VisitCount);

            entity.HasIndex(e => e.Phone);
            entity.HasIndex(e => e.Name);

            entity.HasMany(e => e.Visits)
                .WithOne(v => v.Customer)
                .HasForeignKey(v => v.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.Reservations)
                .WithOne(r => r.Customer)
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.LoyaltyAccount)
                .WithOne(a => a.Customer)
                .HasForeignKey<LoyaltyAccount>(a => a.CustomerId);

            entity.Metadata.FindNavigation(nameof(Customer.Visits))?
                .SetPropertyAccessMode(PropertyAccessMode.Field);
            entity.Metadata.FindNavigation(nameof(Customer.Reservations))?
                .SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<Visit>(entity =>
        {
            entity.HasKey(v => v.Id);
            entity.Property(v => v.TableNumber)
                .IsRequired()
                .HasMaxLength(10);
            entity.Property(v => v.Notes).HasMaxLength(1000);
            entity.Property(v => v.TotalAmount).HasPrecision(18, 2);
            entity.Property(v => v.PosCheckId).HasMaxLength(50);
            entity.Property(v => v.StartedAt).IsRequired();

            entity.HasIndex(v => v.TableNumber);
            entity.HasIndex(v => v.PosCheckId);
            entity.HasIndex(v => v.StartedAt);
            entity.HasIndex(v => v.CustomerId);
            entity.HasIndex(v => v.EndedAt);

            entity.OwnsMany(v => v.OrderItems, oi =>
            {
                oi.ToTable("OrderItems");
                oi.WithOwner().HasForeignKey("VisitId");
                oi.HasKey(o => o.Id);
                oi.Property(x => x.Name).IsRequired().HasMaxLength(200);
                oi.Property(x => x.Price).HasPrecision(18, 2);
                oi.Property(x => x.Quantity).IsRequired();
            });
        });

        modelBuilder.Entity<EventLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Message).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Details).HasMaxLength(2000);
            entity.Property(e => e.Timestamp).IsRequired();
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.EventType);
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.Property(r => r.TablePreference).HasMaxLength(50);
            entity.Property(r => r.Notes).HasMaxLength(500);
        });

        modelBuilder.Entity<LoyaltyAccount>(entity =>
        {
            entity.Property(a => a.Balance).HasPrecision(14, 2);
        });
    }
}
