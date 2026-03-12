using System.Collections.Generic;
using System.Text.Json;
using Dragon_Workspaces.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Dragon_Workspaces.Data.Persistence;

public sealed class WorkspaceDbContext(DbContextOptions<WorkspaceDbContext> options) : DbContext(options)
{
    public DbSet<Project> Projects => Set<Project>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var metadataConverter = new ValueConverter<Dictionary<string, string>, string>(
            metadata => JsonSerializer.Serialize(metadata, WorkspaceJsonSerializerContext.Default.DictionaryStringString),
            metadataJson => DeserializeMetadata(metadataJson));

        var metadataComparer = new ValueComparer<Dictionary<string, string>>(
            (left, right) => JsonSerializer.Serialize(left, WorkspaceJsonSerializerContext.Default.DictionaryStringString)
                == JsonSerializer.Serialize(right, WorkspaceJsonSerializerContext.Default.DictionaryStringString),
            metadata => JsonSerializer.Serialize(metadata, WorkspaceJsonSerializerContext.Default.DictionaryStringString).GetHashCode(),
            metadata => new Dictionary<string, string>(metadata));

        var project = modelBuilder.Entity<Project>();

#if !BROWSER
        project.ToTable("Projects");
#endif
        project.HasKey(item => item.Id);
        project.HasIndex(item => item.Path);
        project.HasIndex(item => item.FolderBookmarkId).IsUnique();
        project.Property(item => item.Name).HasMaxLength(200).IsRequired();
        project.Property(item => item.Path).HasMaxLength(2048).IsRequired();
        project.Property(item => item.FolderBookmarkId).HasMaxLength(4096);
        project.Property(item => item.TypeIcon).HasMaxLength(16).IsRequired();
        project.Property(item => item.LastAccessed).IsRequired();
        project.Property(item => item.LastSynced).IsRequired();
        project.Property(item => item.WorkstationType)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        project.Property(item => item.Metadata)
            .HasConversion(metadataConverter)
            .Metadata.SetValueComparer(metadataComparer);
    }

    private static Dictionary<string, string> DeserializeMetadata(string metadataJson) =>
        JsonSerializer.Deserialize(
            metadataJson,
            WorkspaceJsonSerializerContext.Default.DictionaryStringString) ?? new Dictionary<string, string>();
}
