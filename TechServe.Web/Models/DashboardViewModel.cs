namespace TechServe.Web.Models;

public sealed class DashboardViewModel
{
    public string ApplicationName { get; init; } = "TechServe";
    public string WorkspaceName { get; set; } = "TechServe HQ";
    public string CurrentRole { get; set; } = "ADMIN";
    public string DisplayName { get; set; } = "User";
}
