# Extending Settings

Windows Security Manager is designed to be easily extensible. You can add new security settings by implementing the `ISecuritySettingProvider` interface and registering it in `Program.cs`.

## Step 1 — Create a Setting Provider

Create a new class that implements `ISecuritySettingProvider`. Each provider returns one or more `SecuritySetting` objects:

```csharp
using WindowsSecurityManager.Models;
using WindowsSecurityManager.Services;

namespace WindowsSecurityManager.Definitions;

public class MyCustomSettings : ISecuritySettingProvider
{
    public IEnumerable<SecuritySetting> GetSettings()
    {
        yield return new SecuritySetting
        {
            Id = "CUSTOM-001",
            Name = "My Custom Setting",
            Description = "Description of what this setting does.",
            Category = SecurityCategory.CisBenchmark,
            RegistryHive = "HKLM",
            RegistryPath = @"SOFTWARE\MyPath",
            ValueName = "MySetting",
            ValueType = SettingValueType.DWord,
            EnabledValue = 1,
            DisabledValue = 0,
            RecommendedValue = 1
        };
    }
}
```

### SecuritySetting Properties

| Property | Type | Description |
|---|---|---|
| `Id` | `string` | Unique identifier (e.g., `CUSTOM-001`). Use a consistent prefix for your settings. |
| `Name` | `string` | Human-readable name displayed in lists and reports. |
| `Description` | `string` | Explains what the setting does and why it matters. |
| `Category` | `SecurityCategory` | One of: `WindowsDefender`, `AttackSurfaceReduction`, `Firewall`, `CisBenchmark`, `AccountPolicy`, `NetworkSecurity`. |
| `RegistryHive` | `string` | Registry hive (e.g., `HKLM`, `HKCU`). |
| `RegistryPath` | `string` | Registry key path. |
| `ValueName` | `string` | Registry value name within the key. |
| `ValueType` | `SettingValueType` | `DWord`, `QWord`, `String`, `Binary`, `MultiString`, or `ExpandString`. Built-in settings primarily use `DWord`. |
| `EnabledValue` | `object` | The value that represents the hardened/enabled state. |
| `DisabledValue` | `object` | The value that represents the unhardened/disabled state. |
| `RecommendedValue` | `object` | The value recommended by security benchmarks (usually matches `EnabledValue`). |

### Available Categories

```csharp
public enum SecurityCategory
{
    WindowsDefender,
    AttackSurfaceReduction,
    Firewall,
    CisBenchmark,
    AccountPolicy,
    NetworkSecurity
}
```

### Available Value Types

```csharp
public enum SettingValueType
{
    DWord,        // 32-bit integer (REG_DWORD)
    QWord,        // 64-bit integer (REG_QWORD)
    String,       // String value (REG_SZ)
    Binary,       // Binary data (REG_BINARY)
    MultiString,  // Multi-string value (REG_MULTI_SZ)
    ExpandString  // Expandable string (REG_EXPAND_SZ)
}
```

> **Note:** Built-in settings primarily use `DWord`. The other types are available for custom settings that need them.

## Step 2 — Register the Provider

Add your new provider to the `providers` array in `Program.cs`:

```csharp
ISecuritySettingProvider[] providers =
[
    new DefenderSettings(),
    new AsrSettings(),
    new FirewallSettings(),
    new CisBenchmarkSettings(),
    new AccountPolicySettings(),
    new NetworkSecuritySettings(),
    new MyCustomSettings()          // ← Add here
];
```

That's it. Your settings will now appear in:
- `list` command output
- Compliance reports
- Interactive menu
- Backup/restore operations
- Search results

## Step 3 — Add to a Profile (Optional)

To include your settings in a security profile, edit `Definitions/SecurityProfiles.cs` and add your setting IDs to the relevant profile:

```csharp
public static SecurityProfile CisLevel1()
{
    return new SecurityProfile
    {
        Name = "CIS Level 1",
        SettingIds = new[]
        {
            // ... existing IDs ...
            "CUSTOM-001"  // ← Add your setting
        }
    };
}
```

## Conventions

Follow these conventions when adding settings:

- **ID prefixes:** Use a consistent prefix for your settings group (e.g., `CUSTOM-`, `APP-`). Existing prefixes: `DEF-`, `ASR-`, `FW-`, `CIS-`, `ACCT-`, `NET-`.
- **Sequential numbering:** Number settings sequentially within a prefix (e.g., `CUSTOM-001`, `CUSTOM-002`).
- **Descriptive names:** Keep names concise but descriptive.
- **Meaningful descriptions:** Include what the setting protects against and any side effects.
- **One file per category:** Group related settings in a single provider class under `Definitions/`.

## Testing

Add tests for your settings in `tests/WindowsSecurityManager.Tests/`. The existing test patterns use a mock `IRegistryService` to verify setting behavior without touching the real registry:

```csharp
[Fact]
public void MyCustomSettings_ReturnsExpectedSettings()
{
    var provider = new MyCustomSettings();
    var settings = provider.GetSettings().ToList();

    Assert.Single(settings);
    Assert.Equal("CUSTOM-001", settings[0].Id);
    Assert.Equal(SecurityCategory.CisBenchmark, settings[0].Category);
}
```
