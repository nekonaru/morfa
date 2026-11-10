# Project

- PROJECT_NAME: Morfa
- PROJECT_KIND: app
- PRODUCT: Morfa (Drag. Drop. Converted.)
- What it is: Free, open-source file converter for Windows, macOS and Linux. Hold a key, drag files, drop them on a format in the donut at the cursor; 263 formats are converted locally, without uploading anything.
- Stack: .NET 10, C#, Avalonia (desktop UI), xUnit (tests)
- Repository: https://github.com/nekonaru/morfa
- License: MIT
- Language: English is the source for code, comments, commit messages and README.md; Bahasa Indonesia is the translation (README.id.md, CHANGELOG.id.md)
- How to build and test: `dotnet build Morfa.slnx` then `dotnet test Morfa.slnx`; CI runs a formatting check (`dotnet format --verify-no-changes`), the build and the tests on Windows, plus packaging jobs
