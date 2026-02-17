using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Freight_Cost
{
    /// <summary>
    /// Form1 contains:
    /// - Theme colors
    /// - Event wiring (checkboxes, calculate, history header, YouTube link)
    /// - Calculation logic
    /// - History grid configuration
    /// - Money-safe input filtering (typing + paste + keypad)
    /// </summary>
    public partial class Form1 : Form
    {

        // ============================================================
        // THEME COLORS (dark theme)
        // ============================================================

        private static readonly Color AppBackground = Color.FromArgb(32, 32, 32);
        private static readonly Color CardBackground = Color.FromArgb(45, 45, 45);
        private static readonly Color Accent = Color.FromArgb(59, 130, 246);
        private static readonly Color AccentHover = Color.FromArgb(37, 99, 235);
        private static readonly Color AccentSoft = Color.FromArgb(52, 64, 86);
        private static readonly Color TextPrimary = Color.FromArgb(240, 240, 240);
        private static readonly Color TextMuted = Color.FromArgb(170, 170, 170);
        private static readonly Color BorderColor = Color.FromArgb(64, 64, 64);

        // Help video URL (YouTube)
        private const string HelpVideoUrl =
            "https://www.youtube.com/watch?v=1WaV2x8GXj0&list=RD1WaV2x8GXj0&start_radio=1";

        // Cached culture used for USD parsing/formatting (avoids repeated lookups)
        private static readonly CultureInfo UsCulture = CultureInfo.GetCultureInfo("en-US");

        private TextBox _activeInput;


        // ============================================================
        // CONSTRUCTOR
        // ============================================================
        public Form1()
        {
            InitializeComponent();
            AddMenuBar();
            this.FormClosing += (_, e) =>
            {
                var result = MessageBox.Show(
                    "Exit the Calculator MFer?",
                    "Got soft hands brother?",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                }
            };
            WireupHistoryHeaderEvents();
            WireupOtherEvents();
        }


        private void AddMenuBar()
        {
            var menu = new MenuStrip
            {
                // ===== DARK THEME COLORS =====
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.FromArgb(240, 240, 240),
                RenderMode = ToolStripRenderMode.Professional,

                // Custom renderer so dropdowns are dark too
                Renderer = new ToolStripProfessionalRenderer(new DarkMenuColors())
            };

            // ================= FILE =================
            var fileMenu = new ToolStripMenuItem("File")
            {
                ForeColor = Color.White
            };
            var clearHistoryItem = new ToolStripMenuItem("Clear History")
            {
                ForeColor = Color.White
            };

            clearHistoryItem.Click += (_, __) =>
            {
                var result = MessageBox.Show(
                    "Are you sure you want to clear all history?\nThis cannot be undone.",
                    "Confirm Clear History",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.Yes)
                {
                    _history.Rows.Clear();
                }
            };

            var exitItem = new ToolStripMenuItem("Exit")
            {
                ForeColor = Color.White,
                ShortcutKeys = Keys.Alt | Keys.F4
            };
            exitItem.Click += (_, __) =>
            {

                this.Close();

            };


            fileMenu.DropDownItems.Add(clearHistoryItem);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(exitItem);

            // ================= HELP =================
            var helpMenu = new ToolStripMenuItem("Help")
            {
                ForeColor = Color.White
            };

            var aboutItem = new ToolStripMenuItem("About")
            {
                ForeColor = Color.White
            };
            aboutItem.Click += (_, __) => new AboutForm().ShowDialog(this);

            helpMenu.DropDownItems.Add(aboutItem);


            menu.Items.Add(fileMenu);
            menu.Items.Add(helpMenu); 

            this.MainMenuStrip = menu;
            this.Controls.Add(menu);
            menu.Dock = DockStyle.Top;
            _activeInput = _input1;

        }



        // ============================================================
        // HISTORY HEADER BEHAVIOR
        // Left click: show/hide history
        // Right click: swap history side
        // ============================================================
        private void WireupHistoryHeaderEvents()
        {
            bool historyOnRight = true;
            bool historyVisible = true;

            const int HistoryPanelWidth = 460;
            const int HistoryShrinkExtra = 120;

            // The form's root control is your split TableLayoutPanel.
            // (The designer adds it to Controls first, so Controls[0] works.)
            if (Controls.Count == 0) return;
            if (Controls[0] is not TableLayoutPanel split) return;

            // MouseUp lets us detect left vs right click.
            _historyHeader.MouseUp += (_, me) =>
            {
                // LEFT CLICK: toggle visibility
                if (me.Button == MouseButtons.Left)
                {
                    historyVisible = !historyVisible;

                    if (historyVisible)
                    {
                        // Restore history panel width
                        if (historyOnRight)
                        {
                            // history is left (column 0)
                            split.ColumnStyles[0].SizeType = SizeType.Absolute;
                            split.ColumnStyles[0].Width = HistoryPanelWidth;

                            // main panel fills remaining space
                            split.ColumnStyles[1].SizeType = SizeType.Percent;
                            split.ColumnStyles[1].Width = 100;
                        }
                        else
                        {
                            // history is right (column 1)
                            split.ColumnStyles[1].SizeType = SizeType.Absolute;
                            split.ColumnStyles[1].Width = HistoryPanelWidth;

                            // main panel fills remaining space
                            split.ColumnStyles[0].SizeType = SizeType.Percent;
                            split.ColumnStyles[0].Width = 100;
                        }

                        // Make form wider when history is shown
                        int desired = ClientSize.Width + HistoryPanelWidth + HistoryShrinkExtra;
                        int screenMax = Screen.PrimaryScreen?.WorkingArea.Width ?? desired;
                        ClientSize = new Size(Math.Min(desired, screenMax), ClientSize.Height);
                    }
                    else
                    {
                        // Collapse the history column to width 0
                        if (historyOnRight)
                        {
                            split.ColumnStyles[1].SizeType = SizeType.Absolute;
                            split.ColumnStyles[1].Width = 0;
                        }
                        else
                        {
                            split.ColumnStyles[0].SizeType = SizeType.Absolute;
                            split.ColumnStyles[0].Width = 0;
                        }

                        // Shrink form but keep a minimum width
                        int minWidth = 520;
                        int newWidth = Math.Max(minWidth, ClientSize.Width - HistoryPanelWidth - HistoryShrinkExtra);
                        ClientSize = new Size(newWidth, ClientSize.Height);
                    }

                    // Update header text arrow
                    _historyHeader.Text = $"History {(historyVisible ? "◀" : "▶")}";
                }
                // RIGHT CLICK: swap left/right
                else if (me.Button == MouseButtons.Right)
                {
                    split.SuspendLayout();

                    var leftControl = split.GetControlFromPosition(0, 0);
                    var rightControl = split.GetControlFromPosition(1, 0);

                    split.Controls.Remove(leftControl);
                    split.Controls.Remove(rightControl);

                    if (historyOnRight)
                    {
                        // Swap panels
                        split.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, 100);
                        split.ColumnStyles[1] = new ColumnStyle(SizeType.Absolute, HistoryPanelWidth);

                        split.Controls.Add(rightControl, 0, 0);
                        split.Controls.Add(leftControl, 1, 0);

                        historyOnRight = false;
                    }
                    else
                    {
                        split.ColumnStyles[0] = new ColumnStyle(SizeType.Absolute, HistoryPanelWidth);
                        split.ColumnStyles[1] = new ColumnStyle(SizeType.Percent, 100);

                        split.Controls.Add(leftControl, 0, 0);
                        split.Controls.Add(rightControl, 1, 0);

                        historyOnRight = true;
                    }

                    _historyHeader.Text = $"History {(historyVisible ? "◀" : "▶")}";
                    split.ResumeLayout();
                }
            };

            // Hover effect: blue on hover, normal on leave
            _historyHeader.MouseEnter += (_, __) => _historyHeader.ForeColor = Accent;
            _historyHeader.MouseLeave += (_, __) => _historyHeader.ForeColor = TextPrimary;
        }

        // ============================================================
        // OTHER EVENTS (Option logic, Calculate, YouTube, focus)
        // ============================================================
        private void WireupOtherEvents()
        {
            _input1.Enter += (_, __) => _activeInput = _input1;
            _input2.Enter += (_, __) => _activeInput = _input2;

            // Optional: also handle mouse clicks
            _input1.MouseDown += (_, __) => _activeInput = _input1;
            _input2.MouseDown += (_, __) => _activeInput = _input2;

            // Option B toggles the second input box
            _optB.CheckedChanged += (_, __) =>
            {
                if (_optB.Checked)
                {
                    // When Option B is ON:
                    // - disable Option A
                    // - show second input
                    _optA.Checked = false;
                    _optA.Enabled = false;
                    SetSecondInputVisible(true);
                    _input2.Focus();
                }
                else
                {
                    // When Option B is OFF:
                    // - re-enable Option A
                    // - hide second input
                    _optA.Enabled = true;
                    SetSecondInputVisible(false);
                    _input1.Focus();
                }
            };

            // Calculate button triggers the calculation
            _calc.Click += (_, __) => DoCalculate();

            // YouTube help button opens browser link
            if (_ytButton != null)
            {
                _ytButton.Click -= YtButton_Click;
                _ytButton.Click += YtButton_Click;
            }

            // Focus the first input when shown
            // (Do NOT try to "Shown -=" with anonymous lambdas; that does nothing.)
            Shown += (_, __) => _input1.Focus();
        }

        private void YtButton_Click(object? sender, EventArgs e)
        {
            try
            {
                // UseShellExecute lets Windows open the link in default browser
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
        }

        // ============================================================
        // HISTORY GRID SETUP
        // ============================================================
        private void ConfigureHistoryGrid()
        {
            _history.Dock = DockStyle.Fill;
            _history.BackgroundColor = CardBackground;

            // Make it a read-only "log"
            _history.ReadOnly = true;
            _history.AllowUserToAddRows = false;
            _history.AllowUserToDeleteRows = false;
            _history.AllowUserToResizeRows = false;
            _history.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            _history.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            _history.AllowUserToResizeColumns = false;
            _history.RowHeadersVisible = false;
            _history.MultiSelect = false;

            // Allow selecting cells and copying with Ctrl+C
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

            // Columns: Quote | × | Fees | = | Freight Cost | Remove
            _history.Columns.Clear();

            var colQuote = new DataGridViewTextBoxColumn { Name = "Quote", HeaderText = "Quote", Width = 160 };
            var colMul = new DataGridViewTextBoxColumn { Name = "Mul", HeaderText = "", Width = 20 };
            var colFees = new DataGridViewTextBoxColumn { Name = "Fees", HeaderText = "Fees", Width = 160 };
            var colEq = new DataGridViewTextBoxColumn { Name = "Eq", HeaderText = "", Width = 20 };
            var colFreight = new DataGridViewTextBoxColumn { Name = "Freight", HeaderText = "Freight Cost", Width = 100 };
            var colRemove = new DataGridViewButtonColumn
            {
                Name = "Remove",
                HeaderText = "",
                Width = 40,
                Text = "X",
                UseColumnTextForButtonValue = true
            };

            _history.Columns.AddRange(colQuote, colMul, colFees, colEq, colFreight, colRemove);

            // Center align everything
            _history.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _history.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // Colors for cells and selection
            _history.DefaultCellStyle.BackColor = CardBackground;
            _history.DefaultCellStyle.ForeColor = TextPrimary;
            _history.DefaultCellStyle.SelectionBackColor = AccentSoft;
            _history.DefaultCellStyle.SelectionForeColor = TextPrimary;
            _history.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(40, 40, 40);

            // Disable sorting
            foreach (DataGridViewColumn c in _history.Columns)
                c.SortMode = DataGridViewColumnSortMode.NotSortable;

            // Hook remove button
            _history.CellClick -= HistoryCellClick;
            _history.CellClick += HistoryCellClick;

            AddHistoryRightClickMenu();

        }

        private void HistoryCellClick(object? sender, DataGridViewCellEventArgs e)
        {
            // ignore header clicks
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            // Remove button pressed
            if (_history.Columns[e.ColumnIndex].Name == "Remove")
            {
                var quote = _history.Rows[e.RowIndex].Cells[0].Value?.ToString() ?? "this entry";
                var res = MessageBox.Show($"Remove {quote}?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (res == DialogResult.Yes)
                    _history.Rows.RemoveAt(e.RowIndex);
            }
        }

        // ============================================================
        // CALCULATION / HISTORY ADD
        // ============================================================
        private void DoCalculate()
        {
            // Parse quote
            if (!TryParseUsd(_input1.Text, out var quote, out var err1))
            {
                MessageBox.Show(err1, "Invalid Quote", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _input1.Focus();
                return;
            }

            bool useRobinson = _optB.Checked;
            bool length8to20 = _optA.Checked;

            decimal flatFee;

            // Option B means user typed the flat fee
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
                // Option A means flat fee is 150, otherwise 0
                flatFee = length8to20 ? 150.00m : 0m;
            }

            // Freight formula: quote * multiplier + flatFee
            decimal multiplier = GetMultiplier(quote);
            decimal freight = RoundCents((quote * multiplier) + flatFee);

            // Show how fees were calculated
            string feesDisplay = $"{multiplier.ToString("0.##", UsCulture)} + {flatFee.ToString("C", UsCulture)}";

            // Insert newest at top
            _history.SuspendLayout();
            _history.Rows.Insert(0,
                quote.ToString("C", UsCulture),
                "×",
                feesDisplay,
                "=",
                freight.ToString("C", UsCulture),
                "X"
            );
            _history.ResumeLayout();

            // Show final number
            _output.Text = freight.ToString("C", UsCulture);
            _output.SelectionStart = _output.TextLength;
        }

        // Determine multiplier based on quote tier
        internal static decimal GetMultiplier(decimal quote)
        {
            if (quote < 250m) return 1.5m;
            if (quote < 1000m) return 1.33m;
            return 1.2m;
        }

        // Money-safe rounding
        internal static decimal RoundCents(decimal v) =>
            Math.Round(v, 2, MidpointRounding.AwayFromZero);

        // Parse USD currency strings safely
        internal static bool TryParseUsd(string text, out decimal value, out string error)
        {
            error = "";
            value = 0m;

            var trimmed = (text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(trimmed))
            {
                error = "Enter an amount (example: 12.34 or $12.34).";
                return false;
            }

            if (!decimal.TryParse(trimmed, NumberStyles.Currency, UsCulture, out value))
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

        // ============================================================
        // INPUT FILTERS / PASTE HANDLING (FIXED)
        // ============================================================
        //
        // IMPORTANT IDEA:
        // We normalize the *entire resulting string* after paste/typing.
        // This prevents the decimal point from getting dropped when the textbox
        // already contains input.
        // ============================================================

        /// <summary>
        /// Converts any raw string into a safe "money-ish" string.
        /// Rules:
        /// - Keep digits
        /// - Allow ONE decimal point
        /// - Allow commas
        /// - Allow ONE leading '$'
        /// - Ignore everything else
        /// </summary>
        private static string NormalizeMoneyText(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return string.Empty;

            raw = raw.Trim();

            var sb = new System.Text.StringBuilder(raw.Length);

            bool hasDot = false;
            bool hasDollar = false;

            foreach (char ch in raw)
            {
                if (char.IsDigit(ch))
                {
                    sb.Append(ch);
                    continue;
                }

                if (ch == '.' && !hasDot)
                {
                    sb.Append('.');
                    hasDot = true;
                    continue;
                }

                if (ch == ',')
                {
                    sb.Append(',');
                    continue;
                }

                // Only allow '$' if it is the first character in the result
                if (ch == '$' && !hasDollar && sb.Length == 0)
                {
                    sb.Append('$');
                    hasDollar = true;
                    continue;
                }

                // Everything else gets ignored
            }

            return sb.ToString();
        }

        /// <summary>
        /// Adds safe input behavior to a TextBox:
        /// - KeyPress blocks obvious junk characters
        /// - Ctrl+V paste is intercepted and normalized
        /// - TextChanged normalizes anything that slips in
        /// </summary>
        private static void AttachInputFilters(TextBox tb)
        {
            // KeyPress: light filter while typing
            tb.KeyPress += (s, e) =>
            {
                if (char.IsControl(e.KeyChar)) return;

                char c = e.KeyChar;

                if (char.IsDigit(c) || c == ',' || c == '$')
                    return;

                if (c == '.')
                {
                    // Allow only one dot
                    if (tb.Text.Contains('.'))
                        e.Handled = true;
                    return;
                }

                // Block anything else
                e.Handled = true;
            };

            // KeyDown: handle Ctrl+V paste ourselves
            tb.KeyDown += (s, e) =>
            {
                if (e.Control && e.KeyCode == Keys.V)
                {
                    if (Clipboard.ContainsText())
                    {
                        string clip = Clipboard.GetText();

                        int selStart = tb.SelectionStart;
                        int selLen = tb.SelectionLength;

                        string before = tb.Text[..selStart];
                        string after = tb.Text[(selStart + selLen)..];

                        // Combine old text + pasted text + remainder, then normalize
                        string combined = before + clip + after;
                        tb.Text = NormalizeMoneyText(combined);

                        // Calculator feel: caret at end
                        tb.SelectionStart = tb.TextLength;
                    }

                    // Stop default paste
                    e.SuppressKeyPress = true;
                    return;
                }

                // Allow modifier shortcuts
                if (e.Control || e.Alt || e.Shift) return;

                // Allow navigation/edit keys
                switch (e.KeyCode)
                {
                    case Keys.Back:
                    case Keys.Delete:
                    case Keys.Left:
                    case Keys.Right:
                    case Keys.Home:
                    case Keys.End:
                    case Keys.Tab:
                    case Keys.Enter:
                        return;
                }

                // Allow digits (top row + numpad)
                bool isDigitKey =
                    (e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9) ||
                    (e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9);

                if (isDigitKey) return;

                // Allow dot and comma keys
                if (e.KeyCode == Keys.OemPeriod || e.KeyCode == Keys.Decimal || e.KeyCode == (Keys)188)
                    return;

                // Block everything else
                e.SuppressKeyPress = true;
            };

            // TextChanged: final gate, normalizes any change
            tb.TextChanged += (s, e) =>
            {
                string current = tb.Text ?? string.Empty;
                string normalized = NormalizeMoneyText(current);

                if (normalized != current)
                {
                    int caret = tb.SelectionStart;
                    tb.Text = normalized;
                    tb.SelectionStart = Math.Min(caret, tb.Text.Length);
                }
            };
        }

        private void AddHistoryRightClickMenu()
        {
            // Context menu for the history grid
            var menu = new ContextMenuStrip();

            var copyItem = new ToolStripMenuItem("Copy")
            {
                ShortcutKeys = Keys.Control | Keys.C
            };
            copyItem.Click += (_, __) =>
            {
                try
                {
                    // Copies the current selection (cells), without headers
                    if (_history.GetCellCount(DataGridViewElementStates.Selected) > 0)
                    {
                        Clipboard.SetDataObject(_history.GetClipboardContent());
                    }
                }
                catch
                {
                    // ignore clipboard issues
                }
            };


            menu.Items.Add(copyItem);

            _history.ContextMenuStrip = menu;

            // IMPORTANT: right-click should select the cell you clicked
            _history.CellMouseDown += (_, e) =>
            {
                if (e.Button == MouseButtons.Right && e.RowIndex >= 0 && e.ColumnIndex >= 0)
                {
                    _history.CurrentCell = _history[e.ColumnIndex, e.RowIndex];

                    // If nothing is selected yet, select this cell so Copy works immediately
                    if (!_history[e.ColumnIndex, e.RowIndex].Selected)
                    {
                        _history.ClearSelection();
                        _history[e.ColumnIndex, e.RowIndex].Selected = true;
                    }
                }
            };
        }


        /// <summary>
        /// Adds a right-click menu with Cut/Copy/Paste/Select All.
        /// Paste uses NormalizeMoneyText so decimals are never dropped.
        /// </summary>
        private static void AddRightClickMenu(TextBox tb)
        {
            var menu = new ContextMenuStrip();

            menu.Items.Add("Cut", null, (_, __) => tb.Cut());
            menu.Items.Add("Copy", null, (_, __) => tb.Copy());

            // Paste: build the combined string, normalize, set text
            menu.Items.Add("Paste", null, (_, __) =>
            {
                try
                {
                    if (!Clipboard.ContainsText()) return;

                    string clip = Clipboard.GetText();

                    int selStart = tb.SelectionStart;
                    int selLen = tb.SelectionLength;

                    string before = tb.Text[..selStart];
                    string after = tb.Text[(selStart + selLen)..];

                    string combined = before + clip + after;
                    tb.Text = NormalizeMoneyText(combined);

                    tb.SelectionStart = tb.TextLength;
                }
                catch
                {
                    // Ignore clipboard failures (rare, but possible)
                }
            });

            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Select All", null, (_, __) => tb.SelectAll());

            tb.ContextMenuStrip = menu;
        }

        // ============================================================
        // UI HELPERS
        // ============================================================
        private void SetSecondInputVisible(bool visible)
        {
            _label2.Visible = visible;
            _input2.Visible = visible;

            // If we hide the second input, clear it to avoid accidental reuse
            if (!visible) _input2.Text = "";
        }

        // ============================================================
        // KEYPAD (UPDATED so it doesn't "append raw" anymore)
        // ============================================================
        private Control BuildKeypad
        {
            get
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
                grid.Controls.Add(MakeKeyButton("7", () => InsertToActive("7")), 0, 0);
                grid.Controls.Add(MakeKeyButton("8", () => InsertToActive("8")), 1, 0);
                grid.Controls.Add(MakeKeyButton("9", () => InsertToActive("9")), 2, 0);
                grid.Controls.Add(MakeKeyButton("CE", BackspaceActive), 3, 0);

                // Row 2
                grid.Controls.Add(MakeKeyButton("4", () => InsertToActive("4")), 0, 1);
                grid.Controls.Add(MakeKeyButton("5", () => InsertToActive("5")), 1, 1);
                grid.Controls.Add(MakeKeyButton("6", () => InsertToActive("6")), 2, 1);
                grid.Controls.Add(MakeKeyButton(".", () => InsertToActive(".")), 3, 1);

                // Row 3
                grid.Controls.Add(MakeKeyButton("1", () => InsertToActive("1")), 0, 2);
                grid.Controls.Add(MakeKeyButton("2", () => InsertToActive("2")), 1, 2);
                grid.Controls.Add(MakeKeyButton("3", () => InsertToActive("3")), 2, 2);
                grid.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = CardBackground }, 3, 2);

                // Row 4
                grid.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = CardBackground }, 0, 3);
                grid.Controls.Add(MakeKeyButton("0", () => InsertToActive("0")), 1, 3);
                grid.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = CardBackground }, 2, 3);
                grid.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = CardBackground }, 3, 3);

                return grid;
            }
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

        // Decide which textbox keypad should type into
        private TextBox GetActiveInput()
        {
            if (_activeInput != null && _activeInput.Visible)
                return _activeInput;

            return _input1; // fallback
        }


        /// <summary>
        /// Inserts characters into the active textbox, then normalizes the full text.
        /// This prevents keypad input from creating invalid strings.
        /// </summary>
        private void InsertToActive(string s)
        {
            var tb = GetActiveInput();

            int selStart = tb.SelectionStart;
            int selLen = tb.SelectionLength;

            string before = tb.Text[..selStart];
            string after = tb.Text[(selStart + selLen)..];

            string combined = before + s + after;
            tb.Text = NormalizeMoneyText(combined);

            tb.SelectionStart = tb.TextLength;
            if (!tb.Focused) tb.Focus();
        }

        private void BackspaceActive()
        {
            var tb = GetActiveInput();

            // CE behavior: clear current entry completely
            tb.Text = string.Empty;
            tb.SelectionStart = tb.TextLength;

            if (!tb.Focused) tb.Focus();
        }

        // ============================================================
        // THEME STYLING HELPERS
        // ============================================================
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

                // Recurse into child controls
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
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = BorderColor;
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

class DarkMenuColors : ProfessionalColorTable
{
    public override Color MenuItemSelected => Color.FromArgb(59, 130, 246);
    public override Color MenuItemBorder => Color.FromArgb(59, 130, 246);
    public override Color MenuBorder => Color.FromArgb(45, 45, 45);

    public override Color ToolStripDropDownBackground => Color.FromArgb(45, 45, 45);
    public override Color ImageMarginGradientBegin => Color.FromArgb(45, 45, 45);
    public override Color ImageMarginGradientMiddle => Color.FromArgb(45, 45, 45);
    public override Color ImageMarginGradientEnd => Color.FromArgb(45, 45, 45);

    public override Color MenuItemSelectedGradientBegin => Color.FromArgb(59, 130, 246);
    public override Color MenuItemSelectedGradientEnd => Color.FromArgb(59, 130, 246);

    public override Color MenuItemPressedGradientBegin => Color.FromArgb(37, 99, 235);
    public override Color MenuItemPressedGradientEnd => Color.FromArgb(37, 99, 235);
}