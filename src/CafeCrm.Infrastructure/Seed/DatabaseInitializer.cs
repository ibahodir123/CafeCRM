using CafeCrm.Domain.Entities;
using CafeCrm.Domain.Enums;
using CafeCrm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CafeCrm.Infrastructure.Seed;

public class DatabaseInitializer : IDatabaseInitializer
{
    private readonly CafeCrmDbContext _dbContext;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(CafeCrmDbContext dbContext, ILogger<DatabaseInitializer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.MigrateAsync(cancellationToken);

        if (await _dbContext.Customers.AnyAsync(cancellationToken))
        {
            return;
        }

        await SeedAsync(cancellationToken);
    }

    private async Task SeedAsync(CancellationToken cancellationToken)
    {
        var customers = new[]
        {
            CreateCustomer("Иван Петров", "+79161234567", "ivan@example.com", new DateTime(1985, 5, 15), "Любит острые блюда"),
            CreateCustomer("Мария Сидорова", "+79167654321", null, null, "Веган, аллергия на орехи"),
            CreateCustomer("Алексей Козлов", null, null, null, null)
        };

        await _dbContext.Customers.AddRangeAsync(customers, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var visits = new[]
        {
            CreateVisit(customers[0], "5", "Предпочитает место у окна", false, VisitSource.Reservation, ("Стейк Рибай",1,1200m), ("Картофель фри",1,250m), ("Кола",2,150m)),
            CreateVisit(customers[1], "12", "Веганское меню", false, VisitSource.Reservation, ("Салат Греческий",1,450m), ("Суп Томатный",1,300m), ("Смузи Ягодный",1,280m)),
            CreateVisit(customers[2], "8", null, true, VisitSource.WalkIn, ("Бургер Классический",2,350m), ("Картофель по-деревенски",1,200m))
        };

        await _dbContext.Visits.AddRangeAsync(visits, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        foreach (var visit in visits)
        {
            var customer = customers.Single(c => c.Id == visit.CustomerId);
            customer.AddVisit(visit);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Database seeded with demo data.");
    }

    private static Customer CreateCustomer(string name, string? phone, string? email, DateTime? birthDate, string? preferences)
    {
        var customer = new Customer(name, phone, email);
        customer.UpdateProfile(birthDate, preferences);
        return customer;
    }

    private static Visit CreateVisit(
        Customer customer,
        string tableNumber,
        string? notes,
        bool isTakeaway,
        VisitSource source,
        params (string Name, int Quantity, decimal Price)[] items)
    {
        var visit = new Visit(customer.Id, tableNumber, notes, isTakeaway, source);
        foreach (var item in items)
        {
            visit.AddOrderItem(item.Name, item.Quantity, item.Price);
        }

        visit.AssignPosCheck($"CHK_{Guid.NewGuid():N}".Substring(0, 8).ToUpperInvariant());
        return visit;
    }
}
