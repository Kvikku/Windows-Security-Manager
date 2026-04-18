namespace WindowsSecurityManager.Models;

/// <summary>
/// Indicates the likely compatibility / usability impact of enabling a security setting.
/// Used to help users judge risk before applying changes. Labels are intentionally
/// kept consistent with <c>docs/security-setting-consequences.md</c>.
/// </summary>
public enum ImpactLevel
{
    /// <summary>
    /// Unknown / not classified. Treated like Low for display purposes but indicates
    /// the setting has not been categorised yet.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// 🟢 Low — Safe defaults for almost any environment. Disruption is unlikely.
    /// </summary>
    Low = 1,

    /// <summary>
    /// 🟡 Medium — Generally safe, but may surface compatibility issues with older
    /// software, legacy network services, peripherals, or non-standard workflows.
    /// </summary>
    Medium = 2,

    /// <summary>
    /// 🔴 High — Likely to break specific workflows or third-party integrations
    /// (legacy SMB/NTLM clients, custom RDP setups, scripts, drivers).
    /// Apply only after deliberate testing.
    /// </summary>
    High = 3
}
