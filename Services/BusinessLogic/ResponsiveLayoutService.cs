using System;
using Dragon_Workspaces.Core.Enums;
using Dragon_Workspaces.Core.Interfaces;

namespace Dragon_Workspaces.Services.BusinessLogic;

public sealed class ResponsiveLayoutService : IResponsiveLayoutService
{
    public WorkspaceLayoutMode GetLayoutMode(double width) =>
        OperatingSystem.IsAndroid()
            ? WorkspaceLayoutMode.MobileSingleColumn
            : WorkspaceLayoutMode.DesktopThreeColumn;
}
