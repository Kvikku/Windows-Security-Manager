<div align="center">

# 🛡️ Windows Security Manager

[![CI](https://github.com/Kvikku/Windows-Security-Manager/actions/workflows/ci.yml/badge.svg)](https://github.com/Kvikku/Windows-Security-Manager/actions/workflows/ci.yml)
[![Build and Release](https://github.com/Kvikku/Windows-Security-Manager/actions/workflows/release.yml/badge.svg)](https://github.com/Kvikku/Windows-Security-Manager/actions/workflows/release.yml)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)
![Platform](https://img.shields.io/badge/platform-Windows-0078D4?logo=windows&logoColor=white)
![Usage](https://img.shields.io/badge/usage-no__public__license-red)
![Settings](https://img.shields.io/badge/security_settings-98-green)

**A powerful CLI tool for managing Windows security hardening settings.**\
Enable, disable, audit, and report on Windows Defender, ASR rules, firewall, CIS benchmarks, and more — all from one place.

[Getting Started](docs/getting-started.md) · [CLI Reference](docs/cli-reference.md) · [Download](https://github.com/Kvikku/Windows-Security-Manager/releases)

</div>

---

## 📋 Table of Contents

- [Features](#-features)
- [Quick Start](#-quick-start)
- [Usage Examples](#-usage-examples)
- [Security Categories](#-security-categories)
- [Security Profiles](#-security-profiles)
- [Build from Source](#-build-from-source)
- [Documentation](#-documentation)
- [Project Structure](#-project-structure)
- [CI/CD](#-cicd)
- [Contributing](#-contributing)
- [License](#-license)

## ✨ Features

<table>
<tr>
<td>

**🔒 Security Management**
- Enable/disable individual settings, categories, or all at once
- Multi-select batch operations
- 98 settings across 6 categories
- Scalable provider architecture

</td>
<td>

**📊 Reporting & Compliance**
- Compliance reports with per-setting status
- Export to JSON, CSV, or styled HTML
- Live dashboard with compliance bars
- Auto-refresh after changes

</td>
</tr>
<tr>
<td>

**🎯 Profiles & Presets**
- CIS Level 1 — Baseline security
- Maximum Security — Full hardening
- Developer Workstation — Balanced protection
- Dry-run mode to preview changes

</td>
<td>

**🔄 Safety & Operations**
- Backup/restore registry state
- Dry-run support for enable/disable and profile apply operations
- Timestamped audit logging
- Search/filter across all settings

</td>
</tr>
</table>

### Supported Security Categories

| Category | Settings | What's Covered |
|---|---|---|
| 🦠 **Windows Defender** | 15 | Real-time protection, PUA, cloud protection, threat actions |
| 🧱 **Attack Surface Reduction** | 15 | All 15 standard ASR rules for exploit prevention |
| 🔥 **Firewall** | 18 | Domain, Private, Public profiles with logging |
| 📐 **CIS Benchmark** | 30 | SMB, NTLM, RDP, DLL safety, UAC, PowerShell logging |
| 👤 **Account Policy** | 5 | Lockout thresholds, guest account, audit policies |
| 🌐 **Network Security** | 15 | LLMNR, NetBIOS, WPAD, TLS/SSL configuration |

## 🚀 Quick Start

### Download & Run

Download `WindowsSecurityManager.exe` from the [latest release](https://github.com/Kvikku/Windows-Security-Manager/releases) — no install or runtime needed.

```bash
# Launch interactive mode (recommended for first use)
WindowsSecurityManager.exe

# Or use CLI commands directly
WindowsSecurityManager.exe list
WindowsSecurityManager.exe report
WindowsSecurityManager.exe enable --setting DEF-001
```

### Requirements

| Requirement | Details |
|---|---|
| 💻 Operating System | Windows 10/11 or Windows Server 2016+ |
| 🔑 Privileges | Administrator (for registry changes) |

> **For development:** [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later

## 💡 Usage Examples

### Enable & Disable Settings

```bash
# Enable a single setting
WindowsSecurityManager.exe enable --setting DEF-001

# Enable all settings in a category
WindowsSecurityManager.exe enable --category AttackSurfaceReduction

# Enable all security settings
WindowsSecurityManager.exe enable --all

# Preview changes without writing (dry run)
WindowsSecurityManager.exe enable --all --dry-run

# Disable a specific setting
WindowsSecurityManager.exe disable --setting CIS-001
```

### Search & Inspect

```bash
# Search settings by keyword
WindowsSecurityManager.exe list --search "SMB"

# View full detail for a setting
WindowsSecurityManager.exe detail DEF-001
```

### Reports & Export

```bash
# Generate compliance report
WindowsSecurityManager.exe report

# Export to HTML (styled dashboard)
WindowsSecurityManager.exe report --format Html --output report.html

# Export to JSON or CSV
WindowsSecurityManager.exe report --format Json --output report.json
WindowsSecurityManager.exe report --format Csv --output report.csv
```

### Profiles

```bash
# List available profiles
WindowsSecurityManager.exe profile --list

# Preview a profile
WindowsSecurityManager.exe profile --apply "CIS Level 1" --dry-run

# Apply a profile
WindowsSecurityManager.exe profile --apply "CIS Level 1"
```

### Backup & Restore

```bash
# Backup current state
WindowsSecurityManager.exe backup --output before-changes.json

# Restore from backup
WindowsSecurityManager.exe restore before-changes.json
```

## 🏷️ Security Categories

| Category | ID Prefix | Count | Description |
|---|---|---|---|
| Windows Defender | `DEF-xxx` | 15 | Core Defender protection settings |
| Attack Surface Reduction | `ASR-xxx` | 15 | ASR rules for exploit prevention |
| Firewall | `FW-xxx` | 18 | Firewall profiles and logging |
| CIS Benchmark | `CIS-xxx` | 30 | General OS hardening settings |
| Account Policy | `ACCT-xxx` | 5 | Account lockout and audit settings |
| Network Security | `NET-xxx` | 15 | Protocol and network hardening |

## 🎯 Security Profiles

| Profile | Description | Use Case |
|---|---|---|
| **CIS Level 1** | Baseline security covering Defender, firewall, CIS, accounts, and network | General workstations, offices |
| **Maximum Security** | Enables all 98 settings across every category | High-security servers, sensitive systems |
| **Developer Workstation** | Core protections without breaking dev tools | Developer laptops, CI/CD machines |

> See [Security Profiles documentation](docs/security-profiles.md) for detailed breakdowns of each profile.

## 🔨 Build from Source

```bash
# Build
dotnet build

# Run tests
dotnet test

# Run (development)
dotnet run --project src/WindowsSecurityManager -- --help

# Publish as standalone executable
dotnet publish src/WindowsSecurityManager/WindowsSecurityManager.csproj \
    --configuration Release \
    --runtime win-x64 \
    --self-contained true \
    --output ./publish
```

## 📚 Documentation

| Document | Description |
|---|---|
| [Getting Started](docs/getting-started.md) | Installation, first steps, and recommended workflows |
| [CLI Reference](docs/cli-reference.md) | Complete command reference with all options and examples |
| [Security Profiles](docs/security-profiles.md) | Detailed guide to built-in security profiles |
| [Backup & Restore](docs/backup-and-restore.md) | How to safely back up and restore security settings |
| [CI/CD Pipeline](docs/ci-cd.md) | How the build and release pipeline works |
| [Extending Settings](docs/extending-settings.md) | How to add your own custom security settings |
| [Architecture](docs/architecture.md) | System design, components, and project structure |

## 📁 Project Structure

```
├── .github/workflows/
│   ├── ci.yml                    # CI: restore, format check, build, test, coverage on push/PR
│   └── release.yml               # CD: build & release executable on tags
├── docs/                         # 📚 Documentation and how-to guides
├── src/WindowsSecurityManager/
│   ├── Commands/                 # CLI command handlers
│   ├── Definitions/              # Security setting definitions & profiles
│   ├── Models/                   # Data models
│   ├── Services/                 # Core services (registry, manager, exporter, backup, logger)
│   ├── UI/                       # Interactive terminal menu (Spectre.Console)
│   └── Program.cs                # Application entry point
├── tests/WindowsSecurityManager.Tests/
│   └── *.cs                      # Unit tests (xUnit + Moq)
└── WindowsSecurityManager.slnx
```

## ⚙️ CI/CD

Two GitHub Actions workflows automate quality checks, builds, and releases:

### CI (`ci.yml`) — Every Push & Pull Request

Runs on every push and pull request targeting `main`. Acts as a quality gate before merging.

| Step | Description |
|---|---|
| **Restore** | Restores NuGet packages (with caching for speed) |
| **Format check** | Verifies code style with `dotnet format --verify-no-changes` |
| **Build** | Compiles in Release configuration |
| **Test + Coverage** | Runs all xUnit tests and collects code coverage via Coverlet |
| **Upload coverage** | Uploads Cobertura coverage report as a workflow artifact |

### Release (`release.yml`) — Tag Push & Manual Dispatch

Builds and publishes the standalone executable.

| Trigger | Behavior |
|---|---|
| **Tag push** (`v*`) | Builds, tests, and creates a GitHub Release with the `.exe` attached |
| **Manual dispatch** | Builds on demand; artifact available from the workflow run |

**Release pipeline steps:** restore → test → publish (single-file, self-contained, win-x64) → upload artifact → create GitHub Release.

Both workflows use **NuGet package caching** (`actions/cache`) to speed up dependency restoration.

> See [CI/CD Pipeline](docs/ci-cd.md) for full details on the pipeline architecture.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Add your settings via `ISecuritySettingProvider` ([guide](docs/extending-settings.md))
4. Add tests for your changes
5. Submit a pull request

## 📄 License

This project is provided as-is for security hardening purposes. No public license is included — see the repository for usage terms.