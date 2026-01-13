namespace Freight_Cost
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            components = new System.ComponentModel.Container();
            split = new TableLayoutPanel();
            left = new TableLayoutPanel();
            inputs = new TableLayoutPanel();
            headerPanel = new Panel();
            outputHeaderLabel = new Label();
            optionsRow = new TableLayoutPanel();
            bottomRow = new TableLayoutPanel();
            ytButton = new Button();
            right = new TableLayoutPanel();
            tip = new ToolTip(components);
            split.SuspendLayout();
            left.SuspendLayout();
            inputs.SuspendLayout();
            headerPanel.SuspendLayout();
            bottomRow.SuspendLayout();
            SuspendLayout();
            // 
            // split
            // 
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 460F));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            split.Controls.Add(left, 0, 0);
            split.Controls.Add(right, 1, 0);
            split.Location = new Point(0, 0);
            split.Name = "split";
            split.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            split.Size = new Size(200, 100);
            split.TabIndex = 0;
            // 
            // left
            // 
            left.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            left.Controls.Add(inputs, 0, 0);
            left.Controls.Add(optionsRow, 0, 1);
            left.Controls.Add(bottomRow, 0, 4);
            left.Location = new Point(3, 3);
            left.Name = "left";
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 150F));
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 55F));
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            left.Size = new Size(200, 94);
            left.TabIndex = 0;
            // 
            // inputs
            // 
            inputs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            inputs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            inputs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            inputs.Controls.Add(headerPanel, 1, 0);
            inputs.Location = new Point(3, 3);
            inputs.Name = "inputs";
            inputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            inputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            inputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            inputs.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            inputs.Size = new Size(194, 100);
            inputs.TabIndex = 0;
            // 
            // headerPanel
            // 
            headerPanel.Controls.Add(outputHeaderLabel);
            headerPanel.Location = new Point(100, 3);
            headerPanel.Name = "headerPanel";
            headerPanel.Size = new Size(91, 94);
            headerPanel.TabIndex = 0;
            headerPanel.Paint += headerPanel_Paint;
            // 
            // outputHeaderLabel
            // 
            outputHeaderLabel.Location = new Point(0, 0);
            outputHeaderLabel.Name = "outputHeaderLabel";
            outputHeaderLabel.Size = new Size(100, 23);
            outputHeaderLabel.TabIndex = 0;
            // 
            // optionsRow
            // 
            optionsRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            optionsRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            optionsRow.Location = new Point(3, 153);
            optionsRow.Name = "optionsRow";
            optionsRow.Size = new Size(194, 54);
            optionsRow.TabIndex = 1;
            // 
            // bottomRow
            // 
            bottomRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 44F));
            bottomRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            bottomRow.Controls.Add(ytButton, 0, 0);
            bottomRow.Location = new Point(3, 53);
            bottomRow.Name = "bottomRow";
            bottomRow.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            bottomRow.Size = new Size(194, 38);
            bottomRow.TabIndex = 2;
            // 
            // ytButton
            // 
            ytButton.Location = new Point(3, 3);
            ytButton.Name = "ytButton";
            ytButton.Size = new Size(38, 23);
            ytButton.TabIndex = 0;
            tip.SetToolTip(ytButton, "Open help video (YouTube)");
            // 
            // right
            // 
            right.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            right.Location = new Point(463, 3);
            right.Name = "right";
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            right.Size = new Size(1, 94);
            right.TabIndex = 1;
            // 
            // Form1
            // 
            ClientSize = new Size(1000, 580);
            Controls.Add(split);
            Font = new Font("Segoe UI", 10F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "M.F. BOYS CALCULATOR";
            split.ResumeLayout(false);
            left.ResumeLayout(false);
            inputs.ResumeLayout(false);
            headerPanel.ResumeLayout(false);
            bottomRow.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel split;
        private TableLayoutPanel left;
        private TableLayoutPanel inputs;
        private Panel headerPanel;
        private Label outputHeaderLabel;
        private TableLayoutPanel optionsRow;
        private TableLayoutPanel bottomRow;
        private Button ytButton;
        private ToolTip tip;
        private TableLayoutPanel right;
    }
}
