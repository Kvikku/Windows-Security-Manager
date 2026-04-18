# Getting Started

This guide walks you through installing and running Windows Security Manager for the first time.

## Prerequisites

| Requirement | Details |
|---|---|
| **Operating System** | Windows 10/11 or Windows Server 2016+ |
| **Privileges** | Administrator (required for registry modifications) |
| **Runtime** | None — the pre-built executable is self-contained |

> **For development only:** [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later is required to build from source.

## Installation

### Option 1 — Download the Pre-built Executable (Recommended)

1. Go to the [Releases](https://github.com/Kvikku/Windows-Security-Manager/releases) page.
2. Download `WindowsSecurityManager.exe` from the latest release.
3. Place it in a folder of your choice (e.g., `C:\Tools\`).
4. Open an **elevated** Command Prompt or PowerShell and navigate to the folder.

No installation or runtime dependencies are needed — the executable is fully self-contained.

### Option 2 — Build from Source

```bash
# Clone the repository
git clone https://github.com/Kvikku/Windows-Security-Manager.git
cd Windows-Security-Manager

# Build
dotnet build

# Run
dotnet run --project src/WindowsSecurityManager -- --help
```

To publish a standalone executable:

```bash
dotnet publish src/WindowsSecurityManager/WindowsSecurityManager.csproj \
    --configuration Release \
    --runtime win-x64 \
    --self-contained true \
    --output ./publish
```

This produces `./publish/WindowsSecurityManager.exe`.

## First Steps

### Interactive Mode

Launch the executable with no arguments to enter the interactive terminal menu:

```
WindowsSecurityManager.exe
```

The interactive menu provides a live compliance dashboard and guided navigation to enable/disable settings, apply profiles, generate reports, and more.

### CLI Mode

Pass commands directly for scripting and automation:

```bash
# Show all available commands
WindowsSecurityManager.exe --help

# List all managed security settings
WindowsSecurityManager.exe list

# Generate a compliance report
WindowsSecurityManager.exe report

# Apply a security profile
WindowsSecurityManager.exe profile --apply "CIS Level 1"
```

### Recommended First Workflow

> ⚠️ **Before enabling any setting**, review the [Security Setting Consequences](security-setting-consequences.md) page so you know the compatibility impact of what you're about to apply. Always **backup → dry-run → apply → rollback if needed**.

1. **Backup** your current registry state:
   ```bash
   WindowsSecurityManager.exe backup --output before-changes.json
   ```
2. **Review** available settings:
   ```bash
   WindowsSecurityManager.exe list
   ```
3. **Dry-run** a profile to preview changes:
   ```bash
   WindowsSecurityManager.exe profile --apply "CIS Level 1" --dry-run
   ```
4. **Apply** the profile:
   ```bash
   WindowsSecurityManager.exe profile --apply "CIS Level 1"
   ```
5. **Report** on the new compliance state:
   ```bash
   WindowsSecurityManager.exe report
   ```

## What's Next?

- [CLI Reference](cli-reference.md) — Full command documentation
- [Security Profiles](security-profiles.md) — Learn about the built-in profiles
- [Security Setting Consequences](security-setting-consequences.md) — Impact and side effects of each setting
- [Backup & Restore](backup-and-restore.md) — Safely manage changes
- [Extending Settings](extending-settings.md) — Add your own security settings
