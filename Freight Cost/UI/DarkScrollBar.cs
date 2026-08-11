using System.Drawing;
using System.Windows.Forms;

namespace Freight_Cost.UI;

internal enum DarkScrollOrientation
{
    Vertical,
    Horizontal
}

/// <summary>
/// Lightweight scrollbar with a black track and white thumb. WinForms' native
/// scrollbars are painted by Windows and do not expose dependable color styling.
/// </summary>
internal sealed class DarkScrollBar : Control
{
    private const int MinimumThumbLength = 24;

    private int _value;
    private int _maximum;
    private int _largeChange = 1;
    private bool _dragging;
    private int _dragOffset;

    internal DarkScrollBar(DarkScrollOrientation orientation)
    {
        Orientation = orientation;
        BackColor = Color.FromArgb(32, 32, 32);
        ForeColor = Color.White;
        Cursor = Cursors.Hand;
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.UserPaint,
            true);
    }

    internal DarkScrollOrientation Orientation { get; }
    internal int Minimum => 0;

    internal int Maximum
    {
        get => _maximum;
        set
        {
            _maximum = Math.Max(0, value);
            Value = _value;
            Invalidate();
        }
    }

    internal int LargeChange
    {
        get => _largeChange;
        set
        {
            _largeChange = Math.Max(1, value);
            Value = _value;
            Invalidate();
        }
    }

    internal int SmallChange { get; set; } = 1;

    internal int Value
    {
        get => _value;
        set
        {
            var next = Math.Clamp(value, Minimum, MaximumValue);
            if (_value == next)
            {
                return;
            }

            _value = next;
            Invalidate();
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    internal event EventHandler? ValueChanged;

    private int MaximumValue => Math.Max(Minimum, Maximum - LargeChange + 1);
    private int TrackLength => Orientation == DarkScrollOrientation.Vertical ? ClientSize.Height : ClientSize.Width;

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.Clear(Color.FromArgb(32, 32, 32));

        if (!Enabled || MaximumValue <= Minimum || !TryGetThumbRectangle(out var thumb))
        {
            return;
        }

        const int thumbThickness = 4;
        var visualThumb = Orientation == DarkScrollOrientation.Vertical
            ? new Rectangle(
                Math.Max(0, (ClientSize.Width - thumbThickness) / 2),
                thumb.Top,
                Math.Min(thumbThickness, ClientSize.Width),
                thumb.Height)
            : new Rectangle(
                thumb.Left,
                Math.Max(0, (ClientSize.Height - thumbThickness) / 2),
                thumb.Width,
                Math.Min(thumbThickness, ClientSize.Height));

        using var brush = new SolidBrush(Color.White);
        e.Graphics.FillRectangle(brush, visualThumb);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (!Enabled || e.Button != MouseButtons.Left || !TryGetThumbRectangle(out var thumb))
        {
            return;
        }

        var pointer = GetPointerPosition(e.Location);
        var thumbStart = GetRectangleStart(thumb);
        var thumbEnd = thumbStart + GetRectangleLength(thumb);

        if (pointer >= thumbStart && pointer <= thumbEnd)
        {
            _dragging = true;
            _dragOffset = pointer - thumbStart;
            Capture = true;
            return;
        }

        Value += pointer < thumbStart ? -LargeChange : LargeChange;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (!_dragging || !TryGetThumbRectangle(out var thumb))
        {
            return;
        }

        var availableTrack = Math.Max(1, TrackLength - GetRectangleLength(thumb));
        var requestedPosition = Math.Clamp(
            GetPointerPosition(e.Location) - _dragOffset,
            0,
            availableTrack);
        Value = (int)Math.Round(requestedPosition * (double)MaximumValue / availableTrack);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _dragging = false;
        Capture = false;
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        Value += e.Delta > 0 ? -SmallChange : SmallChange;
    }

    private bool TryGetThumbRectangle(out Rectangle thumb)
    {
        thumb = Rectangle.Empty;
        if (TrackLength <= 0 || Maximum < 1)
        {
            return false;
        }

        var contentLength = Maximum + 1;
        var thumbLength = Math.Clamp(
            (int)Math.Round(TrackLength * (double)LargeChange / contentLength),
            Math.Min(MinimumThumbLength, TrackLength),
            TrackLength);
        var availableTrack = Math.Max(0, TrackLength - thumbLength);
        var thumbPosition = MaximumValue == 0
            ? 0
            : (int)Math.Round(availableTrack * (double)Value / MaximumValue);

        thumb = Orientation == DarkScrollOrientation.Vertical
            ? new Rectangle(0, thumbPosition, ClientSize.Width, thumbLength)
            : new Rectangle(thumbPosition, 0, thumbLength, ClientSize.Height);
        return true;
    }

    private int GetPointerPosition(Point point) =>
        Orientation == DarkScrollOrientation.Vertical ? point.Y : point.X;

    private int GetRectangleStart(Rectangle rectangle) =>
        Orientation == DarkScrollOrientation.Vertical ? rectangle.Top : rectangle.Left;

    private int GetRectangleLength(Rectangle rectangle) =>
        Orientation == DarkScrollOrientation.Vertical ? rectangle.Height : rectangle.Width;
}
