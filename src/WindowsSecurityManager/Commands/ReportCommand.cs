using System.CommandLine;
using WindowsSecurityManager.Models;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Commands;

/// <summary>
/// CLI command to generate a compliance report on security settings.
/// </summary>
public static class ReportCommand
{
    public static Command Create(SecuritySettingsManager manager)
    {
        var command = new Command("report", "Generate a compliance report on security settings");

        var categoryOption = new Option<SecurityCategory?>("--category", "Report on a specific category only");

        command.AddOption(categoryOption);

        command.SetHandler((category) =>
        {
            var report = manager.GenerateReport(category);

            Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║           Windows Security Manager - Compliance Report          ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine($"  Generated: {report.GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC");
            if (category.HasValue)
                Console.WriteLine($"  Category:  {category.Value}");
            Console.WriteLine();

            // Summary
            Console.WriteLine("  ┌──────────────────────────────────────────────────────────┐");
            Console.WriteLine($"  │ Total Settings:     {report.TotalSettings,-38}│");
            Console.WriteLine($"  │ Enabled (Hardened):  {report.EnabledCount,-37}│");
            Console.WriteLine($"  │ Disabled:            {report.DisabledCount,-37}│");
            Console.WriteLine($"  │ Not Configured:      {report.NotConfiguredCount,-37}│");
            Console.WriteLine($"  │ Matches Recommended: {report.MatchesRecommendedCount,-37}│");
            Console.WriteLine($"  │ Compliance:          {report.CompliancePercentage,5:F1}%{new string(' ', 31)}│");
            Console.WriteLine("  └──────────────────────────────────────────────────────────┘");
            Console.WriteLine();

            // Group by category
            var grouped = report.Settings
                .GroupBy(s => s.Setting.Category)
                .OrderBy(g => g.Key.ToString());

            foreach (var group in grouped)
            {
                int groupEnabled = group.Count(s => s.IsEnabled);
                int groupTotal = group.Count();
                Console.WriteLine($"  [{group.Key}] ({groupEnabled}/{groupTotal} enabled)");
                Console.WriteLine($"  {new string('─', 64)}");

                foreach (var status in group)
                {
                    string icon = status.IsEnabled ? "✓" : status.IsConfigured ? "✗" : "?";
                    string state = status.IsEnabled ? "ENABLED " : status.IsConfigured ? "DISABLED" : "MISSING ";
                    string valueStr = status.CurrentValue?.ToString() ?? "N/A";

                    Console.WriteLine($"  {icon} [{status.Setting.Id}] {status.Setting.Name}");
                    Console.WriteLine($"    Status: {state} | Current: {valueStr} | Expected: {status.Setting.EnabledValue}");
                }
                Console.WriteLine();
            }
        }, categoryOption);

        return command;
    }
}
