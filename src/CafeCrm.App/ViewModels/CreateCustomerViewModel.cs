using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CafeCrm.Application.Abstractions.Notifications;
using CafeCrm.Application.DTOs;
using CafeCrm.Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CafeCrm.App.ViewModels;

public partial class CreateCustomerViewModel : ObservableObject
{
    private readonly CustomerService _customerService;
    private readonly INotificationService _notificationService;

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _phone = string.Empty;
    [ObservableProperty] private string? _email;
    [ObservableProperty] private DateTime? _birthDate;
    [ObservableProperty] private string? _preferences;
    [ObservableProperty] private bool _hasValidationErrors;

    public event Action<CustomerDto>? CustomerCreated;

    public CreateCustomerViewModel(CustomerService customerService, INotificationService notificationService)
    {
        _customerService = customerService;
        _notificationService = notificationService;
    }

    [RelayCommand]
    private async Task CreateCustomerAsync()
    {
        if (!ValidateForm())
        {
            return;
        }

        try
        {
            var customer = await _customerService.CreateAsync(
                name: Name.Trim(),
                phone: string.IsNullOrWhiteSpace(Phone) ? null : Phone.Trim(),
                email: string.IsNullOrWhiteSpace(Email) ? null : Email.Trim(),
                preferences: string.IsNullOrWhiteSpace(Preferences) ? null : Preferences.Trim(),
                birthDate: BirthDate);

            CustomerCreated?.Invoke(customer);
            CloseDialog(true);
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Не удалось создать гостя: {ex.Message}");
        }
    }

    [RelayCommand]
    private void Cancel() => CloseDialog(false);

    partial void OnNameChanged(string value) => ValidateForm();
    partial void OnPhoneChanged(string value) => ValidateForm();

    private bool ValidateForm()
    {
        HasValidationErrors = string.IsNullOrWhiteSpace(Name);
        return !HasValidationErrors;
    }

    private void CloseDialog(bool? dialogResult)
    {
        var window = System.Windows.Application.Current.Windows
            .OfType<Window>()
            .FirstOrDefault(w => ReferenceEquals(w.DataContext, this));

        if (window != null)
        {
            window.DialogResult = dialogResult;
            window.Close();
        }
    }
}
