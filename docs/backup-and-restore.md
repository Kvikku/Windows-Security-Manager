# Backup & Restore

Windows Security Manager can back up the current registry state for all managed settings and restore them later. This lets you safely experiment with security configurations and roll back if something breaks.

## How It Works

- **Backup** reads the current value (or absence of a value) for every managed registry key and saves it to a JSON file.
- **Restore** reads the backup file and writes each value back to the registry for matching known setting IDs, returning those settings to the backed-up state.
- Entries in the backup file that don't match a known setting ID are **skipped** for security — the tool never writes arbitrary registry paths from a backup file.

## Creating a Backup

```bash
WindowsSecurityManager.exe backup --output my-backup.json
```

This creates a JSON file containing every managed setting's current registry value. The file includes:

- Setting IDs
- Registry paths and value names
- Current values (or a marker indicating the key didn't exist)
- Timestamp of the backup

> **Tip:** Use descriptive filenames to track what the backup represents:
> ```bash
> WindowsSecurityManager.exe backup --output before-cis-profile.json
> WindowsSecurityManager.exe backup --output 2024-01-15-baseline.json
> ```

## Restoring from a Backup

```bash
WindowsSecurityManager.exe restore my-backup.json
```

The restore process:

1. Reads each entry from the backup file.
2. Validates it against known setting IDs — unknown entries are skipped.
3. Uses the **known setting's registry path** (not the path in the file) for safety.
4. Writes the saved value back, or deletes the registry value if it didn't exist at backup time.

The command reports how many settings were successfully restored.

## Example Workflow

### Before Making Changes

```bash
# 1. Save current state
WindowsSecurityManager.exe backup --output baseline.json

# 2. Apply changes
WindowsSecurityManager.exe profile --apply "Maximum Security"

# 3. Verify the result
WindowsSecurityManager.exe report
```

### Rolling Back

```bash
# Restore to the saved baseline
WindowsSecurityManager.exe restore baseline.json

# Confirm settings are back to normal
WindowsSecurityManager.exe report
```

### Before and After Comparison

```bash
# Export a report before changes
WindowsSecurityManager.exe report --format Json --output before.json

# Make changes
WindowsSecurityManager.exe enable --category AttackSurfaceReduction

# Export a report after changes
WindowsSecurityManager.exe report --format Json --output after.json
```

## Backup File Format

The backup file is standard JSON:

```json
{
  "CreatedAt": "2024-01-15T10:30:00Z",
  "Entries": [
    {
      "SettingId": "DEF-001",
      "RegistryHive": "HKLM",
      "RegistryPath": "SOFTWARE\\Microsoft\\Windows Defender\\...",
      "ValueName": "DisableRealtimeMonitoring",
      "ValueType": "DWord",
      "Value": "0",
      "WasConfigured": true
    }
  ]
}
```

## Security Notes

- **Registry path validation:** Restore only writes to registry paths defined by known settings, not paths from the backup file. This prevents a tampered backup from writing to arbitrary locations.
- **Audit logging:** Both backup and restore operations are recorded in the audit log at `%LOCALAPPDATA%\WindowsSecurityManager\wsm-audit.log`.
- **Store backups securely:** Backup files contain information about your security configuration. Keep them in a secure location.
