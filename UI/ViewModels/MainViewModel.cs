using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using Dragon_Workspaces.Core.Enums;
using Dragon_Workspaces.Core.Interfaces;
using Dragon_Workspaces.Core.Models;

namespace Dragon_Workspaces.UI.ViewModels;

public partial class MainViewModel(
    IWorkspaceManager workspaceManager,
    IResponsiveLayoutService responsiveLayoutService) : ViewModelBase
{
    private const string EmptyStateMessage = "Choose a workspace to inspect its toolchain, metadata, and sync status.";

    public ObservableCollection<Project> Projects { get; } = [];

    public WorkspaceLayoutMode LayoutMode
    {
        get;
        private set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsDesktopLayout));
            OnPropertyChanged(nameof(IsMobileLayout));
        }
    } = WorkspaceLayoutMode.DesktopThreeColumn;

    public bool IsDesktopLayout => LayoutMode == WorkspaceLayoutMode.DesktopThreeColumn;

    public bool IsMobileLayout => LayoutMode == WorkspaceLayoutMode.MobileSingleColumn;

    [ObservableProperty]
    private Project? selectedProject;

    [ObservableProperty]
    private object currentContent = new DashboardPlaceholder(EmptyStateMessage);

    public async Task InitializeAsync()
    {
        await workspaceManager.InitializeAsync();
        await LoadProjectsAsync();
    }

    public async Task ImportWorkspaceAsync(IStorageFolder folder)
    {
        var project = await workspaceManager.ImportWorkspaceAsync(folder);
        var existingProject = Projects.FirstOrDefault(item => item.Id == project.Id);

        if (existingProject is not null)
        {
            Projects.Remove(existingProject);
        }

        Projects.Insert(0, project);
        SelectedProject = project;
    }

    public void UpdateLayout(double width)
    {
        LayoutMode = responsiveLayoutService.GetLayoutMode(width);
    }

    partial void OnSelectedProjectChanged(Project? value)
    {
        CurrentContent = value is null
            ? new DashboardPlaceholder(EmptyStateMessage)
            : value;
    }

    private async Task LoadProjectsAsync()
    {
        var projects = await workspaceManager.GetWorkspacesAsync();

        Projects.Clear();

        foreach (var project in projects)
        {
            Projects.Add(project);
        }

        SelectedProject = Projects.Count > 0 ? Projects[0] : null;
    }
}
