using Freight_Cost.UI;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Freight_Cost;

public sealed class AboutForm : Form
{
    public AboutForm()
    {
        Text = "About Freight Cost";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(460, 420);
        BackColor = Theme.AppBackground;
        Font = new Font("Segoe UI", 10f);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Theme.AppBackground
        };

        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        Controls.Add(root);

        var headerCard = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Theme.CardBackground,
            Padding = new Padding(14)
        };

        var title = new Label
        {
            Text = "Freight Cost Calculator",
            Dock = DockStyle.Top,
            Height = 28,
            ForeColor = Theme.TextPrimary,
            Font = new Font("Segoe UI", 14f, FontStyle.Bold)
        };

        var version = new Label
        {
            Text = "Version 1.0",
            Dock = DockStyle.Top,
            Height = 22,
            ForeColor = Theme.TextMuted
        };

        headerCard.Controls.Add(version);
        headerCard.Controls.Add(title);
        root.Controls.Add(headerCard, 0, 0);

        var bodyCard = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Theme.CardBackground,
            Padding = new Padding(14)
        };

        var message = new Label
        {
            Dock = DockStyle.Fill,
            ForeColor = Theme.TextPrimary,
            TextAlign = ContentAlignment.TopLeft,
            AutoSize = false,
            Font = new Font("Segoe UI", 10f),
            Text =
                "Calculation logic:\n" +
                "• Quote < $250 → multiplier 1.50\n" +
                "• Quote < $1,000 → multiplier 1.33\n" +
                "• Otherwise → multiplier 1.20\n\n" +
                "Length fee rules:\n" +
                "• Option A (8'-20') adds a flat $150\n" +
                "• Option B uses manual C.H. Robinson fee\n\n" +
                "Formula:\n" +
                "Freight Cost = (Quote × multiplier) + flat fee\n" +
                "Rounded to two decimals (AwayFromZero)."
        };

        var linksPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 30,
            BackColor = Theme.CardBackground
        };

        var githubLink = new LinkLabel
        {
            Text = "GitHub Repository",
            LinkColor = Theme.Accent,
            ActiveLinkColor = Theme.Accent,
            VisitedLinkColor = Theme.Accent,
            Margin = new Padding(0, 0, 16, 0)
        };

        githubLink.LinkClicked += (_, _) => Process.Start(new ProcessStartInfo
        {
            FileName = "https://github.com/bmortel/Freight-Cost",
            UseShellExecute = true
        });

        linksPanel.Controls.Add(githubLink);
        bodyCard.Controls.Add(message);
        bodyCard.Controls.Add(linksPanel);
        root.Controls.Add(bodyCard, 0, 1);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            BackColor = Theme.AppBackground
        };

        var okButton = new Button
        {
            Text = "OK",
            Width = 90,
            Height = 30
        };

        Theme.StylePrimaryButton(okButton);
        okButton.Click += (_, _) => Close();

        buttons.Controls.Add(okButton);
        root.Controls.Add(buttons, 0, 2);

        AcceptButton = okButton;
    }
}
