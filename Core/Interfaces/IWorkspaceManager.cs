using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Dragon_Workspaces.Core.Models;

namespace Dragon_Workspaces.Core.Interfaces;

public interface IWorkspaceManager
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Project>> GetWorkspacesAsync(CancellationToken cancellationToken = default);
    Task<Project?> GetWorkspaceAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Project> ImportWorkspaceAsync(IStorageFolder folder, CancellationToken cancellationToken = default);
    Task DeleteWorkspaceAsync(Guid id, CancellationToken cancellationToken = default);
}
