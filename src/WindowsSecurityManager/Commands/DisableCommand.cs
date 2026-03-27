using System.CommandLine;
using WindowsSecurityManager.Models;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Commands;

/// <summary>
/// CLI command to disable (unharden) security settings, with optional dry-run preview.
/// </summary>
public static class DisableCommand
{
    public static Command Create(SecuritySettingsManager manager)
    {
        var command = new Command("disable", "Disable (unharden) security settings");

        var settingOption = new Option<string?>("--setting", "The ID of a specific setting to disable");
        var categoryOption = new Option<SecurityCategory?>("--category", "Disable all settings in a category");
        var allOption = new Option<bool>("--all", "Disable all security settings");
        var dryRunOption = new Option<bool>("--dry-run", "Preview changes without writing to the registry");

        command.AddOption(settingOption);
        command.AddOption(categoryOption);
        command.AddOption(allOption);
        command.AddOption(dryRunOption);

        command.SetHandler((setting, category, all, dryRun) =>
        {
            IReadOnlyList<SecuritySetting> targets;

            if (all)
                targets = manager.GetSettings();
            else if (category.HasValue)
                targets = manager.GetSettings(category.Value);
            else if (!string.IsNullOrWhiteSpace(setting))
                targets = manager.GetSettings().Where(s => s.Id.Equals(setting, StringComparison.OrdinalIgnoreCase)).ToList();
            else
            {
                Console.WriteLine("Please specify --setting, --category, or --all.");
                return;
            }

            if (targets.Count == 0)
            {
                Console.WriteLine(setting != null ? $"Setting '{setting}' not found." : "No settings found.");
                return;
            }

            if (dryRun)
            {
                var changes = manager.DryRunDisable(targets);
                Console.WriteLine("DRY RUN — The following changes would be made:");
                Console.WriteLine();
                foreach (var change in changes)
                {
                    string current = change.IsCurrentlyConfigured ? change.CurrentValue?.ToString() ?? "N/A" : "NOT SET";
                    Console.WriteLine($"  [{change.Setting.Id}] {change.Setting.Name}");
                    Console.WriteLine($"    {change.Setting.RegistryHive}\\{change.Setting.RegistryPath}\\{change.Setting.ValueName}");
                    Console.WriteLine($"    Current: {current} → New: {change.NewValue}");
                    Console.WriteLine();
                }
                Console.WriteLine($"Total: {changes.Count} settings would be changed.");
            }
            else
            {
                if (all)
                {
                    int count = manager.DisableAll();
                    Console.WriteLine($"Disabled {count} security settings.");
                }
                else if (category.HasValue)
                {
                    int count = manager.DisableCategory(category.Value);
                    Console.WriteLine($"Disabled {count} settings in category '{category.Value}'.");
                }
                else
                {
                    bool success = manager.DisableSetting(setting!);
                    if (success)
                        Console.WriteLine($"Setting '{setting}' disabled successfully.");
                    else
                        Console.WriteLine($"Setting '{setting}' not found.");
                }
            }
        }, settingOption, categoryOption, allOption, dryRunOption);

        return command;
    }
}
