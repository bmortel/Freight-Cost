using System.Drawing;
using System.Windows.Forms;

namespace Freight_Cost.UI;

internal sealed class DarkMenuColors : ProfessionalColorTable
{
    public override Color MenuItemSelected => Theme.Accent;
    public override Color MenuItemBorder => Theme.Accent;
    public override Color MenuBorder => Theme.CardBackground;
    public override Color ToolStripDropDownBackground => Theme.CardBackground;
    public override Color ImageMarginGradientBegin => Theme.CardBackground;
    public override Color ImageMarginGradientMiddle => Theme.CardBackground;
    public override Color ImageMarginGradientEnd => Theme.CardBackground;
    public override Color MenuItemSelectedGradientBegin => Theme.Accent;
    public override Color MenuItemSelectedGradientEnd => Theme.Accent;
    public override Color MenuItemPressedGradientBegin => Theme.AccentHover;
    public override Color MenuItemPressedGradientEnd => Theme.AccentHover;
}
