using System.Text;
using WindowsSecurityManager.Models;

namespace WindowsSecurityManager.Services;

/// <summary>
/// Writes timestamped audit log entries to a file.
/// </summary>
public class AuditLogger
{
    private readonly string _logFilePath;

    public AuditLogger(string logFilePath)
    {
        _logFilePath = logFilePath ?? throw new ArgumentNullException(nameof(logFilePath));
    }

    /// <summary>
    /// Logs a single action.
    /// </summary>
    public void Log(string action, string target, string? details = null)
    {
        var entry = new AuditLogEntry
        {
            Timestamp = DateTime.UtcNow,
            Action = action,
            Target = target,
            Details = details
        };

        var line = FormatEntry(entry);

        var dir = Path.GetDirectoryName(_logFilePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        File.AppendAllText(_logFilePath, line + Environment.NewLine, Encoding.UTF8);
    }

    /// <summary>
    /// Reads all log entries from the log file.
    /// </summary>
    public IReadOnlyList<AuditLogEntry> ReadAll()
    {
        if (!File.Exists(_logFilePath))
            return Array.Empty<AuditLogEntry>();

        return File.ReadAllLines(_logFilePath)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(ParseEntry)
            .Where(e => e != null)
            .Select(e => e!)
            .ToList();
    }

    private static string FormatEntry(AuditLogEntry entry)
    {
        var details = entry.Details != null ? $" | {entry.Details}" : "";
        return $"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss}] {entry.Action}: {entry.Target}{details}";
    }

    private static AuditLogEntry? ParseEntry(string line)
    {
        try
        {
            // Format: [2024-01-01 12:00:00] Action: Target | Details
            var timestampEnd = line.IndexOf(']');
            if (timestampEnd < 0) return null;

            var timestampStr = line.Substring(1, timestampEnd - 1);
            var rest = line.Substring(timestampEnd + 2);

            var colonIdx = rest.IndexOf(": ", StringComparison.Ordinal);
            if (colonIdx < 0) return null;

            var action = rest.Substring(0, colonIdx);
            var targetAndDetails = rest.Substring(colonIdx + 2);

            string target;
            string? details = null;
            var pipeIdx = targetAndDetails.IndexOf(" | ", StringComparison.Ordinal);
            if (pipeIdx >= 0)
            {
                target = targetAndDetails.Substring(0, pipeIdx);
                details = targetAndDetails.Substring(pipeIdx + 3);
            }
            else
            {
                target = targetAndDetails;
            }

            return new AuditLogEntry
            {
                Timestamp = DateTime.Parse(timestampStr),
                Action = action,
                Target = target,
                Details = details
            };
        }
        catch
        {
            return null;
        }
    }
}
