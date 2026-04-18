using System.CommandLine;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Commands;

/// <summary>
/// CLI command to show detailed information about a specific security setting.
/// </summary>
public static class DetailCommand
{
    public static Command Create(SecuritySettingsManager manager)
    {
        var command = new Command("detail", "Show detailed information about a specific security setting");

        var settingArg = new Argument<string>("setting-id", "The ID of the setting to show details for");

        command.AddArgument(settingArg);

        command.SetHandler((settingId) =>
        {
            var setting = manager.GetSettings()
                .FirstOrDefault(s => s.Id.Equals(settingId, StringComparison.OrdinalIgnoreCase));

            if (setting == null)
            {
                Console.WriteLine($"Setting '{settingId}' not found.");
                return;
            }

            var status = manager.GetSettingStatus(setting);

            Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║              Windows Security Manager - Setting Detail          ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine($"  ID:            {setting.Id}");
            Console.WriteLine($"  Name:          {setting.Name}");
            Console.WriteLine($"  Description:   {setting.Description}");
            Console.WriteLine($"  Category:      {setting.Category}");
            Console.WriteLine($"  Impact:        {ImpactFormatter.Format(setting.Impact)}");
            if (!string.IsNullOrWhiteSpace(setting.Consequences))
            {
                Console.WriteLine($"  Consequences:  {setting.Consequences}");
            }
            Console.WriteLine();
            Console.WriteLine($"  Registry:");
            Console.WriteLine($"    Hive:        {setting.RegistryHive}");
            Console.WriteLine($"    Path:        {setting.RegistryPath}");
            Console.WriteLine($"    Value Name:  {setting.ValueName}");
            Console.WriteLine($"    Value Type:  {setting.ValueType}");
            Console.WriteLine();
            Console.WriteLine($"  Values:");
            Console.WriteLine($"    Enabled:     {setting.EnabledValue}");
            Console.WriteLine($"    Disabled:    {setting.DisabledValue}");
            Console.WriteLine($"    Recommended: {setting.RecommendedValue}");
            Console.WriteLine();

            string statusIcon = status.IsEnabled ? "✓ ENABLED" : status.IsConfigured ? "✗ DISABLED" : "? MISSING";
            string currentVal = status.CurrentValue?.ToString() ?? "N/A (not configured)";
            Console.WriteLine($"  Current Status:");
            Console.WriteLine($"    State:          {statusIcon}");
            Console.WriteLine($"    Current Value:  {currentVal}");
            Console.WriteLine($"    Is Configured:  {status.IsConfigured}");
            Console.WriteLine($"    Matches Recommended: {status.MatchesRecommended}");
        }, settingArg);

        return command;
    }
}
