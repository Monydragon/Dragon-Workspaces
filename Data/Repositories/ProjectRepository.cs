using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dragon_Workspaces.Core.Interfaces;
using Dragon_Workspaces.Core.Models;
using Dragon_Workspaces.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Dragon_Workspaces.Data.Repositories;

public sealed class ProjectRepository(IDbContextFactory<WorkspaceDbContext> dbContextFactory) : IProjectRepository
{
    public async Task EnsureCreatedAsync(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await WorkspaceDbSchemaUpgrader.EnsureCompatibleSchemaAsync(dbContext, cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> GetProjectsAsync(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.Projects
            .AsNoTracking()
            .OrderByDescending(project => project.LastAccessed)
            .ThenBy(project => project.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Project?> GetProjectAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(project => project.Id == id, cancellationToken);
    }

    public async Task<Project?> GetProjectByStorageAsync(
        string path,
        string? bookmarkId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(bookmarkId))
        {
            return await dbContext.Projects
                .FirstOrDefaultAsync(project => project.FolderBookmarkId == bookmarkId, cancellationToken);
        }

        return await dbContext.Projects
            .FirstOrDefaultAsync(project => project.Path == path, cancellationToken);
    }

    public async Task<Project> SaveProjectAsync(Project project, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var existingProject = await dbContext.Projects
            .FirstOrDefaultAsync(item => item.Id == project.Id, cancellationToken);

        if (existingProject is null)
        {
            if (project.Id == Guid.Empty)
            {
                project.Id = Guid.NewGuid();
            }

            dbContext.Projects.Add(project);
            await dbContext.SaveChangesAsync(cancellationToken);
            return project;
        }

        existingProject.Name = project.Name;
        existingProject.Path = project.Path;
        existingProject.FolderBookmarkId = project.FolderBookmarkId;
        existingProject.LastAccessed = project.LastAccessed;
        existingProject.WorkstationType = project.WorkstationType;
        existingProject.Metadata = new Dictionary<string, string>(project.Metadata);

        await dbContext.SaveChangesAsync(cancellationToken);
        return existingProject;
    }

    public async Task DeleteProjectAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var existingProject = await dbContext.Projects
            .FirstOrDefaultAsync(project => project.Id == id, cancellationToken);

        if (existingProject is null)
        {
            return;
        }

        dbContext.Projects.Remove(existingProject);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
