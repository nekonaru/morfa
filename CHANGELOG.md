# Changelog

What changed in each version of Morfa. Newest version on top.

## v1.2.0

Morfa's whole main window and every page were redesigned: two-tier top navigation, a distinct layout per page, and panels that stack below the main content when the window is narrow.

## Improvements

- **Two-tier top bar.** The sidebar is gone: the first row carries the brand, the four groups and the download, GitHub, Sponsor and version tools; the second row shows the pages of the selected group and remembers the last one you used.
- **A distinct layout per page.** Home opens with a bigger drop zone and a recent-conversions column; the donut toolbar editor is built around a profile pill row and a full-width palette tray; presets are a card gallery next to the editor; shortcuts open on a hook status strip with two bottom panels; watch folders are a flow of panels; engines switch between optional packages and the priority order; theme gains a live preview that stays visible while you scroll; general lists its sections in a table of contents; about opens with a hero and three panels side by side.
- **Panels stack when the window is narrow.** At the minimum width, the side panels drop below the main content instead of squeezing or scrolling sideways.

## Bug fixes

- **The donut toolbar page opens at the top.** The palette tabs no longer scroll the page down to the selected tab.


## v1.1.0

New users can try Morfa with a bundled sample, skip the engine step without any fuss, and move their presets between PCs.

## New features

- **First run with something to try.** A built-in sample picture waits on Home: one click converts it to JPEG, so the first conversion needs no file of your own.
- **Engines on your terms.** The engine step now leads with skipping; most formats already work, and the engines stay optional until a format actually needs one.
- **Several ways to sponsor.** The Sponsor window returns with five options: GitHub Sponsors, Saweria, SociaBuzz, Bagibagi and Buy Me a Coffee.
- **Safer preset imports.** A preset bundle saved by a newer format is now refused with a clear message.

## v1.0.0

Morfa now looks and behaves like its own product instead of its Filee ancestor.

## Improvements

- **Own identity, everywhere.** The logo, window icons, Setup and accent colour switched from the inherited donut and purple to the teal morfa mark.
- **Sponsor goes straight to GitHub.** The Sponsor button now opens GitHub Sponsors directly instead of a window listing outside services.

## v0.1.1

The logo, window icons, Setup and accent colours now use the product's own identity.

## Improvements

- **Own identity, everywhere.** The logo, window icons, Setup and accent colour switched from the inherited donut and purple to the teal morfa mark.
- **Sponsor goes straight to GitHub.** The Sponsor button now opens GitHub Sponsors directly instead of a window listing outside services.

## v0.1.0

Morfa's first standalone release: the drag-and-drop file converter, reworked from Filee's source.

## New features

- **A donut at the cursor.** Hold a key, drag your files and drop them on a format; only formats that fit the files appear.
- **263 formats, no other software.** Images, PDF, Word, Excel, PowerPoint, HWP, e-books, archives, fonts, CAD, 3D models and geospatial data work without Office, Hancom Office or LibreOffice.
- **Straight from the browser.** Drag a picture, a link to a file or a download out of Chrome, Edge, Firefox or Safari the same way.
- **Crop before converting.** The Crop slice opens pictures one by one, and only the chosen part is saved.
- **Cut a clip.** Cut a clip out of a video with a timeline, music, fades and silence removal.
- **Watch folders.** Convert every file that lands in a folder you choose.
- **Two sides to every donut.** Press Tab or turn the mouse wheel while it is open to reach less common formats and tools.
- **English and Indonesian UI.** English by default; switch to Bahasa Indonesia in Settings.

## Improvements

- **Inspired by Filee.** Morfa is a reimplementation inspired by [Filee](https://github.com/KnifeLemon/Filee), reworked into its own project with its own namespaces, structure and identity. The converter engine catalog, pipeline and donut toolbar design come from Filee's MIT-licensed source.
- **Parallel conversions.** Many files convert at once, and multi-step routes (HWP to HWPX to DOCX) are found automatically.
- **Recent conversions.** Every result shows on Home with a thumbnail.

