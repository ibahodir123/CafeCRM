using System;

namespace CafeCrm.App.Services;

public interface IDialogCompletionNotifier
{
    event Action<bool?, string?> DialogCompleted;
}
