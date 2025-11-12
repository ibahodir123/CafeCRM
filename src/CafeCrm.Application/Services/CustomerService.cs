using System;
using System.Linq;
using CafeCrm.Application.DTOs;
using CafeCrm.Domain.Entities;
using CafeCrm.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace CafeCrm.Application.Services;

public class CustomerService
{
    private readonly ICustomerRepository _customers;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(
        ICustomerRepository customers,
        ILogger<CustomerService> logger)
    {
        _customers = customers;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CustomerDto>> GetRecentAsync(int take = 20, CancellationToken cancellationToken = default)
    {
        var items = await _customers.GetRecentAsync(take, cancellationToken);
        return items
            .Select(MapToDto)
            .ToList();
    }

    public async Task<CustomerDto> CreateAsync(
        string name,
        string? phone,
        string? email,
        string? preferences,
        DateTime? birthDate = null,
        CancellationToken cancellationToken = default)
    {
        var customer = new Customer(name, phone, email);
        customer.UpdateProfile(birthDate, preferences);

        var saved = await _customers.AddAsync(customer, cancellationToken);
        _logger.LogInformation("Created customer {Customer}", saved.Name);
        return MapToDto(saved);
    }

    public async Task<IReadOnlyList<CustomerDto>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        var items = await _customers.SearchAsync(query, cancellationToken);
        return items.Select(MapToDto).ToList();
    }

    public async Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _customers.GetByIdAsync(id, cancellationToken);
        return customer is null ? null : MapToDto(customer);
    }

    private static CustomerDto MapToDto(Customer customer)
    {
        var lastVisit = customer.Visits?
            .OrderByDescending(v => v.StartedAt)
            .FirstOrDefault()?.StartedAt;
        var loyalty = customer.LoyaltyAccount;

        return new CustomerDto(
            customer.Id,
            customer.Name,
            customer.Phone,
            customer.Email,
            loyalty?.Tier ?? Domain.Enums.LoyaltyTier.Standard,
            loyalty?.Balance ?? 0,
            lastVisit,
            customer.Preferences);
    }
}
