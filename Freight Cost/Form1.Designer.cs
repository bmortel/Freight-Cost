using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Freight_Cost
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private IContainer components = null;

        private TextBox _input1;
        private TextBox _input2;
        private Label _label1;
        private Label _label2;
        private CheckBox _optA;
        private CheckBox _optB;
        private Button _calc;
        private DataGridView _history;
        private TableLayoutPanel _split;
        private TableLayoutPanel _left;
        private TableLayoutPanel _inputs;
        private TableLayoutPanel _optionsRow;
        private TableLayoutPanel _bottomRow;
        private TableLayoutPanel _right;
        private Button _ytButton;
        private Label _historyTitle;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new Container();
            _input1 = new TextBox();
            _input2 = new TextBox();
            _label1 = new Label();
            _label2 = new Label();
            _optA = new CheckBox();
            _optB = new CheckBox();
            _calc = new Button();
            _history = new DataGridView();
            _split = new TableLayoutPanel();
            _left = new TableLayoutPanel();
            _inputs = new TableLayoutPanel();
            _optionsRow = new TableLayoutPanel();
            _bottomRow = new TableLayoutPanel();
            _right = new TableLayoutPanel();
            _ytButton = new Button();
            _historyTitle = new Label();

            SuspendLayout();

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
            AutoScaleMode = AutoScaleMode.Font;

            // ---- Split: left calculator, right history ----
            _split.Dock = DockStyle.Fill;
            _split.ColumnCount = 2;
            _split.Padding = new Padding(12);
            _split.BackColor = AppBackground;
            _split.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 460));
            _split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            Controls.Add(_split);

            // ================= LEFT =================
            _left.Dock = DockStyle.Fill;
            _left.RowCount = 5;
            _left.BackColor = AppBackground;
            _left.RowStyles.Add(new RowStyle(SizeType.Absolute, 150)); // inputs
            _left.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));  // checkboxes
            _left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // keypad
            _left.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));  // calculate
            _left.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));  // bottom row (youtube)
            _split.Controls.Add(_left, 0, 0);

            // Inputs area
            _inputs.Dock = DockStyle.Fill;
            _inputs.RowCount = 4;
            _inputs.ColumnCount = 1;
            _inputs.Padding = new Padding(12);
            _inputs.BackColor = CardBackground;
            _inputs.Margin = new Padding(0, 0, 0, 12);
            _inputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
            _inputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            _inputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
            _inputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            _left.Controls.Add(_inputs, 0, 0);

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

            _inputs.Controls.Add(_label1, 0, 0);
            _inputs.Controls.Add(_input1, 0, 1);
            _inputs.Controls.Add(_label2, 0, 2);
            _inputs.Controls.Add(_input2, 0, 3);

            SetSecondInputVisible(false);

            // Checkboxes row
            _optionsRow.Dock = DockStyle.Fill;
            _optionsRow.ColumnCount = 2;
            _optionsRow.Padding = new Padding(12);
            _optionsRow.BackColor = CardBackground;
            _optionsRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            _optionsRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

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

            _optionsRow.Controls.Add(_optA, 0, 0);
            _optionsRow.Controls.Add(_optB, 1, 0);
            _left.Controls.Add(_optionsRow, 0, 1);

            // Keypad
            _left.Controls.Add(BuildKeypad(), 0, 2);

            // Calculate
            _calc.Text = "Calculate";
            _calc.Dock = DockStyle.Fill;
            _calc.Font = new Font(Font.FontFamily, 12f, FontStyle.Bold);
            StylePrimaryButton(_calc);
            _calc.Click += (_, __) => DoCalculate();
            _left.Controls.Add(_calc, 0, 3);

            // Bottom-left YouTube button (out of the way)
            _bottomRow.Dock = DockStyle.Fill;
            _bottomRow.ColumnCount = 2;
            _bottomRow.BackColor = AppBackground;
            _bottomRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 44));
            _bottomRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            _ytButton.Text = "🔥";
            _ytButton.Dock = DockStyle.Fill;
            _ytButton.Font = new Font(Font.FontFamily, 10f, FontStyle.Regular);
            StyleGhostButton(_ytButton);
            var tip = new ToolTip();
            tip.SetToolTip(_ytButton, "Open help video (YouTube)");
            _ytButton.Click += (_, __) =>
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

            _bottomRow.Controls.Add(_ytButton, 0, 0);
            _bottomRow.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = AppBackground }, 1, 0);
            _left.Controls.Add(_bottomRow, 0, 4);

            // ================= RIGHT (HISTORY) =================
            _right.Dock = DockStyle.Fill;
            _right.RowCount = 2;
            _right.ColumnCount = 1;
            _right.BackColor = AppBackground;
            _right.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));   // title only
            _right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // table
            _split.Controls.Add(_right, 1, 0);

            _historyTitle.Text = "History";
            _historyTitle.Dock = DockStyle.Fill;
            _historyTitle.TextAlign = ContentAlignment.MiddleLeft;
            _historyTitle.Font = new Font(Font.FontFamily, 12f, FontStyle.Bold);
            _historyTitle.ForeColor = TextPrimary;
            _right.Controls.Add(_historyTitle, 0, 0);

            ConfigureHistoryGrid();
            _right.Controls.Add(_history, 0, 1);

            ApplyModernTheme(this);
            AcceptButton = _calc;
            Shown += (_, __) => _input1.Focus();

            ResumeLayout(false);
        }

        #endregion
    }
}
