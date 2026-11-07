# Security policy

## Supported versions

Only the [latest release](https://github.com/nekonaru/morfa/releases/latest) gets security fixes. Morfa checks for
updates, and an installed copy updates in place, so please check that the problem is still there in the latest
release before you report it.

## Reporting a vulnerability

Please don't open a public issue for a security problem.

Report it privately with **[Report a vulnerability](https://github.com/nekonaru/morfa/security/advisories/new)** on
the repository's Security tab.

Include the Morfa version, your operating system, what an attacker can do and the steps or file that show it. If a
crafted file triggers it, attach the file, or say how to make one.

You'll get a reply as soon as possible. Once the fix is released, the advisory is published and you're credited
unless you'd rather not be. A release that fixes a security problem is marked *required*, so every copy shows it
each time it starts until it is updated.

## Scope

In scope:

- Morfa itself: the app, the CLI, Setup, the context menu and Explorer menu, watch folders.
- Files that make Morfa do more than convert them, such as writing outside the output folder, running code, or
  reading files they shouldn't.
- How Morfa downloads and checks updates and engines.

Out of scope:

- Bugs in the engines (FFmpeg, LibreOffice, Ghostscript, Pandoc, rhwp, 7-Zip and others). Report those to their
  projects. Tell us too if Morfa should update the pinned version.
- SmartScreen warnings: Windows builds are not code-signed yet. macOS builds are ad-hoc signed but not notarized; Gatekeeper can still warn.
- Attacks that need someone who can already run programs as you on your computer.

## How downloads are checked

- **Updates:** an installed copy downloads the new package from GitHub Releases and checks it against the release's
  `SHA256SUMS` file before it runs it.
- **Engines:** every engine comes from the official location pinned in
  [`engines.json`](src/Morfa.Converters/Infrastructure/engines.json), with its SHA-256 and size. A download that doesn't
  match is discarded.
- **Release files:** each release lists the SHA-256 of every package in `Morfa-<version>-SHA256SUMS.txt`.

Morfa converts files on your own computer and never uploads them. See the [Privacy policy](README.md#about).
