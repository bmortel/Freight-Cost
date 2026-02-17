using Freight_Cost.UI;
using Freight_Cost.Core;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Freight_Cost;

partial class Form1
{
    private IContainer? components;

    private TextBox _input1 = null!;
    private TextBox _input2 = null!;
    private Label _outputCaption = null!;
    private Label _outputValue = null!;
    private Label _label1 = null!;
    private Label _label2 = null!;
    private Label _historyTitle = null!;
    private CheckBox _optA = null!;
    private CheckBox _optB = null!;
    private Button _calc = null!;
    private DataGridView _history = null!;

    private TableLayoutPanel _split = null!;
    private TableLayoutPanel _left = null!;
    private TableLayoutPanel _inputs = null!;
    private TableLayoutPanel _optionsRow = null!;
    private TableLayoutPanel _bottomRow = null!;
    private TableLayoutPanel _right = null!;

    private Button _ytButton = null!;
    private PictureBox _rambo = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new Container();

        _input1 = new TextBox();
        _input2 = new TextBox();
        _outputCaption = new Label();
        _outputValue = new Label();
        _label1 = new Label();
        _label2 = new Label();
        _historyTitle = new Label();
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
        _rambo = new PictureBox();

        SuspendLayout();

        Text = $"M.F. BOYS CALCULATOR v{AppUpdater.CurrentVersion}";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = true;
        ClientSize = new Size(1000, 600);
        BackColor = Theme.AppBackground;
        ForeColor = Theme.TextPrimary;
        AutoScaleMode = AutoScaleMode.Font;

        _split.Dock = DockStyle.Fill;
        _split.ColumnCount = 2;
        _split.Padding = new Padding(12);
        _split.BackColor = Theme.AppBackground;
        _split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _split.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 525));
        Controls.Add(_split);

        _left.Dock = DockStyle.Fill;
        _left.RowCount = 5;
        _left.BackColor = Theme.AppBackground;
        _left.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
        _left.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        _left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        _left.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
        _left.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        _split.Controls.Add(_left, 0, 0);

        _inputs.Dock = DockStyle.Fill;
        _inputs.RowCount = 5;
        _inputs.ColumnCount = 1;
        _inputs.Padding = new Padding(12);
        _inputs.BackColor = Theme.CardBackground;
        _inputs.Margin = new Padding(0, 0, 0, 12);
        _inputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 15));
        _inputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        _inputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 17));
        _inputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        _inputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 0));
        _left.Controls.Add(_inputs, 0, 0);

        _label1.Text = "Quote (USD)";
        _label1.Dock = DockStyle.Fill;
        _label1.Font = new Font(Font, FontStyle.Bold);
        _label1.ForeColor = Theme.TextMuted;

        _input1.Dock = DockStyle.Fill;
        _input1.Font = new Font(Font.FontFamily, 16f, FontStyle.Regular);
        _input1.TextAlign = HorizontalAlignment.Right;
        _input1.PlaceholderText = "$0.00";
        AddRightClickMenu(_input1);
        AttachInputFilters(_input1);

        _label2.Text = "Length fee from C.H. Robinson";
        _label2.Dock = DockStyle.Fill;
        _label2.Font = new Font(Font, FontStyle.Bold);
        _label2.ForeColor = Theme.TextMuted;

        _input2.Dock = DockStyle.Fill;
        _input2.Font = new Font(Font.FontFamily, 16f, FontStyle.Regular);
        _input2.TextAlign = HorizontalAlignment.Right;
        _input2.PlaceholderText = "$0.00";
        AddRightClickMenu(_input2);
        AttachInputFilters(_input2);

        _outputCaption.Text = "Freight Cost:";
        _outputCaption.Dock = DockStyle.Left;
        _outputCaption.AutoSize = true;
        _outputCaption.Font = new Font(Font, FontStyle.Bold);
        _outputCaption.TextAlign = ContentAlignment.MiddleLeft;
        _outputCaption.Margin = new Padding(0, 10, 8, 0);

        _outputValue.Text = "";
        _outputValue.Dock = DockStyle.Fill;
        _outputValue.Font = new Font(Font.FontFamily, 14f, FontStyle.Bold);
        _outputValue.TextAlign = ContentAlignment.MiddleRight;
        _outputValue.BackColor = Theme.CardBackground;
        _outputValue.ForeColor = Theme.TextPrimary;
        _outputValue.AutoSize = true;
        _outputValue.Margin = new Padding(0, 8, 0, 0);

        _inputs.Controls.Add(_label1, 0, 0);
        _inputs.Controls.Add(_input1, 0, 1);
        _inputs.Controls.Add(_label2, 0, 2);
        _inputs.Controls.Add(_input2, 0, 3);

        _optionsRow.Dock = DockStyle.Fill;
        _optionsRow.ColumnCount = 2;
        _optionsRow.Padding = new Padding(12);
        _optionsRow.BackColor = Theme.CardBackground;
        _optionsRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        _optionsRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        _optA.Text = "Length is 8' to 20'?";
        _optA.Dock = DockStyle.Fill;
        _optA.ForeColor = Theme.TextPrimary;

        _optB.Text = "Over 20'?";
        _optB.Dock = DockStyle.Fill;
        _optB.ForeColor = Theme.TextPrimary;

        _optionsRow.Controls.Add(_optA, 0, 0);
        _optionsRow.Controls.Add(_optB, 1, 0);
        _left.Controls.Add(_optionsRow, 0, 1);

        _left.Controls.Add(BuildKeypad(), 0, 2);

        _calc.Text = "Calculate";
        _calc.Dock = DockStyle.Fill;
        _calc.Font = new Font(Font.FontFamily, 11f, FontStyle.Bold);
        Theme.StylePrimaryButton(_calc);
        _left.Controls.Add(_calc, 0, 3);

        _bottomRow.Dock = DockStyle.Fill;
        _bottomRow.ColumnCount = 3;
        _bottomRow.BackColor = Theme.AppBackground;
        _bottomRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60));
        _bottomRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _bottomRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60));

        _ytButton.Text = "🔥";
        _ytButton.Size = new Size(40, 40);
        _ytButton.Margin = new Padding(5);
        _ytButton.Font = new Font("Segoe UI Emoji", 15f, FontStyle.Regular);
        Theme.StyleGhostButton(_ytButton);

        var tip = new ToolTip(components);
        tip.SetToolTip(_ytButton, "Open help video (YouTube)");

        _rambo.Dock = DockStyle.None;
        _rambo.SizeMode = PictureBoxSizeMode.Zoom;
        _rambo.BackColor = Theme.AppBackground;
      //  _rambo.Margin = new Padding(5);
        _rambo.Image = Properties.Resources.RAMBO;

        var outputContainer = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoSize = true,
            BackColor = Theme.AppBackground,
            Margin = new Padding(0)
        };
        outputContainer.Controls.Add(_outputCaption);
        outputContainer.Controls.Add(_outputValue);

        _bottomRow.Controls.Add(_ytButton, 0, 0);
        _bottomRow.Controls.Add(outputContainer, 1, 0);
        _bottomRow.Controls.Add(_rambo, 2, 0);
        _left.Controls.Add(_bottomRow, 0, 4);

        _right.Dock = DockStyle.Fill;
        _right.RowCount = 2;
        _right.ColumnCount = 1;
        _right.BackColor = Theme.AppBackground;
        _right.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
        _right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        _split.Controls.Add(_right, 1, 0);

        _historyTitle.Text = "History";
        _historyTitle.Dock = DockStyle.Fill;
        _historyTitle.TextAlign = ContentAlignment.MiddleLeft;
        _historyTitle.Font = new Font(Font.FontFamily, 12f, FontStyle.Bold);
        _historyTitle.ForeColor = Theme.TextPrimary;
        _right.Controls.Add(_historyTitle, 0, 0);

        ConfigureHistoryGrid();
        _right.Controls.Add(_history, 0, 1);

        Theme.Apply(this);
        _outputValue.ForeColor = Color.Gold;
        SetSecondInputVisible(false);
        AcceptButton = _calc;

        ResumeLayout(false);
    }
}
