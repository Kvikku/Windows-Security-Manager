# Windows Security Manager

A Windows C# CLI application that enables and disables Windows security hardening settings including ASR rules, Windows Defender, CIS Benchmark configurations, firewall settings, and more. It reads and writes registry keys to manage security configurations and generates compliance reports.

## Features

- **Enable/Disable** individual settings, entire categories, or all settings at once
- **Multi-select** — Pick multiple settings to enable/disable in one batch
- **Compliance Reporting** with detailed status of each security setting
- **Export Reports** to JSON, CSV, or HTML for auditing and sharing
- **Search/Filter** settings by keyword across names, IDs, and descriptions
- **Security Profiles** — Apply presets like "CIS Level 1", "Maximum Security", or "Developer Workstation"
- **Setting Detail View** — Drill into a single setting for full registry path, description, and current value
- **Live Dashboard** — Per-category compliance bars on the interactive main menu
- **Auto-refresh** — See updated compliance status immediately after changes
- **Dry-run Mode** — Preview what would change without writing to the registry
- **Backup/Restore** — Export current registry state before changes, and roll back if needed
- **Audit Logging** — Timestamped log of all changes made
- **Categorized Settings** for organized management:
  - **Windows Defender** – Real-time protection, PUA protection, cloud protection, threat actions
  - **Attack Surface Reduction (ASR)** – All 15 standard ASR rules
  - **Firewall** – Domain, Private, and Public profile settings with logging
  - **CIS Benchmark** – SMB, NTLM, RDP, DLL safety, credential protection, PowerShell logging, and more
  - **Account Policy** – Lockout thresholds, guest account, audit policies
  - **Network Security** – LLMNR, NetBIOS, WPAD, TLS/SSL protocol configuration
- **Scalable Architecture** – Add new settings by implementing `ISecuritySettingProvider`

## Requirements

- Windows 10/11 or Windows Server 2016+
- **Administrator privileges** (required for registry modifications)

### For development

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later

## Quick Start (Pre-built Executable)

Download the latest `WindowsSecurityManager.exe` from the [Releases](../../releases) page. The executable is self-contained and does not require the .NET SDK or runtime to be installed on the target machine.

```bash
# Run directly — no install needed
WindowsSecurityManager.exe --help
WindowsSecurityManager.exe list
WindowsSecurityManager.exe enable --setting DEF-001
WindowsSecurityManager.exe report
```

## Build

```bash
dotnet build
```

## Publish as Standalone Executable

To package the project as a self-contained single-file executable without Visual Studio:

```bash
dotnet publish src/WindowsSecurityManager/WindowsSecurityManager.csproj --configuration Release --runtime win-x64 --self-contained true --output ./publish
```

This produces `./publish/WindowsSecurityManager.exe` — a single file you can copy to any Windows x64 machine and run without installing the .NET runtime.

## Run (Development)

```bash
# Show help
dotnet run --project src/WindowsSecurityManager -- --help

# List all available settings
dotnet run --project src/WindowsSecurityManager -- list

# List settings in a specific category
dotnet run --project src/WindowsSecurityManager -- list --category WindowsDefender

# Search settings by keyword
dotnet run --project src/WindowsSecurityManager -- list --search "SMB"

# View detail of a specific setting
dotnet run --project src/WindowsSecurityManager -- detail DEF-001

# Enable a specific setting
dotnet run --project src/WindowsSecurityManager -- enable --setting DEF-001

# Enable all settings in a category
dotnet run --project src/WindowsSecurityManager -- enable --category AttackSurfaceReduction

# Enable all security settings
dotnet run --project src/WindowsSecurityManager -- enable --all

# Preview changes without writing (dry run)
dotnet run --project src/WindowsSecurityManager -- enable --all --dry-run

# Disable a specific setting
dotnet run --project src/WindowsSecurityManager -- disable --setting CIS-001

# Generate a compliance report
dotnet run --project src/WindowsSecurityManager -- report

# Report on a specific category
dotnet run --project src/WindowsSecurityManager -- report --category Firewall

# Export report to JSON
dotnet run --project src/WindowsSecurityManager -- report --format Json --output report.json

# Export report to CSV
dotnet run --project src/WindowsSecurityManager -- report --format Csv --output report.csv

# Export report to HTML
dotnet run --project src/WindowsSecurityManager -- report --format Html --output report.html

# List available security profiles
dotnet run --project src/WindowsSecurityManager -- profile --list

# Apply a security profile
dotnet run --project src/WindowsSecurityManager -- profile --apply "CIS Level 1"

# Preview a profile (dry run)
dotnet run --project src/WindowsSecurityManager -- profile --apply "Maximum Security" --dry-run

# Backup current registry state
dotnet run --project src/WindowsSecurityManager -- backup --output my-backup.json

# Restore from backup
dotnet run --project src/WindowsSecurityManager -- restore my-backup.json
```

## Test

```bash
dotnet test
```

## CI/CD — Automated Releases

A GitHub Actions workflow (`.github/workflows/release.yml`) automatically builds and publishes the executable:

- **On tag push** (`v*`): builds the executable, runs tests, and creates a GitHub Release with the `.exe` attached.
- **Manual trigger**: use the "Run workflow" button on the Actions tab to build on demand. The artifact is available for download from the workflow run.

## Available Categories

| Category | ID Prefix | Count | Description |
|---|---|---|---|
| Windows Defender | DEF-xxx | 15 | Core Defender protection settings |
| Attack Surface Reduction | ASR-xxx | 15 | ASR rules for exploit prevention |
| Firewall | FW-xxx | 18 | Firewall profiles and logging |
| CIS Benchmark | CIS-xxx | 30 | General OS hardening settings |
| Account Policy | ACCT-xxx | 5 | Account lockout and audit settings |
| Network Security | NET-xxx | 15 | Protocol and network hardening |

## Adding New Settings

To add new security settings, create a class implementing `ISecuritySettingProvider`:

```csharp
using WindowsSecurityManager.Models;
using WindowsSecurityManager.Services;

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

Then register it in `Program.cs`:

```csharp
ISecuritySettingProvider[] providers =
[
    // ... existing providers ...
    new MyCustomSettings()
];
```

## Project Structure

```
├── .github/workflows/
│   └── release.yml        # CI/CD: build & release executable
├── src/WindowsSecurityManager/
│   ├── Commands/          # CLI command handlers (enable, disable, report, list, detail, profile, backup, restore)
│   ├── Definitions/       # Security setting definitions by category + security profiles
│   ├── Models/            # Data models (SecuritySetting, SecurityReport, SecurityProfile, etc.)
│   ├── Services/          # Core services (registry, settings manager, report exporter, audit logger, backup)
│   ├── UI/                # Interactive terminal menu (Spectre.Console)
│   └── Program.cs         # Application entry point
├── tests/WindowsSecurityManager.Tests/
│   ├── SecuritySettingsManagerTests.cs
│   ├── SettingDefinitionTests.cs
│   ├── SecurityReportTests.cs
│   ├── SearchAndDryRunTests.cs
│   ├── ReportExporterTests.cs
│   ├── AuditLoggerTests.cs
│   ├── BackupServiceTests.cs
│   └── SecurityProfileTests.cs
└── WindowsSecurityManager.slnx
```

## License

This project is provided as-is for security hardening purposes.