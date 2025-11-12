using System;

namespace CafeCrm.Application.Abstractions.Notifications;

public record Notification(NotificationType Type, string Message, DateTime Timestamp);
