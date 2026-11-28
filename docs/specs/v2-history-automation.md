# Spec: v2 history and automation (F3)

Goal: make history searchable and analysable, and let one watch folder route different files to different presets with bounded retries and retention.

## Inputs and outputs

| Feature | Input | Output |
|---------|-------|--------|
| History search | Text, format, state, preset, watch rule | Filtered list of history entries |
| Pin, tags, notes | Per-entry user edits | Persisted in history.json and shown on the entry |
| Dashboard | History entries | Counts, success rate, bytes, popular formats and presets, per week |
| History export | Entries | UTF-8 JSON file with the same fields as the CSV export |
| Conditional routing | Watch rule plus file name, size and detected format | First matching route's preset or recipe |
| Failure suppression | Repeated failures in a watch folder | Persistent record so a restart does not retry forever |
| Retention | Outputs older than N days | Deleted or moved according to the rule |

## Constraints

- Old history entries and watch rules keep working with defaults.
- Any new history field is optional and back-filled from existing data.
- Watch rules keep their single-preset behaviour when no routes are defined.
- Persistence uses atomic temp-plus-rename as in UserDataStore.
- English source code and comments; Indonesian only in README.id.md and CHANGELOG.id.md.

## Done criteria, machine checkable

1. Searching by text, format and state returns only matching entries; clearing all filters restores the full list.
2. Pin, tags and notes round-trip through history.json and survive a reload.
3. Dashboard counts match the entries (total, success rate, bytes, top format, top preset).
4. JSON export parses and has one object per entry with date, preset, state, sources, outputs and failures.
5. A watch rule with two routes sends a `.jpg` and a `.pdf` to different presets; the first match wins.
6. A rule with no routes behaves exactly as before.
7. A file that failed three times is not reprocessed after a restart.
8. An output older than the retention window is removed on the next tick.
9. `dotnet build Morfa.slnx` reports 0 errors and 0 warnings.
10. `dotnet test Morfa.slnx` passes in every test project.
11. `dotnet format Morfa.slnx --verify-no-changes --no-restore --severity warn` exits 0.
