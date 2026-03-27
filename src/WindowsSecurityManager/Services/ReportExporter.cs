using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using WindowsSecurityManager.Models;

namespace WindowsSecurityManager.Services;

/// <summary>
/// Exports compliance reports to JSON, CSV, or HTML format.
/// </summary>
public class ReportExporter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Exports a report to the specified format and returns the content as a string.
    /// </summary>
    public string Export(SecurityReport report, ExportFormat format)
    {
        return format switch
        {
            ExportFormat.Json => ExportJson(report),
            ExportFormat.Csv => ExportCsv(report),
            ExportFormat.Html => ExportHtml(report),
            _ => throw new ArgumentException($"Unknown export format: {format}")
        };
    }

    /// <summary>
    /// Exports a report to a file.
    /// </summary>
    public void ExportToFile(SecurityReport report, ExportFormat format, string filePath)
    {
        var content = Export(report, format);
        File.WriteAllText(filePath, content, Encoding.UTF8);
    }

    private static string ExportJson(SecurityReport report)
    {
        var exportData = new
        {
            report.GeneratedAt,
            report.TotalSettings,
            report.EnabledCount,
            report.DisabledCount,
            report.NotConfiguredCount,
            report.MatchesRecommendedCount,
            report.CompliancePercentage,
            Settings = report.Settings.Select(s => new
            {
                s.Setting.Id,
                s.Setting.Name,
                s.Setting.Description,
                Category = s.Setting.Category.ToString(),
                Registry = $@"{s.Setting.RegistryHive}\{s.Setting.RegistryPath}\{s.Setting.ValueName}",
                ValueType = s.Setting.ValueType.ToString(),
                CurrentValue = s.CurrentValue?.ToString(),
                ExpectedValue = s.Setting.EnabledValue.ToString(),
                s.IsEnabled,
                s.IsConfigured,
                s.MatchesRecommended
            })
        };

        return JsonSerializer.Serialize(exportData, JsonOptions);
    }

    private static string ExportCsv(SecurityReport report)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Id,Name,Category,Status,CurrentValue,ExpectedValue,IsEnabled,IsConfigured,MatchesRecommended,RegistryPath");

        foreach (var s in report.Settings)
        {
            string status = s.IsEnabled ? "Enabled" : s.IsConfigured ? "Disabled" : "Missing";
            string currentVal = EscapeCsv(s.CurrentValue?.ToString() ?? "N/A");
            string expectedVal = EscapeCsv(s.Setting.EnabledValue.ToString() ?? "");
            string name = EscapeCsv(s.Setting.Name);
            string regPath = EscapeCsv($@"{s.Setting.RegistryHive}\{s.Setting.RegistryPath}\{s.Setting.ValueName}");

            sb.AppendLine($"{s.Setting.Id},{name},{s.Setting.Category},{status},{currentVal},{expectedVal},{s.IsEnabled},{s.IsConfigured},{s.MatchesRecommended},{regPath}");
        }

        return sb.ToString();
    }

    private static string ExportHtml(SecurityReport report)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\"><head>");
        sb.AppendLine("<meta charset=\"UTF-8\">");
        sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        sb.AppendLine("<title>Windows Security Manager - Compliance Report</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("body { font-family: 'Segoe UI', Tahoma, sans-serif; margin: 2rem; background: #f5f5f5; }");
        sb.AppendLine("h1 { color: #0078d4; }");
        sb.AppendLine(".summary { display: flex; gap: 1rem; margin: 1rem 0; flex-wrap: wrap; }");
        sb.AppendLine(".card { background: white; padding: 1rem 1.5rem; border-radius: 8px; box-shadow: 0 1px 3px rgba(0,0,0,0.1); min-width: 150px; }");
        sb.AppendLine(".card h3 { margin: 0 0 0.5rem 0; color: #666; font-size: 0.9rem; }");
        sb.AppendLine(".card .value { font-size: 1.8rem; font-weight: bold; }");
        sb.AppendLine(".green { color: #107c10; } .red { color: #d13438; } .yellow { color: #ca5010; }");
        sb.AppendLine("table { width: 100%; border-collapse: collapse; background: white; border-radius: 8px; overflow: hidden; box-shadow: 0 1px 3px rgba(0,0,0,0.1); margin-top: 1rem; }");
        sb.AppendLine("th { background: #0078d4; color: white; padding: 0.75rem; text-align: left; }");
        sb.AppendLine("td { padding: 0.6rem 0.75rem; border-bottom: 1px solid #eee; }");
        sb.AppendLine("tr:hover { background: #f0f6ff; }");
        sb.AppendLine(".status-enabled { color: #107c10; font-weight: bold; }");
        sb.AppendLine(".status-disabled { color: #ca5010; font-weight: bold; }");
        sb.AppendLine(".status-missing { color: #d13438; font-weight: bold; }");
        sb.AppendLine("</style></head><body>");
        sb.AppendLine("<h1>🛡️ Windows Security Manager — Compliance Report</h1>");
        sb.AppendLine($"<p>Generated: {report.GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC</p>");

        // Summary cards
        var compClass = report.CompliancePercentage >= 80 ? "green" : report.CompliancePercentage >= 50 ? "yellow" : "red";
        sb.AppendLine("<div class=\"summary\">");
        sb.AppendLine($"<div class=\"card\"><h3>Total Settings</h3><div class=\"value\">{report.TotalSettings}</div></div>");
        sb.AppendLine($"<div class=\"card\"><h3>Hardened</h3><div class=\"value green\">{report.EnabledCount}</div></div>");
        sb.AppendLine($"<div class=\"card\"><h3>Misconfigured</h3><div class=\"value yellow\">{report.DisabledCount - report.NotConfiguredCount}</div></div>");
        sb.AppendLine($"<div class=\"card\"><h3>Missing</h3><div class=\"value red\">{report.NotConfiguredCount}</div></div>");
        sb.AppendLine($"<div class=\"card\"><h3>Compliance</h3><div class=\"value {compClass}\">{report.CompliancePercentage:F1}%</div></div>");
        sb.AppendLine("</div>");

        // Settings table
        sb.AppendLine("<table><thead><tr>");
        sb.AppendLine("<th>Status</th><th>ID</th><th>Name</th><th>Category</th><th>Current</th><th>Expected</th>");
        sb.AppendLine("</tr></thead><tbody>");

        foreach (var s in report.Settings)
        {
            string statusClass = s.IsEnabled ? "status-enabled" : s.IsConfigured ? "status-disabled" : "status-missing";
            string statusText = s.IsEnabled ? "✓ Enabled" : s.IsConfigured ? "✗ Disabled" : "— Missing";
            string currentVal = HtmlEncode(s.CurrentValue?.ToString() ?? "N/A");
            string expectedVal = HtmlEncode(s.Setting.EnabledValue.ToString() ?? "");

            sb.AppendLine($"<tr><td class=\"{statusClass}\">{statusText}</td>");
            sb.AppendLine($"<td>{HtmlEncode(s.Setting.Id)}</td>");
            sb.AppendLine($"<td>{HtmlEncode(s.Setting.Name)}</td>");
            sb.AppendLine($"<td>{s.Setting.Category}</td>");
            sb.AppendLine($"<td>{currentVal}</td><td>{expectedVal}</td></tr>");
        }

        sb.AppendLine("</tbody></table>");
        sb.AppendLine("</body></html>");

        return sb.ToString();
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }

    private static string HtmlEncode(string value)
    {
        return value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
    }
}
