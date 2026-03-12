using System;
using Avalonia;
using Dragon_Workspaces;

namespace DragonWorkspaces.Desktop;

internal sealed class Program(string[] args)
{
    [STAThread]
    public static void Main(string[] args) => new Program(args).Run();

    private readonly string[] startupArgs = args;

    private void Run() =>
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(startupArgs);

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
