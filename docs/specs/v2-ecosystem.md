# Spec: v2 ecosystem (F4)

Goal: give the CLI an engine manager and a portable configuration bundle, so engines and settings can be inspected, pinned, backed up and restored without the GUI.

## Inputs and outputs

| Feature | Input | Output |
|---------|-------|--------|
| `morfa engines list` | Installed engine packages | One line per package: id, installed, pinned, version |
| `morfa engines verify` | Installed packages and pinned hashes | One line per component with ok or mismatch |
| `morfa engines pin <id> <version>` | Package id and pinned version | Persisted pin in settings; error when the package is unknown |
| `morfa config export <file>` | Current settings, presets and profiles | One JSON bundle with a version marker |
| `morfa config import <file>` | A bundle file | Restored settings, presets and profiles; refusal on a newer format |

## Constraints

- Offline mode prevents any engine download; it is a settings flag and only checked before network operations.
- Pins are advisory metadata in this slice: they are shown and validated, but engine installers keep using their pinned manifest until a later release.
- Config export never contains file contents, passwords or history outputs.
- Import never overwrites a newer bundle format.
- English source code and comments; Indonesian only in README.id.md and CHANGELOG.id.md.

## Done criteria, machine checkable

1. `morfa engines list` exits 0 and lists every package in the manifest.
2. `morfa engines verify` exits 0 when markers match and nonzero when a marker is wrong.
3. `morfa engines pin ghost 1.0` exits 2 and names the unknown package.
4. A valid pin survives a settings reload.
5. `morfa config export` writes a parseable bundle with settings, presets and profiles.
6. `morfa config import` restores those three lists; importing a bundle with a newer format is refused with exit 2.
7. `dotnet build Morfa.slnx` reports 0 errors and 0 warnings.
8. `dotnet test Morfa.slnx` passes in every test project.
9. `dotnet format Morfa.slnx --verify-no-changes --no-restore --severity warn` exits 0.
