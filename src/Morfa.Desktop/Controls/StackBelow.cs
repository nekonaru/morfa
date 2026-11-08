// Side panels stack under the main content when the window gets narrow. Avalonia has no AdaptiveTrigger, so a grid
// marked with StackBelow.Threshold is watched through this property: under the threshold its children move into one
// column (side panels below the main content), above it they go back to their cells. The move is instant, so there
// is no transition for reduced motion to disable.

using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace Morfa.Desktop.Controls;

public static class StackBelow
{
    /// <summary>Width under which the grid's children stack in one column; 0 keeps them in their cells.</summary>
    public static readonly AttachedProperty<double> ThresholdProperty =
        AvaloniaProperty.RegisterAttached<Grid, double>("Threshold", typeof(StackBelow), 0);

    /// <summary>Space the stacked layout puts between two children.</summary>
    public static readonly AttachedProperty<double> GapProperty =
        AvaloniaProperty.RegisterAttached<Grid, double>("Gap", typeof(StackBelow), 16);

    public static double GetThreshold(Grid grid) => grid.GetValue(ThresholdProperty);

    public static void SetThreshold(Grid grid, double value) => grid.SetValue(ThresholdProperty, value);

    public static double GetGap(Grid grid) => grid.GetValue(GapProperty);

    public static void SetGap(Grid grid, double value) => grid.SetValue(GapProperty, value);

    private static readonly ConditionalWeakTable<Grid, Watcher> Watchers = new();

    static StackBelow()
    {
        ThresholdProperty.Changed.AddClassHandler<Grid>((grid, e) =>
        {
            if (e.GetNewValue<double>() > 0)
                Watchers.GetValue(grid, g => new Watcher(g)).Attach();
        });
    }

    /// <summary>One grid's original layout and its current stacked state.</summary>
    private sealed class Watcher(Grid grid)
    {
        private readonly Grid _grid = grid;
        private readonly List<Child> _children = [];
        private readonly List<RowDefinition> _rows = [];
        /// <summary>Children in reading order (top to bottom, left to right), the stacking order.</summary>
        private List<Child> _order = [];
        private bool _attached;
        private bool _narrow;

        public void Attach()
        {
            if (_attached)
                return;
            _attached = true;
            _grid.PropertyChanged += OnGridChanged;
            Update();
        }

        private void OnGridChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == Layoutable.BoundsProperty)
                Update();
        }

        private void Update()
        {
            var threshold = _grid.GetValue(ThresholdProperty);
            if (threshold <= 0 || _grid.Bounds.Width <= 0)
                return;
            if (_children.Count == 0 && _grid.Children.Count > 0)
                Capture();
            var narrow = _grid.Bounds.Width < threshold;
            if (narrow == _narrow)
                return;
            _narrow = narrow;
            if (narrow)
                Stack();
            else
                Restore();
        }

        /// <summary>Remembers where every child and every row was, before the first stacking.</summary>
        private void Capture()
        {
            foreach (var child in _grid.Children)
                _children.Add(new Child(child, Grid.GetColumn(child), Grid.GetRow(child),
                    Grid.GetColumnSpan(child), Grid.GetRowSpan(child), child.Margin));
            _order = [.. _children.OrderBy(c => c.Row).ThenBy(c => c.Column)];
            _rows.AddRange(_grid.RowDefinitions);
        }

        private void Stack()
        {
            var gap = _grid.GetValue(GapProperty);
            var span = Math.Max(1, _grid.ColumnDefinitions.Count);
            _grid.RowDefinitions.Clear();
            for (var i = 0; i < _order.Count; i++)
                _grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            for (var i = 0; i < _order.Count; i++)
            {
                var child = _order[i].Control;
                Grid.SetColumn(child, 0);
                Grid.SetColumnSpan(child, span);
                Grid.SetRow(child, i);
                Grid.SetRowSpan(child, 1);
                if (i > 0)
                    child.Margin = new Thickness(_order[i].Margin.Left, _order[i].Margin.Top + gap,
                        _order[i].Margin.Right, _order[i].Margin.Bottom);
            }
        }

        private void Restore()
        {
            foreach (var child in _children)
            {
                Grid.SetColumn(child.Control, child.Column);
                Grid.SetColumnSpan(child.Control, child.ColumnSpan);
                Grid.SetRow(child.Control, child.Row);
                Grid.SetRowSpan(child.Control, child.RowSpan);
                child.Control.Margin = child.Margin;
            }
            _grid.RowDefinitions.Clear();
            foreach (var row in _rows)
                _grid.RowDefinitions.Add(row);
        }

        private sealed record Child(Control Control, int Column, int Row, int ColumnSpan, int RowSpan, Thickness Margin);
    }
}
