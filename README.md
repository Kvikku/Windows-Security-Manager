# Windows Security Manager

A Windows C# CLI application that enables and disables Windows security hardening settings including ASR rules, Windows Defender, CIS Benchmark configurations, firewall settings, and more. It reads and writes registry keys to manage security configurations and generates compliance reports.

## Features

- **Enable/Disable** individual settings, entire categories, or all settings at once
- **Compliance Reporting** with detailed status of each security setting
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

# Enable a specific setting
dotnet run --project src/WindowsSecurityManager -- enable --setting DEF-001

# Enable all settings in a category
dotnet run --project src/WindowsSecurityManager -- enable --category AttackSurfaceReduction

# Enable all security settings
dotnet run --project src/WindowsSecurityManager -- enable --all

# Disable a specific setting
dotnet run --project src/WindowsSecurityManager -- disable --setting CIS-001

# Generate a compliance report
dotnet run --project src/WindowsSecurityManager -- report

# Report on a specific category
dotnet run --project src/WindowsSecurityManager -- report --category Firewall
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
│   ├── Commands/          # CLI command handlers (enable, disable, report, list)
│   ├── Definitions/       # Security setting definitions by category
│   ├── Models/            # Data models (SecuritySetting, SecurityReport, etc.)
│   ├── Services/          # Core services (registry, settings manager)
│   └── Program.cs         # Application entry point
├── tests/WindowsSecurityManager.Tests/
│   ├── SecuritySettingsManagerTests.cs
│   ├── SettingDefinitionTests.cs
│   └── SecurityReportTests.cs
└── WindowsSecurityManager.slnx
```

## License

This project is provided as-is for security hardening purposes.