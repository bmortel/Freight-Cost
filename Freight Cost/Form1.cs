using Freight_Cost.Core;
using Freight_Cost.UI;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Freight_Cost;

/// <summary>
/// Main calculator window.
/// UI events are routed into small methods so beginners can trace behavior easily.
/// </summary>
public partial class Form1 : Form
{
    private const string HelpVideoUrl = "https://www.youtube.com/watch?v=1WaV2x8GXj0&list=RD1WaV2x8GXj0&start_radio=1";
    private const string ReleasesPageUrl = "https://github.com/bmortel/Freight-Cost/releases";

    private TextBox? _activeInput;
    private bool _isCheckingForUpdates;

    public Form1()
    {
        InitializeComponent();
        AddMenuBar();
        WireEvents();
    }

    /// <summary>
    /// Central place to wire all runtime event handlers.
    /// </summary>
    private void WireEvents()
    {
        FormClosing += OnFormClosing;

        _input1.Enter += (_, _) => _activeInput = _input1;
        _input2.Enter += (_, _) => _activeInput = _input2;
        _input1.MouseDown += (_, _) => _activeInput = _input1;
        _input2.MouseDown += (_, _) => _activeInput = _input2;

        _optB.CheckedChanged += OnOptionBChanged;
        _calc.Click += (_, _) => CalculateAndRender();
        _ytButton.Click += (_, _) => OpenHelpVideo();

        // Startup behavior: focus first box and run a silent update check.
        Shown += async (_, _) =>
        {
            _input1.Focus();
            await CheckForUpdatesAsync(userInitiated: false, preferCachedResult: true);
        };
    }

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        var result = MessageBox.Show(
            "Exit the Calculator MFer?",
            "Got soft hands brother?",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        e.Cancel = result != DialogResult.Yes;
    }

    /// <summary>
    /// Builds the top menu (File / Help) and hooks item actions.
    /// </summary>
    private void AddMenuBar()
    {
        var menu = new MenuStrip
        {
            BackColor = Theme.CardBackground,
            ForeColor = Theme.TextPrimary,
            RenderMode = ToolStripRenderMode.Professional,
            Renderer = new ToolStripProfessionalRenderer(new DarkMenuColors())
        };

        var fileMenu = new ToolStripMenuItem("File") { ForeColor = Color.White };
        var clearHistory = new ToolStripMenuItem("Clear History") { ForeColor = Color.White };
        var exit = new ToolStripMenuItem("Exit")
        {
            ForeColor = Color.White,
            ShortcutKeys = Keys.Alt | Keys.F4
        };

        clearHistory.Click += (_, _) =>
        {
            var confirm = MessageBox.Show(
                "Are you sure you want to clear all history?\nThis cannot be undone.",
                "Confirm Clear History",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                _history.Rows.Clear();
            }
        };

        exit.Click += (_, _) => Close();

        fileMenu.DropDownItems.Add(clearHistory);
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add(exit);

        var helpMenu = new ToolStripMenuItem("Help") { ForeColor = Color.White };
        var checkUpdatesItem = new ToolStripMenuItem("Check for Updates") { ForeColor = Color.White };
        var aboutItem = new ToolStripMenuItem("About") { ForeColor = Color.White };

        checkUpdatesItem.Click += async (_, _) => await CheckForUpdatesAsync(userInitiated: true, preferCachedResult: false);
        aboutItem.Click += (_, _) => new AboutForm().ShowDialog(this);

        helpMenu.DropDownItems.Add(checkUpdatesItem);
        helpMenu.DropDownItems.Add(new ToolStripSeparator());
        helpMenu.DropDownItems.Add(aboutItem);

        menu.Items.Add(fileMenu);
        menu.Items.Add(helpMenu);

        MainMenuStrip = menu;
        Controls.Add(menu);
        menu.Dock = DockStyle.Top;
    }


    /// <summary>
    /// Checks GitHub for a newer release, prompts user, and downloads installer asset.
    /// userInitiated controls whether "no update" / error popups are shown.
    /// </summary>
    private async System.Threading.Tasks.Task CheckForUpdatesAsync(bool userInitiated, bool preferCachedResult)
    {
        if (_isCheckingForUpdates)
        {
            return;
        }

        _isCheckingForUpdates = true;
        try
        {
            var result = await AppUpdater.CheckForUpdateAsync(useCache: preferCachedResult);

            if (!result.HasUpdate)
            {
                if (userInitiated)
                {
                    MessageBox.Show(
                        "You are already on the latest version.",
                        "No Updates Available",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                return;
            }

            var latestTag = result.LatestTag ?? result.LatestVersion?.ToString() ?? "latest";

            // Tag-only updates can happen when GitHub has tags but no published release assets.
            if (result.Asset is null)
            {
                if (!userInitiated)
                {
                    return;
                }

                var openReleases = MessageBox.Show(
                    $"A newer version ({latestTag}) exists, but no downloadable release asset was found.\n\nOpen the releases page?",
                    "Update Available",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (openReleases == DialogResult.Yes)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = ReleasesPageUrl,
                        UseShellExecute = true
                    });
                }

                return;
            }

            var prompt = MessageBox.Show(
                $"A new version ({latestTag}) is available.\n\nDo you want to download {result.Asset.Name}?",
                "Update Available",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);

            if (prompt != DialogResult.Yes)
            {
                return;
            }

            var defaultDownloadsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (string.IsNullOrWhiteSpace(defaultDownloadsDirectory))
            {
                defaultDownloadsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            }

            using var saveDialog = new SaveFileDialog
            {
                Title = "Choose where to save the update",
                FileName = result.Asset.Name,
                InitialDirectory = defaultDownloadsDirectory,
                Filter = "All files (*.*)|*.*",
                RestoreDirectory = true,
                OverwritePrompt = true
            };

            if (saveDialog.ShowDialog(this) != DialogResult.OK || string.IsNullOrWhiteSpace(saveDialog.FileName))
            {
                return;
            }

            var downloadPath = saveDialog.FileName;
            await AppUpdater.DownloadAssetAsync(result.Asset.DownloadUrl, downloadPath);

            var launch = MessageBox.Show(
                $"Update downloaded to:\n{downloadPath}\n\nOpen it now?",
                "Download Complete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (launch == DialogResult.Yes)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = downloadPath,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            if (userInitiated)
            {
                MessageBox.Show(
                    $"Unable to check for updates.\n{ex.Message}",
                    "Update Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        finally
        {
            _isCheckingForUpdates = false;
        }
    }

    /// <summary>
    /// Option B means user enters custom length fee, so we disable Option A.
    /// </summary>
    private void OnOptionBChanged(object? sender, EventArgs e)
    {
        if (_optB.Checked)
        {
            _optA.Checked = false;
            _optA.Enabled = false;
            SetSecondInputVisible(true);
            _input2.Focus();
            return;
        }

        _optA.Enabled = true;
        SetSecondInputVisible(false);
        _input1.Focus();
    }

    /// <summary>
    /// Opens the external help video in the default browser.
    /// </summary>
    private void OpenHelpVideo()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = HelpVideoUrl,
                UseShellExecute = true
            });
        }
        catch
        {
            MessageBox.Show("Unable to open the help video.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Validates inputs, calculates freight cost, then renders output + history row.
    /// </summary>
    private void CalculateAndRender()
    {
        if (!CurrencyInput.TryParseUsd(_input1.Text, out var quote, out var quoteError))
        {
            MessageBox.Show(quoteError, "Invalid Quote", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _input1.Focus();
            return;
        }

        var flatFee = 0m;
        if (_optB.Checked)
        {
            if (!CurrencyInput.TryParseUsd(_input2.Text, out flatFee, out var feeError))
            {
                MessageBox.Show(feeError, "Invalid Length Fee", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _input2.Focus();
                return;
            }
        }
        else if (_optA.Checked)
        {
            flatFee = 150m;
        }

        var multiplier = FreightCalculator.GetMultiplier(quote);
        var freightCost = FreightCalculator.Calculate(quote, flatFee);

        _history.Rows.Insert(
            0,
            quote.ToString("C", CurrencyInput.UsCulture),
            "×",
            $"{multiplier:0.##} + {flatFee.ToString("C", CurrencyInput.UsCulture)}",
            "=",
            freightCost.ToString("C", CurrencyInput.UsCulture),
            "X");

        _outputValue.Text = freightCost.ToString("C", CurrencyInput.UsCulture);
    }

    /// <summary>
    /// Shows/hides the second input used by Option B.
    /// </summary>
    private void SetSecondInputVisible(bool visible)
    {
        _label2.Visible = visible;
        _input2.Visible = visible;

        if (!visible)
        {
            _input2.Text = string.Empty;
        }
    }

    /// <summary>
    /// Creates a numeric keypad for touch/mouse input convenience.
    /// </summary>
    private Control BuildKeypad()
    {
        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 4,
            Margin = new Padding(0),
            Padding = new Padding(6),
            BackColor = Theme.CardBackground
        };

        for (var c = 0; c < 4; c++) grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        for (var r = 0; r < 4; r++) grid.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));

        grid.Controls.Add(MakeKeyButton("7", () => InsertToActive("7")), 0, 0);
        grid.Controls.Add(MakeKeyButton("8", () => InsertToActive("8")), 1, 0);
        grid.Controls.Add(MakeKeyButton("9", () => InsertToActive("9")), 2, 0);
        grid.Controls.Add(MakeKeyButton("CE", ClearActiveInput), 3, 0);

        grid.Controls.Add(MakeKeyButton("4", () => InsertToActive("4")), 0, 1);
        grid.Controls.Add(MakeKeyButton("5", () => InsertToActive("5")), 1, 1);
        grid.Controls.Add(MakeKeyButton("6", () => InsertToActive("6")), 2, 1);
        grid.Controls.Add(MakeKeyButton(".", () => InsertToActive(".")), 3, 1);

        grid.Controls.Add(MakeKeyButton("1", () => InsertToActive("1")), 0, 2);
        grid.Controls.Add(MakeKeyButton("2", () => InsertToActive("2")), 1, 2);
        grid.Controls.Add(MakeKeyButton("3", () => InsertToActive("3")), 2, 2);

        grid.Controls.Add(MakeKeyButton("0", () => InsertToActive("0")), 1, 3);

        return grid;
    }

    private Button MakeKeyButton(string text, Action onClick)
    {
        var button = new Button
        {
            Text = text,
            Dock = DockStyle.Fill,
            Margin = new Padding(6),
            Font = new Font(Font.FontFamily, 12f, FontStyle.Bold)
        };

        Theme.StyleSecondaryButton(button);
        button.Click += (_, _) => onClick();

        return button;
    }

    private TextBox GetActiveInput() => _activeInput is { Visible: true } ? _activeInput : _input1;

    private void InsertToActive(string value)
    {
        var textBox = GetActiveInput();
        var selectionStart = textBox.SelectionStart;
        var selectionLength = textBox.SelectionLength;

        var combined = string.Concat(
            textBox.Text.AsSpan(0, selectionStart),
            value,
            textBox.Text.AsSpan(selectionStart + selectionLength));

        textBox.Text = CurrencyInput.Normalize(combined);
        textBox.SelectionStart = textBox.TextLength;

        if (!textBox.Focused)
        {
            textBox.Focus();
        }
    }

    private void ClearActiveInput()
    {
        var textBox = GetActiveInput();
        textBox.Text = string.Empty;
        _outputValue.Text = string.Empty;   
        textBox.SelectionStart = 0;

        if (!textBox.Focused)
        {
            textBox.Focus();
        }
    }

    /// <summary>
    /// Applies structure + style to the history DataGridView.
    /// </summary>
    private void ConfigureHistoryGrid()
    {
        _history.Dock = DockStyle.Fill;
        _history.BackgroundColor = Theme.CardBackground;
        _history.ReadOnly = true;
        _history.AllowUserToAddRows = false;
        _history.AllowUserToDeleteRows = false;
        _history.AllowUserToResizeRows = false;
        _history.AllowUserToResizeColumns = false;
        _history.RowHeadersVisible = false;
        _history.MultiSelect = false;
        _history.SelectionMode = DataGridViewSelectionMode.CellSelect;
        _history.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
        _history.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
        _history.EnableHeadersVisualStyles = false;
        _history.GridColor = Theme.BorderColor;
        _history.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

        _history.ColumnHeadersDefaultCellStyle.BackColor = Theme.CardBackground;
        _history.ColumnHeadersDefaultCellStyle.ForeColor = Theme.TextMuted;
        _history.ColumnHeadersDefaultCellStyle.Font = new Font(Font, FontStyle.Bold);
        _history.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

        _history.DefaultCellStyle.BackColor = Theme.CardBackground;
        _history.DefaultCellStyle.ForeColor = Theme.TextPrimary;
        _history.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        _history.DefaultCellStyle.SelectionBackColor = Theme.AccentSoft;
        _history.DefaultCellStyle.SelectionForeColor = Theme.TextPrimary;
        _history.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(40, 40, 40);

        _history.Columns.Clear();
        _history.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quote", HeaderText = "Quote", Width = 160 });
        _history.Columns.Add(new DataGridViewTextBoxColumn { Name = "Mul", HeaderText = string.Empty, Width = 20 });
        _history.Columns.Add(new DataGridViewTextBoxColumn { Name = "Fees", HeaderText = "Fees", Width = 160 });
        _history.Columns.Add(new DataGridViewTextBoxColumn { Name = "Eq", HeaderText = string.Empty, Width = 20 });
        _history.Columns.Add(new DataGridViewTextBoxColumn { Name = "Freight", HeaderText = "Freight Cost", Width = 110 });
        _history.Columns.Add(new DataGridViewButtonColumn
        {
            Name = "Remove",
            HeaderText = string.Empty,
            Width = 40,
            Text = "X",
            UseColumnTextForButtonValue = true
        });

        foreach (DataGridViewColumn column in _history.Columns)
        {
            column.SortMode = DataGridViewColumnSortMode.NotSortable;
        }

        _history.CellClick += OnHistoryCellClick;
        AddHistoryContextMenu();
    }

    private void OnHistoryCellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0)
        {
            return;
        }

        if (_history.Columns[e.ColumnIndex].Name != "Remove")
        {
            return;
        }

        var quote = _history.Rows[e.RowIndex].Cells[0].Value?.ToString() ?? "this entry";
        var confirm = MessageBox.Show($"Remove {quote}?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm == DialogResult.Yes)
        {
            _history.Rows.RemoveAt(e.RowIndex);
        }
    }

    private void AddHistoryContextMenu()
    {
        var menu = new ContextMenuStrip();
        var copyItem = new ToolStripMenuItem("Copy") { ShortcutKeys = Keys.Control | Keys.C };
        copyItem.Click += (_, _) =>
        {
            if (_history.GetCellCount(DataGridViewElementStates.Selected) <= 0)
            {
                return;
            }

            var content = _history.GetClipboardContent();
            if (content is not null)
            {
                Clipboard.SetDataObject(content);
            }
        };

        menu.Items.Add(copyItem);
        _history.ContextMenuStrip = menu;

        _history.CellMouseDown += (_, e) =>
        {
            if (e.Button != MouseButtons.Right || e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            _history.CurrentCell = _history[e.ColumnIndex, e.RowIndex];
            if (_history[e.ColumnIndex, e.RowIndex].Selected)
            {
                return;
            }

            _history.ClearSelection();
            _history[e.ColumnIndex, e.RowIndex].Selected = true;
        };
    }

    private static void AddRightClickMenu(TextBox textBox)
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Cut", null, (_, _) => textBox.Cut());
        menu.Items.Add("Copy", null, (_, _) => textBox.Copy());
        menu.Items.Add("Paste", null, (_, _) => PasteNormalized(textBox));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Select All", null, (_, _) => textBox.SelectAll());

        textBox.ContextMenuStrip = menu;
    }

    /// <summary>
    /// Restricts textbox input to currency-friendly characters and normalizes paste.
    /// </summary>
    private static void AttachInputFilters(TextBox textBox)
    {
        textBox.KeyPress += (_, e) =>
        {
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            var keyChar = e.KeyChar;
            if (char.IsDigit(keyChar) || keyChar == ',' || keyChar == '$')
            {
                return;
            }

            if (keyChar == '.')
            {
                e.Handled = textBox.Text.Contains('.');
                return;
            }

            e.Handled = true;
        };

        textBox.KeyDown += (_, e) =>
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                PasteNormalized(textBox);
                e.SuppressKeyPress = true;
            }
        };

        textBox.TextChanged += (_, _) =>
        {
            var normalized = CurrencyInput.Normalize(textBox.Text);
            if (normalized == textBox.Text)
            {
                return;
            }

            var caret = textBox.SelectionStart;
            textBox.Text = normalized;
            textBox.SelectionStart = Math.Min(caret, textBox.Text.Length);
        };
    }

    private static void PasteNormalized(TextBox textBox)
    {
        if (!Clipboard.ContainsText())
        {
            return;
        }

        var selectionStart = textBox.SelectionStart;
        var selectionLength = textBox.SelectionLength;
        var combined = string.Concat(
            textBox.Text.AsSpan(0, selectionStart),
            Clipboard.GetText(),
            textBox.Text.AsSpan(selectionStart + selectionLength));

        textBox.Text = CurrencyInput.Normalize(combined);
        textBox.SelectionStart = textBox.TextLength;
    }
}
