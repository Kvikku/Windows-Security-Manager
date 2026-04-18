# Security Profiles

Security profiles are pre-built presets that enable a curated set of security settings in one action. They let you quickly apply a consistent security posture without selecting individual settings.

> 💡 **Before applying any profile**, review the [Security Setting Consequences](security-setting-consequences.md) page for the per-setting compatibility impact, and follow the **backup → dry-run → apply → rollback** workflow described below.

## Available Profiles

### CIS Level 1

**Baseline security suitable for most environments.**

Covers Defender basics, firewall, core CIS settings, and account policies. This profile aligns with CIS Benchmark Level 1 recommendations and is designed for broad compatibility.

| Category | What's Enabled |
|---|---|
| Windows Defender | Real-time protection, PUA protection, cloud protection |
| Firewall | All profiles enabled with default inbound block |
| CIS Benchmark | SMB, NTLM, signing, RDP, safe DLL, UAC, and more |
| Account Policy | Lockout thresholds, guest account disabled, audit policies |
| Network Security | Disable LLMNR, NetBIOS, WPAD; enable TLS 1.2 |

**Best for:** General-purpose workstations, office environments, standard servers.

**Likely side effects:** Mostly 🟢 Low impact. The strict NTLMv2 setting (CIS-003) and SMB signing requirements (CIS-009 – CIS-012) can break authentication / file sharing with very old systems — see [Security Setting Consequences](security-setting-consequences.md#-cis-benchmark-cis-xxx).

---

### Maximum Security

**Enables every available hardening setting.**

This profile activates all 98 managed settings across every category, including all 15 ASR rules and full firewall logging.

| Category | What's Enabled |
|---|---|
| Windows Defender | All 15 settings |
| Attack Surface Reduction | All 15 ASR rules |
| Firewall | All 18 settings (profiles, default actions, logging) |
| CIS Benchmark | All 30 settings |
| Account Policy | All 5 settings |
| Network Security | All 15 settings |

> ⚠️ **May impact usability.** Some settings (e.g., strict NTLM restrictions, disabled legacy protocols) can break compatibility with older software or network services. Test in a non-production environment first.

**Best for:** High-security servers, sensitive workstations, compliance-critical systems.

**Likely side effects:** Includes every 🔴 High-impact setting in this tool, notably:

- **ASR-008** (Block PSExec/WMI) — breaks many remote-admin and deployment workflows.
- **ASR-014** (Block low-prevalence executables) — breaks freshly built or internal binaries.
- **CIS-003** (Refuse LM/NTLM) — breaks legacy authentication.
- **CIS-022** (Disable Windows Script Host) — breaks `.vbs`/`.js`/`.wsf` scripts and many legacy installers/logon scripts.
- **ACCT-005** (Crash On Audit Fail) — bluescreens the system if security events cannot be written.
- **All TLS 1.0/1.1 disables** — breaks legacy clients and embedded devices that cannot negotiate TLS 1.2+.

Read the full per-setting impact in [Security Setting Consequences](security-setting-consequences.md) before applying this profile.

---

### Developer Workstation

**Balanced security for development machines.**

Enables core protections while avoiding settings that may interfere with development tools, local servers, and debugging workflows.

| Category | What's Enabled |
|---|---|
| Windows Defender | Basics enabled; aggressive cloud blocking skipped |
| Firewall | Profile firewalls enabled; strict logging skipped |
| CIS Benchmark | Core hardening; strict signing/NTLM restrictions skipped |
| Account Policy | Lockout thresholds and guest account disabled |
| Network Security | Insecure resolution disabled; TLS 1.2 enforced |

**Best for:** Developer laptops, CI/CD build machines, test environments.

**Likely side effects:** Mostly 🟢 Low impact. Avoids the settings most likely to interfere with developer workflows (aggressive cloud blocking of unsigned dev builds, strict NTLM, strict SMB signing, low-prevalence executable blocking). Per-setting notes in [Security Setting Consequences](security-setting-consequences.md).

---

## Using Profiles

### List Available Profiles

```bash
WindowsSecurityManager.exe profile --list
```

### Preview a Profile (Dry Run)

See what settings would be enabled without making any changes:

```bash
WindowsSecurityManager.exe profile --apply "CIS Level 1" --dry-run
```

### Apply a Profile

```bash
WindowsSecurityManager.exe profile --apply "CIS Level 1"
```

### Recommended Workflow

1. **Backup** before applying:
   ```bash
   WindowsSecurityManager.exe backup --output before-profile.json
   ```
2. **Preview** with dry-run:
   ```bash
   WindowsSecurityManager.exe profile --apply "Maximum Security" --dry-run
   ```
3. **Apply** the profile:
   ```bash
   WindowsSecurityManager.exe profile --apply "Maximum Security"
   ```
4. **Verify** with a compliance report:
   ```bash
   WindowsSecurityManager.exe report
   ```
5. **Rollback** if needed:
   ```bash
   WindowsSecurityManager.exe restore before-profile.json
   ```

## Profile Details

Profiles only **enable** settings — they do not disable settings that aren't included. If you've previously enabled settings outside the profile, those remain enabled.

To start from a known state, you can disable all settings first:

```bash
WindowsSecurityManager.exe disable --all
WindowsSecurityManager.exe profile --apply "CIS Level 1"
```
