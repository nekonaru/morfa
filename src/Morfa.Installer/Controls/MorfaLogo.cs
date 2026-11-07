// Morfa's logo (src/Morfa.Desktop/Assets/Icons/morfa.svg) drawn from the same paths, so it can assemble itself: the tile
// swells into place, then the strokes rise one after the other and the amber dot pops in. Without animations (or after
// 1.4 s) it is simply the logo.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace Morfa.Installer.Controls;

public sealed class MorfaLogo : Control
{
    /// <summary>Plays the entrance when shown; off draws the finished logo.</summary>
    public static readonly StyledProperty<bool> AnimateProperty =
        AvaloniaProperty.Register<MorfaLogo, bool>(nameof(Animate), true);

    private const double ViewBox = 512;
    private static readonly TimeSpan Duration = TimeSpan.FromMilliseconds(1400);

    // morfa.svg: the tile, the white strokes (a vertical bar and the rising arc), the amber arc, the amber dot.
    private static readonly Geometry VerticalBar = Geometry.Parse("M116 344 L116 176");
    private static readonly Geometry WhiteArc = Geometry.Parse("M148 176 C 196 176, 224 224, 240 280");
    private static readonly Geometry AmberArc = Geometry.Parse("M232 176 C 268 176, 292 214, 308 268");
    private static readonly Geometry Dot = Geometry.Parse("M358 352 a34 34 0 1 0 68 0 a34 34 0 1 0 -68 0");

    private static readonly IBrush TileBrush = new LinearGradientBrush
    {
        StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
        EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
        GradientStops =
        {
            new GradientStop(Color.Parse("#0EA5A4"), 0),
            new GradientStop(Color.Parse("#0B6E6E"), 1),
        },
    };

    private static readonly IBrush WhiteBrush = Brush("#FFFFFF");

    private static readonly IBrush AmberBrush = Brush("#FFB454");

    private static readonly Pen WhitePen = new(WhiteBrush, 44) { LineCap = PenLineCap.Round };
    private static readonly Pen AmberPen = new(AmberBrush, 40) { LineCap = PenLineCap.Round };

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

    private static IBrush Brush(string color) => new ImmutableSolidColorBrush(Color.Parse(color));

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
                context.DrawRectangle(TileBrush, null, new Rect(0, 0, ViewBox, ViewBox), 112, 112);
        }

        // Then the strokes rise, one after the other.
        Rise(context, view, VerticalBar, WhitePen, progress, 0.18);
        Rise(context, view, WhiteArc, WhitePen, progress, 0.32);
        Rise(context, view, AmberArc, AmberPen, progress, 0.44);

        // The amber dot pops in last.
        var dot = Part(progress, 0.58, 0.36);
        if (dot > 0)
        {
            var pop = Math.Max(0.001, BackOut(dot));
            var dotCenter = new Point(392, 352);
            var grow = Matrix.CreateTranslation(-dotCenter.X, -dotCenter.Y)
                     * Matrix.CreateScale(pop, pop)
                     * Matrix.CreateTranslation(dotCenter.X, dotCenter.Y);
            using (context.PushTransform(grow * view))
            using (context.PushOpacity(Math.Min(1, dot * 1.6)))
                context.DrawGeometry(AmberBrush, null, Dot);
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
