using System;

namespace CafeCrm.Application.Abstractions.Notifications;

public interface INotificationService
{
    IObservable<Notification> Notifications { get; }
    void ShowSuccess(string message);
    void ShowError(string message);
    void ShowWarning(string message);
    void ShowInfo(string message);
}
