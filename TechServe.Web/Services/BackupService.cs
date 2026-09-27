namespace TechServe.Web.Services;

public sealed class BackupService
{
    public Task<string> GetLastBackupLabelAsync()
    {
        return Task.FromResult("Last backup restored from legacy WinForms backup flow.");
    }
}
