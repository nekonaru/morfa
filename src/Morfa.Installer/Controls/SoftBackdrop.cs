// The soft background of Setup's card: one broad accent wash plus a soft secondary tint, drifting slowly
// (about 30 fps, only while the window is visible and animations are on; otherwise they stay where they are).

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Morfa.Installer.Controls;

public sealed class SoftBackdrop : Control
{
    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        Border.BackgroundProperty.AddOwner<SoftBackdrop>();

    public static readonly StyledProperty<Color> AccentProperty =
        AvaloniaProperty.Register<SoftBackdrop, Color>(nameof(Accent), Color.Parse("#0EA5A4"));

    public static readonly StyledProperty<IBrush?> SecondProperty =
        AvaloniaProperty.Register<SoftBackdrop, IBrush?>(nameof(Second), new SolidColorBrush(Color.Parse("#ECEBE6")));

    /// <summary>How strong the wash is (lower in dark mode).</summary>
    public static readonly StyledProperty<double> IntensityProperty =
        AvaloniaProperty.Register<SoftBackdrop, double>(nameof(Intensity), 0.22);

    private readonly FrameLoop _loop;
    private double _time = 1.7;
    private TimeSpan _lastDrawn = TimeSpan.FromSeconds(-1);

    static SoftBackdrop()
    {
        AffectsRender<SoftBackdrop>(BackgroundProperty, AccentProperty, SecondProperty, IntensityProperty);
    }

    public SoftBackdrop()
    {
        IsHitTestVisible = false;
        _loop = new FrameLoop(this, elapsed =>
        {
            if (elapsed - _lastDrawn < TimeSpan.FromMilliseconds(33))
                return true;
            _lastDrawn = elapsed;
            _time = 1.7 + elapsed.TotalSeconds;
            InvalidateVisual();
            return true;
        });
    }

    public IBrush? Background { get => GetValue(BackgroundProperty); set => SetValue(BackgroundProperty, value); }
    public Color Accent { get => GetValue(AccentProperty); set => SetValue(AccentProperty, value); }
    public IBrush? Second { get => GetValue(SecondProperty); set => SetValue(SecondProperty, value); }
    public double Intensity { get => GetValue(IntensityProperty); set => SetValue(IntensityProperty, value); }

    public override void Render(DrawingContext context)
    {
        var bounds = new Rect(Bounds.Size);
        if (Background is { } background)
            context.FillRectangle(background, bounds);
        var w = bounds.Width;
        var h = bounds.Height;
        var t = _time;
        var secondColor = Second is SolidColorBrush solid ? solid.Color : Color.Parse("#ECEBE6");
        // One broad accent wash drifting slowly, one barely-there warm neutral from the opposite corner.
        Wash(context, Accent, w * (0.22 + 0.04 * Math.Sin(t * 0.17)), h * (0.18 + 0.05 * Math.Cos(t * 0.15)), w * 0.62, Intensity);
        Wash(context, secondColor, w * (0.82 - 0.04 * Math.Cos(t * 0.13 + 1.2)), h * (0.88 - 0.04 * Math.Sin(t * 0.11 + 0.8)), w * 0.55, Intensity * 0.8);
    }

    private static void Wash(DrawingContext context, Color color, double x, double y, double radius, double strength)
    {
        var brush = new RadialGradientBrush
        {
            Center = new RelativePoint(0.5, 0.5, RelativeUnit.Relative),
            GradientOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative),
            RadiusX = new RelativeScalar(0.5, RelativeUnit.Relative),
            RadiusY = new RelativeScalar(0.5, RelativeUnit.Relative),
            GradientStops =
            {
                new GradientStop(Color.FromArgb((byte)(255 * strength), color.R, color.G, color.B), 0),
                new GradientStop(Color.FromArgb((byte)(255 * strength * 0.45), color.R, color.G, color.B), 0.55),
                new GradientStop(Color.FromArgb(0, color.R, color.G, color.B), 1),
            },
        };
        context.DrawEllipse(brush, null, new Point(x, y), radius, radius);
    }
}
