using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;
using Dragon_Workspaces;

namespace DragonWorkspaces.Android;

[Activity(
    Label = "Dragon Workspaces",
    Theme = "@style/MyTheme.NoActionBar",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) =>
        base.CustomizeAppBuilder(builder)
            .WithInterFont()
            .LogToTrace();
}
