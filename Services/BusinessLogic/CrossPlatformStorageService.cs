using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Dragon_Workspaces.Core.Interfaces;
using Dragon_Workspaces.Core.Models;

namespace Dragon_Workspaces.Services.BusinessLogic;

public sealed class CrossPlatformStorageService : IStorageService
{
    public async Task<IStorageFolder?> PickFolderAsync(Visual visual, CancellationToken cancellationToken = default)
    {
        var storageProvider = GetStorageProvider(visual);
        var folders = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Import Workspace",
            AllowMultiple = false
        });

        cancellationToken.ThrowIfCancellationRequested();
        return folders.FirstOrDefault();
    }

    public async Task<WorkspaceStorageReference> CreateStorageReferenceAsync(
        IStorageFolder folder,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var displayPath = folder.Path.IsFile
            ? folder.Path.LocalPath
            : folder.Path.ToString();

        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
        {
            return new WorkspaceStorageReference(displayPath, null);
        }

        if (folder is IStorageBookmarkItem bookmarkItem && bookmarkItem.CanBookmark)
        {
            return new WorkspaceStorageReference(displayPath, await bookmarkItem.SaveBookmarkAsync());
        }

        throw new InvalidOperationException(
            "This platform requires bookmark support to persist workspace access across sessions.");
    }

    public async Task<IStorageFolder?> OpenFolderAsync(
        Visual visual,
        Project project,
        CancellationToken cancellationToken = default)
    {
        var storageProvider = GetStorageProvider(visual);

        if (!string.IsNullOrWhiteSpace(project.FolderBookmarkId))
        {
            return await storageProvider.OpenFolderBookmarkAsync(project.FolderBookmarkId);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return await storageProvider.TryGetFolderFromPathAsync(project.Path);
    }

    private static IStorageProvider GetStorageProvider(Visual visual) =>
        TopLevel.GetTopLevel(visual)?.StorageProvider
        ?? throw new InvalidOperationException("A storage provider is not available for this visual.");
}
