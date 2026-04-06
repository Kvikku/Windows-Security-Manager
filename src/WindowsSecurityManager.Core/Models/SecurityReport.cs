namespace WindowsSecurityManager.Models;

/// <summary>
/// Represents a security compliance report.
/// </summary>
public class SecurityReport
{
    /// <summary>
    /// The date and time this report was generated.
    /// </summary>
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// All setting statuses in this report.
    /// </summary>
    public required IReadOnlyList<SettingStatus> Settings { get; init; }

    /// <summary>
    /// Total number of settings evaluated.
    /// </summary>
    public int TotalSettings => Settings.Count;

    /// <summary>
    /// Number of settings that are enabled (hardened).
    /// </summary>
    public int EnabledCount => Settings.Count(s => s.IsEnabled);

    /// <summary>
    /// Number of settings that are not enabled.
    /// </summary>
    public int DisabledCount => Settings.Count(s => !s.IsEnabled);

    /// <summary>
    /// Number of settings that are not configured (missing from registry).
    /// </summary>
    public int NotConfiguredCount => Settings.Count(s => !s.IsConfigured);

    /// <summary>
    /// Number of settings matching their recommended value.
    /// </summary>
    public int MatchesRecommendedCount => Settings.Count(s => s.MatchesRecommended);

    /// <summary>
    /// Compliance percentage (enabled / total * 100).
    /// </summary>
    public double CompliancePercentage =>
        TotalSettings > 0 ? Math.Round((double)EnabledCount / TotalSettings * 100, 1) : 0;
}
