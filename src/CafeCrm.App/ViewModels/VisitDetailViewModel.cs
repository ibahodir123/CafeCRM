using System;
using System.Threading.Tasks;
using CafeCrm.Application.Abstractions.Notifications;
using CafeCrm.Application.DTOs;
using CafeCrm.Application.Services;
using CafeCrm.App.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CafeCrm.App.ViewModels;

public partial class VisitDetailViewModel : ObservableObject
{
    private readonly ITableService _tableService;
    private readonly VisitService _visitService;
    private readonly INavigationService _navigationService;
    private readonly INotificationService _notificationService;

    [ObservableProperty] private VisitDetailDto? _visit;
    [ObservableProperty] private bool _isLoading;

    public VisitDetailViewModel(
        ITableService tableService,
        VisitService visitService,
        INavigationService navigationService,
        INotificationService notificationService)
    {
        _tableService = tableService;
        _visitService = visitService;
        _navigationService = navigationService;
        _notificationService = notificationService;
    }

    public async Task InitializeAsync(Guid visitId)
    {
        try
        {
            IsLoading = true;
            Visit = await _tableService.GetVisitDetailAsync(visitId);
            if (Visit is null)
            {
                _notificationService.ShowWarning("Visit was not found.");
                _navigationService.GoBack();
            }
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Failed to load visit details: {ex.Message}");
            _navigationService.GoBack();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Close() => _navigationService.GoBack();

    [RelayCommand]
    private async Task CloseVisitAsync()
    {
        if (Visit is null)
        {
            return;
        }

        try
        {
            IsLoading = true;
            await _visitService.CloseAsync(Visit.Id);
            _notificationService.ShowSuccess($"Визит стола {Visit.TableNumber} завершен");
            _navigationService.GoBack();
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Не удалось завершить визит: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}



