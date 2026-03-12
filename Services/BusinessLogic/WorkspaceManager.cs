using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Dragon_Workspaces.Core.Enums;
using Dragon_Workspaces.Core.Interfaces;
using Dragon_Workspaces.Core.Models;

namespace Dragon_Workspaces.Services.BusinessLogic;

public sealed class WorkspaceManager(
    IProjectRepository projectRepository,
    IStorageService storageService) : IWorkspaceManager
{
    public Task InitializeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task<IReadOnlyList<Project>> GetWorkspacesAsync(CancellationToken cancellationToken = default) =>
        projectRepository.GetProjectsAsync(cancellationToken);

    public Task<Project?> GetWorkspaceAsync(Guid id, CancellationToken cancellationToken = default) =>
        projectRepository.GetProjectAsync(id, cancellationToken);

    public async Task<Project> ImportWorkspaceAsync(IStorageFolder folder, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(folder);

        var storageReference = await storageService.CreateStorageReferenceAsync(folder, cancellationToken);
        var analyzedProject = await AnalyzeFolderAsync(folder, storageReference.DisplayPath, cancellationToken);
        var existingProject = await projectRepository.GetProjectByStorageAsync(
            storageReference.DisplayPath,
            storageReference.BookmarkId,
            cancellationToken);

        analyzedProject.FolderBookmarkId = storageReference.BookmarkId;

        if (existingProject is not null)
        {
            analyzedProject.Id = existingProject.Id;
            analyzedProject.Metadata = new Dictionary<string, string>(existingProject.Metadata);
        }

        return await projectRepository.SaveProjectAsync(analyzedProject, cancellationToken);
    }

    public Task DeleteWorkspaceAsync(Guid id, CancellationToken cancellationToken = default) =>
        projectRepository.DeleteProjectAsync(id, cancellationToken);

    private static async Task<Project> AnalyzeFolderAsync(
        IStorageFolder folder,
        string displayPath,
        CancellationToken cancellationToken)
    {
        var detectedType = await DetectWorkstationTypeAsync(folder, cancellationToken);
        var project = new Project
        {
            Name = folder.Name,
            Path = displayPath,
            LastAccessed = DateTime.UtcNow,
            WorkstationType = detectedType,
        };

        project.SetMetadata("engine", detectedType.ToString());
        project.SetMetadata(
            "workflow",
            detectedType switch
            {
                WorkstationType.Unity => "editor-driven",
                WorkstationType.DotNet => "solution-based",
                WorkstationType.Unreal => "content-pipeline",
                _ => "custom"
            });

        return project;
    }

    private static async Task<WorkstationType> DetectWorkstationTypeAsync(
        IStorageFolder folder,
        CancellationToken cancellationToken)
    {
        var hasAssetsFolder = false;
        var hasProjectSettingsFolder = false;
        var hasContentFolder = false;
        var hasSolutionFile = false;

        await foreach (var item in folder.GetItemsAsync().WithCancellation(cancellationToken))
        {
            if (item is IStorageFolder)
            {
                hasAssetsFolder |= string.Equals(item.Name, "Assets", StringComparison.OrdinalIgnoreCase);
                hasProjectSettingsFolder |= string.Equals(item.Name, "ProjectSettings", StringComparison.OrdinalIgnoreCase);
                hasContentFolder |= string.Equals(item.Name, "Content", StringComparison.OrdinalIgnoreCase);
            }

            if (item is IStorageFile && item.Name.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
            {
                hasSolutionFile = true;
            }
        }

        if (hasAssetsFolder && hasProjectSettingsFolder)
        {
            return WorkstationType.Unity;
        }

        if (hasContentFolder)
        {
            return WorkstationType.Unreal;
        }

        return hasSolutionFile
            ? WorkstationType.DotNet
            : WorkstationType.Other;
    }
}
