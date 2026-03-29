# CLI Reference

Complete reference for all Windows Security Manager commands. Run any command with `--help` for built-in usage information.

> **Tip:** Run `WindowsSecurityManager.exe` with no arguments to launch the interactive terminal menu instead.

---

## `list`

List all managed security settings with their current status.

```bash
WindowsSecurityManager.exe list [options]
```

| Option | Description |
|---|---|
| `--category <name>` | Filter by category (e.g., `WindowsDefender`, `Firewall`) |
| `--search <keyword>` | Search settings by keyword across IDs, names, and descriptions |

**Examples:**

```bash
# List all settings
WindowsSecurityManager.exe list

# List only firewall settings
WindowsSecurityManager.exe list --category Firewall

# Search for SMB-related settings
WindowsSecurityManager.exe list --search "SMB"
```

---

## `detail`

View detailed information about a single setting, including its registry path, description, and current value.

```bash
WindowsSecurityManager.exe detail <setting-id>
```

**Example:**

```bash
WindowsSecurityManager.exe detail DEF-001
```

---

## `enable`

Enable (harden) one or more security settings by writing the recommended values to the registry.

```bash
WindowsSecurityManager.exe enable [options]
```

| Option | Description |
|---|---|
| `--setting <id>` | Enable a single setting by ID |
| `--category <name>` | Enable all settings in a category |
| `--all` | Enable all managed settings |
| `--dry-run` | Preview changes without writing to the registry |

**Examples:**

```bash
# Enable a single setting
WindowsSecurityManager.exe enable --setting DEF-001

# Enable all ASR rules
WindowsSecurityManager.exe enable --category AttackSurfaceReduction

# Enable everything
WindowsSecurityManager.exe enable --all

# Preview what would change
WindowsSecurityManager.exe enable --all --dry-run
```

---

## `disable`

Disable (unharden) one or more security settings.

```bash
WindowsSecurityManager.exe disable [options]
```

| Option | Description |
|---|---|
| `--setting <id>` | Disable a single setting by ID |
| `--category <name>` | Disable all settings in a category |
| `--all` | Disable all managed settings |
| `--dry-run` | Preview changes without writing to the registry |

**Examples:**

```bash
# Disable a specific setting
WindowsSecurityManager.exe disable --setting CIS-001

# Disable all network security settings
WindowsSecurityManager.exe disable --category NetworkSecurity
```

---

## `report`

Generate a compliance report showing the status of all managed settings.

```bash
WindowsSecurityManager.exe report [options]
```

| Option | Description |
|---|---|
| `--category <name>` | Report on a specific category only |
| `--format <format>` | Export format: `Json`, `Csv`, or `Html` |
| `--output <path>` | Write the report to a file instead of the console |

**Examples:**

```bash
# Display report in the console
WindowsSecurityManager.exe report

# Report for a specific category
WindowsSecurityManager.exe report --category Firewall

# Export to JSON
WindowsSecurityManager.exe report --format Json --output report.json

# Export to CSV
WindowsSecurityManager.exe report --format Csv --output report.csv

# Export to HTML (includes styled dashboard)
WindowsSecurityManager.exe report --format Html --output report.html
```

---

## `profile`

List or apply a built-in security profile.

```bash
WindowsSecurityManager.exe profile [options]
```

| Option | Description |
|---|---|
| `--list` | List all available profiles |
| `--apply <name>` | Apply a profile by name |
| `--dry-run` | Preview profile changes without writing to the registry |

**Examples:**

```bash
# List profiles
WindowsSecurityManager.exe profile --list

# Apply CIS Level 1
WindowsSecurityManager.exe profile --apply "CIS Level 1"

# Preview Maximum Security profile
WindowsSecurityManager.exe profile --apply "Maximum Security" --dry-run
```

See [Security Profiles](security-profiles.md) for detailed profile descriptions.

---

## `backup`

Export the current registry state for managed settings to a JSON file.

```bash
WindowsSecurityManager.exe backup [options]
```

| Option | Description |
|---|---|
| `--output <path>` | Path for the backup JSON file (defaults to `wsm-backup-<timestamp>.json`) |
| `--category <name>` | Back up only a specific category |

**Examples:**

```bash
# Backup all settings (auto-generated filename)
WindowsSecurityManager.exe backup

# Backup to a specific file
WindowsSecurityManager.exe backup --output my-backup.json

# Backup only firewall settings
WindowsSecurityManager.exe backup --category Firewall
```

---

## `restore`

Restore settings from a previously created backup file.

```bash
WindowsSecurityManager.exe restore <path>
```

**Example:**

```bash
WindowsSecurityManager.exe restore my-backup.json
```

See [Backup & Restore](backup-and-restore.md) for a complete walkthrough.

---

## Available Categories

Use these names with `--category` in any command:

| Category Name | ID Prefix | Settings | Description |
|---|---|---|---|
| `WindowsDefender` | DEF-xxx | 15 | Core Defender protection settings |
| `AttackSurfaceReduction` | ASR-xxx | 15 | ASR rules for exploit prevention |
| `Firewall` | FW-xxx | 18 | Firewall profiles and logging |
| `CisBenchmark` | CIS-xxx | 30 | General OS hardening settings |
| `AccountPolicy` | ACCT-xxx | 5 | Account lockout and audit settings |
| `NetworkSecurity` | NET-xxx | 15 | Protocol and network hardening |

---

## Global Behavior

- **Administrator privileges** are required for all commands that modify the registry (`enable`, `disable`, `profile --apply`, `restore`).
- **Audit logging** is automatic. All changes are recorded to `%LOCALAPPDATA%\WindowsSecurityManager\wsm-audit.log`.
- **Dry-run mode** (`--dry-run`) is available on `enable`, `disable`, and `profile --apply` to preview changes safely.
