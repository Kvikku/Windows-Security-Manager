using System.CommandLine;
using WindowsSecurityManager.Models;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Commands;

/// <summary>
/// CLI command to disable (unharden) security settings.
/// </summary>
public static class DisableCommand
{
    public static Command Create(SecuritySettingsManager manager)
    {
        var command = new Command("disable", "Disable (unharden) security settings");

        var settingOption = new Option<string?>("--setting", "The ID of a specific setting to disable");
        var categoryOption = new Option<SecurityCategory?>("--category", "Disable all settings in a category");
        var allOption = new Option<bool>("--all", "Disable all security settings");

        command.AddOption(settingOption);
        command.AddOption(categoryOption);
        command.AddOption(allOption);

        command.SetHandler((setting, category, all) =>
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
            else if (!string.IsNullOrWhiteSpace(setting))
            {
                bool success = manager.DisableSetting(setting);
                if (success)
                    Console.WriteLine($"Setting '{setting}' disabled successfully.");
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
