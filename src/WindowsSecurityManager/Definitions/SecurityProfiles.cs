using WindowsSecurityManager.Models;

namespace WindowsSecurityManager.Definitions;

/// <summary>
/// Provides built-in security profiles (presets) for common hardening scenarios.
/// </summary>
public static class SecurityProfiles
{
    /// <summary>
    /// Returns all available security profiles.
    /// </summary>
    public static IReadOnlyList<SecurityProfile> GetProfiles()
    {
        return new[]
        {
            CisLevel1(),
            MaximumSecurity(),
            DeveloperWorkstation()
        };
    }

    /// <summary>
    /// CIS Level 1 — Baseline security suitable for most environments.
    /// Covers Defender basics, firewall, core CIS settings, and account policies.
    /// </summary>
    public static SecurityProfile CisLevel1()
    {
        return new SecurityProfile
        {
            Name = "CIS Level 1",
            Description = "Baseline security suitable for most environments. Covers Defender basics, firewall, core CIS settings, and account policies.",
            SettingIds = new[]
            {
                // Defender essentials
                "DEF-001", "DEF-002", "DEF-003", "DEF-007",
                // Firewall — all profiles enabled with default actions
                "FW-001", "FW-002", "FW-003",
                "FW-007", "FW-008", "FW-009",
                "FW-013", "FW-014", "FW-015",
                // CIS — SMB, NTLM, signing, RDP, safe DLL, UAC
                "CIS-001", "CIS-002", "CIS-003", "CIS-004",
                "CIS-005", "CIS-006", "CIS-007", "CIS-008",
                "CIS-009", "CIS-010", "CIS-011", "CIS-012",
                "CIS-014", "CIS-015", "CIS-016",
                // Account policies
                "ACCT-001", "ACCT-002", "ACCT-003", "ACCT-004",
                // Network — disable insecure protocols
                "NET-001", "NET-002", "NET-003",
                "NET-006", "NET-007", "NET-008", "NET-009",
                "NET-010", "NET-011", "NET-012", "NET-013"
            }
        };
    }

    /// <summary>
    /// Maximum Security — Enables every available hardening setting.
    /// Best for high-security servers and sensitive workstations.
    /// </summary>
    public static SecurityProfile MaximumSecurity()
    {
        return new SecurityProfile
        {
            Name = "Maximum Security",
            Description = "Enables every available hardening setting. Best for high-security servers and sensitive workstations. May impact usability.",
            SettingIds = new[]
            {
                // All Defender
                "DEF-001", "DEF-002", "DEF-003", "DEF-004", "DEF-005",
                "DEF-006", "DEF-007", "DEF-008", "DEF-009", "DEF-010",
                "DEF-011", "DEF-012", "DEF-013", "DEF-014", "DEF-015",
                // All ASR
                "ASR-001", "ASR-002", "ASR-003", "ASR-004", "ASR-005",
                "ASR-006", "ASR-007", "ASR-008", "ASR-009", "ASR-010",
                "ASR-011", "ASR-012", "ASR-013", "ASR-014", "ASR-015",
                // All Firewall
                "FW-001", "FW-002", "FW-003", "FW-004", "FW-005", "FW-006",
                "FW-007", "FW-008", "FW-009", "FW-010", "FW-011", "FW-012",
                "FW-013", "FW-014", "FW-015", "FW-016", "FW-017", "FW-018",
                // All CIS
                "CIS-001", "CIS-002", "CIS-003", "CIS-004", "CIS-005",
                "CIS-006", "CIS-007", "CIS-008", "CIS-009", "CIS-010",
                "CIS-011", "CIS-012", "CIS-013", "CIS-014", "CIS-015",
                "CIS-016", "CIS-017", "CIS-018", "CIS-019", "CIS-020",
                "CIS-021", "CIS-022", "CIS-023", "CIS-024", "CIS-025",
                "CIS-026", "CIS-027", "CIS-028", "CIS-029", "CIS-030",
                // All Account
                "ACCT-001", "ACCT-002", "ACCT-003", "ACCT-004", "ACCT-005",
                // All Network
                "NET-001", "NET-002", "NET-003", "NET-004", "NET-005",
                "NET-006", "NET-007", "NET-008", "NET-009", "NET-010",
                "NET-011", "NET-012", "NET-013", "NET-014", "NET-015"
            }
        };
    }

    /// <summary>
    /// Developer Workstation — Balanced security for development machines.
    /// Enables core protections but avoids settings that interfere with development tools.
    /// </summary>
    public static SecurityProfile DeveloperWorkstation()
    {
        return new SecurityProfile
        {
            Name = "Developer Workstation",
            Description = "Balanced security for development machines. Enables core protections while avoiding settings that may interfere with development tools and local servers.",
            SettingIds = new[]
            {
                // Defender basics (no aggressive cloud blocking)
                "DEF-001", "DEF-002", "DEF-003", "DEF-006", "DEF-007",
                // Firewall — enable but with defaults only
                "FW-001", "FW-007", "FW-013",
                // CIS — core hardening, skip strict signing/NTLM that may break dev tools
                "CIS-001", "CIS-002", "CIS-007", "CIS-008",
                "CIS-014", "CIS-015", "CIS-016",
                "CIS-023", "CIS-024",
                // Account policies
                "ACCT-001", "ACCT-002", "ACCT-003",
                // Network — disable insecure resolution, enable TLS 1.2
                "NET-001", "NET-002", "NET-003",
                "NET-006", "NET-007"
            }
        };
    }
}
