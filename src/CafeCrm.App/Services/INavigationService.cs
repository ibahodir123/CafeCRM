using System;
using System.Threading.Tasks;

namespace CafeCrm.App.Services;

public interface INavigationService
{
    void NavigateTo<TViewModel>(Action<TViewModel>? configure = null) where TViewModel : class;
    void GoBack();
    Task<bool?> ShowDialogAsync<TViewModel>(Func<TViewModel, Task>? configureAsync = null) where TViewModel : class;
}
