using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dragon_Workspaces.Core.Models;

namespace Dragon_Workspaces.Core.Interfaces;

public interface IProjectRepository
{
    Task<IReadOnlyList<Project>> GetProjectsAsync(CancellationToken cancellationToken = default);
    Task<Project?> GetProjectAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Project?> GetProjectByStorageAsync(
        string path,
        string? bookmarkId,
        CancellationToken cancellationToken = default);
    Task<Project> SaveProjectAsync(Project project, CancellationToken cancellationToken = default);
    Task DeleteProjectAsync(Guid id, CancellationToken cancellationToken = default);
}
