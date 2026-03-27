using System.CommandLine;
using WindowsSecurityManager.Models;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Commands;

/// <summary>
/// CLI command to back up current registry values for security settings.
/// </summary>
public static class BackupCommand
{
    public static Command Create(SecuritySettingsManager manager, IRegistryService registryService, AuditLogger? auditLogger)
    {
        var command = new Command("backup", "Back up current registry values for security settings");

        var outputOption = new Option<string>("--output", () => $"wsm-backup-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json",
            "File path for the backup file");
        var categoryOption = new Option<SecurityCategory?>("--category", "Back up only a specific category");

        command.AddOption(outputOption);
        command.AddOption(categoryOption);

        command.SetHandler((output, category) =>
        {
            var backupService = new BackupService(manager, registryService);
            var backup = backupService.CreateBackup(category);
            backupService.SaveToFile(backup, output);

            Console.WriteLine($"Backup saved to '{output}' ({backup.Entries.Count} settings).");

            auditLogger?.Log("Backup", output, $"Backed up {backup.Entries.Count} settings");
        }, outputOption, categoryOption);

        return command;
    }
}
