using System.Collections.ObjectModel;
using WindowsSecurityManager.Models;

namespace WindowsSecurityManager.Definitions;

/// <summary>
/// Central catalog of impact level + compatibility consequence notes for every
/// built-in setting. Sourced from <c>docs/security-setting-consequences.md</c>; the two
/// sources must stay in sync (see <c>docs/extending-settings.md</c>).
/// </summary>
public static class SettingConsequencesCatalog
{
    /// <summary>
    /// Map of setting ID → (impact level, short consequences note).
    /// </summary>
    public static readonly IReadOnlyDictionary<string, (ImpactLevel Impact, string Consequences)> Entries =
        new ReadOnlyDictionary<string, (ImpactLevel Impact, string Consequences)>(
            new Dictionary<string, (ImpactLevel, string)>(StringComparer.OrdinalIgnoreCase)
        {
            // --- Windows Defender ---
            ["DEF-001"] = (ImpactLevel.Low,    "Prevents users (and malware) from disabling Defender. May conflict with third-party AV that expects to disable Defender — switch via the third-party installer instead."),
            ["DEF-002"] = (ImpactLevel.Low,    "Enables real-time scanning. May slow down very I/O-intensive workloads (large compiles, big-data tooling); add explicit Defender exclusions for known-safe paths."),
            ["DEF-003"] = (ImpactLevel.Low,    "Behavioral detections may flag aggressive admin tooling or red-team scripts as suspicious."),
            ["DEF-004"] = (ImpactLevel.Low,    "All file opens are scanned; minor latency on first-access of large files."),
            ["DEF-005"] = (ImpactLevel.Low,    "One-time process scan when real-time protection toggles on. Brief CPU spike."),
            ["DEF-006"] = (ImpactLevel.Low,    "Scans downloaded files / email attachments. Negligible day-to-day impact."),
            ["DEF-007"] = (ImpactLevel.Medium, "Blocks 'potentially unwanted applications' — bundled installers, some adware-adjacent tools, certain freeware. May block legitimate software users explicitly want."),
            ["DEF-008"] = (ImpactLevel.Medium, "Aggressive cloud-driven blocking can produce false positives on uncommon binaries, custom internal tools, and freshly compiled developer artifacts."),
            ["DEF-009"] = (ImpactLevel.Low,    "Suspicious files may be blocked from launching for up to 50 seconds while cloud lookup completes. Rarely user-visible."),
            ["DEF-010"] = (ImpactLevel.Medium, "Sends safe sample files to Microsoft. Privacy-sensitive environments may need to leave this disabled or set to a non-default mode."),
            ["DEF-011"] = (ImpactLevel.Medium, "Sends telemetry to Microsoft's cloud. Same privacy considerations as DEF-010."),
            ["DEF-012"] = (ImpactLevel.Low,    "Defender quarantines low-severity detections automatically; restore from quarantine if a false positive blocks an admin tool."),
            ["DEF-013"] = (ImpactLevel.Low,    "Defender quarantines medium-severity detections automatically; restore from quarantine if a false positive occurs."),
            ["DEF-014"] = (ImpactLevel.Low,    "Defender quarantines high-severity detections automatically; restore from quarantine if a false positive occurs."),
            ["DEF-015"] = (ImpactLevel.Medium, "Severe-severity detections are removed (not quarantined). False positives are unrecoverable from the console — restore from backup if needed."),

            // --- Attack Surface Reduction ---
            ["ASR-001"] = (ImpactLevel.Medium, "Breaks Office macros / templates that legitimately drop scripts or executables."),
            ["ASR-002"] = (ImpactLevel.Medium, "Breaks Office add-ins, Mail Merge to external apps, and macros that launch other tools."),
            ["ASR-003"] = (ImpactLevel.Medium, "Breaks complex VBA macros that call Win32 APIs (PInvoke-style)."),
            ["ASR-004"] = (ImpactLevel.Low,    "Rarely affects normal Office use; may break some debugging/inspection add-ins."),
            ["ASR-005"] = (ImpactLevel.Low,    "Breaks workflows that intentionally chain downloads via Windows Script Host."),
            ["ASR-006"] = (ImpactLevel.Medium, "Heuristic — may block heavily minified/obfuscated but legitimate JS/PowerShell scripts."),
            ["ASR-007"] = (ImpactLevel.Low,    "Can interfere with security tooling and IT scripts that legitimately read LSASS (rare)."),
            ["ASR-008"] = (ImpactLevel.High,   "Breaks many remote-admin tools: PSExec, Sysinternals workflows, WMI-based remote execution, some deployment/monitoring agents."),
            ["ASR-009"] = (ImpactLevel.Medium, "Blocks running unsigned binaries directly from removable media. Test field-engineer USB toolkits."),
            ["ASR-010"] = (ImpactLevel.Low,    "Blocks executables/scripts dropped by email clients. May affect users who legitimately receive .exe files."),
            ["ASR-011"] = (ImpactLevel.Low,    "Breaks Adobe Reader workflows that launch external editors / form helpers."),
            ["ASR-012"] = (ImpactLevel.Low,    "Can affect monitoring/management tools that legitimately use permanent WMI subscriptions."),
            ["ASR-013"] = (ImpactLevel.Medium, "Cloud-driven heuristic; may produce false positives on backup, sync, or encryption tools that touch many files quickly."),
            ["ASR-014"] = (ImpactLevel.High,   "Will block freshly built or internal binaries with no public reputation — very disruptive on developer machines."),
            ["ASR-015"] = (ImpactLevel.Medium, "Blocks known-vulnerable signed drivers — may break legacy hardware utilities, some overclocking/diagnostic tools."),

            // --- Firewall ---
            ["FW-001"] = (ImpactLevel.Low,    "Turns on Windows Firewall for the Domain profile. Strongly recommended; leaving it off relies on perimeter security only."),
            ["FW-002"] = (ImpactLevel.Medium, "Blocks unsolicited inbound connections by default for Domain profile. Required apps must have explicit allow rules — verify file shares, RDP, dev servers, and third-party agents still work."),
            ["FW-003"] = (ImpactLevel.Low,    "Sets the conventional default outbound = Allow for Domain profile. Does not introduce restrictions; intentionally leaves outbound open for compatibility."),
            ["FW-004"] = (ImpactLevel.Low,    "Suppresses the popup when a new app is blocked on Domain profile. End users won't know why an app is being blocked — make sure admins can review logs."),
            ["FW-005"] = (ImpactLevel.Low,    "Increases firewall log file size for Domain profile. Rotate / monitor pfirewall.log."),
            ["FW-006"] = (ImpactLevel.Medium, "High log volume on busy servers (Domain profile). Monitor disk usage and rotate logs."),

            ["FW-007"] = (ImpactLevel.Low,    "Turns on Windows Firewall for the Private profile. Strongly recommended; leaving it off relies on perimeter security only."),
            ["FW-008"] = (ImpactLevel.Medium, "Blocks unsolicited inbound connections by default for Private profile. Required apps must have explicit allow rules — verify file shares, RDP, dev servers, and third-party agents still work."),
            ["FW-009"] = (ImpactLevel.Low,    "Sets the conventional default outbound = Allow for Private profile. Does not introduce restrictions; intentionally leaves outbound open for compatibility."),
            ["FW-010"] = (ImpactLevel.Low,    "Suppresses the popup when a new app is blocked on Private profile. End users won't know why an app is being blocked — make sure admins can review logs."),
            ["FW-011"] = (ImpactLevel.Low,    "Increases firewall log file size for Private profile. Rotate / monitor pfirewall.log."),
            ["FW-012"] = (ImpactLevel.Medium, "High log volume on busy servers (Private profile). Monitor disk usage and rotate logs."),

            ["FW-013"] = (ImpactLevel.Low,    "Turns on Windows Firewall for the Public profile. Strongly recommended; leaving it off relies on perimeter security only."),
            ["FW-014"] = (ImpactLevel.Medium, "Blocks unsolicited inbound connections by default for Public profile. Required apps must have explicit allow rules — verify file shares, RDP, dev servers, and third-party agents still work."),
            ["FW-015"] = (ImpactLevel.Low,    "Sets the conventional default outbound = Allow for Public profile. Does not introduce restrictions; intentionally leaves outbound open for compatibility."),
            ["FW-016"] = (ImpactLevel.Low,    "Suppresses the popup when a new app is blocked on Public profile. End users won't know why an app is being blocked — make sure admins can review logs."),
            ["FW-017"] = (ImpactLevel.Low,    "Increases firewall log file size for Public profile. Rotate / monitor pfirewall.log."),
            ["FW-018"] = (ImpactLevel.Medium, "High log volume on busy servers (Public profile). Monitor disk usage and rotate logs."),

            // --- CIS Benchmark ---
            ["CIS-001"] = (ImpactLevel.Medium, "Breaks file sharing with very old clients (XP, some NAS/MFP firmware). Verify no legacy clients depend on SMBv1 before enabling."),
            ["CIS-002"] = (ImpactLevel.Medium, "Cannot connect to SMBv1-only servers (legacy NAS, old Linux Samba, some appliances)."),
            ["CIS-003"] = (ImpactLevel.High,   "Breaks authentication to systems that only support older NTLM/LM (legacy servers, some appliances, ancient print systems)."),
            ["CIS-004"] = (ImpactLevel.Medium, "Some legacy enumeration / monitoring tools rely on anonymous access to named pipes/shares."),
            ["CIS-005"] = (ImpactLevel.Medium, "Breaks tools that anonymously enumerate local accounts (some auditing utilities)."),
            ["CIS-006"] = (ImpactLevel.Medium, "Anonymous 'browse' of shares no longer works; users need authenticated sessions."),
            ["CIS-007"] = (ImpactLevel.Medium, "Older RDP clients (pre-Windows 7 era, some thin clients) cannot connect."),
            ["CIS-008"] = (ImpactLevel.Medium, "Same compatibility concern as CIS-007 for very old RDP clients."),
            ["CIS-009"] = (ImpactLevel.Medium, "Clients without SMB signing cannot connect to this server. Test legacy Linux/macOS clients."),
            ["CIS-010"] = (ImpactLevel.Medium, "Clients without SMB signing cannot connect to this server. Test legacy Linux/macOS clients."),
            ["CIS-011"] = (ImpactLevel.Medium, "This machine cannot connect to servers without SMB signing."),
            ["CIS-012"] = (ImpactLevel.Medium, "This machine cannot connect to servers without SMB signing."),
            ["CIS-013"] = (ImpactLevel.Low,    "No autorun popups for inserted media. End users may need to be shown how to launch installers manually."),
            ["CIS-014"] = (ImpactLevel.Low,    "No 'What do you want to do with this drive?' prompt."),
            ["CIS-015"] = (ImpactLevel.Low,    "Some legacy in-house apps that depend on CWD-relative DLL loading may fail."),
            ["CIS-016"] = (ImpactLevel.Medium, "Forces ASLR on all images. Very old or poorly written native binaries may crash."),
            ["CIS-017"] = (ImpactLevel.Low,    "Old 32-bit apps relying on broken SEH chains may crash. Rare."),
            ["CIS-018"] = (ImpactLevel.Medium, "End users cannot start a Remote Assistance session for help-desk support."),
            ["CIS-019"] = (ImpactLevel.Low,    "Affects only environments still using WDigest (mostly legacy). Recommended everywhere."),
            ["CIS-020"] = (ImpactLevel.Medium, "May block legitimate plug-ins that load into LSASS (older smart-card / SSO software)."),
            ["CIS-021"] = (ImpactLevel.Medium, "Requires VBS-capable hardware; may conflict with some hypervisors and older drivers. Plan for reboot and driver review."),
            ["CIS-022"] = (ImpactLevel.High,   "Breaks all .vbs, .js, .wsf scripts — including many legacy logon scripts, installers, MDM scripts."),
            ["CIS-023"] = (ImpactLevel.Low,    "Generates a lot of event-log volume; size operational logs accordingly."),
            ["CIS-024"] = (ImpactLevel.Low,    "Generates a lot of event-log volume; size operational logs accordingly."),
            ["CIS-025"] = (ImpactLevel.Low,    "Source-routed IPv6 packets are dropped (rarely used legitimately)."),
            ["CIS-026"] = (ImpactLevel.Low,    "Source-routed IPv4 packets are dropped (rarely used legitimately)."),
            ["CIS-027"] = (ImpactLevel.Low,    "OSPF-driven networks: ensure you do not rely on ICMP redirects for dynamic routing."),
            ["CIS-028"] = (ImpactLevel.Low,    "Users must re-authenticate after the screen-saver — minor UX impact."),
            ["CIS-029"] = (ImpactLevel.Medium, "Help-desk staff cannot push an unsolicited Remote Assistance offer to users."),
            ["CIS-030"] = (ImpactLevel.Medium, "Tools that rely on anonymous SID resolution (some monitoring/auditing) may fail."),

            // --- Account Policy ---
            ["ACCT-001"] = (ImpactLevel.Medium, "Five failed logons lock the account. Mistyped passwords, stale stored credentials, or scripted services with old passwords can trigger lockouts."),
            ["ACCT-002"] = (ImpactLevel.Medium, "Locked accounts are unusable for 30 minutes unless an admin unlocks them."),
            ["ACCT-003"] = (ImpactLevel.Low,    "Guest logon is no longer possible. Almost always desired."),
            ["ACCT-004"] = (ImpactLevel.Low,    "Granular audit subcategory settings take precedence over legacy categories — ensures modern audit policies behave as configured."),
            ["ACCT-005"] = (ImpactLevel.High,   "Bluescreens the system if it cannot write security events. Use only on high-assurance systems with monitored event-log infrastructure; not appropriate for user workstations."),

            // --- Network Security ---
            ["NET-001"] = (ImpactLevel.Low,    "Closes a common MITM vector. Hosts using LLMNR-only name resolution (rare) won't be discoverable."),
            ["NET-002"] = (ImpactLevel.Medium, "Breaks discovery / browsing on networks that still rely on NetBIOS (legacy file shares, old printers)."),
            ["NET-003"] = (ImpactLevel.Low,    "Auto-proxy discovery is disabled — set proxy explicitly if required by your network."),
            ["NET-004"] = (ImpactLevel.Medium, "Forces DoH for upstream DNS where supported; may bypass internal DNS-based filtering / split-horizon DNS."),
            ["NET-005"] = (ImpactLevel.Medium, "Networks still using WINS for name resolution will lose lookups."),
            ["NET-006"] = (ImpactLevel.Low,    "Enables modern TLS 1.2 client. No downside on any supported Windows release."),
            ["NET-007"] = (ImpactLevel.Low,    "Enables modern TLS 1.2 server. No downside on any supported Windows release."),
            ["NET-008"] = (ImpactLevel.Low,    "Removes a long-deprecated, broken protocol (SSL 2.0 client)."),
            ["NET-009"] = (ImpactLevel.Low,    "Removes a long-deprecated, broken protocol (SSL 2.0 server)."),
            ["NET-010"] = (ImpactLevel.Low,    "Removes the protocol vulnerable to POODLE (SSL 3.0 client)."),
            ["NET-011"] = (ImpactLevel.Low,    "Removes the protocol vulnerable to POODLE (SSL 3.0 server)."),
            ["NET-012"] = (ImpactLevel.Medium, "Some legacy client/server software, embedded devices, and old .NET Framework apps still negotiate TLS 1.0 only."),
            ["NET-013"] = (ImpactLevel.Medium, "Some legacy client/server software, embedded devices, and old .NET Framework apps still negotiate TLS 1.0 only."),
            ["NET-014"] = (ImpactLevel.Medium, "Some legacy client/server software, embedded devices, and old .NET Framework apps still negotiate TLS 1.1 only."),
            ["NET-015"] = (ImpactLevel.Medium, "Some legacy client/server software, embedded devices, and old .NET Framework apps still negotiate TLS 1.1 only."),
        });

    /// <summary>
    /// Returns the catalog entry for a setting ID, or <c>(Unknown, "")</c> if missing.
    /// </summary>
    public static (ImpactLevel Impact, string Consequences) Get(string? settingId)
    {
        if (settingId is not null && Entries.TryGetValue(settingId, out var entry))
            return entry;
        return (ImpactLevel.Unknown, string.Empty);
    }

    /// <summary>
    /// Populates <see cref="SecuritySetting.Impact"/> and <see cref="SecuritySetting.Consequences"/>
    /// from the catalog for any setting that hasn't been classified by its provider.
    /// Settings that already declare an explicit impact are left untouched.
    /// </summary>
    public static void Enrich(IEnumerable<SecuritySetting> settings)
    {
        if (settings is null) return;
        foreach (var setting in settings)
        {
            if (setting is null)
                continue;

            if (setting.Impact != ImpactLevel.Unknown && !string.IsNullOrWhiteSpace(setting.Consequences))
                continue;

            var (impact, consequences) = Get(setting.Id);
            if (setting.Impact == ImpactLevel.Unknown)
                setting.Impact = impact;
            if (string.IsNullOrWhiteSpace(setting.Consequences))
                setting.Consequences = consequences;
        }
    }
}
