using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using CafeCrm.App.Services;
using CafeCrm.Application.Abstractions.Notifications;
using CafeCrm.Application.DTOs;
using CafeCrm.Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace CafeCrm.App.ViewModels;

public partial class DashboardViewModel : ObservableRecipient, IDisposable
{
    private readonly CustomerService _customerService;
    private readonly ReservationService _reservationService;
    private readonly VisitService _visitService;
    private readonly ITableService _tableService;
    private readonly IEventHistoryService _eventHistoryService;
    private readonly INavigationService _navigationService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<DashboardViewModel> _logger;
    private readonly DispatcherTimer _tableRefreshTimer;
    private readonly EventHandler _tableRefreshHandler;
    private readonly DispatcherTimer _eventsRefreshTimer;
    private readonly EventHandler _eventsRefreshHandler;

    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private bool _isRefreshing;
    [ObservableProperty] private EventLogDto[] _recentEvents = Array.Empty<EventLogDto>();
    [ObservableProperty] private bool _isLoadingEvents;

    public ObservableCollection<CustomerDto> Customers { get; } = new();
    public ObservableCollection<CustomerDto> FilteredCustomers { get; } = new();
    public ObservableCollection<TableStatusDto> ActiveTables { get; } = new();
    public ObservableCollection<ReservationDto> UpcomingReservations { get; } = new();
    public ObservableCollection<VisitDto> RecentVisits { get; } = new();

    public IAsyncRelayCommand RefreshCommand { get; }
    public IAsyncRelayCommand AddCustomerCommand { get; }
    public IRelayCommand ClearSearchCommand { get; }
    public IRelayCommand SearchCommand { get; }
    public IAsyncRelayCommand<CustomerDto?> SelectCustomerCommand { get; }
    public IAsyncRelayCommand RefreshActiveTablesCommand { get; }
    public IRelayCommand<TableStatusDto?> SelectTableCommand { get; }
    public IAsyncRelayCommand LoadRecentEventsCommand { get; }

    public DashboardViewModel(
        CustomerService customerService,
        ReservationService reservationService,
        VisitService visitService,
        ITableService tableService,
        IEventHistoryService eventHistoryService,
        INavigationService navigationService,
        INotificationService notificationService,
        ILogger<DashboardViewModel> logger)
    {
        _customerService = customerService;
        _reservationService = reservationService;
        _visitService = visitService;
        _tableService = tableService;
        _eventHistoryService = eventHistoryService;
        _navigationService = navigationService;
        _notificationService = notificationService;
        _logger = logger;

        RefreshCommand = new AsyncRelayCommand(InitializeAsync);
        AddCustomerCommand = new AsyncRelayCommand(AddCustomerAsync);
        ClearSearchCommand = new RelayCommand(() => SearchText = string.Empty);
        SearchCommand = new RelayCommand(ApplyFilter);
        SelectCustomerCommand = new AsyncRelayCommand<CustomerDto?>(SelectCustomerAsync);
        RefreshActiveTablesCommand = new AsyncRelayCommand(RefreshActiveTablesAsync);
        SelectTableCommand = new RelayCommand<TableStatusDto?>(SelectTable);
        LoadRecentEventsCommand = new AsyncRelayCommand(LoadRecentEventsAsync);

        _tableRefreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
        _tableRefreshHandler = async (_, _) => await RefreshActiveTablesAsync();
        _tableRefreshTimer.Tick += _tableRefreshHandler;
        _tableRefreshTimer.Start();

        _eventsRefreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(15) };
        _eventsRefreshHandler = async (_, _) => await LoadRecentEventsAsync();
        _eventsRefreshTimer.Tick += _eventsRefreshHandler;
        _eventsRefreshTimer.Start();
    }

    public async Task InitializeAsync()
    {
        await LoadCustomersAsync();
        await RefreshActiveTablesAsync();
        await LoadRecentEventsAsync();
    }

    private async Task LoadCustomersAsync()
    {
        try
        {
            var customers = await _customerService.GetRecentAsync(60);
            var reservations = await _reservationService.GetUpcomingAsync();
            var visits = await _visitService.GetRecentAsync();

            ReplaceContents(Customers, customers);
            ReplaceContents(UpcomingReservations, reservations);
            ReplaceContents(RecentVisits, visits);

            ApplyFilter();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось загрузить данные панели.");
            _notificationService.ShowError("Не удалось загрузить данные гостей.");
        }
    }

    private async Task AddCustomerAsync()
    {
        void OnCustomerCreated(CustomerDto customer)
        {
            Customers.Insert(0, customer);
            ApplyFilter();
            _notificationService.ShowSuccess($"Гость {customer.Name} добавлен");
        }

        CreateCustomerViewModel? dialogVm = null;
        bool? result = await _navigationService.ShowDialogAsync<CreateCustomerViewModel>(vm =>
        {
            dialogVm = vm;
            vm.CustomerCreated += OnCustomerCreated;
            return Task.CompletedTask;
        });

        if (dialogVm is not null)
        {
            dialogVm.CustomerCreated -= OnCustomerCreated;
        }

        if (result == true)
        {
            await LoadCustomersAsync();
        }
    }

    private void ApplyFilter()
    {
        IEnumerable<CustomerDto> source = Customers;
        var query = SearchText?.Trim();

        if (!string.IsNullOrWhiteSpace(query))
        {
            source = source.Where(c =>
                (!string.IsNullOrWhiteSpace(c.Name) && c.Name.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(c.Phone) && c.Phone.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(c.Email) && c.Email.Contains(query, StringComparison.OrdinalIgnoreCase)));
        }

        ReplaceContents(FilteredCustomers, source);
    }

    private async Task SelectCustomerAsync(CustomerDto? customer)
    {
        if (customer is null)
        {
            return;
        }

        bool? result = await _navigationService.ShowDialogAsync<CustomerVisitViewModel>(vm => vm.InitializeAsync(customer.Id));
        if (result == true)
        {
            _notificationService.ShowSuccess($"Визит для {customer.Name} начат");
            await InitializeAsync();
        }
    }

    private async Task RefreshActiveTablesAsync()
    {
        try
        {
            IsRefreshing = true;
            var tables = await _tableService.GetActiveTablesAsync();
            ReplaceContents(ActiveTables, tables);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка обновления активных столов.");
            _notificationService.ShowWarning("Не удалось обновить список столов.");
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private void SelectTable(TableStatusDto? table)
    {
        if (table is null)
        {
            return;
        }

        _navigationService.NavigateTo<VisitDetailViewModel>(vm => vm.InitializeAsync(table.VisitId).GetAwaiter().GetResult());
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private static void ReplaceContents<T>(ObservableCollection<T> target, IEnumerable<T> source)
    {
        target.Clear();
        foreach (var item in source)
        {
            target.Add(item);
        }
    }

    public void Dispose()
    {
        _tableRefreshTimer.Stop();
        _tableRefreshTimer.Tick -= _tableRefreshHandler;
        _eventsRefreshTimer.Stop();
        _eventsRefreshTimer.Tick -= _eventsRefreshHandler;
        GC.SuppressFinalize(this);
    }

    private async Task LoadRecentEventsAsync()
    {
        try
        {
            IsLoadingEvents = true;
            var events = await _eventHistoryService.GetRecentAsync(20);
            RecentEvents = events.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "�?�?��+��� �?'�?�?�?�? ��?��?�?�?�?��'�? POS-�?�?�?�'���.");
            _notificationService.ShowError($"�?�� �?�?���>�?�?�? �?'�?�?�?�?: {ex.Message}");
        }
        finally
        {
            IsLoadingEvents = false;
        }
    }
}
