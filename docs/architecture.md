# Architecture

This document describes the high-level architecture of Windows Security Manager.

## Overview

Windows Security Manager is a C# CLI application built on .NET 8 that manages Windows security hardening settings through the Windows Registry. It supports both an interactive terminal menu (powered by [Spectre.Console](https://spectreconsole.net/)) and a CLI interface (powered by [System.CommandLine](https://learn.microsoft.com/en-us/dotnet/standard/commandline/)).

```
┌─────────────────────────────────────────────────────┐
│                     Program.cs                      │
│              (Entry point & DI wiring)              │
├─────────────────┬───────────────────────────────────┤
│  Interactive UI  │         CLI Commands             │
│  (Spectre.Console)│  (System.CommandLine)            │
├─────────────────┴───────────────────────────────────┤
│               SecuritySettingsManager               │
│         (Core orchestration service)                │
├────────────┬────────────┬───────────────────────────┤
│ IRegistry  │  ISetting  │      Services             │
│  Service   │  Provider  │ (Exporter, Backup,        │
│            │            │  AuditLogger)             │
├────────────┴────────────┴───────────────────────────┤
│                  Windows Registry                   │
└─────────────────────────────────────────────────────┘
```

## Project Structure

```
├── .github/workflows/
│   └── release.yml              # CI/CD: build & release executable
├── src/WindowsSecurityManager/
│   ├── Commands/                # CLI command handlers
│   │   ├── EnableCommand.cs
│   │   ├── DisableCommand.cs
│   │   ├── ReportCommand.cs
│   │   ├── ListCommand.cs
│   │   ├── DetailCommand.cs
│   │   ├── ProfileCommand.cs
│   │   ├── BackupCommand.cs
│   │   └── RestoreCommand.cs
│   ├── Definitions/             # Security setting definitions
│   │   ├── DefenderSettings.cs
│   │   ├── AsrSettings.cs
│   │   ├── FirewallSettings.cs
│   │   ├── CisBenchmarkSettings.cs
│   │   ├── AccountPolicySettings.cs
│   │   ├── NetworkSecuritySettings.cs
│   │   └── SecurityProfiles.cs
│   ├── Models/                  # Data models
│   │   ├── SecuritySetting.cs
│   │   ├── SecurityCategory.cs
│   │   ├── SecurityReport.cs
│   │   ├── SecurityProfile.cs
│   │   ├── SettingStatus.cs
│   │   ├── SettingValueType.cs
│   │   ├── ExportFormat.cs
│   │   ├── DryRunChange.cs
│   │   ├── BackupData.cs
│   │   └── AuditLogEntry.cs
│   ├── Services/                # Core services
│   │   ├── IRegistryService.cs
│   │   ├── RegistryService.cs
│   │   ├── ISecuritySettingProvider.cs
│   │   ├── SecuritySettingsManager.cs
│   │   ├── ReportExporter.cs
│   │   ├── BackupService.cs
│   │   └── AuditLogger.cs
│   ├── UI/
│   │   └── InteractiveMenu.cs
│   └── Program.cs
├── tests/WindowsSecurityManager.Tests/
│   ├── SecuritySettingsManagerTests.cs
│   ├── SettingDefinitionTests.cs
│   ├── SecurityReportTests.cs
│   ├── SearchAndDryRunTests.cs
│   ├── ReportExporterTests.cs
│   ├── AuditLoggerTests.cs
│   ├── BackupServiceTests.cs
│   └── SecurityProfileTests.cs
├── docs/                        # Documentation
└── WindowsSecurityManager.slnx
```

## Key Components

### `ISecuritySettingProvider`

The interface that all setting definition classes implement. Each provider returns a collection of `SecuritySetting` objects that describe individual registry-based security configurations.

```csharp
public interface ISecuritySettingProvider
{
    IEnumerable<SecuritySetting> GetSettings();
}
```

Built-in providers: `DefenderSettings`, `AsrSettings`, `FirewallSettings`, `CisBenchmarkSettings`, `AccountPolicySettings`, `NetworkSecuritySettings`.

### `IRegistryService`

Abstracts Windows Registry access behind an interface, enabling testability. The production implementation (`RegistryService`) reads and writes real registry keys. Tests use a mock implementation.

### `SecuritySettingsManager`

The central orchestrator. Aggregates settings from all providers and exposes operations:

- **GetSettings / SearchSettings** — Query and filter settings
- **EnableSetting / DisableSetting** — Modify individual settings
- **EnableCategory / DisableCategory** — Batch operations by category
- **EnableAll / DisableAll** — Global operations
- **DryRunEnable / DryRunDisable** — Preview changes
- **GenerateReport** — Compliance reporting

### `ReportExporter`

Converts `SecurityReport` data into JSON, CSV, or HTML format. The HTML export includes a styled compliance dashboard with summary cards.

### `BackupService`

Creates and restores backup snapshots of registry state. Validates backup entries against known setting IDs to prevent registry injection from tampered files.

### `AuditLogger`

Writes timestamped log entries to `%LOCALAPPDATA%\WindowsSecurityManager\wsm-audit.log`. IO errors are silently caught so that logging failures never interrupt the main workflow.

### Commands (`Commands/`)

Each CLI command is a static class that creates a `System.CommandLine.Command` with the appropriate options and handlers. Commands delegate to `SecuritySettingsManager` and related services.

### Interactive Menu (`UI/InteractiveMenu.cs`)

A terminal-based UI built with Spectre.Console. Features a live compliance dashboard, multi-select setting management, and guided navigation.

## Design Principles

1. **Registry abstraction** — All registry access goes through `IRegistryService`, making the core logic fully testable without touching the real registry.
2. **Provider pattern** — New settings are added by implementing `ISecuritySettingProvider` and registering it in `Program.cs`. No existing code needs to change.
3. **Safety first** — Dry-run mode, backup/restore, and audit logging give users confidence to make changes. The restore process validates entries against known settings to prevent registry injection.
4. **Separation of concerns** — Commands handle CLI parsing, services handle business logic, definitions hold setting data, and models carry state.

## CI/CD

The GitHub Actions workflow (`.github/workflows/release.yml`):

- **Triggers:** On `v*` tag push or manual workflow dispatch.
- **Steps:** Checkout → Setup .NET 8 → Restore → Test → Publish self-contained executable → Upload artifact → Create GitHub Release (on tag).
- **Output:** A single `WindowsSecurityManager.exe` (self-contained, win-x64) attached to the GitHub Release.
