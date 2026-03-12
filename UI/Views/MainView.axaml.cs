using Avalonia;
using Avalonia.Controls;
using Dragon_Workspaces.Core.Interfaces;
using Dragon_Workspaces.UI.ViewModels;

namespace Dragon_Workspaces.UI.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        AttachedToVisualTree += (_, _) => SyncLayout();
        DataContextChanged += (_, _) => SyncLayout();
        SizeChanged += (_, _) => SyncLayout();
    }

    private async void AddWorkspaceButton_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (Application.Current is not App app ||
            app.Services.GetService(typeof(IStorageService)) is not IStorageService storageService ||
            DataContext is not MainViewModel viewModel)
        {
            return;
        }

        var folder = await storageService.PickFolderAsync(this);

        if (folder is null)
        {
            return;
        }

        await viewModel.ImportWorkspaceAsync(folder);
    }

    private void SyncLayout()
    {
        if (DataContext is MainViewModel viewModel && Bounds.Width > 0)
        {
            viewModel.UpdateLayout(Bounds.Width);
        }
    }
}
