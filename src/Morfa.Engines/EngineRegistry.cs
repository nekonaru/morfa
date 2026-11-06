// THE place where conversion engines are registered. Adding an engine = one line in CreateAll.

using Microsoft.Extensions.Logging;
using Morfa.Core.Conversion;
using Morfa.Engines.Archives;
using Morfa.Engines.Cad;
using Morfa.Engines.Data;
using Morfa.Engines.Ebooks;
using Morfa.Engines.Email;
using Morfa.Engines.Fonts;
using Morfa.Engines.Geo;
using Morfa.Engines.Hwp;
using Morfa.Engines.Hwp.Hwpx;
using Morfa.Engines.Icns;
using Morfa.Engines.Infrastructure;
using Morfa.Engines.Magick;
using Morfa.Engines.Media;
using Morfa.Engines.Models3D;
using Morfa.Engines.Ocr;
using Morfa.Engines.Office;
using Morfa.Engines.Pdf;
using Morfa.Engines.Subtitles;
using Morfa.Engines.Text;
using Morfa.Engines.Vector;
using Morfa.Engines.Wic;

namespace Morfa.Engines;

/// <summary>Creates every built-in converter.</summary>
public static class EngineRegistry
{
    /// <summary>
    /// Instantiates all converters. Order only matters as a tie-breaker when no priority is set, so it mirrors
    /// AppSettings.EnginePriority: built-in engines first, the optional downloads last. Software installed on the
    /// system (Microsoft Office, a system-wide LibreOffice) is never used.
    /// </summary>
    /// <param name="loggers">
    /// The app's log; FFmpeg writes which hardware encoders work and when one failed, LibreOffice when a file needed
    /// its container repacked.
    /// </param>
    public static IReadOnlyList<IConverter> CreateAll(EngineEnvironment env, ILoggerFactory? loggers = null) =>
    [
        new MagickImageConverter(),
        new PdfSharpConverter(),
        new PdfiumConverter(),
        new RhwpConverter(),
        new UnhwpConverter(),
        new MarkdownConverter(),
        new SpreadsheetConverter(),
        new OoxmlConverter(),
        new HwpxConverter(),
        new ArchiveConverter(),
        new VectorConverter(),
        new IcnsConverter(),
        new FontConverter(),
        new DocxWriterConverter(),
        new PdfTextConverter(),
        new PdfSlidesConverter(),
        new PdfTablesConverter(),
        new EmlConverter(),
        new DataConverter(),
        new SubtitleConverter(),
        new WicImageConverter(),
        new FfmpegConverter(loggers?.CreateLogger<FfmpegConverter>()),
        new CadConverter(),
        new ModelConverter(),
        new GeoConverter(),
        new EbookConverter(),
        new PandocConverter(),
        new GhostscriptConverter(),
        new VisionConverter(), // before Tesseract: a Mac's OCR step (Tesseract reads the languages Vision doesn't)
        new TesseractConverter(),
        new CalibreConverter(),
        new LibreOfficeConverter(env, loggers?.CreateLogger<LibreOfficeConverter>()),
    ];

    /// <summary>Finds the PDF merger among the converters.</summary>
    public static IPdfMerger? FindPdfMerger(IEnumerable<IConverter> converters) =>
        converters.OfType<IPdfMerger>().FirstOrDefault();

    /// <summary>Finds the engine that writes several pages into one multi-page TIFF.</summary>
    public static ITiffMerger? FindTiffMerger(IEnumerable<IConverter> converters) =>
        converters.OfType<ITiffMerger>().FirstOrDefault();

    /// <summary>Finds the engine that packs several files into one archive ("Compress into one archive").</summary>
    public static IFileCombiner? FindFileCombiner(IEnumerable<IConverter> converters) =>
        converters.OfType<IFileCombiner>().FirstOrDefault();

    /// <summary>The engine that joins videos, audio files and pictures into one (FFmpeg).</summary>
    public static IMediaJoiner? FindMediaJoiner(IEnumerable<IConverter> converters) =>
        converters.OfType<IMediaJoiner>().FirstOrDefault();
}
