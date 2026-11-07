<div align="center">
  <img src="src/Morfa.Desktop/Assets/Icons/morfa.png" alt="Morfa logo" width="120" />
  <h1>Morfa: Drag. Drop. Converted.</h1>
  <p><b>Hold a key, drag your files and drop them on a format in the donut that opens at your cursor.<br/>
  Morfa converts 263 formats locally, without uploading anything.</b></p>

  <a href="https://github.com/nekonaru/morfa/releases"><img src="https://img.shields.io/github/v/release/nekonaru/morfa?style=flat-square&color=blue" alt="Release" /></a>
  <a href="https://github.com/nekonaru/morfa/actions/workflows/ci.yml"><img src="https://github.com/nekonaru/morfa/actions/workflows/ci.yml/badge.svg" alt="CI" /></a>
  <img src="https://img.shields.io/badge/.NET-10-512BD4?style=flat-square&logo=dotnet&logoColor=white" alt=".NET 10" />
  <img src="https://img.shields.io/badge/Platform-Windows%20%7C%20macOS%20%7C%20Linux-3C3489?style=flat-square" alt="Platform" />
  <img src="https://img.shields.io/badge/License-MIT-green?style=flat-square" alt="License" />
  <img src="https://img.shields.io/badge/Status-Active-brightgreen?style=flat-square" alt="Status" />
</div>

<br />

**English** | [Bahasa Indonesia](README.id.md)

> [!NOTE]
> Morfa converts files on your own computer and never uploads them. It works offline and saves the result next to the original.

---

## Table of Contents

- [About](#about)
- [Features](#features)
- [Formats](#formats)
- [Requirements](#requirements)
- [Installation](#installation)
- [Usage](#usage)
- [Command Line](#command-line)
- [Display Language](#display-language)
- [Project Structure](#project-structure)
- [Development](#development)
- [Testing](#testing)
- [Continuous Integration](#continuous-integration)
- [FAQ](#faq)
- [Credits](#credits)
- [Author](#author)
- [License](#license)

---

## About

**Morfa** is a free, open-source file converter you use by dragging. Hold a key (Ctrl, Option on macOS), drag your files and drop them on a format in the donut that opens at your cursor. Morfa converts 263 formats locally: images, PDF, Word, Excel, PowerPoint, HWP, e-books, archives, fonts, CAD, 3D models and geospatial data work without Office, Hancom Office or LibreOffice. Nothing is uploaded, and the result is saved next to the original.

Morfa is inspired by, and builds on, [Filee](https://github.com/KnifeLemon/Filee), the open-source file converter it is a reimplementation of. The menu-at-cursor idea, the converter pipeline and the engine design come from Filee; morfa reworks the codebase, structure and identity into its own project.

> _"A free, open-source file converter you use by dragging."_

---

## Features

### Conversion

| Feature | Description |
|---------|-------------|
| **A donut at the cursor** | Hold a key, drag files and drop them on a format; only formats that fit the files appear |
| **263 formats, no other software** | Images, PDF, Word, Excel, PowerPoint, HWP, e-books, archives, fonts, CAD, 3D models and geospatial data work without Office, Hancom Office or LibreOffice |
| **Straight from the browser** | Drag a picture, a link to a file or a download out of Chrome, Edge, Firefox or Safari the same way; Morfa saves it to your Downloads folder and converts it |
| **Many files at once, in parallel** | Routes with several steps (HWP to HWPX to DOCX) are found automatically |
| **Watch folders** | Convert every file that lands in a folder you choose |
| **Recent conversions** | Every result on Home with a thumbnail: open it, or open it with another app |

### Toolbars and Presets

| Feature | Description |
|---------|-------------|
| **One toolbar per file type** | Arrange it by drag and drop, with presets for quality, size, DPI, PDF pages and compression, video and audio, output folder and file names |
| **Two sides to every donut** | Press Tab or turn the mouse wheel while it is open to flip it over to less common formats and tools |
| **Crop before converting** | The Crop slice opens pictures one by one; drag the frame, pick a ratio, and only that part is saved |
| **Cut a clip** | Cut a clip out of a video with a timeline, music, fades and silence removal |

### Other ways in

| Feature | Description |
|---------|-------------|
| **Right-click menu** | Convert with Morfa from Explorer, Finder or your Linux file manager |
| **Keyboard shortcut** | Convert the files selected in your file manager |
| **Drop zone** | Drop files on the Home window |
| **Command line** | The `morfa` command uses the same engines and presets as the app |

### Appearance and Language

| Feature | Description |
|---------|-------------|
| **English and Indonesian UI** | English by default; switch to Bahasa Indonesia in Settings |
| **Themes** | Morfa Soft or Material You, light or dark or follow system, with your own accent colour and donut opacity |
| **Window size** | Make every window larger or smaller, 70 percent to 180 percent |

---

## Formats

| From | To |
|---|---|
| **Images**: JPG, PNG, WEBP, AVIF, TIFF, BMP, GIF, APNG, ICO, ICNS, JPEG XL, JPEG 2000, PSD/PSB, TGA, PPM, HEIC, QOI, DDS, PCX, MNG, CUR; GIMP XCF, camera RAW (CR2, CR3, NEF, ARW, DNG), DICOM, OpenEXR, HDR, FITS, JPEG XR (Windows), iPhone Live Photo (LIVP) read | any of these (HEIC), PDF, SVG (traced), resized, grayscale; Live/Motion Photos to their video, and a video to a Live Photo (iPhone) or Motion Photo (Android), experimental |
| **Vector**: SVG, SVGZ, EMF, WMF, AI; EPS, PS; CDR, VSD, CGM, ODG, FODG | PDF (vector), PNG and other images, SVG to SVGZ, SVG to DXF/DWG, PDF to EPS/PS, SVG to EMF, WMF, ODG |
| **PDF** | DOCX, HWPX, TXT, PNG, JPG, TIFF, CBZ, PPTX (a picture of each page), SVG; merge, split, page ranges, compress |
| **Documents**: DOCX, DOCM, DOTX, EML, Outlook MSG; OFD read; DOC, ODT, FODT, OTT, RTF, WPS, WPD, Pages, ClarisWorks, MacWrite, T602 | PDF, DOCX, HWPX, EPUB, TXT, HTML; MSG to EML; ODT, FODT, OTT |
| **Spreadsheets**: XLSX, XLSM, XLS, XLT, ODS, CSV, TSV; OTS, FODS, Numbers, ET | PDF, XLSX, ODS, CSV, TSV, JSON, HWPX, HTML; XLT, OTS |
| **Presentations**: PPTX, PPTM, POTX, POTM, PPSX; PPT, ODP, OTP, FODP, Keynote | PDF, PNG, JPG, DOCX, HWPX; OTP |
| **HWP**: HWP, HWPX | PDF, PNG, DOCX, HWPX to HWP, TXT, Markdown, HTML |
| **Text**: TXT, LOG, script source (PY, PAC, BAT, COMMAND; read, never run), Markdown, HTML; MHTML read; reStructuredText, LaTeX, Org, AsciiDoc, Typst, MediaWiki, DocBook, Jupyter, Textile, man, OPML | UTF-8 TXT, PDF, DOCX, HWPX, EPUB; each other, PPTX slides |
| **E-books**: EPUB, KEPUB, MOBI, AZW3, AZW, AZW4, FB2, CBZ, CBR, CB7, HTMLZ, TXTZ; LIT, LRF, CHM, PDB | PDF, EPUB, KEPUB (Kobo), DOCX, TXT, HTML, HWPX; MOBI, AZW3 |
| **Video**: MP4, MOV, MKV, WEBM, AVI, WMV, FLV, MPEG, Video CD (DAT), TS, M2TS, 3GP, OGV, VOB | MP4 (H.264, HEVC, AV1), WEBM, MOV, MKV, AVI, GIF, APNG, MP3, M4A, 720p, subtitles (SRT, ASS, WebVTT) |
| **Audio**: MP3, M4A, AAC, WAV, FLAC, OGG, OPUS, WMA, AIFF, AMR, APE, WavPack, TTA, DSD (DSF, DFF), DTS, AC3, E-AC3 | MP3, M4A (AAC, ALAC), WAV, FLAC, OGG, OPUS, AAC, WavPack, TTA, W64, AC3, E-AC3 |
| **Subtitles**: SRT, ASS/SSA, WebVTT, SAMI (SMI), LRC, TTML, MicroDVD, SCC | each other as UTF-8 (Korean CP949 files read correctly) |
| **Archives**: ZIP, ANSYS Workbench (WBPZ), 7Z, RAR, TAR, TAR.GZ, TAR.BZ2, TAR.XZ, GZ, ISO, CAB, DMG, ALZ, EGG | extract, ZIP, 7Z, TAR, TAR.GZ; any files to one ZIP |
| **CAD**: DWG, DXF | PDF, SVG, PNG, DWG to DXF |
| **Fonts**: TTF, OTF, WOFF, WOFF2, EOT | each other |
| **Data**: JSON, JSON Lines, YAML, TOML, XML; vCard, iCalendar | each other, CSV and XLSX (tables); vCard, iCalendar to CSV |
| **3D models**: GLB, glTF, STL, OBJ (+ MTL), 3MF, PLY; FBX read | each other, PNG preview |
| **Geospatial vectors**: Shapefile, GeoJSON, KML, KMZ, GPX; CSV with coordinates | each other and CSV / XLSX / JSON; projected data (UTM, Web Mercator, Lambert, Albers) comes out as WGS 84 longitude / latitude |

Unmarked formats work out of the box. Marked ones need an optional engine, downloaded only when needed: Ghostscript, LibreOffice, Pandoc, calibre, FFmpeg ([details](docs/ENGINES.md)).

---

## Requirements

| Dependency | Minimum Version | Notes |
|------------|-----------------|-------|
| **.NET SDK** | 10.0 | Required only to build from source |
| **Windows** | 10 version 1809 or later, 64-bit | Required to run the app on Windows |
| **macOS** | 13 or later | Required to run the app on macOS |
| **Linux** | Ubuntu 22.04 or later, x64 or ARM64 | Required to run the app on Linux |

> [!TIP]
> If you only want to use Morfa, you do not need the .NET SDK. Download the package from the [Releases](https://github.com/nekonaru/morfa/releases) page instead.

### Never used a terminal? Start here

<details>
<summary><b>How to open a terminal on Windows</b></summary>

1. Press the **Windows** key, type `PowerShell`, then press **Enter**
2. A blue window opens. That is where you type commands
3. Every `dotnet ...` or `git ...` command in this guide is typed in that window, followed by **Enter**

</details>

---

## Installation

### Option A: Download the prebuilt package (easiest)

v0.1.0 ships prebuilt packages for **Linux and macOS only**. There is no Windows installer and no portable `Morfa.exe` in this release (the Windows build flow waits on a signing certificate; the unsigned Windows build is kept as a CI artifact for later signing, it is not published).

| System | Download | How to install |
|--------|----------|----------------|
| **Linux x64** | `Morfa-0.1.0-linux-x64.tar.gz` | Unpack, run `./install.sh` (needs Python 3; no root) |
| **Linux ARM64** | `Morfa-0.1.0-linux-arm64.tar.gz` | Unpack, run `./install.sh` (needs Python 3; no root) |
| **macOS Apple Silicon** | `Morfa-0.1.0-osx-arm64.dmg` | Open the DMG, drag Morfa to Applications |
| **macOS Apple Silicon (Alt)** | `Morfa-0.1.0-osx-arm64.tar.gz` | Unpack, run `./install.sh` |
| **macOS Intel** | `Morfa-0.1.0-osx-x64.dmg` | Open the DMG, drag Morfa to Applications |
| **macOS Intel (Alt)** | `Morfa-0.1.0-osx-x64.tar.gz` | Unpack, run `./install.sh` |

Each package has a matching `-SHA256SUMS.txt` next to it if you want to verify the download.

On macOS, the app is ad-hoc signed and not notarized: on first start, allow it in **System Settings -> Privacy & Security -> Open Anyway**.

> [!IMPORTANT]
> Drag gestures need X11. On Wayland, use the drop zone and the file manager's right-click menu.

### Option B: Windows, or build from source (for development / to read or change the code)

Windows is not in this release. Build it yourself with .NET 10:

```bash
git clone https://github.com/nekonaru/morfa.git
cd morfa
dotnet restore
dotnet build Morfa.slnx
dotnet run --project src/Morfa.Desktop
```

The Windows installer (`Morfa-<version>-win-Setup.exe`) and the portable zip are produced by `build/build-installer.ps1` on Windows but are not part of the v0.1.0 release.

### Option B: Build from source (for development / to read or change the code)

#### 1. Get the project code

<details>
<summary><b>I have Git installed</b></summary>

```bash
git clone https://github.com/nekonaru/morfa.git
cd morfa
```

</details>

<details>
<summary><b>I don't have Git</b></summary>

1. Open the repository page on GitHub
2. Click the green **`Code`** button, choose **`Download ZIP`**
3. Extract the ZIP to a folder of your choice
4. In the terminal, go into the extracted folder, for example:

```bash
cd Downloads/morfa-main
```

</details>

#### 2. Install dependencies and verify

```bash
dotnet restore
dotnet build Morfa.slnx
```

> [!IMPORTANT]
> `dotnet build` must report **0 Errors**.

#### 3. Run the app

```bash
dotnet run --project src/Morfa.Desktop
```

---

## Usage

### First launch

1. Morfa starts in the notification area of the taskbar
2. Hold the trigger key (Ctrl by default) and drag files onto the donut that opens at your cursor
3. Drop them on a format to convert

> [!NOTE]
> The trigger key, the donut toolbar and watch folders are all set in Settings. Nothing runs automatically.

### Everyday use

- Hold the trigger key and drag files to open the donut at your cursor
- Press **Tab** or turn the mouse wheel to flip the donut to its back face
- Drag a slice back to the donut to remove it; right-click a slice or press the edit key to change it
- Open **Home** to see recent conversions with thumbnails
- Right-click files in your file manager, or use the `morfa` command

---

## Command Line

The `morfa` command uses the same engines and presets as the app.

```bash
morfa convert photo.heic --to jpg
morfa convert *.png --to webp --quality 80 -o converted
morfa watch D:\Inbox --to pdf --move-originals
```

Every command and option: `morfa help`.

---

## Display Language

The app UI (menus, buttons, messages, and settings) is available in **English** (default) and **Bahasa Indonesia**.

1. Open **Settings**
2. Under **Language**, pick **English** or **Bahasa Indonesia**
3. The choice is remembered between sessions

---

## Project Structure

```
morfa/
├── src/
│   ├── Morfa.Domain/               # domain model: conversion, formats, presets, settings
│   ├── Morfa.Converters/            # IConverter implementations, one folder per engine
│   ├── Morfa.Platforms.Windows/   # Windows platform services
│   ├── Morfa.Platforms.Linux/     # Linux platform services
│   ├── Morfa.Platforms.MacOS/     # macOS platform services
│   ├── Morfa.Desktop/                # the Avalonia desktop app
│   │   └── Assets/i18n/          # English (en.json) and Indonesian (id.json) strings
│   ├── Morfa.CommandLine/                # the morfa command line
│   ├── Morfa.Installer/              # the Windows installer and uninstaller
│   └── Morfa.ExplorerMenu/       # Windows 11 Explorer menu COM server (plain C)
├── tests/                        # one test project per layer
├── build/                        # build, packaging and release scripts
├── installer/                    # Linux and macOS install assets
├── docs/                         # architecture and engine documentation
├── Directory.Build.props         # shared MSBuild settings
├── Directory.Packages.props      # central package versions
├── Morfa.slnx                    # the solution
├── LICENSE
├── README.md                     # main documentation (English)
└── README.id.md                  # documentation (Bahasa Indonesia)
```

---

## Development

### Adding a converter

1. Implement `IConverter` in a new file under `src/Morfa.Converters`
2. Declare its edges in `Edges`
3. Register it in `EngineRegistry.CreateAll`

> [!WARNING]
> `EngineRegistry.cs` is the single registration point. Conversion routes are planned from the declared edges, so a new engine unlocks new routes without touching other code.

### Adding a user-facing string

1. Add the key to `src/Morfa.Desktop/Assets/i18n/en.json` (English) and `id.json` (Indonesian)
2. Use it via the localizer with the same key

> [!WARNING]
> Keep `en.json` and `id.json` in sync. Both files must have the same keys.

---

## Testing

Verification before every push, both must be clean:

```bash
dotnet build Morfa.slnx
dotnet test
```

| Check | Requirement |
|-------|-------------|
| `dotnet build` | Must report **0 Errors** |
| `dotnet test` | Must pass the full suite |

Lints are enforced through `.editorconfig` and the .NET analyzers.

---

## Continuous Integration

| Workflow | Purpose |
|----------|---------|
| `ci.yml` | Build, format check and tests on every push to main and pull request |
| `portable.yml` | Build the Linux and macOS packages |
| `release.yml` | Build every package and create a draft GitHub release on a `v*` tag |
| `label-issue.yml` | Label new issues by keyword |
| `label-pr.yml` | Label pull requests from `labeler.yml` and the title |

---

## FAQ

<details>
<summary><b>Does Morfa upload my files?</b></summary>

No. Morfa converts files on your own computer and never uploads them. Internet is used only to check for updates, to download the engines you choose to install, and when you click a link.

</details>

<details>
<summary><b>Do I need Office or LibreOffice?</b></summary>

No for most formats. Images, PDF, Word, Excel, PowerPoint, HWP, e-books, archives, fonts, CAD and 3D models convert without them. Some older or rarer formats need an optional engine, downloaded only when you ask for it.

</details>

<details>
<summary><b>How do I change the language?</b></summary>

Settings, section **Language**, pick English or Bahasa Indonesia. English is the default.

</details>

---

## Credits

- **Filee** ([KnifeLemon/Filee](https://github.com/KnifeLemon/Filee)), the inspiration for morfa. Hold a key and drop files on file-type actions from the cursor. The donut toolbar, the converter pipeline and the engine catalog come from Filee's design and MIT-licensed source.
- **Nicolas Dwi Dharma**, morfa's maintainer.

---

## Author

<!-- sponsors: src/Morfa.Desktop/Assets/Sponsors/sponsors.json -->

<div align="center">

| [![Nicolas Dwi Dharma](https://github.com/nekonaru.png?size=100)](https://github.com/nekonaru) |
|:---:|
| **Nicolas Dwi Dharma** |
| [github.com/nekonaru](https://github.com/nekonaru) |

</div>

---

## License

Distributed under the **MIT License**. See [`LICENSE`](LICENSE) for more information.
