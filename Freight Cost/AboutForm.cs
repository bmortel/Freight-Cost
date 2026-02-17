using Freight_Cost.Core;
using Freight_Cost.UI;
using System.Diagnostics;


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
        ClientSize = new Size(460, 590);
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

        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
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
            // Application.ProductVersion pulls from your assembly/app version
            Text = $"Version {AppUpdater.CurrentVersion}",
            Dock = DockStyle.Top,
            Height = 22,
            ForeColor = Theme.TextMuted
        };

        var latest = new Label
        {
            Text = "Latest: checking...",
            Dock = DockStyle.Top,
            Height = 22,
            ForeColor = Theme.TextMuted
        };

        Shown += async (_, __) =>
        {
            try
            {
                // Reuse cached updater result to avoid an API call every time About opens.
                var update = await AppUpdater.CheckForUpdateAsync(useCache: true);
                var tag = update.LatestTag ?? update.LatestVersion?.ToString();

                latest.Text = !string.IsNullOrWhiteSpace(tag)
                    ? $"Latest: {tag}"
                    : "Latest: (unknown)";
            }
            catch
            {
                // If work PC blocks GitHub or no internet, don't crash the About form.
                latest.Text = "Latest: (offline)";
            }
        };




        headerCard.Controls.Add(latest);
        headerCard.Controls.Add(version);
        headerCard.Controls.Add(title);

        root.Controls.Add(headerCard, 0, 0);

        var bodyCard = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Theme.CardBackground,
            Padding = new Padding(14),
            AutoScroll = true,
            TabStop = true,   
        };

        var message = new Label
        {
            Dock = DockStyle.Top,
            ForeColor = Theme.TextPrimary,
            TextAlign = ContentAlignment.TopLeft,
            AutoSize = true,
            Font = new Font("Segoe UI", 10f),
            Text =
                "Created by Rambo for real M.F.'ers\n\n" +
                "Calculation Logic:\n" +
                "• If Quote < $250 → multiplier = 1.50\n" +
                "• If Quote < $1,000 → multiplier = 1.33\n" +
                "• Otherwise → multiplier = 1.20\n\n" +
                "Length Fee Rules:\n" +
                "• Option A (Length 8'–20') adds a flat $150 fee\n" +
                "• Option B uses the C.H. Robinson fee and overrides Option A\n\n" +
                "Formula:\n" +
                "Freight Cost = (Quote × multiplier) + flat fee\n" +
                "Rounded to 2 decimals (AwayFromZero)." 
        };

        bodyCard.Resize += (s, e) =>
        {
            message.MaximumSize = new Size(bodyCard.ClientSize.Width - bodyCard.Padding.Horizontal, 0);
        };

        var linksPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 30,
            BackColor = Theme.CardBackground
        };

        var githubLink = new LinkLabel
        {
            Text = "GitHub",
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
            Height = 40

        };



        Theme.StylePrimaryButton(okButton);
        okButton.Click += (_, _) => Close();

        buttons.Controls.Add(okButton);
        root.Controls.Add(buttons, 0, 2);

        AcceptButton = okButton;
    }
}
