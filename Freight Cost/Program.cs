using System;
using System.Drawing;
using System.Globalization;
using System.Text;
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

        // History
        private readonly ListView _history = new ListView();
        private readonly Button _copySelected = new Button();
        private readonly Button _copyAll = new Button();
        private readonly Button _clearHistory = new Button();

        public CalcForm()
        {
            // ---- App Window ----
            Text = "M.F. BOYS CALCULATOR";
            Font = SystemFonts.MessageBoxFont;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(1000, 580);

            // Force LIGHT THEME (simple + consistent)
            ApplyLightTheme(this);

            // ---- Split: left calculator, right history ----
            var split = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Padding = new Padding(12),
            };
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 460));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            Controls.Add(split);

            // ================= LEFT =================
            var left = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 5,
            };
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 150)); // inputs
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));  // checkboxes
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // keypad
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));  // calculate
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));  // bottom row (youtube button only)
            split.Controls.Add(left, 0, 0);

            // Inputs area
            var inputs = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 1,
            };
            inputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
            inputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            inputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
            inputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            left.Controls.Add(inputs, 0, 0);

            _label1.Text = "Quote (USD)";
            _label1.Dock = DockStyle.Fill;

            _input1.Dock = DockStyle.Fill;
            _input1.Font = new Font(SystemFonts.MessageBoxFont.FontFamily, 16f, FontStyle.Regular);
            _input1.TextAlign = HorizontalAlignment.Right;
            _input1.PlaceholderText = "$0.00";
            _input1.ShortcutsEnabled = true;
            AddRightClickMenu(_input1);

            _label2.Text = "Length fee from C.H. Robinson";
            _label2.Dock = DockStyle.Fill;

            _input2.Dock = DockStyle.Fill;
            _input2.Font = new Font(SystemFonts.MessageBoxFont.FontFamily, 16f, FontStyle.Regular);
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
                ColumnCount = 2
            };
            optionsRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            optionsRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            _optA.Text = "Length is 8' to 20' ?";
            _optA.Dock = DockStyle.Fill;

            _optB.Text = "Use C.H. Robinson length fee?";
            _optB.Dock = DockStyle.Fill;
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
            left.Controls.Add(optionsRow, 0, 1);

            // Keypad
            left.Controls.Add(BuildKeypad(), 0, 2);

            // Calculate
            _calc.Text = "Calculate";
            _calc.Dock = DockStyle.Fill;
            _calc.Font = new Font(SystemFonts.MessageBoxFont.FontFamily, 12f, FontStyle.Bold);
            _calc.Click += (_, __) => DoCalculate();
            left.Controls.Add(_calc, 0, 3);

            // Bottom-left YouTube button (out of the way)
            var bottomRow = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2
            };
            bottomRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 44));
            bottomRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var ytButton = new Button
            {
                Text = "▶",
                Dock = DockStyle.Fill,
                Font = new Font(SystemFonts.MessageBoxFont.FontFamily, 10f, FontStyle.Regular),
            };
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
            bottomRow.Controls.Add(new Panel { Dock = DockStyle.Fill }, 1, 0);
            left.Controls.Add(bottomRow, 0, 4);

            // ================= RIGHT (HISTORY) =================
            var right = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1
            };
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));   // header buttons
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // history list
            split.Controls.Add(right, 1, 0);

            var header = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4
            };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));

            header.Controls.Add(new Label
            {
                Text = "History",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font(SystemFonts.MessageBoxFont.FontFamily, 12f, FontStyle.Bold)
            }, 0, 0);

            _copySelected.Text = "Copy";
            _copySelected.Dock = DockStyle.Fill;
            _copySelected.Click += (_, __) => CopySelectedRows();

            _copyAll.Text = "Copy all";
            _copyAll.Dock = DockStyle.Fill;
            _copyAll.Click += (_, __) => CopyAllRows();

            _clearHistory.Text = "Clear";
            _clearHistory.Dock = DockStyle.Fill;
            _clearHistory.Click += (_, __) => _history.Items.Clear();

            header.Controls.Add(_copySelected, 1, 0);
            header.Controls.Add(_copyAll, 2, 0);
            header.Controls.Add(_clearHistory, 3, 0);
            right.Controls.Add(header, 0, 0);

            _history.Dock = DockStyle.Fill;
            _history.View = View.Details;
            _history.FullRowSelect = true;
            _history.GridLines = true;
            _history.HideSelection = false;
            _history.MultiSelect = true;

            _history.Columns.Add("Quote", 150, HorizontalAlignment.Center);
            _history.Columns.Add("×", 40, HorizontalAlignment.Center);
            _history.Columns.Add("Fees", 150, HorizontalAlignment.Center);
            _history.Columns.Add("Freight Cost", 150, HorizontalAlignment.Center);

            var histMenu = new ContextMenuStrip();
            histMenu.Items.Add("Copy selected", null, (_, __) => CopySelectedRows());
            histMenu.Items.Add("Copy all", null, (_, __) => CopyAllRows());
            histMenu.Items.Add(new ToolStripSeparator());
            histMenu.Items.Add("Clear", null, (_, __) => _history.Items.Clear());
            _history.ContextMenuStrip = histMenu;

            right.Controls.Add(_history, 0, 1);

            AcceptButton = _calc;
            Shown += (_, __) => _input1.Focus();
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

            // Add row to HISTORY (this is the ONLY place we show Freight Cost now)
            var item = new ListViewItem(quote.ToString("C", _us));
            item.SubItems.Add("×");
            item.SubItems.Add(feesDisplay);
            item.SubItems.Add(freight.ToString("C", _us));

            _history.Items.Insert(0, item);
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
                Margin = new Padding(0)
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
            grid.Controls.Add(new Panel { Dock = DockStyle.Fill }, 3, 2);

            // Row 4
            grid.Controls.Add(new Panel { Dock = DockStyle.Fill }, 0, 3);
            grid.Controls.Add(MakeKeyButton("0", () => AppendToActive("0")), 1, 3);
            grid.Controls.Add(new Panel { Dock = DockStyle.Fill }, 2, 3);
            grid.Controls.Add(new Panel { Dock = DockStyle.Fill }, 3, 3);

            return grid;
        }

        private Button MakeKeyButton(string text, Action onClick)
        {
            var b = new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                Margin = new Padding(6),
                Font = new Font(SystemFonts.MessageBoxFont.FontFamily, 14f, FontStyle.Bold),
                FlatStyle = FlatStyle.System
            };
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

        private void CopySelectedRows()
        {
            if (_history.SelectedItems.Count == 0) return;

            var sb = new StringBuilder();
            foreach (ListViewItem item in _history.SelectedItems)
            {
                sb.Append(item.SubItems[0].Text).Append('\t')
                  .Append(item.SubItems[1].Text).Append('\t')
                  .Append(item.SubItems[2].Text).Append('\t')
                  .Append(item.SubItems[3].Text).AppendLine();
            }
            Clipboard.SetText(sb.ToString().TrimEnd());
        }

        private void CopyAllRows()
        {
            if (_history.Items.Count == 0) return;

            var sb = new StringBuilder();
            foreach (ListViewItem item in _history.Items)
            {
                sb.Append(item.SubItems[0].Text).Append('\t')
                  .Append(item.SubItems[1].Text).Append('\t')
                  .Append(item.SubItems[2].Text).Append('\t')
                  .Append(item.SubItems[3].Text).AppendLine();
            }
            Clipboard.SetText(sb.ToString().TrimEnd());
        }

        private static void ApplyLightTheme(Control root)
        {
            // Light-only, simple: lets Windows draw most controls normally.
            // We keep this minimal so it stays “native”.
            root.BackColor = SystemColors.Control;
            root.ForeColor = SystemColors.ControlText;

            foreach (Control c in root.Controls)
            {
                // Let controls keep default unless you want strict light colors everywhere.
                if (c is TextBox tb)
                {
                    tb.BackColor = SystemColors.Window;
                    tb.ForeColor = SystemColors.WindowText;
                }
                else if (c is ListView lv)
                {
                    lv.BackColor = SystemColors.Window;
                    lv.ForeColor = SystemColors.WindowText;
                }
                ApplyLightTheme(c);
            }
        }
    }
}
