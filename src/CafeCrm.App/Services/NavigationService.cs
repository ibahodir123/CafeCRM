using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CafeCrm.App.Views;
using Microsoft.Extensions.DependencyInjection;

namespace CafeCrm.App.Services;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Stack<FrameworkElement> _navigationStack = new();

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void NavigateTo<TViewModel>(Action<TViewModel>? configure = null)
        where TViewModel : class
    {
        var view = CreateViewWithViewModel(configure);
        DisplayView(view);
        _navigationStack.Push(view);
    }

    public async Task<bool?> ShowDialogAsync<TViewModel>(Func<TViewModel, Task>? configureAsync = null)
        where TViewModel : class
    {
        var (view, viewModel) = CreateViewInstance<TViewModel>();
        if (configureAsync is not null)
        {
            await configureAsync(viewModel);
        }

        if (view is not Window window)
        {
            throw new InvalidOperationException($"View for {typeof(TViewModel).Name} must inherit Window for dialog usage.");
        }

        window.DataContext = viewModel;
        window.Owner = System.Windows.Application.Current.MainWindow;
        window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

        EventHandler? closedHandler = null;

        if (viewModel is IDialogCompletionNotifier notifier)
        {
            void Handler(bool? result, string? _)
            {
                window.DialogResult = result;
                if (window.IsVisible)
                {
                    window.Close();
                }
            }

            notifier.DialogCompleted += Handler;
            closedHandler = (_, _) => notifier.DialogCompleted -= Handler;
            window.Closed += closedHandler;
        }

        var result = window.ShowDialog();

        if (closedHandler != null)
        {
            window.Closed -= closedHandler;
        }

        return result;
    }

    public void GoBack()
    {
        if (_navigationStack.Count <= 1)
        {
            return;
        }

        _navigationStack.Pop();
        var previousView = _navigationStack.Peek();
        DisplayView(previousView);
    }

    private FrameworkElement CreateViewWithViewModel<TViewModel>(Action<TViewModel>? configure)
        where TViewModel : class
    {
        var (view, viewModel) = CreateViewInstance<TViewModel>();
        configure?.Invoke(viewModel);

        if (view is not FrameworkElement frameworkElement)
        {
            throw new InvalidOperationException($"View for {typeof(TViewModel).Name} must inherit FrameworkElement.");
        }

        frameworkElement.DataContext = viewModel;
        return frameworkElement;
    }

    private (object view, TViewModel viewModel) CreateViewInstance<TViewModel>()
        where TViewModel : class
    {
        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
        var viewType = ResolveViewType(typeof(TViewModel));
        var view = Activator.CreateInstance(viewType)
                   ?? throw new InvalidOperationException($"Unable to create view for {viewType.Name}");

        return (view, viewModel);
    }

    private static Type ResolveViewType(Type viewModelType)
    {
        var expectedName = viewModelType.Name.Replace("ViewModel", "View", StringComparison.Ordinal);

        var viewType = typeof(App).Assembly
            .GetTypes()
            .FirstOrDefault(t => string.Equals(t.Name, expectedName, StringComparison.Ordinal));

        if (viewType == null)
        {
            throw new InvalidOperationException($"View for {viewModelType.Name} not found. Expected type named {expectedName} in CafeCrm.App assembly.");
        }

        return viewType;
    }

    private static void DisplayView(FrameworkElement view)
    {
        if (System.Windows.Application.Current.MainWindow is not MainWindow shell)
        {
            throw new InvalidOperationException("Main window is not initialized.");
        }

        shell.DisplayContent(view);
    }
}
