using System.CommandLine;
using WindowsSecurityManager.Models;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Commands;

/// <summary>
/// CLI command to list available security settings, with optional search filtering.
/// </summary>
public static class ListCommand
{
    public static Command Create(SecuritySettingsManager manager)
    {
        var command = new Command("list", "List available security settings");

        var categoryOption = new Option<SecurityCategory?>("--category", "List settings in a specific category only");
        var searchOption = new Option<string?>("--search", "Search settings by keyword across names, IDs, and descriptions");

        command.AddOption(categoryOption);
        command.AddOption(searchOption);

        command.SetHandler((category, search) =>
        {
            IReadOnlyList<SecuritySetting> settings;

            if (!string.IsNullOrWhiteSpace(search))
            {
                settings = manager.SearchSettings(search);
                if (category.HasValue)
                    settings = settings.Where(s => s.Category == category.Value).ToList();
            }
            else
            {
                settings = manager.GetSettings(category);
            }

            Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║           Windows Security Manager - Available Settings         ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            if (!string.IsNullOrWhiteSpace(search))
                Console.WriteLine($"  Search: \"{search}\"");

            var grouped = settings
                .GroupBy(s => s.Category)
                .OrderBy(g => g.Key.ToString());

            foreach (var group in grouped)
            {
                Console.WriteLine($"  [{group.Key}] ({group.Count()} settings)");
                Console.WriteLine($"  {new string('─', 64)}");

                foreach (var setting in group)
                {
                    Console.WriteLine($"  [{setting.Id}] {setting.Name}  ({ImpactFormatter.Format(setting.Impact)})");
                    Console.WriteLine($"    {setting.Description}");
                    if (!string.IsNullOrWhiteSpace(setting.Consequences))
                        Console.WriteLine($"    ⚠ Consequences: {setting.Consequences}");
                    Console.WriteLine($"    Registry: {setting.RegistryHive}\\{setting.RegistryPath}\\{setting.ValueName}");
                    Console.WriteLine($"    Type: {setting.ValueType} | Enable: {setting.EnabledValue} | Disable: {setting.DisabledValue}");
                    Console.WriteLine();
                }
            }

            Console.WriteLine($"  Total: {settings.Count} settings");
        }, categoryOption, searchOption);

        return command;
    }
}
