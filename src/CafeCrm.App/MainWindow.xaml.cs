using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using CafeCrm.Application.Abstractions.Notifications;

namespace CafeCrm.App;

public partial class MainWindow : Window, IObserver<Notification>, IDisposable
{
    private readonly ObservableCollection<Notification> _toasts = new();
    private readonly INotificationService _notificationService;
    private IDisposable? _subscription;

    public MainWindow(INotificationService notificationService)
    {
        InitializeComponent();
        _notificationService = notificationService;
        ToastContainer.ItemsSource = _toasts;
        _subscription = _notificationService.Notifications.Subscribe(this);
    }

    public void DisplayContent(object content)
    {
        ShellContent.Content = content;
    }

    private void AddToast(Notification notification)
    {
        _toasts.Insert(0, notification);
        _ = RemoveToastAsync(notification);
    }

    private async Task RemoveToastAsync(Notification notification)
    {
        await Task.Delay(TimeSpan.FromSeconds(5));
        await Dispatcher.InvokeAsync(() =>
        {
            _toasts.Remove(notification);
        });
    }

    public void OnCompleted()
    {
    }

    public void OnError(Exception error)
    {
    }

    public void OnNext(Notification value)
    {
        Dispatcher.Invoke(() => AddToast(value));
    }

    public void Dispose()
    {
        _subscription?.Dispose();
    }
}
