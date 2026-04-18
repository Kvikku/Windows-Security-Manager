using WindowsSecurityManager.Models;

namespace WindowsSecurityManager.Commands;

/// <summary>
/// Helpers for formatting <see cref="ImpactLevel"/> for plain-text CLI output.
/// </summary>
internal static class ImpactFormatter
{
    /// <summary>
    /// Returns a short label suitable for plain-text CLI output, e.g. "🟢 Low".
    /// </summary>
    public static string Format(ImpactLevel level) => level switch
    {
        ImpactLevel.Low => "🟢 Low",
        ImpactLevel.Medium => "🟡 Medium",
        ImpactLevel.High => "🔴 High",
        _ => "⚪ Unknown"
    };
}
