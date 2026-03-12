using System;
using System.Collections.Generic;
using Dragon_Workspaces.Core.Enums;

namespace Dragon_Workspaces.Core.Models;

public sealed class Project
{
    public Guid Id
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            Touch();
        }
    } = Guid.NewGuid();

    public string Name
    {
        get;
        set
        {
            var normalizedValue = string.IsNullOrWhiteSpace(value)
                ? throw new ArgumentException("Project name cannot be empty.", nameof(value))
                : value.Trim();

            if (field == normalizedValue)
            {
                return;
            }

            field = normalizedValue;
            Touch();
        }
    } = "New Workspace";

    public string Path
    {
        get;
        set
        {
            var normalizedValue = string.IsNullOrWhiteSpace(value)
                ? throw new ArgumentException("Project path cannot be empty.", nameof(value))
                : value.Trim();

            if (field == normalizedValue)
            {
                return;
            }

            field = normalizedValue;
            Touch();
        }
    } = ".";

    public string? FolderBookmarkId
    {
        get;
        set
        {
            var normalizedValue = string.IsNullOrWhiteSpace(value) ? null : value.Trim();

            if (field == normalizedValue)
            {
                return;
            }

            field = normalizedValue;
            Touch();
        }
    }

    public Dictionary<string, string> Metadata
    {
        get;
        set
        {
            field = value ?? [];
            Touch();
        }
    } = [];

    public DateTime LastAccessed
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = NormalizeUtc(value);
            Touch();
        }
    } = DateTime.UtcNow;

    public DateTime LastSynced
    {
        get;
        private set => field = NormalizeUtc(value);
    } = DateTime.UtcNow;

    public WorkstationType WorkstationType
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            TypeIcon = value switch
            {
                WorkstationType.Unity => "U",
                WorkstationType.DotNet => ".NET",
                WorkstationType.Unreal => "UE",
                _ => "GEN"
            };

            Touch();
        }
    } = WorkstationType.Other;

    public string TypeIcon
    {
        get;
        private set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            Touch();
        }
    } = "GEN";

    public void SetMetadata(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Metadata key cannot be empty.", nameof(key));
        }

        Metadata[key.Trim()] = value.Trim();
        Touch();
    }

    private static DateTime NormalizeUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);

    private void Touch()
    {
        LastSynced = DateTime.UtcNow;
    }
}
