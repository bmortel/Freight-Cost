using Freight_Cost.Core;
using Freight_Cost.UI;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
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
    private const string LogoSoundAlias = "FreightCostLogoSound";

    private TextBox? _activeInput;
    private bool _isCheckingForUpdates;
    private DarkScrollBar? _historyVerticalScrollBar;
    private DarkScrollBar? _historyHorizontalScrollBar;

    public Form1()
    {
        InitializeComponent();
        Text = $"M.F. BOYS CALCULATOR v{AppUpdater.CurrentVersion}";
        ApplyCalculatorLayout();
        AddMenuBar();
        WireEvents();
    }

    /// <summary>
    /// Applies the quiet spacing and display hierarchy used by Windows 11 Calculator.
    /// Kept outside the designer file so Visual Studio can safely regenerate it.
    /// </summary>
    private void ApplyCalculatorLayout()
    {
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        ClientSize = new Size(1080, 680);

        _split.Padding = new Padding(20, 12, 20, 20);
        _split.ColumnStyles[1].Width = 500;
        _left.Margin = new Padding(0, 0, 18, 0);
        _right.Margin = Padding.Empty;

        _left.RowStyles[0].Height = 180;
        _left.RowStyles[1].Height = 56;
        _left.RowStyles[3].Height = 58;
        _left.RowStyles[4].Height = 70;

        _inputs.BackColor = Theme.AppBackground;
        _inputs.Padding = new Padding(8, 10, 8, 4);
        _inputs.Margin = Padding.Empty;
        _inputs.RowStyles[0].Height = 22;
        _inputs.RowStyles[1].Height = 60;
        _inputs.RowStyles[2].Height = 22;
        _inputs.RowStyles[3].Height = 60;

        StyleDisplay(_input1, _label1);
        StyleDisplay(_input2, _label2);
        _label1.Text = "Quote";
        _label2.Text = "C.H. Robinson length fee";

        _optionsRow.BackColor = Theme.AppBackground;
        _optionsRow.Padding = new Padding(8, 8, 8, 6);
        _optA.Font = new Font("Segoe UI", 9.5f);
        _optB.Font = new Font("Segoe UI", 9.5f);
        _optA.FlatStyle = FlatStyle.Flat;
        _optB.FlatStyle = FlatStyle.Flat;
        _optA.FlatAppearance.BorderSize = 0;
        _optB.FlatAppearance.BorderSize = 0;
        StyleGoldCheckBox(_optA);
        StyleGoldCheckBox(_optB);

        _calc.Text = "Calculate  =";
        _calc.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
        _calc.Margin = new Padding(6, 5, 6, 5);

        _bottomRow.BackColor = Theme.AppBackground;
        _rambo.Cursor = Cursors.Hand;
        _rambo.TabStop = true;
        _rambo.AccessibleName = "Play or stop the Rambo sound clip";
        _outputCaption.ForeColor = Theme.TextMuted;
        _outputCaption.Font = new Font("Segoe UI", 9.5f);
        _outputValue.Font = new Font("Segoe UI", 19f, FontStyle.Bold);
        _outputValue.ForeColor = Color.Gold;

        _right.BackColor = Theme.AppBackground;
        _historyTitle.Font = new Font("Segoe UI", 16f, FontStyle.Bold);
        _historyTitle.Padding = new Padding(8, 0, 0, 0);
        _history.BorderStyle = BorderStyle.FixedSingle;
        ConfigureExternalHistoryScrollBars();

        // Avoid WinForms drawing a persistent default-button outline around
        // Calculate. Enter still calculates while either currency input is active.
        AcceptButton = null;
        KeyPreview = true;
        KeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter || ActiveControl is not TextBox)
            {
                return;
            }

            CalculateAndRender();
            e.Handled = true;
            e.SuppressKeyPress = true;
        };
    }

    private static void StyleGoldCheckBox(CheckBox checkBox)
    {
        checkBox.UseVisualStyleBackColor = false;
        checkBox.CheckedChanged += (_, _) => checkBox.Invalidate();
        checkBox.Paint += (_, e) =>
        {
            const int boxSize = 14;
            var top = Math.Max(0, (checkBox.ClientSize.Height - boxSize) / 2);
            var box = new Rectangle(1, top, boxSize, boxSize);

            using var background = new SolidBrush(Theme.AppBackground);
            using var border = new Pen(Color.FromArgb(105, 105, 105), 2f);
            e.Graphics.FillRectangle(background, box);
            e.Graphics.DrawRectangle(border, box);

            if (!checkBox.Checked)
            {
                return;
            }

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var check = new Pen(Color.Gold, 2.2f)
            {
                StartCap = System.Drawing.Drawing2D.LineCap.Round,
                EndCap = System.Drawing.Drawing2D.LineCap.Round
            };
            e.Graphics.DrawLines(check,
            new Point[]
            {
                new Point(box.Left + 3, box.Top + 7),
                new Point(box.Left + 6, box.Top + 10),
                new Point(box.Left + 12, box.Top + 3)
            });
        };
    }

    private void ConfigureExternalHistoryScrollBars()
    {
        if (_historyVerticalScrollBar is not null)
        {
            return;
        }

        var host = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Theme.AppBackground,
            ColumnCount = 2,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        host.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        host.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20f));
        host.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        host.RowStyles.Add(new RowStyle(SizeType.Absolute, 20f));

        _historyVerticalScrollBar = new DarkScrollBar(DarkScrollOrientation.Vertical)
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(4, 0, 0, 0),
            SmallChange = 1
        };
        _historyHorizontalScrollBar = new DarkScrollBar(DarkScrollOrientation.Horizontal)
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 4, 0, 0),
            SmallChange = 10
        };

        _right.Controls.Remove(_history);
        host.Controls.Add(_history, 0, 0);
        host.Controls.Add(_historyVerticalScrollBar, 1, 0);
        host.Controls.Add(_historyHorizontalScrollBar, 0, 1);
        host.Controls.Add(new Panel { BackColor = Theme.AppBackground, Dock = DockStyle.Fill }, 1, 1);
        _right.Controls.Add(host, 0, 1);

        _historyVerticalScrollBar.ValueChanged += (_, _) =>
        {
            if (_history.RowCount > 0)
            {
                _history.FirstDisplayedScrollingRowIndex = Math.Min(
                    _historyVerticalScrollBar.Value,
                    _history.RowCount - 1);
            }
        };
        _historyHorizontalScrollBar.ValueChanged += (_, _) =>
            _history.HorizontalScrollingOffset = _historyHorizontalScrollBar.Value;

        _history.RowsAdded += (_, _) => UpdateExternalHistoryScrollBars();
        _history.RowsRemoved += (_, _) => UpdateExternalHistoryScrollBars();
        _history.Resize += (_, _) => UpdateExternalHistoryScrollBars();
        _history.ColumnWidthChanged += (_, _) => UpdateExternalHistoryScrollBars();
        _history.Scroll += (_, _) => UpdateExternalHistoryScrollBars();
        Shown += (_, _) => UpdateExternalHistoryScrollBars();
    }

    private void UpdateExternalHistoryScrollBars()
    {
        if (_historyVerticalScrollBar is null || _historyHorizontalScrollBar is null)
        {
            return;
        }

        var visibleRows = Math.Max(1, _history.DisplayedRowCount(false));
        _historyVerticalScrollBar.Maximum = Math.Max(0, _history.RowCount - 1);
        _historyVerticalScrollBar.LargeChange = visibleRows;
        _historyVerticalScrollBar.Enabled = _history.RowCount > visibleRows;

        var maximumVerticalValue = Math.Max(
            _historyVerticalScrollBar.Minimum,
            _historyVerticalScrollBar.Maximum - _historyVerticalScrollBar.LargeChange + 1);
        var currentRow = Math.Max(0, _history.FirstDisplayedScrollingRowIndex);
        _historyVerticalScrollBar.Value = Math.Min(currentRow, maximumVerticalValue);

        var contentWidth = _history.Columns.GetColumnsWidth(DataGridViewElementStates.Visible);
        var visibleWidth = Math.Max(1, _history.DisplayRectangle.Width);
        _historyHorizontalScrollBar.Maximum = Math.Max(0, contentWidth - 1);
        _historyHorizontalScrollBar.LargeChange = visibleWidth;
        _historyHorizontalScrollBar.Enabled = contentWidth > visibleWidth;

        var maximumHorizontalValue = Math.Max(
            _historyHorizontalScrollBar.Minimum,
            _historyHorizontalScrollBar.Maximum - _historyHorizontalScrollBar.LargeChange + 1);
        _historyHorizontalScrollBar.Value = Math.Min(
            _history.HorizontalScrollingOffset,
            maximumHorizontalValue);
    }

    private static void StyleDisplay(TextBox input, Label label)
    {
        label.Font = new Font("Segoe UI", 9f, FontStyle.Regular);
        label.ForeColor = Theme.TextMuted;
        input.BackColor = Theme.AppBackground;
        input.ForeColor = Theme.TextPrimary;
        input.BorderStyle = BorderStyle.FixedSingle;
        input.Font = new Font("Segoe UI", 27f, FontStyle.Regular);
        input.Margin = new Padding(0, 2, 0, 2);
        input.TextAlign = HorizontalAlignment.Right;
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
        _rambo.Click += (_, _) => ToggleLogoSound();
        FormClosed += (_, _) => StopLogoSound();

        // Startup behavior: focus first box and run a silent update check.
        Shown += async (_, _) =>
        {
            _input1.Focus();
            await CheckForUpdatesAsync(userInitiated: false, preferCachedResult: true);
        };
    }

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        // Windows must be allowed to shut down without this application
        // displaying or waiting on the normal user-close confirmation.
        if (e.CloseReason == CloseReason.WindowsShutDown)
        {
            return;
        }

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
            BackColor = Theme.AppBackground,
            ForeColor = Theme.TextPrimary,
            Font = new Font("Segoe UI", 10f),
            Padding = new Padding(10, 4, 8, 4),
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

    private void ToggleLogoSound()
    {
        try
        {
            if (IsLogoSoundPlaying())
            {
                StopLogoSound();
                return;
            }

            StopLogoSound();
            var soundPath = ExtractLogoSound();
            ThrowIfMciFailed(MciSendString(
                $"open \"{soundPath}\" type mpegvideo alias {LogoSoundAlias}",
                null,
                0,
                IntPtr.Zero));
            ThrowIfMciFailed(MciSendString(
                $"play {LogoSoundAlias}",
                null,
                0,
                IntPtr.Zero));
        }
        catch (Exception ex)
        {
            StopLogoSound();
            MessageBox.Show(
                $"Unable to play the sound clip.\n{ex.Message}",
                "Audio Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private static bool IsLogoSoundPlaying()
    {
        var status = new StringBuilder(32);
        var result = MciSendString(
            $"status {LogoSoundAlias} mode",
            status,
            status.Capacity,
            IntPtr.Zero);
        return result == 0 && status.ToString().Trim().Equals("playing", StringComparison.OrdinalIgnoreCase);
    }

    private static void StopLogoSound()
    {
        _ = MciSendString($"stop {LogoSoundAlias}", null, 0, IntPtr.Zero);
        _ = MciSendString($"close {LogoSoundAlias}", null, 0, IntPtr.Zero);
    }

    private static string ExtractLogoSound()
    {
        var resourceName = Assembly.GetExecutingAssembly()
            .GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith(".Rambo.mp3", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("The embedded sound clip was not found.");

        using var source = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException("The embedded sound clip could not be opened.");

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var mediaDirectory = Path.Combine(appData, "Freight Cost", "Media");
        Directory.CreateDirectory(mediaDirectory);

        var soundPath = Path.Combine(mediaDirectory, "Rambo.mp3");
        if (!File.Exists(soundPath) || new FileInfo(soundPath).Length != source.Length)
        {
            using var destination = File.Create(soundPath);
            source.CopyTo(destination);
        }

        return soundPath;
    }

    private static void ThrowIfMciFailed(int errorCode)
    {
        if (errorCode == 0)
        {
            return;
        }

        var message = new StringBuilder(256);
        _ = MciGetErrorString(errorCode, message, message.Capacity);
        throw new InvalidOperationException(message.Length > 0 ? message.ToString() : $"Audio error {errorCode}.");
    }

    [DllImport("winmm.dll", EntryPoint = "mciSendStringW", CharSet = CharSet.Unicode)]
    private static extern int MciSendString(
        string command,
        StringBuilder? returnValue,
        int returnLength,
        IntPtr callback);

    [DllImport("winmm.dll", EntryPoint = "mciGetErrorStringW", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool MciGetErrorString(int errorCode, StringBuilder errorText, int errorTextSize);

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
            Margin = new Padding(3),
            Font = new Font("Segoe UI", 13f, FontStyle.Regular)
        };

        Theme.StyleSecondaryButton(button);
        if (text is "CE" or ".")
        {
            button.BackColor = Theme.Accent;
            button.ForeColor = Color.White;
            button.FlatAppearance.MouseOverBackColor = Theme.AccentHover;
            button.FlatAppearance.MouseDownBackColor = Theme.AccentPressed;
        }
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
        _history.ScrollBars = ScrollBars.None;
        _history.MultiSelect = false;
        _history.SelectionMode = DataGridViewSelectionMode.CellSelect;
        _history.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
        _history.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
        _history.Font = new Font("Segoe UI", 9.5f);
        _history.EnableHeadersVisualStyles = false;
        _history.GridColor = Theme.BorderColor;
        _history.BorderStyle = BorderStyle.FixedSingle;
        _history.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        _history.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        _history.ColumnHeadersHeight = 42;
        _history.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        _history.RowTemplate.Height = 38;

        _history.ColumnHeadersDefaultCellStyle.BackColor = Theme.AppBackground;
        _history.ColumnHeadersDefaultCellStyle.ForeColor = Theme.TextMuted;
        _history.ColumnHeadersDefaultCellStyle.Font = new Font(Font, FontStyle.Bold);
        _history.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

        _history.DefaultCellStyle.BackColor = Theme.AppBackground;
        _history.DefaultCellStyle.ForeColor = Theme.TextPrimary;
        _history.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        _history.DefaultCellStyle.SelectionBackColor = Theme.AccentSoft;
        _history.DefaultCellStyle.SelectionForeColor = Theme.TextPrimary;
        _history.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(36, 36, 36);

        _history.Columns.Clear();
        _history.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quote", HeaderText = "Quote", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 31 });
        _history.Columns.Add(new DataGridViewTextBoxColumn { Name = "Mul", HeaderText = string.Empty, Width = 20 });
        _history.Columns.Add(new DataGridViewTextBoxColumn { Name = "Fees", HeaderText = "Fees", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 31 });
        _history.Columns.Add(new DataGridViewTextBoxColumn { Name = "Eq", HeaderText = string.Empty, Width = 20 });
        _history.Columns.Add(new DataGridViewTextBoxColumn { Name = "Freight", HeaderText = "Freight Cost", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 25 });
        _history.Columns.Add(new DataGridViewButtonColumn
        {
            Name = "Remove",
            HeaderText = string.Empty,
            Width = 40,
            Text = "×",
            FlatStyle = FlatStyle.Flat,
            UseColumnTextForButtonValue = true
        });

        foreach (DataGridViewColumn column in _history.Columns)
        {
            column.SortMode = DataGridViewColumnSortMode.NotSortable;
        }

        _history.CellClick += OnHistoryCellClick;
        _history.MouseWheel += OnHistoryMouseWheel;
        AddHistoryContextMenu();
    }

    private void OnHistoryMouseWheel(object? sender, MouseEventArgs e)
    {
        if (_history.RowCount == 0)
        {
            return;
        }

        var currentRow = Math.Max(0, _history.FirstDisplayedScrollingRowIndex);
        var direction = e.Delta > 0 ? -3 : 3;
        _history.FirstDisplayedScrollingRowIndex = Math.Clamp(
            currentRow + direction,
            0,
            _history.RowCount - 1);
        UpdateExternalHistoryScrollBars();
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
