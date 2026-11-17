# Changelog

What changed in each version of Morfa. Newest version on top.

## v1.3.0

Stability refresh: exits cancel in-flight conversions, merge jobs report failures properly, watch folders retry transient problems, and a new CSV export joins Home.

## New features

- **Export history as CSV.** A new button on Home writes the recent conversions (date, preset, state, sources, outputs, failures) to a UTF-8 CSV file.

## Bug fixes

- **Exiting Morfa now cancels running conversions.** Quit used to abandon jobs mid-run, leaving half-written files at user-visible paths.
- **Merged jobs no longer report zero failures.** A merge that converted every part but failed to stitch them now fails the files themselves, so the batch can be retried and watch folders no longer credit it as converted.
- **Failed watch-folder batches are re-queued with fresh stability.** Up to three attempts per session before they count as failed.
- **Settings migrations no longer skip when profiles.json is missing.** A migration could be silently lost even with the schema still bumped.
- **The colour of the danger brush in dark mode is readable.** White text on a light red was unreadable; danger is now a solid strong red.
- **Primary CTA buttons styled again.** The `accent` class used by Install, Launch, retry and similar actions had no style; they now render as filled accent buttons.
- **Enabling a context menu with a broken Thunar config fails cleanly.** Only the export fails, not the whole SetContextMenu call.
- **Disabling the context menu keeps going past a broken Thunar config.** The other file manager entries are removed regardless.
- **The Linux installer no longer replaces a foreign `~/.local/bin/morfa`.** It refreshes only a Morfa-created symlink; otherwise it leaves it alone and reports.
- **Linux and macOS uninstallers continue when Morfa cannot clean up its registration.** The rest of the uninstall still runs.
- **Output name resolution stops after 1000 numbered candidates.** A pathological watched folder cannot spin this forever.
- **Settings writes use a delete-share, retry the rename, and sweep stale `.tmp` files at startup.**
- **Passwordless shortcuts are no longer resurrected by the migration.** The migration that re-added empty trigger lists is gone.
- **Trigger entries keep `.exe` matching case-insensitively.** Leading `.exe` was stripped from the middle of names before; now it is stripped only at the end.
- **Markdown media paths rewritten by a Pandoc round-trip are replaced only inside image targets.** A plain string that happened to match the path elsewhere in the document is untouched.
- **SVG conversions free their native Skia picture when done.**
- **XHTML exports keep only recognised http/https/mailto/ftp/ftps/tel link schemes.** Other schemes are kept as text, not turned into clickable links.
- **XML output filters characters that XML 1.0 forbids.** A corrupted unicode escape in a cell no longer breaks the file.
- **FFmpeg concat lists use forward slashes.** Paths from Windows no longer need double-escaping.
- **Watch-folder status updates now run on the UI thread.** Previously the DataContext-bound state may have been updated from a background thread.

## Improvements

- **Recent-conversion thumbnails evict the oldest fifth instead of flushing all.** A full cache no longer reloads the entire list from disk; hot entries survive.
- **Piped concurrency: Planner order is built in linear time and the sanitizer reuses one frozen character-set.** Allocation on every conversion path goes down.
- **Settings schema is only raised, never lowered.** A settings file written by a newer build keeps its newer schema marker on a downgrade.

## v1.2.2

Stability refresh: the queue no longer dies mid-batch, watch folders honor cancellation and retry transient failures, and several converter bugs that could corrupt files are fixed.

## Bug fixes

- **Conversions no longer stop after one file fails.** A job-finished handler that recorded the history could throw and kill the queue worker, leaving every following file stuck. Handlers are now isolated.
- **Watch folders stop cleanly.** The stop token was not passed to the conversion job, so a running batch blocked shutdown forever and rule changes could double-start a watcher. Conversions now honor the token, applying rules is serialized, and the apply race that could leak watchers is gone.
- **Watch folders retry failed batches.** A batch whose conversion could not start (preset deleted, engine missing) was dropped forever. It is now re-queued with a cap of three attempts per file per session.
- **DICOM files no longer overflow their pixel count.** Rows times columns wrapped a 32-bit integer for large images, making the pixel-data guard useless and allocation failures silent.
- **KF8 (Kindle) books no longer write mojibake.** Part files carried a UTF-8 BOM over Latin-1 bytes, so non-ASCII text was unreadable. They are now decoded with the header's code page.
- **Native pixel buffers from Magick.NET are now disposed.** Three call sites kept native memory alive longer than needed.
- **Cleared shortcut lists are no longer resurrected.** A settings migration re-added default triggers whenever the list was empty, so deleting every shortcut did not stick.
- **Settings and watch state are written via a unique temp file.** Two processes (app and CLI) could write to the same `.tmp` file and publish a mixed file.

## Improvements

- **Catalog ordering is allocation-free.** Converter priorities are precomputed once, and the ordered list is built in linear time.
- **Output path sanitizing reuses one frozen character set,** saving the concat allocation on every output path.
- **Clip cutter window uses one route planner** for every dropped path instead of rebuilding one per path.
- **Obsolete APIs replaced.** `PlaceholderText` instead of `Watermark`; `BoundingBox` instead of removed `Bounds` / `GlyphRectangle`; `cref` qualified for clarity.

## v1.2.1

Morfa ships the "What's new in Morfa 1.2.0" slides, a tidier About page and a softer chrome, and drops the first-run sample.

## New features

- **What's new in Morfa 1.2.0.** Updating now plays three slides: the two-row top bar, the general page scrolling by, and the panels dropping below the content on narrow windows.

## Improvements

- **A softer chrome.** The window, every page and Setup were restyled: warmer page and surface colours, smaller type, rounder corners, lighter shadows.
- **The first-run sample is gone.** The "Convert the sample" card no longer appears on Home; Morfa opens on a clean drop zone.

## Bug fixes

- **The third-party notices on About are readable.** They open full width now: the heading is no longer clipped and the licence table wraps normally instead of one character per line.

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

