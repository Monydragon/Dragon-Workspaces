using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using Dragon_Workspaces;

namespace DragonWorkspaces.Browser;

internal sealed class Program(string[] args)
{
    public static Task Main(string[] args) => new Program(args).RunAsync();

    private readonly string[] startupArgs = args;

    private Task RunAsync()
    {
        _ = startupArgs;

        return BuildAvaloniaApp()
            .WithInterFont()
            .LogToTrace()
            .StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>();
}
