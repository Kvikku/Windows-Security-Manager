using WindowsSecurityManager.Models;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Definitions;

/// <summary>
/// CIS Benchmark and general Windows hardening settings.
/// Covers SMB, NTLM, remote desktop, network security, and other OS hardening configurations.
/// </summary>
public class CisBenchmarkSettings : ISecuritySettingProvider
{
    private const string Hive = "HKLM";

    public IEnumerable<SecuritySetting> GetSettings()
    {
        // --- SMB Settings ---
        yield return new SecuritySetting
        {
            Id = "CIS-001",
            Name = "Disable SMBv1 Server",
            Description = "Disables the SMBv1 server protocol to prevent EternalBlue and similar exploits.",
            Category = SecurityCategory.CisBenchmark,
            RegistryHive = Hive,
            RegistryPath = @"SYSTEM\CurrentControlSet\Services\LanmanServer\Parameters",
            ValueName = "SMB1",
            ValueType = SettingValueType.DWord,
            EnabledValue = 0,
            DisabledValue = 1,
            RecommendedValue = 0
        };

        yield return new SecuritySetting
        {
            Id = "CIS-002",
            Name = "Disable SMBv1 Client",
            Description = "Disables the SMBv1 client driver.",
            Category = SecurityCategory.CisBenchmark,
            RegistryHive = Hive,
            RegistryPath = @"SYSTEM\CurrentControlSet\Services\mrxsmb10",
            ValueName = "Start",
            ValueType = SettingValueType.DWord,
            EnabledValue = 4,
            DisabledValue = 2,
            RecommendedValue = 4
        };

        // --- NTLM Settings ---
        yield return new SecuritySetting
        {
            Id = "CIS-003",
            Name = "Require NTLMv2 and 128-bit Encryption",
            Description = "Sets LAN Manager authentication level to send NTLMv2 response only, refuse LM & NTLM.",
            Category = SecurityCategory.CisBenchmark,
            RegistryHive = Hive,
            RegistryPath = @"SYSTEM\CurrentControlSet\Control\Lsa",
            ValueName = "LmCompatibilityLevel",
            ValueType = SettingValueType.DWord,
            EnabledValue = 5,
            DisabledValue = 0,
            RecommendedValue = 5
        };

        yield return new SecuritySetting
        {
            Id = "CIS-004",
            Name = "Restrict Anonymous Access to Named Pipes and Shares",
            Description = "Restricts anonymous access to named pipes and shares.",
            Category = SecurityCategory.CisBenchmark,
            RegistryHive = Hive,
            RegistryPath = @"SYSTEM\CurrentControlSet\Services\LanmanServer\Parameters",
            ValueName = "RestrictNullSessAccess",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };

        yield return new SecuritySetting
        {
            Id = "CIS-005",
            Name = "Restrict Anonymous Enumeration of SAM Accounts",
            Description = "Prevents anonymous users from enumerating SAM accounts.",
            Category = SecurityCategory.CisBenchmark,
            RegistryHive = Hive,
            RegistryPath = @"SYSTEM\CurrentControlSet\Control\Lsa",
            ValueName = "RestrictAnonymousSAM",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };

        yield return new SecuritySetting
        {
            Id = "CIS-006",
            Name = "Restrict Anonymous Enumeration of Shares",
            Description = "Prevents anonymous users from enumerating network shares.",
            Category = SecurityCategory.CisBenchmark,
            RegistryHive = Hive,
            RegistryPath = @"SYSTEM\CurrentControlSet\Control\Lsa",
            ValueName = "RestrictAnonymous",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };

        // --- Remote Desktop Settings ---
        yield return new SecuritySetting
        {
            Id = "CIS-007",
            Name = "Require NLA for Remote Desktop",
            Description = "Requires Network Level Authentication for Remote Desktop connections.",
            Category = SecurityCategory.CisBenchmark,
            RegistryHive = Hive,
            RegistryPath = @"SYSTEM\CurrentControlSet\Control\Terminal Server\WinStations\RDP-Tcp",
            ValueName = "UserAuthentication",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };

        yield return new SecuritySetting
        {
            Id = "CIS-008",
            Name = "Set RDP Encryption Level to High",
            Description = "Sets Remote Desktop encryption level to High.",
            Category = SecurityCategory.CisBenchmark,
            RegistryHive = Hive,
            RegistryPath = @"SYSTEM\CurrentControlSet\Control\Terminal Server\WinStations\RDP-Tcp",
            ValueName = "MinEncryptionLevel",
            ValueType = SettingValueType.DWord,
            EnabledValue = 3,
            DisabledValue = 1,
            RecommendedValue = 3
        };

        // --- Network Security ---
        yield return new SecuritySetting
        {
            Id = "CIS-009",
            Name = "Enable SMB Signing (Server - Required)",
            Description = "Requires SMB packet signing on the server side.",
            Category = SecurityCategory.CisBenchmark,
            RegistryHive = Hive,
            RegistryPath = @"SYSTEM\CurrentControlSet\Services\LanmanServer\Parameters",
            ValueName = "RequireSecuritySignature",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };

        yield return new SecuritySetting
        {
            Id = "CIS-010",
            Name = "Enable SMB Signing (Server - Enabled)",
            Description = "Enables SMB packet signing on the server side.",
            Category = SecurityCategory.CisBenchmark,
            RegistryHive = Hive,
            RegistryPath = @"SYSTEM\CurrentControlSet\Services\LanmanServer\Parameters",
            ValueName = "EnableSecuritySignature",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };

        yield return new SecuritySetting
        {
            Id = "CIS-011",
            Name = "Enable SMB Signing (Client - Required)",
            Description = "Requires SMB packet signing on the client side.",
            Category = SecurityCategory.CisBenchmark,
            RegistryHive = Hive,
            RegistryPath = @"SYSTEM\CurrentControlSet\Services\LanmanWorkstation\Parameters",
            ValueName = "RequireSecuritySignature",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };

        yield return new SecuritySetting
        {
            Id = "CIS-012",
            Name = "Enable SMB Signing (Client - Enabled)",
            Description = "Enables SMB packet signing on the client side.",
            Category = SecurityCategory.CisBenchmark,
            RegistryHive = Hive,
            RegistryPath = @"SYSTEM\CurrentControlSet\Services\LanmanWorkstation\Parameters",
            ValueName = "EnableSecuritySignature",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };

        // --- Misc Hardening ---
        yield return new SecuritySetting
        {
            Id = "CIS-013",
            Name = "Disable Autorun",
            Description = "Disables autorun for all drive types.",
            Category = SecurityCategory.CisBenchmark,
            RegistryHive = Hive,
            RegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer",
            ValueName = "NoDriveTypeAutoRun",
            ValueType = SettingValueType.DWord,
            EnabledValue = 255,
            DisabledValue = 0,
            RecommendedValue = 255
        };

        yield return new SecuritySetting
        {
            Id = "CIS-014",
            Name = "Disable Autoplay",
            Description = "Disables autoplay for all drives.",
            Category = SecurityCategory.CisBenchmark,
            RegistryHive = Hive,
            RegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer",
            ValueName = "NoAutoplayfornonVolume",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };

        yield return new SecuritySetting
        {
            Id = "CIS-015",
            Name = "Enable Safe DLL Search Mode",
            Description = "Enables safe DLL search mode to prevent DLL hijacking.",
            Category = SecurityCategory.CisBenchmark,
            RegistryHive = Hive,
            RegistryPath = @"SYSTEM\CurrentControlSet\Control\Session Manager",
            ValueName = "SafeDllSearchMode",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };

        yield return new SecuritySetting
        {
            Id = "CIS-016",
            Name = "Enable DEP (Data Execution Prevention)",
            Description = "Enables Data Execution Prevention for all programs and services.",
            Category = SecurityCategory.CisBenchmark,
            RegistryHive = Hive,
            RegistryPath = @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management",
            ValueName = "MoveImages",
            ValueType = SettingValueType.DWord,
            EnabledValue = 0xFFFFFFFF,
            DisabledValue = 0,
            RecommendedValue = 0xFFFFFFFF
        };

        yield return new SecuritySetting
        {
            Id = "CIS-017",
            Name = "Enable SEHOP",
            Description = "Enables Structured Exception Handler Overwrite Protection.",
            Category = SecurityCategory.CisBenchmark,
            RegistryHive = Hive,
            RegistryPath = @"SYSTEM\CurrentControlSet\Control\Session Manager\kernel",
            ValueName = "DisableExceptionChainValidation",
            ValueType = SettingValueType.DWord,
            EnabledValue = 0,
            DisabledValue = 1,
            RecommendedValue = 0
        };

        yield return new SecuritySetting
        {
            Id = "CIS-018",
            Name = "Disable Remote Assistance",
            Description = "Disables unsolicited remote assistance offers.",
            Category = SecurityCategory.CisBenchmark,
            RegistryHive = Hive,
            RegistryPath = @"SYSTEM\CurrentControlSet\Control\Remote Assistance",
            ValueName = "fAllowToGetHelp",
            ValueType = SettingValueType.DWord,
            EnabledValue = 0,
            DisabledValue = 1,
            RecommendedValue = 0
        };

        yield return new SecuritySetting
        {
            Id = "CIS-019",
            Name = "Disable WDigest Authentication",
            Description = "Prevents WDigest from storing credentials in plain text in memory.",
            Category = SecurityCategory.CisBenchmark,
            RegistryHive = Hive,
            RegistryPath = @"SYSTEM\CurrentControlSet\Control\SecurityProviders\WDigest",
            ValueName = "UseLogonCredential",
            ValueType = SettingValueType.DWord,
            EnabledValue = 0,
            DisabledValue = 1,
            RecommendedValue = 0
        };

        yield return new SecuritySetting
        {
            Id = "CIS-020",
            Name = "Enable LSA Protection",
            Description = "Enables Local Security Authority (LSA) protection to prevent credential theft.",
            Category = SecurityCategory.CisBenchmark,
            RegistryHive = Hive,
            RegistryPath = @"SYSTEM\CurrentControlSet\Control\Lsa",
            ValueName = "RunAsPPL",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };

        yield return new SecuritySetting
        {
            Id = "CIS-021",
            Name = "Enable Credential Guard",
            Description = "Enables Credential Guard to protect domain credentials.",
            Category = SecurityCategory.CisBenchmark,
            RegistryHive = Hive,
            RegistryPath = @"SOFTWARE\Policies\Microsoft\Windows\DeviceGuard",
            ValueName = "LsaCfgFlags",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };

        yield return new SecuritySetting
        {
            Id = "CIS-022",
            Name = "Disable Windows Script Host",
            Description = "Disables Windows Script Host to prevent script-based attacks.",
            Category = SecurityCategory.CisBenchmark,
            RegistryHive = Hive,
            RegistryPath = @"SOFTWARE\Microsoft\Windows Script Host\Settings",
            ValueName = "Enabled",
            ValueType = SettingValueType.DWord,
            EnabledValue = 0,
            DisabledValue = 1,
            RecommendedValue = 0
        };

        yield return new SecuritySetting
        {
            Id = "CIS-023",
            Name = "Enable PowerShell Script Block Logging",
            Description = "Enables logging of all PowerShell script blocks for auditing.",
            Category = SecurityCategory.CisBenchmark,
            RegistryHive = Hive,
            RegistryPath = @"SOFTWARE\Policies\Microsoft\Windows\PowerShell\ScriptBlockLogging",
            ValueName = "EnableScriptBlockLogging",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };

        yield return new SecuritySetting
        {
            Id = "CIS-024",
            Name = "Enable PowerShell Module Logging",
            Description = "Enables logging of PowerShell module activity.",
            Category = SecurityCategory.CisBenchmark,
            RegistryHive = Hive,
            RegistryPath = @"SOFTWARE\Policies\Microsoft\Windows\PowerShell\ModuleLogging",
            ValueName = "EnableModuleLogging",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };

        yield return new SecuritySetting
        {
            Id = "CIS-025",
            Name = "Disable IPv6 Source Routing",
            Description = "Disables IPv6 source routing to prevent routing-based attacks.",
            Category = SecurityCategory.CisBenchmark,
            RegistryHive = Hive,
            RegistryPath = @"SYSTEM\CurrentControlSet\Services\Tcpip6\Parameters",
            ValueName = "DisableIPSourceRouting",
            ValueType = SettingValueType.DWord,
            EnabledValue = 2,
            DisabledValue = 0,
            RecommendedValue = 2
        };

        yield return new SecuritySetting
        {
            Id = "CIS-026",
            Name = "Disable IPv4 Source Routing",
            Description = "Disables IPv4 source routing to prevent routing-based attacks.",
            Category = SecurityCategory.CisBenchmark,
            RegistryHive = Hive,
            RegistryPath = @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters",
            ValueName = "DisableIPSourceRouting",
            ValueType = SettingValueType.DWord,
            EnabledValue = 2,
            DisabledValue = 0,
            RecommendedValue = 2
        };

        yield return new SecuritySetting
        {
            Id = "CIS-027",
            Name = "Enable ICMP Redirect Prevention",
            Description = "Prevents ICMP redirects from overriding OSPF-generated routes.",
            Category = SecurityCategory.CisBenchmark,
            RegistryHive = Hive,
            RegistryPath = @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters",
            ValueName = "EnableICMPRedirect",
            ValueType = SettingValueType.DWord,
            EnabledValue = 0,
            DisabledValue = 1,
            RecommendedValue = 0
        };

        yield return new SecuritySetting
        {
            Id = "CIS-028",
            Name = "Enable Screen Saver with Password",
            Description = "Requires password when waking from screen saver.",
            Category = SecurityCategory.CisBenchmark,
            RegistryHive = "HKCU",
            RegistryPath = @"Control Panel\Desktop",
            ValueName = "ScreenSaverIsSecure",
            ValueType = SettingValueType.String,
            EnabledValue = "1",
            DisabledValue = "0",
            RecommendedValue = "1"
        };

        yield return new SecuritySetting
        {
            Id = "CIS-029",
            Name = "Disable Solicited Remote Assistance",
            Description = "Disables solicited remote assistance.",
            Category = SecurityCategory.CisBenchmark,
            RegistryHive = Hive,
            RegistryPath = @"SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services",
            ValueName = "fAllowUnsolicited",
            ValueType = SettingValueType.DWord,
            EnabledValue = 0,
            DisabledValue = 1,
            RecommendedValue = 0
        };

        yield return new SecuritySetting
        {
            Id = "CIS-030",
            Name = "Disable Anonymous SID/Name Translation",
            Description = "Prevents anonymous users from translating SIDs to account names.",
            Category = SecurityCategory.CisBenchmark,
            RegistryHive = Hive,
            RegistryPath = @"SYSTEM\CurrentControlSet\Control\Lsa",
            ValueName = "TurnOffAnonymousBlock",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };
    }
}
