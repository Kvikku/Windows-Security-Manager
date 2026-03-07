using WindowsSecurityManager.Models;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Definitions;

/// <summary>
/// Attack Surface Reduction (ASR) rule settings.
/// ASR rules are DWORD values stored under the ASR\Rules registry key.
/// Value of 1 = Block, 0 = Disabled, 2 = Audit, 6 = Warn.
/// </summary>
public class AsrSettings : ISecuritySettingProvider
{
    private const string Hive = "HKLM";
    private const string BasePath = @"SOFTWARE\Policies\Microsoft\Windows Defender\Windows Defender Exploit Guard\ASR\Rules";

    public IEnumerable<SecuritySetting> GetSettings()
    {
        yield return CreateAsrRule(
            "ASR-001",
            "Block Office apps from creating executable content",
            "Prevents Office applications from creating potentially malicious executable content.",
            "3B576869-A4EC-4529-8536-B80A7769E899");

        yield return CreateAsrRule(
            "ASR-002",
            "Block Office apps from creating child processes",
            "Prevents Office applications from creating child processes.",
            "D4F940AB-401B-4EFC-AADC-AD5F3C50688A");

        yield return CreateAsrRule(
            "ASR-003",
            "Block Win32 API calls from Office macros",
            "Blocks Win32 API calls from Office macros.",
            "92E97FA1-2EDF-4476-BDD6-9DD0B4DDDC7B");

        yield return CreateAsrRule(
            "ASR-004",
            "Block Office apps from injecting code into other processes",
            "Prevents Office applications from injecting code into other processes.",
            "75668C1F-73B5-4CF0-BB93-3ECF5CB7CC84");

        yield return CreateAsrRule(
            "ASR-005",
            "Block JavaScript or VBScript from launching downloaded executables",
            "Blocks JavaScript or VBScript from launching downloaded executable content.",
            "D3E037E1-3EB8-44C8-A917-57927947596D");

        yield return CreateAsrRule(
            "ASR-006",
            "Block execution of potentially obfuscated scripts",
            "Detects and blocks suspicious properties within an obfuscated script.",
            "5BEB7EFE-FD9A-4556-801D-275E5FFC04CC");

        yield return CreateAsrRule(
            "ASR-007",
            "Block credential stealing from LSASS",
            "Blocks credential stealing from Windows Local Security Authority Subsystem (lsass.exe).",
            "9E6C4E1F-7D60-472F-BA1A-A39EF669E4B2");

        yield return CreateAsrRule(
            "ASR-008",
            "Block process creations from PSExec and WMI",
            "Blocks process creations originating from PSExec and WMI commands.",
            "D1E49AAC-8F56-4280-B9BA-993A6D77406C");

        yield return CreateAsrRule(
            "ASR-009",
            "Block untrusted/unsigned processes from USB",
            "Blocks untrusted and unsigned processes that run from USB.",
            "B2B3F03D-6A65-4F7B-A9C7-1C7EF74A9BA4");

        yield return CreateAsrRule(
            "ASR-010",
            "Block executable content from email and webmail",
            "Blocks executable content from email client and webmail.",
            "BE9BA2D9-53EA-4CDC-84E5-9B1EEEE46550");

        yield return CreateAsrRule(
            "ASR-011",
            "Block Adobe Reader from creating child processes",
            "Prevents Adobe Reader from creating child processes.",
            "7674BA52-37EB-4A4F-A9A1-F0F9A1619A2C");

        yield return CreateAsrRule(
            "ASR-012",
            "Block persistence through WMI event subscription",
            "Blocks persistence through WMI event subscription.",
            "E6DB77E5-3DF2-4CF1-B95A-636979351E5B");

        yield return CreateAsrRule(
            "ASR-013",
            "Use advanced protection against ransomware",
            "Provides extra protection against ransomware using cloud intelligence.",
            "C1DB55AB-C21A-4637-BB3F-A12568109D35");

        yield return CreateAsrRule(
            "ASR-014",
            "Block executable files unless they meet criteria",
            "Blocks executable files from running unless they meet prevalence, age, or trusted list criteria.",
            "01443614-CD74-433A-B99E-2ECDC07BFC25");

        yield return CreateAsrRule(
            "ASR-015",
            "Block abuse of exploited vulnerable signed drivers",
            "Prevents exploitation of vulnerable signed drivers.",
            "56A863A9-875E-4185-98A7-B882C64B5CE5");
    }

    private static SecuritySetting CreateAsrRule(string id, string name, string description, string ruleGuid)
    {
        return new SecuritySetting
        {
            Id = id,
            Name = name,
            Description = description,
            Category = SecurityCategory.AttackSurfaceReduction,
            RegistryHive = Hive,
            RegistryPath = BasePath,
            ValueName = ruleGuid,
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };
    }
}
