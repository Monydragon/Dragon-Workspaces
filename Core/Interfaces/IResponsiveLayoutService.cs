using Dragon_Workspaces.Core.Enums;

namespace Dragon_Workspaces.Core.Interfaces;

public interface IResponsiveLayoutService
{
    WorkspaceLayoutMode GetLayoutMode(double width);
}
