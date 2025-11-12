using System;
using System.Collections.Generic;
using CafeCrm.Application.Abstractions.Notifications;

namespace CafeCrm.App.Services;

public class NotificationService : INotificationService
{
    private readonly List<IObserver<Notification>> _observers = new();

    public IObservable<Notification> Notifications => new NotificationObservable(this);

    public void ShowSuccess(string message) => Publish(NotificationType.Success, message);
    public void ShowError(string message) => Publish(NotificationType.Error, message);
    public void ShowWarning(string message) => Publish(NotificationType.Warning, message);
    public void ShowInfo(string message) => Publish(NotificationType.Info, message);

    private void Publish(NotificationType type, string message)
    {
        var notification = new Notification(type, message, DateTime.Now);
        List<IObserver<Notification>> snapshot;

        lock (_observers)
        {
            snapshot = new List<IObserver<Notification>>(_observers);
        }

        foreach (var observer in snapshot)
        {
            observer.OnNext(notification);
        }
    }

    private IDisposable Subscribe(IObserver<Notification> observer)
    {
        lock (_observers)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
        }

        return new Unsubscriber(_observers, observer);
    }

    private sealed class NotificationObservable : IObservable<Notification>
    {
        private readonly NotificationService _service;

        public NotificationObservable(NotificationService service)
        {
            _service = service;
        }

        public IDisposable Subscribe(IObserver<Notification> observer)
        {
            if (observer == null) throw new ArgumentNullException(nameof(observer));
            return _service.Subscribe(observer);
        }
    }

    private sealed class Unsubscriber : IDisposable
    {
        private readonly List<IObserver<Notification>> _observers;
        private readonly IObserver<Notification> _observer;

        public Unsubscriber(List<IObserver<Notification>> observers, IObserver<Notification> observer)
        {
            _observers = observers;
            _observer = observer;
        }

        public void Dispose()
        {
            if (_observer == null)
            {
                return;
            }

            lock (_observers)
            {
                _observers.Remove(_observer);
            }
        }
    }
}
