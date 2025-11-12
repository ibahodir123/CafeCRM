using System.IO;
using CafeCrm.Application;
using CafeCrm.Application.Abstractions.Notifications;
using CafeCrm.App.Services;
using CafeCrm.App.ViewModels;
using CafeCrm.Infrastructure.Extensions;
using CafeCrm.Infrastructure.Seed;
using CafeCrm.Pos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CafeCrm.App;

public partial class App : System.Windows.Application
{
    private IHost? _host;

    protected override async void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e);
        EnsureDataDirectory();
        LogDataDirectories();

        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((_, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddApplicationServices();
                services.AddInfrastructure(context.Configuration);
                services.AddPosAdapter(context.Configuration);

                services.AddSingleton<INotificationService, NotificationService>();
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddTransient<DashboardViewModel>();
                services.AddTransient<CustomerVisitViewModel>();
                services.AddTransient<VisitDetailViewModel>();
                services.AddTransient<CreateCustomerViewModel>();
                services.AddSingleton<MainWindow>();
            })
            .ConfigureLogging(builder =>
            {
                builder.AddDebug();
            })
            .Build();

        await _host.StartAsync();

        using (var scope = _host.Services.CreateScope())
        {
            var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
            await initializer.InitializeAsync();
        }

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        MainWindow = mainWindow;
        mainWindow.Show();

        var navigationService = _host.Services.GetRequiredService<INavigationService>();
        navigationService.NavigateTo<DashboardViewModel>(vm => vm.InitializeAsync().GetAwaiter().GetResult());
    }

    protected override async void OnExit(System.Windows.ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        base.OnExit(e);
    }

    private static void EnsureDataDirectory()
    {
        var baseDirectory = AppContext.BaseDirectory;
        var rootDataDirectory = Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", "Data"));
        var dataDirectory = Directory.Exists(rootDataDirectory)
            ? rootDataDirectory
            : Path.Combine(baseDirectory, "Data");

        if (!Directory.Exists(dataDirectory))
        {
            Directory.CreateDirectory(dataDirectory);
        }

        AppDomain.CurrentDomain.SetData("DataDirectory", dataDirectory);
    }

    private static void LogDataDirectories()
    {
        var dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory");
        var dbPath = Path.Combine(AppContext.BaseDirectory, "Data", "cafecrm.db");
        Console.WriteLine($"[CafeCRM] DataDirectory = {dataDirectory}");
        Console.WriteLine($"[CafeCRM] AppContext DB path = {dbPath}");
    }
}
