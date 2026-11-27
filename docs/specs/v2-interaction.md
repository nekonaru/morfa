# Spec: v2 donut interaction (F1)

Goal: make the donut usable without a pointer, let frequent formats surface at the top, and place the radial window correctly on multi-monitor setups.

## Inputs and outputs

| Feature | Input | Output |
|---------|-------|--------|
| Keyboard navigation | Left/Right/Up/Down, Enter, Escape, Tab on the open donut | Selection moves around visible slices; Enter activates the selection; Escape closes; Tab flips the side |
| Type to filter | Printable text while the donut is open | Only matching slices remain visible and selectable; Backspace shortens the filter; Escape clears it before closing |
| Adaptive order | Recent history entries and the current item list | Formats used recently are offered first, without removing any format |
| Multi-monitor | Screen and pointer position when the donut opens | The radial window is centred near the pointer but kept inside that screen's working area |
| Quick tools | Existing tool presets | Tool presets are reachable from the donut's second side without duplicating their logic |

## Constraints

- Pointer behaviour stays unchanged. Keyboard and filter paths add to it, they do not replace it.
- Filtering must never hide an item permanently: clearing the filter restores the original list.
- Activation goes through the same command/event path as a click, so conversion and tool presets behave identically.
- No new network call, no new settings file. Recent order is derived from existing history.
- English source code and comments; Indonesian only in README.id.md and CHANGELOG.id.md.

## Done criteria, machine checkable

1. Headless test: Right and Left move the selection to the next and previous visible slice; Enter raises activation exactly once for the selected item.
2. Headless test: Tab switches the visible side; Escape closes when the filter is empty but clears the filter first when it is not.
3. Headless test: typing "pn" leaves only PNG visible; Backspace restores the full list.
4. Test: a recent format from history is ordered ahead of an unused format, and no item is lost.
5. Test: the radial window centre stays inside the working area of the screen that contains the pointer.
6. `dotnet build Morfa.slnx` reports 0 errors and 0 warnings.
7. `dotnet test Morfa.slnx` passes in every test project.
8. `dotnet format Morfa.slnx --verify-no-changes --no-restore --severity warn` exits 0.

## Deferred to F4

Touch and stylus gestures need platform-specific event work and are not part of F1. They are listed in the rancangan as F4, not dropped.
