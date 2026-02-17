using System.Drawing;
using System.Windows.Forms;

namespace Freight_Cost.UI;

internal static class Theme
{
    internal static readonly Color AppBackground = Color.FromArgb(32, 32, 32);
    internal static readonly Color CardBackground = Color.FromArgb(45, 45, 45);
    internal static readonly Color Accent = Color.FromArgb(59, 130, 246);
    internal static readonly Color AccentHover = Color.FromArgb(37, 99, 235);
    internal static readonly Color AccentSoft = Color.FromArgb(52, 64, 86);
    internal static readonly Color TextPrimary = Color.FromArgb(240, 240, 240);
    internal static readonly Color TextMuted = Color.FromArgb(170, 170, 170);
    internal static readonly Color BorderColor = Color.FromArgb(64, 64, 64);

    internal static void Apply(Control root)
    {
        root.BackColor = AppBackground;
        root.ForeColor = TextPrimary;

        foreach (Control control in root.Controls)
        {
            switch (control)
            {
                case TextBox textBox:
                    textBox.BackColor = CardBackground;
                    textBox.ForeColor = TextPrimary;
                    textBox.BorderStyle = BorderStyle.FixedSingle;
                    break;
                case DataGridView dataGridView:
                    dataGridView.BackgroundColor = CardBackground;
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
        button.Padding = new Padding(6);
        button.FlatAppearance.MouseOverBackColor = AccentHover;
    }

    internal static void StyleSecondaryButton(Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.BorderColor = BorderColor;
        button.BackColor = CardBackground;
        button.ForeColor = TextPrimary;
        button.UseVisualStyleBackColor = false;
        button.Cursor = Cursors.Hand;
        button.Padding = new Padding(4);
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(55, 55, 55);
    }

    internal static void StyleGhostButton(Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = Color.Transparent;
        button.ForeColor = TextPrimary;
        button.UseVisualStyleBackColor = false;
        button.Cursor = Cursors.Hand;
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(52, 52, 52);
    }
}
