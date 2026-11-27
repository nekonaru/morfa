# Spec: v2 workflows (F2)

Goal: let a user save a repeatable sequence of conversion steps as a portable JSON file and run the same sequence from the CLI.

## Inputs and outputs

| Feature | Input | Output |
|---------|-------|--------|
| Recipe file | JSON with `format: 2`, id, name, ordered steps | Validated `Recipe` object or a named validation error |
| Recipe compiler | Recipe + source files + target format | Ordered list of `Preset`s, one per step, with the last step producing the final output |
| CLI run | `morfa run recipe.json FILES...` | One line per final file; same exit codes as `convert` |
| Round trip | Recipe saved, loaded, saved | Identical JSON content apart from whitespace |

## Constraints

- Each step uses `IConverter` through `JobQueue`; recipes never write files themselves.
- Intermediate files live in a work folder created by the queue and are deleted after the run.
- Unknown operations, missing engines and cycles are validation errors, not silent no-ops.
- Recipe ids are stable and used in history and in `morfa run`.
- English source code and comments; Indonesian only in README.id.md and CHANGELOG.id.md.

## Done criteria, machine checkable

1. A recipe with `resize` then `convert` applies both steps in order; a fake converter records two conversions with the second input equal to the first output.
2. A recipe with an unknown operation fails validation with an error naming the operation and the step index.
3. A recipe with `format: 1` is refused with a clear version error.
4. Save then load then save produces the same recipe JSON.
5. `morfa run recipe.json photo.png --json` exits 0, reports one converted file, and creates the target output.
6. A missing recipe file gives exit code 2 and a message naming the path.
7. `dotnet build Morfa.slnx` reports 0 errors and 0 warnings.
8. `dotnet test Morfa.slnx` passes in every test project.
9. `dotnet format Morfa.slnx --verify-no-changes --no-restore --severity warn` exits 0.

## Deferred to F4

The visual node editor and recipe library UI are F4. F2 delivers the file format, compiler, validation and CLI execution.
