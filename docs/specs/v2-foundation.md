# Spec: v2 foundation (F0)

Goal: make conversion work survive a restart, let users inspect a plan before writing output, give the CLI a self-check command, and prepare settings for the v2 features.

## Inputs and outputs

| Feature | Input | Output |
|---------|-------|--------|
| Persistent queue | Job metadata (id, state, priority, sources, preset, recipe, watch rule, attempts) | `.queue-v2.json` in the Morfa data directory; jobs restored on startup |
| Dry run | Same arguments as `morfa convert`, plus `--dry-run` | Human readable plan or JSON events; no output file created |
| Doctor | `morfa doctor [--fix]` | One line per check with ok, warning or error; exit code 0 when no error |
| Settings v2 | v1.3.0 settings, presets, profiles, history | v2 settings with new sections and defaults; old data preserved |

## Constraints

- Everything stays local. No network call is made by doctor unless the user asks for an engine download.
- The queue stores paths and metadata only, never file contents. `attempts`, `maxAttempts`, `watchRuleId` and `recipeId` are written with defaults in F0. Retry and rule logic fill them in F2 and F3.
- Finished jobs are removed from the queue file after seven days. History keeps its own records independently.
- All writes use atomic temp plus rename with retry, matching `UserDataStore`.
- Existing CLI commands and exit codes keep their meaning. New commands are additive.
- The schema marker never goes down. A newer file read by an older build keeps its marker.
- English source code and comments. Indonesian stays in README.id.md and CHANGELOG.id.md.

## Done criteria, machine checkable

1. `dotnet build Morfa.slnx` reports 0 errors and 0 warnings.
2. `dotnet test Morfa.slnx` passes in every test project, with new tests for each feature below.
3. `dotnet format Morfa.slnx --verify-no-changes --no-restore --severity warn` exits 0.
4. Queue store test: save a job in each state, reload, compare id, state, priority, sources, preset, recipe, watch rule, attempts and outputs.
5. Queue recovery test: a job marked Running is restored as Queued; a Paused job stays Paused.
6. Dry run test: `morfa convert <sample> --to png --dry-run` exits 0, prints the target format and output path, and creates no output file.
7. Dry run test: `--dry-run` combined with `--json-events` emits a `plan` event and then exits without a `done` event.
8. Doctor test: a clean data directory returns exit 0; a corrupt settings file returns a nonzero exit and names the file.
9. Migration test: a v1.3.0 settings fixture migrates to v2 with all presets, profiles and history entries intact, new sections present, and the marker raised once.
10. Downgrade test: a v2 settings file read by the v1 marker path keeps the higher marker.
11. Smoke: `dotnet run --project src/Morfa.CommandLine -c Release -- doctor` prints all checks and exits 0 on this machine.
