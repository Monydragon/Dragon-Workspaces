using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Dragon_Workspaces.Core.Interfaces;
using Dragon_Workspaces.Data.Persistence;
using Dragon_Workspaces.Data.Repositories;
using Dragon_Workspaces.Services.BusinessLogic;
using Dragon_Workspaces.UI.ViewModels;
using Dragon_Workspaces.UI.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Dragon_Workspaces;

public partial class App : Application
{
    public IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Services = ConfigureServices();
        InitializeDatabaseAsync(Services).GetAwaiter().GetResult();
        var mainViewModel = Services.GetRequiredService<MainViewModel>();
        mainViewModel.InitializeAsync().GetAwaiter().GetResult();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new Dragon_Workspaces.UI.Views.MainWindow
            {
                DataContext = mainViewModel
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            singleView.MainView = new MainView
            {
                DataContext = mainViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static async Task InitializeDatabaseAsync(IServiceProvider services)
    {
        if (OperatingSystem.IsBrowser())
        {
            return;
        }

        await using var dbContext = await services
            .GetRequiredService<IDbContextFactory<WorkspaceDbContext>>()
            .CreateDbContextAsync();

        await WorkspaceDbSchemaUpgrader.EnsureCompatibleSchemaAsync(dbContext);
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        ConfigureDatabase(services);

        services.AddSingleton<IProjectRepository, ProjectRepository>();
        services.AddSingleton<IWorkspaceManager, WorkspaceManager>();
        services.AddSingleton<IResponsiveLayoutService, ResponsiveLayoutService>();
        services.AddSingleton<IStorageService, CrossPlatformStorageService>();
        services.AddSingleton<MainViewModel>();

        return services.BuildServiceProvider();
    }

    private static void ConfigureDatabase(IServiceCollection services)
    {
#if BROWSER
        services.AddDbContextFactory<WorkspaceDbContext>(options =>
            options.UseInMemoryDatabase("DragonWorkspaces"));
#else
        if (OperatingSystem.IsBrowser())
        {
            services.AddDbContextFactory<WorkspaceDbContext>(options =>
                options.UseInMemoryDatabase("DragonWorkspaces"));

            return;
        }

        var databasePath = BuildDatabasePath();

        services.AddDbContextFactory<WorkspaceDbContext>(options =>
            options.UseSqlite($"Data Source={databasePath}"));
#endif
    }

    private static string BuildDatabasePath()
    {
        var appDataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DragonWorkspaces");

        Directory.CreateDirectory(appDataDirectory);

        return Path.Combine(appDataDirectory, "dragon-workspaces.db");
    }
}