# Changelog

What changed in each version of Morfa. Newest version on top.

## v2.1.0

## New features

- **A donut at the cursor.** Hold a key, drag your files and drop them on a format; only formats that fit the files appear.
- **263 formats, no other software.** Images, PDF, Word, Excel, PowerPoint, HWP, e-books, archives, fonts, CAD, 3D models and geospatial data work without Office, Hancom Office or LibreOffice.
- **Straight from the browser.** Drag a picture, a link to a file or a download out of Chrome, Edge, Firefox or Safari the same way.
- **Crop before converting.** The Crop slice opens pictures one by one, and only the chosen part is saved.
- **Cut a clip.** Cut a clip out of a video with a timeline, music, fades and silence removal.
- **Watch folders.** Convert every file that lands in a folder you choose.
- **Two sides to every donut.** Press Tab or turn the mouse wheel while it is open to reach less common formats and tools.
- **English and Indonesian UI.** English by default; switch to Bahasa Indonesia in Settings.

## Optimizations

- **Parallel conversions.** Many files convert at once, and multi-step routes (HWP to HWPX to DOCX) are found automatically.
- **Recent conversions.** Every result shows on Home with a thumbnail.

## Usage

Download the package for your system from the [Releases](https://github.com/nekonaru/morfa/releases) page, or build from source with the .NET 10 SDK.

```bash
morfa convert photo.heic --to jpg
morfa watch D:\Inbox --to pdf --move-originals
```
