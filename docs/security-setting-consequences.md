# Security Setting Consequences

Hardening Windows almost always involves trade-offs. A setting that blocks an attack technique can also break legacy software, change a familiar UX, or interfere with diagnostic tools. This page summarises the **potential consequences** of each managed security setting so you can decide what to enable on a given machine.

> **⚠️ Read before enabling.** None of the settings in this tool are destructive on their own, but combinations (especially via the `Maximum Security` profile) can break business-critical workflows. Always follow the [Before Enabling](#before-enabling) workflow.

> 💡 **In-app visibility.** The same impact label and consequences note for every setting on this page is also shown inside the running app — in the `list` / `detail` / `enable --dry-run` / `profile --apply --dry-run` CLI commands and in the Interactive Menu (List, Setting Detail, Enable, and Security Profiles views). You don't need to keep this page open while using the tool.

## Before Enabling

Apply the same workflow for any individual setting, category, or profile change:

1. **Backup** the current state so you can roll back instantly:
   ```bash
   WindowsSecurityManager.exe backup --output before-changes.json
   ```
2. **Dry-run** the change to preview exactly what will be written:
   ```bash
   WindowsSecurityManager.exe enable --setting DEF-008 --dry-run
   WindowsSecurityManager.exe profile --apply "Maximum Security" --dry-run
   ```
3. **Apply** the change.
4. **Test** the affected workflows (logon, file shares, RDP, printing, line-of-business apps, dev tools).
5. **Rollback** with `restore` if anything regresses:
   ```bash
   WindowsSecurityManager.exe restore before-changes.json
   ```

See [Backup & Restore](backup-and-restore.md) for the full workflow.

## Impact Levels

Each setting below is tagged with an impact label that reflects the typical chance of disrupting normal usage on a general-purpose Windows machine:

| Label | Meaning |
|---|---|
| 🟢 **Low** | Safe defaults for almost any environment. Disruption is unlikely; mostly affects logging, telemetry, or already-deprecated functionality. |
| 🟡 **Medium** | Generally safe, but may surface compatibility issues with older software, legacy network services, peripherals, or non-standard workflows. Test before broad deployment. |
| 🔴 **High** | Likely to break specific workflows or third-party integrations (legacy SMB/NTLM clients, custom RDP setups, scripts, drivers). Apply only after deliberate testing. |

> Impact labels reflect compatibility risk, not security value. A "Low" setting can still provide significant security benefit; a "High" setting is not necessarily more important — it just requires more care.

---

## 🦠 Windows Defender (`DEF-xxx`)

| ID | Setting | Impact | Potential Consequences |
|---|---|---|---|
| DEF-001 | Disable AntiSpyware Override | 🟢 Low | Prevents users (and malware) from disabling Defender. May conflict with third-party AV that expects to disable Defender — switch via the third-party installer instead. |
| DEF-002 | Enable Real-Time Protection | 🟢 Low | Enables real-time scanning. May slow down very I/O-intensive workloads (large compiles, big-data tooling); add explicit Defender exclusions for known-safe paths. |
| DEF-003 | Enable Behavior Monitoring | 🟢 Low | Behavioral detections may flag aggressive admin tooling or red-team scripts as suspicious. |
| DEF-004 | Enable On Access Protection | 🟢 Low | All file opens are scanned; minor latency on first-access of large files. |
| DEF-005 | Enable Scan on Real-Time Enable | 🟢 Low | One-time process scan when RTP toggles on. Brief CPU spike. |
| DEF-006 | Enable IOAV Protection | 🟢 Low | Scans downloaded files / email attachments. Negligible day-to-day impact. |
| DEF-007 | Enable PUA Protection | 🟡 Medium | Blocks "potentially unwanted applications" — bundled installers, some adware-adjacent tools, certain freeware. May block legitimate software users explicitly want. |
| DEF-008 | Cloud Block Level (High+) | 🟡 Medium | Aggressive cloud-driven blocking can produce **false positives** on uncommon binaries, custom internal tools, and freshly compiled developer artifacts. |
| DEF-009 | Cloud Extended Timeout (50s) | 🟢 Low | Suspicious files may be blocked from launching for up to 50 seconds while cloud lookup completes. Rarely user-visible. |
| DEF-010 | Submit Samples Consent | 🟡 Medium | Sends safe sample files to Microsoft. Privacy-sensitive environments may need to leave this disabled or set to a non-default mode. |
| DEF-011 | Join Microsoft MAPS | 🟡 Medium | Sends telemetry to Microsoft's cloud. Same privacy considerations as DEF-010. |
| DEF-012 | Severity Override – Low Threat → Quarantine | 🟢 Low | Defender quarantines low-severity detections automatically; restore from quarantine if a false positive blocks an admin tool. |
| DEF-013 | Severity Override – Medium Threat → Quarantine | 🟢 Low | Same as DEF-012 for medium-severity detections. |
| DEF-014 | Severity Override – High Threat → Quarantine | 🟢 Low | Same as DEF-012 for high-severity detections. |
| DEF-015 | Severity Override – Severe Threat → Remove | 🟡 Medium | Severe-severity detections are removed (not quarantined). False positives are unrecoverable from the console — restore from backup if needed. |

---

## 🧱 Attack Surface Reduction (`ASR-xxx`)

ASR rules block specific exploitation techniques. They are powerful but can interfere with legitimate Office macros, internal tooling, and admin workflows. Consider running each rule in **Audit mode** (value `2`) before switching to Block, and add per-app/per-path exclusions where needed.

| ID | Rule | Impact | Potential Consequences |
|---|---|---|---|
| ASR-001 | Block Office apps from creating executable content | 🟡 Medium | Breaks Office macros / templates that legitimately drop scripts or executables. |
| ASR-002 | Block Office apps from creating child processes | 🟡 Medium | Breaks Office add-ins, Mail Merge to external apps, and macros that launch other tools. |
| ASR-003 | Block Win32 API calls from Office macros | 🟡 Medium | Breaks complex VBA macros that call Win32 APIs (PInvoke-style). |
| ASR-004 | Block Office apps from injecting code into other processes | 🟢 Low | Rarely affects normal Office use; may break some debugging/inspection add-ins. |
| ASR-005 | Block JS/VBScript from launching downloaded executables | 🟢 Low | Breaks workflows that intentionally chain downloads via WSH. |
| ASR-006 | Block execution of potentially obfuscated scripts | 🟡 Medium | Heuristic — may block heavily minified/obfuscated but legitimate JS/PS scripts. |
| ASR-007 | Block credential stealing from LSASS | 🟢 Low | Can interfere with security tooling and IT scripts that legitimately read LSASS (rare). |
| ASR-008 | Block process creations from PSExec and WMI | 🔴 High | **Breaks many remote-admin tools**: PSExec, Sysinternals workflows, WMI-based remote execution, some deployment/monitoring agents. |
| ASR-009 | Block untrusted/unsigned processes from USB | 🟡 Medium | Blocks running unsigned binaries directly from removable media. Test field-engineer USB toolkits. |
| ASR-010 | Block executable content from email and webmail | 🟢 Low | Blocks executables/scripts dropped by email clients. May affect users who legitimately receive .exe files. |
| ASR-011 | Block Adobe Reader from creating child processes | 🟢 Low | Breaks Reader workflows that launch external editors / form helpers. |
| ASR-012 | Block persistence through WMI event subscription | 🟢 Low | Can affect monitoring/management tools that legitimately use permanent WMI subscriptions. |
| ASR-013 | Use advanced ransomware protection | 🟡 Medium | Cloud-driven heuristic; may produce false positives on backup, sync, or encryption tools that touch many files quickly. |
| ASR-014 | Block executables unless prevalent / aged / trusted | 🔴 High | **Will block freshly built or internal binaries** with no public reputation — very disruptive on developer machines. |
| ASR-015 | Block abuse of vulnerable signed drivers | 🟡 Medium | Blocks known-vulnerable signed drivers — may break legacy hardware utilities, some overclocking/diagnostic tools. |

---

## 🔥 Firewall (`FW-xxx`)

Firewall settings cover three profiles — **Domain**, **Private**, **Public** — with the same options per profile.

| ID | Setting | Impact | Potential Consequences |
|---|---|---|---|
| FW-001 / FW-007 / FW-013 | Enable Firewall (Domain / Private / Public) | 🟢 Low | Turns on Windows Firewall for that profile. Strongly recommended; leaving it off relies on perimeter security only. |
| FW-002 / FW-008 / FW-014 | Default Inbound Action = Block (Domain / Private / Public) | 🟡 Medium | Blocks unsolicited inbound connections by default. Required apps must have explicit allow rules — verify file shares, RDP listeners, dev servers, and third-party agents still work. |
| FW-003 / FW-009 / FW-015 | Default Outbound Action = Allow (Domain / Private / Public) | 🟢 Low | Sets the conventional default. Does not introduce restrictions; intentionally leaves outbound open for compatibility. |
| FW-004 / FW-010 / FW-016 | Disable Notifications (Domain / Private / Public) | 🟢 Low | Suppresses the popup when a new app is blocked. End users won't know why an app is being blocked — make sure admins can review logs. |
| FW-005 / FW-011 / FW-017 | Enable Log Dropped Packets (Domain / Private / Public) | 🟢 Low | Increases firewall log file size. Rotate / monitor `pfirewall.log`. |
| FW-006 / FW-012 / FW-018 | Enable Log Successful Connections (Domain / Private / Public) | 🟡 Medium | **High log volume**, especially on busy servers. Monitor disk usage and rotate logs. |

---

## 📐 CIS Benchmark (`CIS-xxx`)

| ID | Setting | Impact | Potential Consequences |
|---|---|---|---|
| CIS-001 | Disable SMBv1 Server | 🟡 Medium | Breaks file sharing with very old clients (XP, some NAS/MFP firmware). Verify no legacy clients depend on SMBv1 before enabling. |
| CIS-002 | Disable SMBv1 Client | 🟡 Medium | Cannot connect to SMBv1-only servers (legacy NAS, old Linux Samba, some appliances). |
| CIS-003 | Require NTLMv2 / refuse LM & NTLM | 🔴 High | Breaks authentication to systems that only support older NTLM/LM (legacy servers, some appliances, ancient print systems). |
| CIS-004 | Restrict Anonymous Access to Named Pipes / Shares | 🟡 Medium | Some legacy enumeration / monitoring tools rely on anonymous access. |
| CIS-005 | Restrict Anonymous Enumeration of SAM Accounts | 🟡 Medium | Breaks tools that anonymously enumerate local accounts (some auditing utilities). |
| CIS-006 | Restrict Anonymous Enumeration of Shares | 🟡 Medium | Anonymous "browse" of shares no longer works; users need authenticated sessions. |
| CIS-007 | Require NLA for Remote Desktop | 🟡 Medium | Older RDP clients (pre-Windows 7 era, some thin clients) cannot connect. |
| CIS-008 | RDP Encryption Level = High | 🟡 Medium | Same compatibility concern as CIS-007 for very old RDP clients. |
| CIS-009 / CIS-010 | SMB Signing — Server (Required / Enabled) | 🟡 Medium | Clients without SMB signing cannot connect to this server. Test legacy Linux/macOS clients. |
| CIS-011 / CIS-012 | SMB Signing — Client (Required / Enabled) | 🟡 Medium | This machine cannot connect to servers without SMB signing. |
| CIS-013 | Disable Autorun (all drives) | 🟢 Low | No autorun popups for inserted media. End users may need to be shown how to launch installers manually. |
| CIS-014 | Disable Autoplay | 🟢 Low | No "What do you want to do with this drive?" prompt. |
| CIS-015 | Enable Safe DLL Search Mode | 🟢 Low | Some legacy in-house apps that depend on CWD-relative DLL loading may fail. |
| CIS-016 | Mandatory ASLR | 🟡 Medium | Forces ASLR on all images. Very old or poorly written native binaries may crash. |
| CIS-017 | Enable SEHOP | 🟢 Low | Old 32-bit apps relying on broken SEH chains may crash. Rare. |
| CIS-018 | Disable User-Requested Remote Assistance | 🟡 Medium | End users cannot start a Remote Assistance session for help-desk support. |
| CIS-019 | Disable WDigest Authentication | 🟢 Low | Affects only environments still using WDigest (mostly legacy). Recommended everywhere. |
| CIS-020 | Enable LSA Protection (RunAsPPL) | 🟡 Medium | May block legitimate plug-ins that load into LSASS (older smart-card / SSO software). |
| CIS-021 | Enable Credential Guard | 🟡 Medium | Requires VBS-capable hardware; may conflict with some hypervisors and older drivers. Plan for reboot and driver review. |
| CIS-022 | Disable Windows Script Host | 🔴 High | **Breaks all `.vbs`, `.js`, `.wsf` scripts** — including many legacy logon scripts, installers, MDM scripts. |
| CIS-023 | Enable PowerShell Script Block Logging | 🟢 Low | Generates a lot of event-log volume; size operational logs accordingly. |
| CIS-024 | Enable PowerShell Module Logging | 🟢 Low | Same log-volume consideration as CIS-023. |
| CIS-025 | Disable IPv6 Source Routing | 🟢 Low | Source-routed IPv6 packets are dropped (rarely used legitimately). |
| CIS-026 | Disable IPv4 Source Routing | 🟢 Low | Same as CIS-025 for IPv4. |
| CIS-027 | Prevent ICMP Redirect Override | 🟢 Low | OSPF-driven networks: ensure you do not rely on ICMP redirects for dynamic routing. |
| CIS-028 | Screen Saver Requires Password | 🟢 Low | Users must re-authenticate after screen-saver — minor UX impact. |
| CIS-029 | Disable Unsolicited Remote Assistance | 🟡 Medium | Help-desk staff cannot push an unsolicited Remote Assistance offer to users. |
| CIS-030 | Disable Anonymous SID/Name Translation | 🟡 Medium | Tools that rely on anonymous SID resolution (some monitoring/auditing) may fail. |

---

## 👤 Account Policy (`ACCT-xxx`)

| ID | Setting | Impact | Potential Consequences |
|---|---|---|---|
| ACCT-001 | Account Lockout Threshold (5 attempts) | 🟡 Medium | Five failed logons lock the account. Mistyped passwords, stale stored credentials, or scripted services with old passwords can trigger lockouts. |
| ACCT-002 | Account Lockout Duration (30 min) | 🟡 Medium | Locked accounts are unusable for 30 minutes unless an admin unlocks them. |
| ACCT-003 | Disable Guest Account | 🟢 Low | Guest logon is no longer possible. Almost always desired. |
| ACCT-004 | Audit Policy Subcategory Override | 🟢 Low | Granular audit subcategory settings take precedence over legacy categories — ensures modern audit policies behave as configured. |
| ACCT-005 | Crash On Audit Fail | 🔴 High | **Bluescreens the system** if it cannot write security events. Use only on high-assurance systems with monitored event-log infrastructure; not appropriate for user workstations. |

---

## 🌐 Network Security (`NET-xxx`)

| ID | Setting | Impact | Potential Consequences |
|---|---|---|---|
| NET-001 | Disable LLMNR | 🟢 Low | Closes a common MITM vector. Hosts using LLMNR-only name resolution (rare) won't be discoverable. |
| NET-002 | Disable NetBIOS over TCP/IP | 🟡 Medium | Breaks discovery / browsing on networks that still rely on NetBIOS (legacy file shares, old printers). |
| NET-003 | Disable WPAD | 🟢 Low | Auto-proxy discovery is disabled — set proxy explicitly if required by your network. |
| NET-004 | Enable DNS over HTTPS | 🟡 Medium | Forces DoH for upstream DNS where supported; may bypass internal DNS-based filtering / split-horizon DNS. |
| NET-005 | Disable WINS Resolution | 🟡 Medium | Networks still using WINS for name resolution will lose lookups. |
| NET-006 / NET-007 | Enable TLS 1.2 (Client / Server) | 🟢 Low | Enables modern TLS. No downside on any supported Windows release. |
| NET-008 / NET-009 | Disable SSL 2.0 (Client / Server) | 🟢 Low | Removes a long-deprecated, broken protocol. |
| NET-010 / NET-011 | Disable SSL 3.0 (Client / Server) | 🟢 Low | Removes the protocol vulnerable to POODLE. |
| NET-012 / NET-013 | Disable TLS 1.0 (Client / Server) | 🟡 Medium | Some legacy client/server software, embedded devices, and old .NET Framework apps still negotiate TLS 1.0 only. |
| NET-014 / NET-015 | Disable TLS 1.1 (Client / Server) | 🟡 Medium | Same compatibility considerations as NET-012/013 for TLS 1.1. |

---

## Tips for Reducing Disruption

- **Apply per category, not all at once.** Start with `WindowsDefender`, `Firewall`, and `NetworkSecurity` (mostly Low impact); leave `CisBenchmark` for last.
- **Use `--dry-run` everywhere.** Both `enable` and `profile --apply` support it.
- **Stage ASR rules in Audit first.** ASR rules can be set to `2` (Audit) outside this tool to gather telemetry before flipping to Block.
- **Keep a recent backup file checked in / archived** before each hardening session.
- **Watch the audit log** at `%LOCALAPPDATA%\WindowsSecurityManager\wsm-audit.log` to track every change made.

## See Also

- [Security Profiles](security-profiles.md) — profile-level summaries and side-effect notes.
- [Backup & Restore](backup-and-restore.md) — full rollback workflow.
- [CLI Reference](cli-reference.md) — all commands and options, including `--dry-run`.
