using System.CommandLine;
using WindowsSecurityManager.Models;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Commands;

/// <summary>
/// CLI command to enable (harden) security settings.
/// </summary>
public static class EnableCommand
{
    public static Command Create(SecuritySettingsManager manager)
    {
        var command = new Command("enable", "Enable (harden) security settings");

        var settingOption = new Option<string?>("--setting", "The ID of a specific setting to enable");
        var categoryOption = new Option<SecurityCategory?>("--category", "Enable all settings in a category");
        var allOption = new Option<bool>("--all", "Enable all security settings");

        command.AddOption(settingOption);
        command.AddOption(categoryOption);
        command.AddOption(allOption);

        command.SetHandler((setting, category, all) =>
        {
            if (all)
            {
                int count = manager.EnableAll();
                Console.WriteLine($"Enabled {count} security settings.");
            }
            else if (category.HasValue)
            {
                int count = manager.EnableCategory(category.Value);
                Console.WriteLine($"Enabled {count} settings in category '{category.Value}'.");
            }
            else if (!string.IsNullOrWhiteSpace(setting))
            {
                bool success = manager.EnableSetting(setting);
                if (success)
                    Console.WriteLine($"Setting '{setting}' enabled successfully.");
                else
                    Console.WriteLine($"Setting '{setting}' not found.");
            }
            else
            {
                Console.WriteLine("Please specify --setting, --category, or --all.");
            }
        }, settingOption, categoryOption, allOption);

        return command;
    }
}
