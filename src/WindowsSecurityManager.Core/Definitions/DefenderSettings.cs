using WindowsSecurityManager.Models;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Definitions;

/// <summary>
/// Windows Defender security settings.
/// </summary>
public class DefenderSettings : ISecuritySettingProvider
{
    private const string Hive = "HKLM";
    private const string BasePath = @"SOFTWARE\Policies\Microsoft\Windows Defender";
    private const string RtpPath = BasePath + @"\Real-Time Protection";
    private const string SpynetPath = BasePath + @"\Spynet";
    private const string MpEnginePath = BasePath + @"\MpEngine";
    private const string ThreatSevPath = BasePath + @"\Threats\ThreatSeverityDefaultAction";

    public IEnumerable<SecuritySetting> GetSettings()
    {
        yield return new SecuritySetting
        {
            Id = "DEF-001",
            Name = "Disable AntiSpyware Override",
            Description = "Prevents users and malware from disabling Windows Defender AntiSpyware.",
            Category = SecurityCategory.WindowsDefender,
            RegistryHive = Hive,
            RegistryPath = BasePath,
            ValueName = "DisableAntiSpyware",
            ValueType = SettingValueType.DWord,
            EnabledValue = 0,
            DisabledValue = 1,
            RecommendedValue = 0
        };

        yield return new SecuritySetting
        {
            Id = "DEF-002",
            Name = "Enable Real-Time Protection",
            Description = "Enables real-time monitoring of files and processes for malware.",
            Category = SecurityCategory.WindowsDefender,
            RegistryHive = Hive,
            RegistryPath = RtpPath,
            ValueName = "DisableRealtimeMonitoring",
            ValueType = SettingValueType.DWord,
            EnabledValue = 0,
            DisabledValue = 1,
            RecommendedValue = 0
        };

        yield return new SecuritySetting
        {
            Id = "DEF-003",
            Name = "Enable Behavior Monitoring",
            Description = "Monitors process behavior to detect and block suspicious activities.",
            Category = SecurityCategory.WindowsDefender,
            RegistryHive = Hive,
            RegistryPath = RtpPath,
            ValueName = "DisableBehaviorMonitoring",
            ValueType = SettingValueType.DWord,
            EnabledValue = 0,
            DisabledValue = 1,
            RecommendedValue = 0
        };

        yield return new SecuritySetting
        {
            Id = "DEF-004",
            Name = "Enable On Access Protection",
            Description = "Scans files when they are accessed.",
            Category = SecurityCategory.WindowsDefender,
            RegistryHive = Hive,
            RegistryPath = RtpPath,
            ValueName = "DisableOnAccessProtection",
            ValueType = SettingValueType.DWord,
            EnabledValue = 0,
            DisabledValue = 1,
            RecommendedValue = 0
        };

        yield return new SecuritySetting
        {
            Id = "DEF-005",
            Name = "Enable Scan on Real-Time Enable",
            Description = "Runs a process scan whenever real-time protection is turned on.",
            Category = SecurityCategory.WindowsDefender,
            RegistryHive = Hive,
            RegistryPath = RtpPath,
            ValueName = "DisableScanOnRealtimeEnable",
            ValueType = SettingValueType.DWord,
            EnabledValue = 0,
            DisabledValue = 1,
            RecommendedValue = 0
        };

        yield return new SecuritySetting
        {
            Id = "DEF-006",
            Name = "Enable IOAV Protection",
            Description = "Scans downloaded files and attachments for malware.",
            Category = SecurityCategory.WindowsDefender,
            RegistryHive = Hive,
            RegistryPath = RtpPath,
            ValueName = "DisableIOAVProtection",
            ValueType = SettingValueType.DWord,
            EnabledValue = 0,
            DisabledValue = 1,
            RecommendedValue = 0
        };

        yield return new SecuritySetting
        {
            Id = "DEF-007",
            Name = "Enable PUA Protection",
            Description = "Enables Potentially Unwanted Application detection and blocking.",
            Category = SecurityCategory.WindowsDefender,
            RegistryHive = Hive,
            RegistryPath = BasePath,
            ValueName = "PUAProtection",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };

        yield return new SecuritySetting
        {
            Id = "DEF-008",
            Name = "Cloud Block Level",
            Description = "Sets cloud-delivered protection level to High+ (aggressive blocking).",
            Category = SecurityCategory.WindowsDefender,
            RegistryHive = Hive,
            RegistryPath = MpEnginePath,
            ValueName = "MpCloudBlockLevel",
            ValueType = SettingValueType.DWord,
            EnabledValue = 2,
            DisabledValue = 0,
            RecommendedValue = 2
        };

        yield return new SecuritySetting
        {
            Id = "DEF-009",
            Name = "Cloud Extended Timeout",
            Description = "Extends cloud check timeout for suspicious files to 50 seconds.",
            Category = SecurityCategory.WindowsDefender,
            RegistryHive = Hive,
            RegistryPath = MpEnginePath,
            ValueName = "MpBafsExtendedTimeout",
            ValueType = SettingValueType.DWord,
            EnabledValue = 50,
            DisabledValue = 0,
            RecommendedValue = 50
        };

        yield return new SecuritySetting
        {
            Id = "DEF-010",
            Name = "Submit Samples Consent",
            Description = "Automatically sends safe samples to Microsoft for analysis.",
            Category = SecurityCategory.WindowsDefender,
            RegistryHive = Hive,
            RegistryPath = SpynetPath,
            ValueName = "SubmitSamplesConsent",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };

        yield return new SecuritySetting
        {
            Id = "DEF-011",
            Name = "Join Microsoft MAPS",
            Description = "Enables cloud-delivered protection via Microsoft Active Protection Service.",
            Category = SecurityCategory.WindowsDefender,
            RegistryHive = Hive,
            RegistryPath = SpynetPath,
            ValueName = "SpynetReporting",
            ValueType = SettingValueType.DWord,
            EnabledValue = 2,
            DisabledValue = 0,
            RecommendedValue = 2
        };

        yield return new SecuritySetting
        {
            Id = "DEF-012",
            Name = "Severity Override - Low Threat Action",
            Description = "Configures action for low-severity threats to Quarantine.",
            Category = SecurityCategory.WindowsDefender,
            RegistryHive = Hive,
            RegistryPath = ThreatSevPath,
            ValueName = "1",
            ValueType = SettingValueType.DWord,
            EnabledValue = 2,
            DisabledValue = 6,
            RecommendedValue = 2
        };

        yield return new SecuritySetting
        {
            Id = "DEF-013",
            Name = "Severity Override - Medium Threat Action",
            Description = "Configures action for medium-severity threats to Quarantine.",
            Category = SecurityCategory.WindowsDefender,
            RegistryHive = Hive,
            RegistryPath = ThreatSevPath,
            ValueName = "2",
            ValueType = SettingValueType.DWord,
            EnabledValue = 2,
            DisabledValue = 6,
            RecommendedValue = 2
        };

        yield return new SecuritySetting
        {
            Id = "DEF-014",
            Name = "Severity Override - High Threat Action",
            Description = "Configures action for high-severity threats to Quarantine.",
            Category = SecurityCategory.WindowsDefender,
            RegistryHive = Hive,
            RegistryPath = ThreatSevPath,
            ValueName = "4",
            ValueType = SettingValueType.DWord,
            EnabledValue = 2,
            DisabledValue = 6,
            RecommendedValue = 2
        };

        yield return new SecuritySetting
        {
            Id = "DEF-015",
            Name = "Severity Override - Severe Threat Action",
            Description = "Configures action for severe threats to Remove.",
            Category = SecurityCategory.WindowsDefender,
            RegistryHive = Hive,
            RegistryPath = ThreatSevPath,
            ValueName = "5",
            ValueType = SettingValueType.DWord,
            EnabledValue = 3,
            DisabledValue = 6,
            RecommendedValue = 3
        };
    }
}
