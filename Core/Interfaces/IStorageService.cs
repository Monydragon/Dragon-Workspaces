using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Platform.Storage;
using Dragon_Workspaces.Core.Models;

namespace Dragon_Workspaces.Core.Interfaces;

public interface IStorageService
{
    Task<IStorageFolder?> PickFolderAsync(Visual visual, CancellationToken cancellationToken = default);
    Task<WorkspaceStorageReference> CreateStorageReferenceAsync(
        IStorageFolder folder,
        CancellationToken cancellationToken = default);
    Task<IStorageFolder?> OpenFolderAsync(
        Visual visual,
        Project project,
        CancellationToken cancellationToken = default);
}
