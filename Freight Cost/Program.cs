using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace FreightCost
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new CalcForm());
        }
    }

    public class CalcForm : Form
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

        private readonly TextBox _input1 = new TextBox();
        private readonly TextBox _input2 = new TextBox();

        private readonly Label _label1 = new Label();
        private readonly Label _label2 = new Label();

        private readonly CheckBox _optA = new CheckBox();
        private readonly CheckBox _optB = new CheckBox();

        private readonly Button _calc = new Button();

        // History table (cell copy friendly)
        private readonly DataGridView _history = new DataGridView();

        public CalcForm()
        {
            // ---- App Window ----
            Text = "M.F. BOYS CALCULATOR";
            Font = new Font("Segoe UI", 10f, FontStyle.Regular);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(1000, 580);
            BackColor = AppBackground;
            ForeColor = TextPrimary;

            // ---- Split: left calculator, right history ----
            var split = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Padding = new Padding(12),
                BackColor = AppBackground
            };
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 460));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            Controls.Add(split);

            // ================= LEFT =================
            var left = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 5,
                BackColor = AppBackground
            };
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));  // header
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 150)); // inputs
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));  // checkboxes
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // keypad
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));  // calculate
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));  // bottom row (youtube)
            split.Controls.Add(left, 0, 0);

            // Header
            var header = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                BackColor = AppBackground,
                Padding = new Padding(4, 0, 4, 0)
            };
            header.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            header.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
            var title = new Label
            {
                Text = "Freight Cost Calculator",
                Dock = DockStyle.Fill,
                Font = new Font(Font.FontFamily, 16f, FontStyle.Bold),
                ForeColor = TextPrimary
            };
            var subtitle = new Label
            {
                Text = "Fast quotes with built‑in fees and history",
                Dock = DockStyle.Fill,
                Font = new Font(Font.FontFamily, 9f, FontStyle.Regular),
                ForeColor = TextMuted
            };
            header.Controls.Add(title, 0, 0);
            header.Controls.Add(subtitle, 0, 1);
            left.Controls.Add(header, 0, 0);

            // Inputs area
            var inputs = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 1,
                Padding = new Padding(12),
                BackColor = CardBackground,
                Margin = new Padding(0, 0, 0, 12)
            };
            inputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
            inputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            inputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
            inputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            left.Controls.Add(inputs, 0, 1);

            _label1.Text = "Quote (USD)";
            _label1.Dock = DockStyle.Fill;
            _label1.Font = new Font(Font, FontStyle.Bold);
            _label1.ForeColor = TextMuted;

            _input1.Dock = DockStyle.Fill;
            _input1.Font = new Font(Font.FontFamily, 16f, FontStyle.Regular);
            _input1.TextAlign = HorizontalAlignment.Right;
            _input1.PlaceholderText = "$0.00";
            _input1.ShortcutsEnabled = true;
            AddRightClickMenu(_input1);

            _label2.Text = "Length fee from C.H. Robinson";
            _label2.Dock = DockStyle.Fill;
            _label2.Font = new Font(Font, FontStyle.Bold);
            _label2.ForeColor = TextMuted;

            _input2.Dock = DockStyle.Fill;
            _input2.Font = new Font(Font.FontFamily, 16f, FontStyle.Regular);
            _input2.TextAlign = HorizontalAlignment.Right;
            _input2.PlaceholderText = "$0.00";
            _input2.ShortcutsEnabled = true;
            AddRightClickMenu(_input2);

            inputs.Controls.Add(_label1, 0, 0);
            inputs.Controls.Add(_input1, 0, 1);
            inputs.Controls.Add(_label2, 0, 2);
            inputs.Controls.Add(_input2, 0, 3);

            SetSecondInputVisible(false);

            // Checkboxes row
            var optionsRow = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Padding = new Padding(12),
                BackColor = CardBackground
            };
            optionsRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            optionsRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            _optA.Text = "Length is 8' to 20' ?";
            _optA.Dock = DockStyle.Fill;
            _optA.ForeColor = TextPrimary;

            _optB.Text = "Use C.H. Robinson length fee?";
            _optB.Dock = DockStyle.Fill;
            _optB.ForeColor = TextPrimary;
            _optB.CheckedChanged += (_, __) =>
            {
                if (_optB.Checked)
                {
                    _optA.Checked = false;
                    _optA.Enabled = false;
                    SetSecondInputVisible(true);
                    _input2.Focus();
                }
                else
                {
                    _optA.Enabled = true;
                    SetSecondInputVisible(false);
                    _input1.Focus();
                }
            };

            optionsRow.Controls.Add(_optA, 0, 0);
            optionsRow.Controls.Add(_optB, 1, 0);
            left.Controls.Add(optionsRow, 0, 2);

            // Keypad
            left.Controls.Add(BuildKeypad(), 0, 3);

            // Calculate
            _calc.Text = "Calculate";
            _calc.Dock = DockStyle.Fill;
            _calc.Font = new Font(Font.FontFamily, 12f, FontStyle.Bold);
            StylePrimaryButton(_calc);
            _calc.Click += (_, __) => DoCalculate();
            left.Controls.Add(_calc, 0, 4);

            // Bottom-left YouTube button (out of the way)
            var bottomRow = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                BackColor = AppBackground
            };
            bottomRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 44));
            bottomRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var ytButton = new Button
            {
                Text = "🔥",
                Dock = DockStyle.Fill,
                Font = new Font(Font.FontFamily, 10f, FontStyle.Regular),
            };
            StyleGhostButton(ytButton);
            var tip = new ToolTip();
            tip.SetToolTip(ytButton, "Open help video");
            ytButton.Click += (_, __) =>
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = HelpVideoUrl,
                        UseShellExecute = true
                    });
                }
                catch
                {
                    MessageBox.Show("Unable to open the video link.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            bottomRow.Controls.Add(ytButton, 0, 0);
            bottomRow.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = AppBackground }, 1, 0);
            left.Controls.Add(bottomRow, 0, 4);


            // ================= RIGHT (HISTORY) =================
            var right = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                BackColor = AppBackground
            };
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));   // title only
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // table
            split.Controls.Add(right, 1, 0);

            right.Controls.Add(new Label
            {
                Text = "History",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font(Font.FontFamily, 12f, FontStyle.Bold),
                ForeColor = TextPrimary
            }, 0, 0);

            ConfigureHistoryGrid();
            right.Controls.Add(_history, 0, 1);

            ApplyModernTheme(this);
            AcceptButton = _calc;
            Shown += (_, __) => _input1.Focus();
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
