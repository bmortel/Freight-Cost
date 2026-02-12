using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Freight_Cost
{
    public sealed class AboutForm : Form
    {
        private static readonly Color AppBackground = Color.FromArgb(32, 32, 32);
        private static readonly Color CardBackground = Color.FromArgb(45, 45, 45);
        private static readonly Color TextPrimary = Color.FromArgb(240, 240, 240);
        private static readonly Color TextMuted = Color.FromArgb(170, 170, 170);
        private static readonly Color Accent = Color.FromArgb(59, 130, 246);

        public AboutForm()
        {
            Text = "About Freight Cost";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(460, 560);
            BackColor = AppBackground;
            Font = new Font("Segoe UI", 10f);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16),
                ColumnCount = 1,
                RowCount = 3,
                BackColor = AppBackground
            };

            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

            Controls.Add(root);

            // ===== HEADER =====
            var headerCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardBackground,
                Padding = new Padding(14)
            };

            var title = new Label
            {
                Text = "Freight Cost Calculator",
                Dock = DockStyle.Top,
                Height = 28,
                ForeColor = TextPrimary,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold)
            };

            var version = new Label
            {
                Text = "Version 1.0",
                Dock = DockStyle.Top,
                Height = 22,
                ForeColor = TextMuted
            };

            headerCard.Controls.Add(version);
            headerCard.Controls.Add(title);
            root.Controls.Add(headerCard, 0, 0);

            // ===== BODY =====
            var bodyCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = CardBackground,
                Padding = new Padding(14)
            };

            var message = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = TextPrimary,
                TextAlign = ContentAlignment.TopLeft,
                AutoSize = false,
                Padding = new Padding(0, 0, 0, 10),
                Font = new Font("Segoe UI", 10f, FontStyle.Regular),
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



            var linksPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 30,
                BackColor = CardBackground
            };

            var githubLink = new LinkLabel
            {
                Text = "GitHub",
                LinkColor = Accent,
                ActiveLinkColor = Accent,
                VisitedLinkColor = Accent,
                Margin = new Padding(0, 0, 16, 0)
            };

            githubLink.LinkClicked += (_, __) =>
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/bmortel/Freight-Cost",
                    UseShellExecute = true
                });
            };

            //var linkedinLink = new LinkLabel
            //{
            //    Text = "LinkedIn",
            //    LinkColor = Accent,
            //    ActiveLinkColor = Accent,
            //    VisitedLinkColor = Accent
            //};

            //linkedinLink.LinkClicked += (_, __) =>
            //{
            //    Process.Start(new ProcessStartInfo
            //    {
            //        FileName = "https://www.linkedin.com/in/brandon-mortel/",
            //        UseShellExecute = true
            //    });
            //};

            linksPanel.Controls.Add(githubLink);
            //linksPanel.Controls.Add(linkedinLink);

            bodyCard.Controls.Add(message);
            bodyCard.Controls.Add(linksPanel);
            root.Controls.Add(bodyCard, 0, 1);

            // ===== OK BUTTON =====
            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                BackColor = AppBackground
            };

            var ok = new Button
            {
                Text = "OK",
                Width = 90,
                Height = 30,
                BackColor = Accent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            ok.FlatAppearance.BorderSize = 0;
            ok.Click += (_, __) => Close();

            buttons.Controls.Add(ok);
            root.Controls.Add(buttons, 0, 2);

            AcceptButton = ok;
        }
    }

}


