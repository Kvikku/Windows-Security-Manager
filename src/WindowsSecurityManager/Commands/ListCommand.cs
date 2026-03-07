using System.CommandLine;
using WindowsSecurityManager.Models;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Commands;

/// <summary>
/// CLI command to list available security settings.
/// </summary>
public static class ListCommand
{
    public static Command Create(SecuritySettingsManager manager)
    {
        var command = new Command("list", "List available security settings");

        var categoryOption = new Option<SecurityCategory?>("--category", "List settings in a specific category only");

        command.AddOption(categoryOption);

        command.SetHandler((category) =>
        {
            var settings = manager.GetSettings(category);

            Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║           Windows Security Manager - Available Settings         ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            var grouped = settings
                .GroupBy(s => s.Category)
                .OrderBy(g => g.Key.ToString());

            foreach (var group in grouped)
            {
                Console.WriteLine($"  [{group.Key}] ({group.Count()} settings)");
                Console.WriteLine($"  {new string('─', 64)}");

                foreach (var setting in group)
                {
                    Console.WriteLine($"  [{setting.Id}] {setting.Name}");
                    Console.WriteLine($"    {setting.Description}");
                    Console.WriteLine($"    Registry: {setting.RegistryHive}\\{setting.RegistryPath}\\{setting.ValueName}");
                    Console.WriteLine($"    Type: {setting.ValueType} | Enable: {setting.EnabledValue} | Disable: {setting.DisabledValue}");
                    Console.WriteLine();
                }
            }

            Console.WriteLine($"  Total: {settings.Count} settings");
        }, categoryOption);

        return command;
    }
}
