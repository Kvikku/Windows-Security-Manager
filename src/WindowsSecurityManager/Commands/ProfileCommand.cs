using System.CommandLine;
using WindowsSecurityManager.Definitions;
using WindowsSecurityManager.Models;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Commands;

/// <summary>
/// CLI command to list and apply security profiles (presets).
/// </summary>
public static class ProfileCommand
{
    public static Command Create(SecuritySettingsManager manager)
    {
        var command = new Command("profile", "List or apply security profiles (presets)");

        var listOption = new Option<bool>("--list", "List all available profiles");
        var applyOption = new Option<string?>("--apply", "Apply a profile by name");
        var dryRunOption = new Option<bool>("--dry-run", "Preview profile changes without writing to the registry");

        command.AddOption(listOption);
        command.AddOption(applyOption);
        command.AddOption(dryRunOption);

        command.SetHandler((list, apply, dryRun) =>
        {
            var profiles = SecurityProfiles.GetProfiles();

            if (list || string.IsNullOrWhiteSpace(apply))
            {
                Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║           Windows Security Manager - Security Profiles          ║");
                Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
                Console.WriteLine();

                foreach (var profile in profiles)
                {
                    var profileSettings = manager.GetSettings()
                        .Where(s => profile.SettingIds.Contains(s.Id, StringComparer.OrdinalIgnoreCase))
                        .ToList();
                    int high = profileSettings.Count(s => s.Impact == ImpactLevel.High);
                    int medium = profileSettings.Count(s => s.Impact == ImpactLevel.Medium);
                    int low = profileSettings.Count(s => s.Impact == ImpactLevel.Low);

                    Console.WriteLine($"  [{profile.Name}] ({profile.SettingIds.Count} settings)");
                    Console.WriteLine($"    {profile.Description}");
                    Console.WriteLine($"    Impact: 🔴 {high} High | 🟡 {medium} Medium | 🟢 {low} Low");
                    Console.WriteLine();
                }
                Console.WriteLine("  Tip: run 'profile --apply <Name> --dry-run' to preview changes (with consequences) before applying.");
                return;
            }

            var selected = profiles.FirstOrDefault(p =>
                p.Name.Equals(apply, StringComparison.OrdinalIgnoreCase));

            if (selected == null)
            {
                Console.WriteLine($"Profile '{apply}' not found. Use --list to see available profiles.");
                return;
            }

            if (dryRun)
            {
                var settings = manager.GetSettings()
                    .Where(s => selected.SettingIds.Contains(s.Id, StringComparer.OrdinalIgnoreCase))
                    .ToList();

                var changes = manager.DryRunEnable(settings);
                Console.WriteLine($"DRY RUN — Profile '{selected.Name}' would make these changes:");
                Console.WriteLine();
                foreach (var change in changes)
                {
                    string current = change.IsCurrentlyConfigured ? change.CurrentValue?.ToString() ?? "N/A" : "NOT SET";
                    Console.WriteLine($"  [{change.Setting.Id}] {change.Setting.Name}  ({ImpactFormatter.Format(change.Setting.Impact)})");
                    Console.WriteLine($"    Current: {current} → New: {change.NewValue}");
                    if (!string.IsNullOrWhiteSpace(change.Setting.Consequences))
                        Console.WriteLine($"    ⚠ {change.Setting.Consequences}");
                }
                Console.WriteLine();
                Console.WriteLine($"Total: {changes.Count} settings would be changed.");

                int high = settings.Count(s => s.Impact == ImpactLevel.High);
                int medium = settings.Count(s => s.Impact == ImpactLevel.Medium);
                int low = settings.Count(s => s.Impact == ImpactLevel.Low);
                Console.WriteLine($"Impact summary: 🔴 {high} High | 🟡 {medium} Medium | 🟢 {low} Low");
                if (high > 0 || medium > 0)
                {
                    Console.WriteLine("⚠  Profile contains Medium/High-impact settings. Back up first; see docs/security-setting-consequences.md.");
                }
            }
            else
            {
                int count = manager.EnableSettings(selected.SettingIds);
                Console.WriteLine($"Applied profile '{selected.Name}': enabled {count} settings.");

                var settings = manager.GetSettings()
                    .Where(s => selected.SettingIds.Contains(s.Id, StringComparer.OrdinalIgnoreCase))
                    .ToList();
                int high = settings.Count(s => s.Impact == ImpactLevel.High);
                int medium = settings.Count(s => s.Impact == ImpactLevel.Medium);
                int low = settings.Count(s => s.Impact == ImpactLevel.Low);
                if (high > 0 || medium > 0)
                {
                    Console.WriteLine($"Impact: 🔴 {high} High | 🟡 {medium} Medium | 🟢 {low} Low settings were enabled.");
                    Console.WriteLine("If something stops working, restore from your most recent backup. See docs/security-setting-consequences.md.");
                }
            }
        }, listOption, applyOption, dryRunOption);

        return command;
    }
}
