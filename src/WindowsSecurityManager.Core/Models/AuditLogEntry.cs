namespace WindowsSecurityManager.Models;

/// <summary>
/// Represents a single audit log entry for a setting change.
/// </summary>
public class AuditLogEntry
{
    /// <summary>
    /// Timestamp of the action.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// The action performed (e.g., "Enable", "Disable", "Backup", "Restore").
    /// </summary>
    public required string Action { get; init; }

    /// <summary>
    /// The setting ID affected, or a description for bulk actions.
    /// </summary>
    public required string Target { get; init; }

    /// <summary>
    /// Additional details about the action.
    /// </summary>
    public string? Details { get; init; }
}
