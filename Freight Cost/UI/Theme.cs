using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Freight_Cost.UI;

internal static class Theme
{
    // Windows 11 dark palette.
    internal static readonly Color AppBackground = Color.FromArgb(32, 32, 32);
    internal static readonly Color CardBackground = Color.FromArgb(32, 32, 32);
    internal static readonly Color ControlBackground = Color.FromArgb(50, 50, 50);
    internal static readonly Color ControlHover = Color.FromArgb(59, 59, 59);
    internal static readonly Color Accent = Color.FromArgb(8, 36, 66);
    internal static readonly Color AccentHover = Color.FromArgb(13, 50, 88);
    internal static readonly Color AccentPressed = Color.FromArgb(5, 27, 51);
    internal static readonly Color AccentSoft = Color.FromArgb(23, 35, 49);
    internal static readonly Color TextPrimary = Color.FromArgb(243, 243, 243);
    internal static readonly Color TextMuted = Color.FromArgb(196, 196, 196);
    internal static readonly Color BorderColor = Color.FromArgb(52, 52, 52);

    internal static void Apply(Control root)
    {
        if (root is Form form)
        {
            form.BackColor = AppBackground;
            form.ForeColor = TextPrimary;
            form.Font = new Font("Segoe UI", 10f, FontStyle.Regular, GraphicsUnit.Point);
            form.Opacity = 1.0;
            form.HandleCreated += (_, _) => ApplyWindows11Frame(form);

            if (form.IsHandleCreated)
            {
                ApplyWindows11Frame(form);
            }
        }

        foreach (Control control in root.Controls)
        {
            switch (control)
            {
                case TextBox textBox:
                    textBox.BackColor = ControlBackground;
                    textBox.ForeColor = TextPrimary;
                    textBox.BorderStyle = BorderStyle.None;
                    break;
                case DataGridView dataGridView:
                    dataGridView.BackgroundColor = CardBackground;
                    dataGridView.BorderStyle = BorderStyle.None;
                    break;
                case Button button:
                    Round(button, 4);
                    break;
                case Label label when label.ForeColor == SystemColors.ControlText:
                    label.ForeColor = TextPrimary;
                    break;
            }

            Apply(control);
        }
    }

    internal static void StylePrimaryButton(Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = Accent;
        button.ForeColor = Color.White;
        button.UseVisualStyleBackColor = false;
        button.Cursor = Cursors.Hand;
        button.Padding = new Padding(4);
        button.FlatAppearance.MouseOverBackColor = AccentHover;
        button.FlatAppearance.MouseDownBackColor = AccentPressed;
        button.TextAlign = ContentAlignment.MiddleCenter;
        Round(button, 4);
    }

    internal static void StyleSecondaryButton(Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = ControlBackground;
        button.ForeColor = TextPrimary;
        button.UseVisualStyleBackColor = false;
        button.Cursor = Cursors.Hand;
        button.Padding = new Padding(3);
        button.FlatAppearance.MouseOverBackColor = ControlHover;
        button.FlatAppearance.MouseDownBackColor = Color.FromArgb(46, 46, 46);
        button.TextAlign = ContentAlignment.MiddleCenter;
        Round(button, 4);
    }

    internal static void StyleGhostButton(Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = Color.Transparent;
        button.ForeColor = TextPrimary;
        button.UseVisualStyleBackColor = false;
        button.Cursor = Cursors.Hand;
        button.FlatAppearance.MouseOverBackColor = ControlHover;
        button.FlatAppearance.MouseDownBackColor = ControlBackground;
        Round(button, 4);
    }

    private static void Round(Control control, int radius)
    {
        void UpdateRegion()
        {
            if (control.Width <= 0 || control.Height <= 0)
            {
                return;
            }

            using var path = new GraphicsPath();
            var diameter = radius * 2;
            var bounds = new Rectangle(0, 0, control.Width, control.Height);
            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            var oldRegion = control.Region;
            control.Region = new Region(path);
            oldRegion?.Dispose();
        }

        UpdateRegion();
        control.Resize += (_, _) => UpdateRegion();
    }

    private static void ApplyWindows11Frame(Form form)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var enabled = 1;
        _ = DwmSetWindowAttribute(form.Handle, 20, ref enabled, sizeof(int));

        // Prefer Windows 11 rounded corners; unsupported Windows versions ignore it.
        var cornerPreference = 2;
        _ = DwmSetWindowAttribute(form.Handle, 33, ref cornerPreference, sizeof(int));

        // Explicitly disable system backdrops: the client area is fully opaque.
        var backdropType = 1;
        _ = DwmSetWindowAttribute(form.Handle, 38, ref backdropType, sizeof(int));
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr window, int attribute, ref int value, int valueSize);
}
