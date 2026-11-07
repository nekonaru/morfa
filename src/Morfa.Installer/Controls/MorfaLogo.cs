// Morfa's logo (src/Morfa.Desktop/Assets/Icons/morfa.svg) drawn from the same paths, so it can assemble itself: the tile
// swells into place, the M rises, and the amber conversion chevron pops in. Without animations (or after 1.4 s) it is
// simply the logo.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Morfa.Installer.Controls;

public sealed class MorfaLogo : Control
{
    /// <summary>Plays the entrance when shown; off draws the finished logo.</summary>
    public static readonly StyledProperty<bool> AnimateProperty =
        AvaloniaProperty.Register<MorfaLogo, bool>(nameof(Animate), true);

    private const double ViewBox = 512;
    private static readonly TimeSpan Duration = TimeSpan.FromMilliseconds(1400);

    // morfa.svg: the white M, the amber conversion chevron and the tile.
    private static readonly Geometry MarkStroke = Geometry.Parse("M132 344 V172 L224 260 L316 172 V344");
    private static readonly Geometry Chevron = Geometry.Parse("M220 216 L266 256 L220 296");

    private static readonly IBrush TileBrush = new LinearGradientBrush
    {
        StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
        EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
        GradientStops =
        {
            new GradientStop(Color.Parse("#11B3A6"), 0),
            new GradientStop(Color.Parse("#0A6E6E"), 1),
        },
    };

    private static readonly Pen MarkPen = new(Brushes.White, 56) { LineCap = PenLineCap.Round, LineJoin = PenLineJoin.Round };
    private static readonly Pen ChevronPen = new(new SolidColorBrush(Color.Parse("#FFB454")), 46) { LineCap = PenLineCap.Round, LineJoin = PenLineJoin.Round };

    private readonly FrameLoop _loop;
    private double _progress;

    static MorfaLogo()
    {
        AffectsRender<MorfaLogo>(AnimateProperty);
    }

    public MorfaLogo()
    {
        // The logo is never mirrored, also in Arabic.
        FlowDirection = FlowDirection.LeftToRight;
        _progress = SetupMotion.Enabled ? 0 : 1;
        _loop = new FrameLoop(this, elapsed =>
        {
            _progress = Math.Min(1, elapsed / Duration);
            InvalidateVisual();
            return _progress < 1;
        });
    }

    public bool Animate { get => GetValue(AnimateProperty); set => SetValue(AnimateProperty, value); }

    /// <summary>Plays the entrance again.</summary>
    public void Replay()
    {
        if (!SetupMotion.Enabled || !Animate)
            return;
        _progress = 0;
        _loop.Restart();
    }

    private static double Part(double progress, double start, double length) =>
        Math.Clamp((progress - start) / length, 0, 1);

    protected override Size MeasureOverride(Size availableSize)
    {
        var side = Math.Min(double.IsInfinity(availableSize.Width) ? 96 : availableSize.Width,
            double.IsInfinity(availableSize.Height) ? 96 : availableSize.Height);
        return new Size(side, side);
    }

    public override void Render(DrawingContext context)
    {
        var progress = Animate && SetupMotion.Enabled ? _progress : 1;
        var side = Math.Min(Bounds.Width, Bounds.Height);
        if (side <= 0)
            return;
        var scale = side / ViewBox;
        var origin = new Point((Bounds.Width - side) / 2, (Bounds.Height - side) / 2);
        var center = new Point(ViewBox / 2, ViewBox / 2);
        var view = Matrix.CreateScale(scale, scale) * Matrix.CreateTranslation(origin.X, origin.Y);

        // The tile swells into place first.
        var tile = Part(progress, 0, 0.5);
        if (tile > 0)
        {
            var grow = Matrix.CreateTranslation(-center.X, -center.Y)
                     * Matrix.CreateScale(0.55 + 0.45 * BackOut(tile), 0.55 + 0.45 * BackOut(tile))
                     * Matrix.CreateTranslation(center.X, center.Y);
            using (context.PushTransform(grow * view))
            using (context.PushOpacity(Math.Min(1, tile * 1.6)))
                context.DrawRectangle(TileBrush, null, new Rect(0, 0, ViewBox, ViewBox), 116, 116);
        }

        // The M rises next, then the chevron pops in as "what to convert".
        Rise(context, view, MarkStroke, MarkPen, progress, 0.15);
        var chevron = Part(progress, 0.45, 0.4);
        if (chevron > 0)
        {
            var pop = Math.Max(0.001, BackOut(chevron));
            var chevronCenter = new Point(243, 256);
            var grow = Matrix.CreateTranslation(-chevronCenter.X, -chevronCenter.Y)
                     * Matrix.CreateScale(pop, pop)
                     * Matrix.CreateTranslation(chevronCenter.X, chevronCenter.Y);
            using (context.PushTransform(grow * view))
            using (context.PushOpacity(Math.Min(1, chevron * 1.6)))
                context.DrawGeometry(null, ChevronPen, Chevron);
        }
    }

    private static void Rise(DrawingContext context, Matrix view, Geometry path, Pen pen, double progress, double start)
    {
        var local = Part(progress, start, 0.4);
        if (local <= 0)
            return;
        var offset = (1 - BackOut(local)) * -26;
        using (context.PushTransform(Matrix.CreateTranslation(0, offset) * view))
        using (context.PushOpacity(Math.Min(1, local * 1.6)))
            context.DrawGeometry(null, pen, path);
    }

    internal static double EaseOut(double t) => 1 - Math.Pow(1 - t, 3);

    internal static double BackOut(double t)
    {
        const double c1 = 1.70158;
        const double c3 = c1 + 1;
        return 1 + c3 * Math.Pow(t - 1, 3) + c1 * Math.Pow(t - 1, 2);
    }
}
