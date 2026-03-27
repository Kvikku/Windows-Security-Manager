using System.CommandLine;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Commands;

/// <summary>
/// CLI command to restore registry values from a backup file.
/// </summary>
public static class RestoreCommand
{
    public static Command Create(SecuritySettingsManager manager, IRegistryService registryService, AuditLogger? auditLogger)
    {
        var command = new Command("restore", "Restore registry values from a backup file");

        var inputArg = new Argument<string>("backup-file", "Path to the backup JSON file");

        command.AddArgument(inputArg);

        command.SetHandler((input) =>
        {
            var backupService = new BackupService(manager, registryService);

            try
            {
                int count = backupService.RestoreFromFile(input);
                Console.WriteLine($"Restored {count} settings from '{input}'.");

                auditLogger?.Log("Restore", input, $"Restored {count} settings");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"Backup file not found: {input}");
            }
        }, inputArg);

        return command;
    }
}
