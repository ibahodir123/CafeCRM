using System;
using System.Threading.Tasks;
using CafeCrm.App.Services;
using CafeCrm.Application.DTOs;
using CafeCrm.Application.Services;
using CafeCrm.Domain.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CafeCrm.App.ViewModels;

public partial class CustomerVisitViewModel : ObservableRecipient, IDialogCompletionNotifier
{
    private readonly VisitService _visitService;
    private readonly CustomerService _customerService;

    [ObservableProperty] private CustomerDto? _customer;
    [ObservableProperty] private string _tableNumber = string.Empty;
    [ObservableProperty] private string _notes = string.Empty;
    [ObservableProperty] private bool _isTakeaway;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private string? _posCheckId;

    public event Action<bool?, string?>? DialogCompleted;

    public CustomerVisitViewModel(VisitService visitService, CustomerService customerService)
    {
        _visitService = visitService;
        _customerService = customerService;
    }

    public async Task InitializeAsync(Guid customerId)
    {
        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var customer = await _customerService.GetByIdAsync(customerId);
            if (customer is null)
            {
                ErrorMessage = "Гость не найден";
                DialogCompleted?.Invoke(false, ErrorMessage);
                return;
            }

            Customer = customer;
            TableNumber = string.Empty;
            Notes = string.Empty;
            IsTakeaway = false;
            PosCheckId = null;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка загрузки данных гостя: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task StartVisitAsync()
    {
        if (Customer is null)
        {
            ErrorMessage = "Гость не выбран";
            return;
        }

        if (string.IsNullOrWhiteSpace(TableNumber))
        {
            ErrorMessage = "Укажите номер стола";
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            await _visitService.RegisterAsync(
                Customer.Id,
                checkAmount: 0,
                tableNumber: TableNumber.Trim(),
                source: IsTakeaway ? VisitSource.Delivery : VisitSource.WalkIn,
                posTicketId: PosCheckId,
                isTakeaway: IsTakeaway,
                notes: string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim());

            DialogCompleted?.Invoke(true, $"Визит для {Customer.Name} открыт");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка регистрации визита: {ex.Message}";
            DialogCompleted?.Invoke(false, ErrorMessage);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Cancel() => DialogCompleted?.Invoke(false, null);
}
