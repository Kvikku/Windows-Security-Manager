# Windows Security Manager — Improvement Ideas

## Functionality
1. **Export reports** — Save compliance reports to JSON, CSV, or HTML for auditing and sharing
2. **Search/filter** — Search settings by keyword across all categories (useful with 98 settings)
3. **Security profiles** — Presets like "CIS Level 1", "Maximum Security", "Developer Workstation" that enable/disable curated sets of settings in one action
4. **Setting detail view** — Drill into a single setting to see full registry path, description, current value, and history

## UX Polish
5. **Live dashboard** — Show per-category compliance bars right on the main menu so you always see your posture at a glance
6. **Auto-refresh after changes** — After enabling/disabling, immediately show updated status instead of returning to menu
7. **Multi-select** — Pick multiple individual settings to enable/disable in one batch using checkboxes

## Operations
8. **Dry-run mode** — Preview what would change before actually writing to the registry
9. **Backup/restore** — Export current registry state before making changes, allowing rollback
10. **Logging** — Write a timestamped audit log of all changes made
