using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Freight_Cost
{
    public partial class Form1 : Form
    {
        private static readonly Color AppBackground = Color.FromArgb(32, 32, 32);
        private static readonly Color CardBackground = Color.FromArgb(45, 45, 45);
        private static readonly Color Accent = Color.FromArgb(59, 130, 246);
        private static readonly Color AccentHover = Color.FromArgb(37, 99, 235);
        private static readonly Color AccentSoft = Color.FromArgb(52, 64, 86);
        private static readonly Color TextPrimary = Color.FromArgb(240, 240, 240);
        private static readonly Color TextMuted = Color.FromArgb(170, 170, 170);
        private static readonly Color BorderColor = Color.FromArgb(64, 64, 64);

        private const string HelpVideoUrl =
            "https://www.youtube.com/watch?v=1WaV2x8GXj0&list=RD1WaV2x8GXj0&start_radio=1";

        private readonly CultureInfo _us = CultureInfo.GetCultureInfo("en-US");

        public Form1()
        {
            InitializeComponent();
        }

        // ========= HISTORY GRID SETUP =========
        private void ConfigureHistoryGrid()
        {
            _history.Dock = DockStyle.Fill;
            _history.BackgroundColor = CardBackground;

            // Make it feel like a read-only history table
            _history.ReadOnly = true;
            _history.AllowUserToAddRows = false;
            _history.AllowUserToDeleteRows = false;
            _history.AllowUserToResizeRows = false;
            _history.AllowUserToResizeColumns = false;
            _history.RowHeadersVisible = false;
            _history.MultiSelect = true;

            // IMPORTANT: lets user click individual cells and Ctrl+C copies selection
            _history.SelectionMode = DataGridViewSelectionMode.CellSelect;
            _history.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;

            _history.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            _history.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            _history.BorderStyle = BorderStyle.FixedSingle;
            _history.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            _history.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            _history.GridColor = BorderColor;

            _history.EnableHeadersVisualStyles = false;
            _history.ColumnHeadersDefaultCellStyle.BackColor = CardBackground;
            _history.ColumnHeadersDefaultCellStyle.ForeColor = TextMuted;
            _history.ColumnHeadersDefaultCellStyle.Font = new Font(Font, FontStyle.Bold);

            // Add columns: Quote | × | Fees | = | Freight Cost
            _history.Columns.Clear();

            var colQuote = new DataGridViewTextBoxColumn { Name = "Quote", HeaderText = "Quote", Width = 150 };
            var colMul = new DataGridViewTextBoxColumn { Name = "Mul", HeaderText = "×", Width = 25 };
            var colFees = new DataGridViewTextBoxColumn { Name = "Fees", HeaderText = "Fees", Width = 150 };
            var colEq = new DataGridViewTextBoxColumn { Name = "Eq", HeaderText = "=", Width = 25 };
            var colFreight = new DataGridViewTextBoxColumn { Name = "Freight", HeaderText = "Freight Cost", Width = 150 };

            _history.Columns.AddRange(colQuote, colMul, colFees, colEq, colFreight);

            // Center-align EVERYTHING (headers and cells), per your request
            _history.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _history.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _history.DefaultCellStyle.BackColor = CardBackground;
            _history.DefaultCellStyle.ForeColor = TextPrimary;
            _history.DefaultCellStyle.SelectionBackColor = AccentSoft;
            _history.DefaultCellStyle.SelectionForeColor = TextPrimary;
            _history.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(40, 40, 40);

            // Optional: prevent sorting arrows / clicks
            foreach (DataGridViewColumn c in _history.Columns)
                c.SortMode = DataGridViewColumnSortMode.NotSortable;
        }

        // ========= CALCULATION / HISTORY =========
        private void DoCalculate()
        {
            if (!TryParseUsd(_input1.Text, out var quote, out var err1))
            {
                MessageBox.Show(err1, "Invalid Quote", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _input1.Focus();
                return;
            }

            bool useRobinson = _optB.Checked;
            bool length8to20 = _optA.Checked;

            decimal flatFee;

            if (useRobinson)
            {
                if (!TryParseUsd(_input2.Text, out flatFee, out var err2))
                {
                    MessageBox.Show(err2, "Invalid Length Fee", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _input2.Focus();
                    return;
                }
            }
            else
            {
                flatFee = length8to20 ? 150.00m : 0m;
            }

            decimal multiplier = GetMultiplier(quote);
            decimal freight = RoundCents((quote * multiplier) + flatFee);

            // Fees column: show multiplier + flat fee used
            string feesDisplay = $"{multiplier.ToString("0.##", _us)} + {flatFee.ToString("C", _us)}";

            // Add row at TOP (newest first)
            _history.Rows.Insert(0,
                quote.ToString("C", _us),
                "×",
                feesDisplay,
                "=",
                freight.ToString("C", _us)
            );
        }

        private static decimal GetMultiplier(decimal quote)
        {
            if (quote < 250m) return 1.5m;
            if (quote < 1000m) return 1.33m;
            return 1.2m;
        }

        private static decimal RoundCents(decimal v) =>
            Math.Round(v, 2, MidpointRounding.AwayFromZero);

        private static bool TryParseUsd(string text, out decimal value, out string error)
        {
            error = "";
            value = 0m;

            var trimmed = (text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                error = "Enter an amount (example: 12.34 or $12.34).";
                return false;
            }

            var us = CultureInfo.GetCultureInfo("en-US");
            if (!decimal.TryParse(trimmed, NumberStyles.Currency, us, out value))
            {
                error = "That doesn't look like a valid USD amount.\nExamples: 12.34, $12.34, $1,234.56";
                return false;
            }

            if (value < 0m)
            {
                error = "Amount cannot be negative.";
                return false;
            }

            return true;
        }

        // ========= UI HELPERS =========
        private void SetSecondInputVisible(bool visible)
        {
            _label2.Visible = visible;
            _input2.Visible = visible;
            if (!visible) _input2.Text = "";
        }

        private Control BuildKeypad()
        {
            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 4,
                Margin = new Padding(0),
                Padding = new Padding(6),
                BackColor = CardBackground
            };

            for (int c = 0; c < 4; c++)
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            for (int r = 0; r < 4; r++)
                grid.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));

            // Row 1
            grid.Controls.Add(MakeKeyButton("7", () => AppendToActive("7")), 0, 0);
            grid.Controls.Add(MakeKeyButton("8", () => AppendToActive("8")), 1, 0);
            grid.Controls.Add(MakeKeyButton("9", () => AppendToActive("9")), 2, 0);
            grid.Controls.Add(MakeKeyButton("⌫", BackspaceActive), 3, 0);

            // Row 2
            grid.Controls.Add(MakeKeyButton("4", () => AppendToActive("4")), 0, 1);
            grid.Controls.Add(MakeKeyButton("5", () => AppendToActive("5")), 1, 1);
            grid.Controls.Add(MakeKeyButton("6", () => AppendToActive("6")), 2, 1);
            grid.Controls.Add(MakeKeyButton(".", AppendDecimal), 3, 1);

            // Row 3
            grid.Controls.Add(MakeKeyButton("1", () => AppendToActive("1")), 0, 2);
            grid.Controls.Add(MakeKeyButton("2", () => AppendToActive("2")), 1, 2);
            grid.Controls.Add(MakeKeyButton("3", () => AppendToActive("3")), 2, 2);
            grid.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = CardBackground }, 3, 2);

            // Row 4
            grid.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = CardBackground }, 0, 3);
            grid.Controls.Add(MakeKeyButton("0", () => AppendToActive("0")), 1, 3);
            grid.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = CardBackground }, 2, 3);
            grid.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = CardBackground }, 3, 3);

            return grid;
        }

        private Button MakeKeyButton(string text, Action onClick)
        {
            var b = new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                Margin = new Padding(6),
                Font = new Font(Font.FontFamily, 14f, FontStyle.Bold)
            };
            StyleSecondaryButton(b);
            b.Click += (_, __) => onClick();
            return b;
        }

        private TextBox GetActiveInput()
        {
            if (_input2.Visible && _input2.Focused) return _input2;
            return _input1;
        }

        private void AppendToActive(string s)
        {
            var tb = GetActiveInput();
            tb.AppendText(s);
            tb.Focus();
            tb.SelectionStart = tb.TextLength;
        }

        private void AppendDecimal()
        {
            var tb = GetActiveInput();
            if (!tb.Text.Contains("."))
            {
                if (tb.Text.Length == 0) tb.Text = "0.";
                else tb.AppendText(".");
            }
            tb.Focus();
            tb.SelectionStart = tb.TextLength;
        }

        private void BackspaceActive()
        {
            var tb = GetActiveInput();
            if (tb.TextLength > 0)
            {
                tb.Text = tb.Text.Substring(0, tb.TextLength - 1);
                tb.SelectionStart = tb.TextLength;
            }
            tb.Focus();
        }

        private static void AddRightClickMenu(TextBox tb)
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("Cut", null, (_, __) => tb.Cut());
            menu.Items.Add("Copy", null, (_, __) => tb.Copy());
            menu.Items.Add("Paste", null, (_, __) => tb.Paste());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Select All", null, (_, __) => tb.SelectAll());
            tb.ContextMenuStrip = menu;
        }

        private static void ApplyModernTheme(Control root)
        {
            root.BackColor = AppBackground;
            root.ForeColor = TextPrimary;

            foreach (Control c in root.Controls)
            {
                if (c is TextBox tb)
                {
                    tb.BackColor = CardBackground;
                    tb.ForeColor = TextPrimary;
                    tb.BorderStyle = BorderStyle.FixedSingle;
                }
                else if (c is DataGridView gv)
                {
                    gv.BackgroundColor = CardBackground;
                }
                else if (c is Label label && label.ForeColor == SystemColors.ControlText)
                {
                    label.ForeColor = TextPrimary;
                }
                ApplyModernTheme(c);
            }
        }

        private static void StylePrimaryButton(Button button)
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

        private static void StyleSecondaryButton(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = CardBackground;
            button.ForeColor = TextPrimary;
            button.UseVisualStyleBackColor = false;
            button.Cursor = Cursors.Hand;
            button.Padding = new Padding(4);
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(55, 55, 55);
        }

        private static void StyleGhostButton(Button button)
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
}
