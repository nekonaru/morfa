// Generates src/Morfa.Desktop/Assets/Icons/morfa.png and morfa.ico from morfa.svg, the morfa mark.
// Run from the repository root:  dotnet run build/tools/make-icon.cs
#:package Magick.NET-Q8-AnyCPU

using ImageMagick;

var root = Environment.CurrentDirectory;
while (!File.Exists(Path.Combine(root, "Morfa.slnx")))
    root = Path.GetDirectoryName(root) ?? throw new InvalidOperationException("Run this from inside the Morfa repository.");

var icons = Path.Combine(root, "src", "Morfa.Desktop", "Assets", "Icons");
var svgPath = Path.Combine(icons, "morfa.svg");
var svg = await File.ReadAllBytesAsync(svgPath);

const int Size = 512;
var settings = new MagickReadSettings { Format = MagickFormat.Svg, BackgroundColor = MagickColors.Transparent };
using var master = new MagickImage(svg, settings);
if (master.Width != Size || master.Height != Size)
    master.Resize(Size, Size);
master.Write(Path.Combine(icons, "morfa.png"), MagickFormat.Png);

// One image + "auto-resize" makes ImageMagick embed every size into a single .ico file.
master.Settings.SetDefine(MagickFormat.Icon, "auto-resize", "256,64,48,32,24,16");
master.Write(Path.Combine(icons, "morfa.ico"), MagickFormat.Icon);
Console.WriteLine($"Icons written to {icons}");
