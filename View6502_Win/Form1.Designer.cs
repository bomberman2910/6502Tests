namespace View6502_Win
{
    partial class Form_Main
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pictureBoxScreen = new System.Windows.Forms.PictureBox();
            this.timerClock = new System.Windows.Forms.Timer(this.components);
            this.groupBoxCPU = new System.Windows.Forms.GroupBox();
            this.labelCurInst = new System.Windows.Forms.Label();
            this.tableLayoutPanelCPU = new System.Windows.Forms.TableLayoutPanel();
            this.labelADesc = new System.Windows.Forms.Label();
            this.labelAValue = new System.Windows.Forms.Label();
            this.labelXDesc = new System.Windows.Forms.Label();
            this.labelXValue = new System.Windows.Forms.Label();
            this.labelYDesc = new System.Windows.Forms.Label();
            this.labelYValue = new System.Windows.Forms.Label();
            this.labelPCDesc = new System.Windows.Forms.Label();
            this.labelPCValue = new System.Windows.Forms.Label();
            this.labelSPDesc = new System.Windows.Forms.Label();
            this.labelSPValue = new System.Windows.Forms.Label();
            this.labelSRDesc = new System.Windows.Forms.Label();
            this.labelSRValue = new System.Windows.Forms.Label();
            this.buttonCycle = new System.Windows.Forms.Button();
            this.buttonStep = new System.Windows.Forms.Button();
            this.buttonReset = new System.Windows.Forms.Button();
            this.buttonStart = new System.Windows.Forms.Button();
            this.buttonStop = new System.Windows.Forms.Button();
            this.textBoxMemory = new System.Windows.Forms.TextBox();
            this.labelMemStartDesc = new System.Windows.Forms.Label();
            this.numericUpDownMemStartValue = new System.Windows.Forms.NumericUpDown();
            this.buttonMemStartUpdate = new System.Windows.Forms.Button();
            this.groupBoxCharset = new System.Windows.Forms.GroupBox();
            this.numericUpDownPosValue = new System.Windows.Forms.NumericUpDown();
            this.labelPosDesc = new System.Windows.Forms.Label();
            this.labelCharDesc = new System.Windows.Forms.Label();
            this.pictureBoxChar = new System.Windows.Forms.PictureBox();
            this.textBoxTerminal = new System.Windows.Forms.TextBox();
            this.buttonSend = new System.Windows.Forms.Button();
            this.groupBoxMemChange = new System.Windows.Forms.GroupBox();
            this.numericUpDownMemChangeAddress = new System.Windows.Forms.NumericUpDown();
            this.labelColon = new System.Windows.Forms.Label();
            this.numericUpDownMemChangeValue = new System.Windows.Forms.NumericUpDown();
            this.buttonMemChange = new System.Windows.Forms.Button();
            this.buttonMemLoad = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxScreen)).BeginInit();
            this.groupBoxCPU.SuspendLayout();
            this.tableLayoutPanelCPU.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMemStartValue)).BeginInit();
            this.groupBoxCharset.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPosValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxChar)).BeginInit();
            this.groupBoxMemChange.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMemChangeAddress)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMemChangeValue)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBoxScreen
            // 
            this.pictureBoxScreen.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxScreen.Name = "pictureBoxScreen";
            this.pictureBoxScreen.Size = new System.Drawing.Size(640, 400);
            this.pictureBoxScreen.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxScreen.TabIndex = 0;
            this.pictureBoxScreen.TabStop = false;
            // 
            // timerClock
            // 
            this.timerClock.Interval = 1;
            this.timerClock.Tick += new System.EventHandler(this.timerClock_Tick);
            // 
            // groupBoxCPU
            // 
            this.groupBoxCPU.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxCPU.Controls.Add(this.labelCurInst);
            this.groupBoxCPU.Controls.Add(this.tableLayoutPanelCPU);
            this.groupBoxCPU.Location = new System.Drawing.Point(646, 12);
            this.groupBoxCPU.Name = "groupBoxCPU";
            this.groupBoxCPU.Size = new System.Drawing.Size(232, 147);
            this.groupBoxCPU.TabIndex = 1;
            this.groupBoxCPU.TabStop = false;
            this.groupBoxCPU.Text = "CPU";
            // 
            // labelCurInst
            // 
            this.labelCurInst.AutoSize = true;
            this.labelCurInst.Location = new System.Drawing.Point(7, 134);
            this.labelCurInst.Name = "labelCurInst";
            this.labelCurInst.Size = new System.Drawing.Size(152, 11);
            this.labelCurInst.TabIndex = 5;
            this.labelCurInst.Text = "Current Instruction: ";
            // 
            // tableLayoutPanelCPU
            // 
            this.tableLayoutPanelCPU.ColumnCount = 2;
            this.tableLayoutPanelCPU.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelCPU.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelCPU.Controls.Add(this.labelADesc, 0, 0);
            this.tableLayoutPanelCPU.Controls.Add(this.labelAValue, 1, 0);
            this.tableLayoutPanelCPU.Controls.Add(this.labelXDesc, 0, 1);
            this.tableLayoutPanelCPU.Controls.Add(this.labelXValue, 1, 1);
            this.tableLayoutPanelCPU.Controls.Add(this.labelYDesc, 0, 2);
            this.tableLayoutPanelCPU.Controls.Add(this.labelYValue, 1, 2);
            this.tableLayoutPanelCPU.Controls.Add(this.labelPCDesc, 0, 3);
            this.tableLayoutPanelCPU.Controls.Add(this.labelPCValue, 1, 3);
            this.tableLayoutPanelCPU.Controls.Add(this.labelSPDesc, 0, 4);
            this.tableLayoutPanelCPU.Controls.Add(this.labelSPValue, 1, 4);
            this.tableLayoutPanelCPU.Controls.Add(this.labelSRDesc, 0, 5);
            this.tableLayoutPanelCPU.Controls.Add(this.labelSRValue, 1, 5);
            this.tableLayoutPanelCPU.Location = new System.Drawing.Point(7, 16);
            this.tableLayoutPanelCPU.Name = "tableLayoutPanelCPU";
            this.tableLayoutPanelCPU.RowCount = 6;
            this.tableLayoutPanelCPU.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanelCPU.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanelCPU.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanelCPU.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanelCPU.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanelCPU.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanelCPU.Size = new System.Drawing.Size(218, 115);
            this.tableLayoutPanelCPU.TabIndex = 0;
            // 
            // labelADesc
            // 
            this.labelADesc.AutoSize = true;
            this.labelADesc.Location = new System.Drawing.Point(3, 0);
            this.labelADesc.Name = "labelADesc";
            this.labelADesc.Size = new System.Drawing.Size(19, 11);
            this.labelADesc.TabIndex = 0;
            this.labelADesc.Text = "A:";
            // 
            // labelAValue
            // 
            this.labelAValue.AutoSize = true;
            this.labelAValue.Location = new System.Drawing.Point(112, 0);
            this.labelAValue.Name = "labelAValue";
            this.labelAValue.Size = new System.Drawing.Size(12, 11);
            this.labelAValue.TabIndex = 1;
            this.labelAValue.Text = "A";
            // 
            // labelXDesc
            // 
            this.labelXDesc.AutoSize = true;
            this.labelXDesc.Location = new System.Drawing.Point(3, 19);
            this.labelXDesc.Name = "labelXDesc";
            this.labelXDesc.Size = new System.Drawing.Size(19, 11);
            this.labelXDesc.TabIndex = 2;
            this.labelXDesc.Text = "X:";
            // 
            // labelXValue
            // 
            this.labelXValue.AutoSize = true;
            this.labelXValue.Location = new System.Drawing.Point(112, 19);
            this.labelXValue.Name = "labelXValue";
            this.labelXValue.Size = new System.Drawing.Size(12, 11);
            this.labelXValue.TabIndex = 3;
            this.labelXValue.Text = "X";
            // 
            // labelYDesc
            // 
            this.labelYDesc.AutoSize = true;
            this.labelYDesc.Location = new System.Drawing.Point(3, 38);
            this.labelYDesc.Name = "labelYDesc";
            this.labelYDesc.Size = new System.Drawing.Size(19, 11);
            this.labelYDesc.TabIndex = 4;
            this.labelYDesc.Text = "Y:";
            // 
            // labelYValue
            // 
            this.labelYValue.AutoSize = true;
            this.labelYValue.Location = new System.Drawing.Point(112, 38);
            this.labelYValue.Name = "labelYValue";
            this.labelYValue.Size = new System.Drawing.Size(12, 11);
            this.labelYValue.TabIndex = 5;
            this.labelYValue.Text = "Y";
            // 
            // labelPCDesc
            // 
            this.labelPCDesc.AutoSize = true;
            this.labelPCDesc.Location = new System.Drawing.Point(3, 57);
            this.labelPCDesc.Name = "labelPCDesc";
            this.labelPCDesc.Size = new System.Drawing.Size(26, 11);
            this.labelPCDesc.TabIndex = 6;
            this.labelPCDesc.Text = "PC:";
            // 
            // labelPCValue
            // 
            this.labelPCValue.AutoSize = true;
            this.labelPCValue.Location = new System.Drawing.Point(112, 57);
            this.labelPCValue.Name = "labelPCValue";
            this.labelPCValue.Size = new System.Drawing.Size(19, 11);
            this.labelPCValue.TabIndex = 7;
            this.labelPCValue.Text = "PC";
            // 
            // labelSPDesc
            // 
            this.labelSPDesc.AutoSize = true;
            this.labelSPDesc.Location = new System.Drawing.Point(3, 76);
            this.labelSPDesc.Name = "labelSPDesc";
            this.labelSPDesc.Size = new System.Drawing.Size(26, 11);
            this.labelSPDesc.TabIndex = 8;
            this.labelSPDesc.Text = "SP:";
            // 
            // labelSPValue
            // 
            this.labelSPValue.AutoSize = true;
            this.labelSPValue.Location = new System.Drawing.Point(112, 76);
            this.labelSPValue.Name = "labelSPValue";
            this.labelSPValue.Size = new System.Drawing.Size(19, 11);
            this.labelSPValue.TabIndex = 9;
            this.labelSPValue.Text = "SP";
            // 
            // labelSRDesc
            // 
            this.labelSRDesc.AutoSize = true;
            this.labelSRDesc.Location = new System.Drawing.Point(3, 95);
            this.labelSRDesc.Name = "labelSRDesc";
            this.labelSRDesc.Size = new System.Drawing.Size(26, 11);
            this.labelSRDesc.TabIndex = 10;
            this.labelSRDesc.Text = "SR:";
            // 
            // labelSRValue
            // 
            this.labelSRValue.AutoSize = true;
            this.labelSRValue.Location = new System.Drawing.Point(112, 95);
            this.labelSRValue.Name = "labelSRValue";
            this.labelSRValue.Size = new System.Drawing.Size(19, 11);
            this.labelSRValue.TabIndex = 11;
            this.labelSRValue.Text = "SR";
            // 
            // buttonCycle
            // 
            this.buttonCycle.Location = new System.Drawing.Point(646, 165);
            this.buttonCycle.Name = "buttonCycle";
            this.buttonCycle.Size = new System.Drawing.Size(72, 23);
            this.buttonCycle.TabIndex = 2;
            this.buttonCycle.Text = "Cycle";
            this.buttonCycle.UseVisualStyleBackColor = true;
            this.buttonCycle.Click += new System.EventHandler(this.buttonCycle_Click);
            // 
            // buttonStep
            // 
            this.buttonStep.Location = new System.Drawing.Point(724, 165);
            this.buttonStep.Name = "buttonStep";
            this.buttonStep.Size = new System.Drawing.Size(73, 23);
            this.buttonStep.TabIndex = 3;
            this.buttonStep.Text = "Step";
            this.buttonStep.UseVisualStyleBackColor = true;
            this.buttonStep.Click += new System.EventHandler(this.buttonStep_Click);
            // 
            // buttonReset
            // 
            this.buttonReset.Location = new System.Drawing.Point(803, 165);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(75, 23);
            this.buttonReset.TabIndex = 4;
            this.buttonReset.Text = "Reset";
            this.buttonReset.UseVisualStyleBackColor = true;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(646, 194);
            this.buttonStart.Margin = new System.Windows.Forms.Padding(3, 3, 5, 3);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(112, 23);
            this.buttonStart.TabIndex = 6;
            this.buttonStart.Text = "Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // buttonStop
            // 
            this.buttonStop.Location = new System.Drawing.Point(766, 194);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(112, 23);
            this.buttonStop.TabIndex = 7;
            this.buttonStop.Text = "Stop";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // textBoxMemory
            // 
            this.textBoxMemory.Location = new System.Drawing.Point(12, 435);
            this.textBoxMemory.MaxLength = 0;
            this.textBoxMemory.Multiline = true;
            this.textBoxMemory.Name = "textBoxMemory";
            this.textBoxMemory.ReadOnly = true;
            this.textBoxMemory.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxMemory.Size = new System.Drawing.Size(628, 116);
            this.textBoxMemory.TabIndex = 8;
            // 
            // labelMemStartDesc
            // 
            this.labelMemStartDesc.AutoSize = true;
            this.labelMemStartDesc.Location = new System.Drawing.Point(644, 438);
            this.labelMemStartDesc.Name = "labelMemStartDesc";
            this.labelMemStartDesc.Size = new System.Drawing.Size(96, 11);
            this.labelMemStartDesc.TabIndex = 9;
            this.labelMemStartDesc.Text = "Memory Start:";
            // 
            // numericUpDownMemStartValue
            // 
            this.numericUpDownMemStartValue.Hexadecimal = true;
            this.numericUpDownMemStartValue.Increment = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.numericUpDownMemStartValue.InterceptArrowKeys = false;
            this.numericUpDownMemStartValue.Location = new System.Drawing.Point(746, 436);
            this.numericUpDownMemStartValue.Maximum = new decimal(new int[] {
            64512,
            0,
            0,
            0});
            this.numericUpDownMemStartValue.Name = "numericUpDownMemStartValue";
            this.numericUpDownMemStartValue.Size = new System.Drawing.Size(62, 18);
            this.numericUpDownMemStartValue.TabIndex = 10;
            // 
            // buttonMemStartUpdate
            // 
            this.buttonMemStartUpdate.Location = new System.Drawing.Point(814, 432);
            this.buttonMemStartUpdate.Name = "buttonMemStartUpdate";
            this.buttonMemStartUpdate.Size = new System.Drawing.Size(64, 23);
            this.buttonMemStartUpdate.TabIndex = 11;
            this.buttonMemStartUpdate.Text = "Update";
            this.buttonMemStartUpdate.UseVisualStyleBackColor = true;
            this.buttonMemStartUpdate.Click += new System.EventHandler(this.buttonMemStartUpdate_Click);
            // 
            // groupBoxCharset
            // 
            this.groupBoxCharset.Controls.Add(this.numericUpDownPosValue);
            this.groupBoxCharset.Controls.Add(this.labelPosDesc);
            this.groupBoxCharset.Controls.Add(this.labelCharDesc);
            this.groupBoxCharset.Controls.Add(this.pictureBoxChar);
            this.groupBoxCharset.Location = new System.Drawing.Point(646, 223);
            this.groupBoxCharset.Name = "groupBoxCharset";
            this.groupBoxCharset.Size = new System.Drawing.Size(232, 100);
            this.groupBoxCharset.TabIndex = 12;
            this.groupBoxCharset.TabStop = false;
            this.groupBoxCharset.Text = "Charset";
            // 
            // numericUpDownPosValue
            // 
            this.numericUpDownPosValue.Hexadecimal = true;
            this.numericUpDownPosValue.Location = new System.Drawing.Point(139, 26);
            this.numericUpDownPosValue.Maximum = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.numericUpDownPosValue.Name = "numericUpDownPosValue";
            this.numericUpDownPosValue.Size = new System.Drawing.Size(44, 18);
            this.numericUpDownPosValue.TabIndex = 3;
            this.numericUpDownPosValue.ValueChanged += new System.EventHandler(this.numericUpDownPosValue_ValueChanged);
            // 
            // labelPosDesc
            // 
            this.labelPosDesc.AutoSize = true;
            this.labelPosDesc.Location = new System.Drawing.Point(65, 28);
            this.labelPosDesc.Name = "labelPosDesc";
            this.labelPosDesc.Size = new System.Drawing.Size(68, 11);
            this.labelPosDesc.TabIndex = 2;
            this.labelPosDesc.Text = "Position:";
            // 
            // labelCharDesc
            // 
            this.labelCharDesc.AutoSize = true;
            this.labelCharDesc.Location = new System.Drawing.Point(6, 14);
            this.labelCharDesc.Name = "labelCharDesc";
            this.labelCharDesc.Size = new System.Drawing.Size(40, 11);
            this.labelCharDesc.TabIndex = 1;
            this.labelCharDesc.Text = "Char:";
            // 
            // pictureBoxChar
            // 
            this.pictureBoxChar.BackColor = System.Drawing.Color.Black;
            this.pictureBoxChar.Location = new System.Drawing.Point(6, 28);
            this.pictureBoxChar.Name = "pictureBoxChar";
            this.pictureBoxChar.Size = new System.Drawing.Size(48, 48);
            this.pictureBoxChar.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxChar.TabIndex = 0;
            this.pictureBoxChar.TabStop = false;
            this.pictureBoxChar.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBoxChar_Paint);
            // 
            // textBoxTerminal
            // 
            this.textBoxTerminal.Location = new System.Drawing.Point(12, 409);
            this.textBoxTerminal.Name = "textBoxTerminal";
            this.textBoxTerminal.Size = new System.Drawing.Size(547, 18);
            this.textBoxTerminal.TabIndex = 13;
            // 
            // buttonSend
            // 
            this.buttonSend.Location = new System.Drawing.Point(565, 406);
            this.buttonSend.Name = "buttonSend";
            this.buttonSend.Size = new System.Drawing.Size(75, 23);
            this.buttonSend.TabIndex = 14;
            this.buttonSend.Text = "Send";
            this.buttonSend.UseVisualStyleBackColor = true;
            this.buttonSend.Click += new System.EventHandler(this.buttonSend_Click);
            // 
            // groupBoxMemChange
            // 
            this.groupBoxMemChange.Controls.Add(this.buttonMemLoad);
            this.groupBoxMemChange.Controls.Add(this.buttonMemChange);
            this.groupBoxMemChange.Controls.Add(this.numericUpDownMemChangeValue);
            this.groupBoxMemChange.Controls.Add(this.labelColon);
            this.groupBoxMemChange.Controls.Add(this.numericUpDownMemChangeAddress);
            this.groupBoxMemChange.Location = new System.Drawing.Point(646, 461);
            this.groupBoxMemChange.Name = "groupBoxMemChange";
            this.groupBoxMemChange.Size = new System.Drawing.Size(232, 90);
            this.groupBoxMemChange.TabIndex = 15;
            this.groupBoxMemChange.TabStop = false;
            this.groupBoxMemChange.Text = "Change Memory";
            // 
            // numericUpDownMemChangeAddress
            // 
            this.numericUpDownMemChangeAddress.Hexadecimal = true;
            this.numericUpDownMemChangeAddress.Location = new System.Drawing.Point(6, 21);
            this.numericUpDownMemChangeAddress.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numericUpDownMemChangeAddress.Name = "numericUpDownMemChangeAddress";
            this.numericUpDownMemChangeAddress.Size = new System.Drawing.Size(53, 18);
            this.numericUpDownMemChangeAddress.TabIndex = 0;
            this.numericUpDownMemChangeAddress.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericUpDownMemChangeAddress.UpDownAlign = System.Windows.Forms.LeftRightAlignment.Left;
            this.numericUpDownMemChangeAddress.Value = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            // 
            // labelColon
            // 
            this.labelColon.AutoSize = true;
            this.labelColon.Location = new System.Drawing.Point(65, 23);
            this.labelColon.Name = "labelColon";
            this.labelColon.Size = new System.Drawing.Size(12, 11);
            this.labelColon.TabIndex = 1;
            this.labelColon.Text = ":";
            // 
            // numericUpDownMemChangeValue
            // 
            this.numericUpDownMemChangeValue.Hexadecimal = true;
            this.numericUpDownMemChangeValue.Location = new System.Drawing.Point(83, 21);
            this.numericUpDownMemChangeValue.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDownMemChangeValue.Name = "numericUpDownMemChangeValue";
            this.numericUpDownMemChangeValue.Size = new System.Drawing.Size(37, 18);
            this.numericUpDownMemChangeValue.TabIndex = 2;
            this.numericUpDownMemChangeValue.Value = new decimal(new int[] {
            255,
            0,
            0,
            0});
            // 
            // buttonMemChange
            // 
            this.buttonMemChange.Location = new System.Drawing.Point(126, 17);
            this.buttonMemChange.Name = "buttonMemChange";
            this.buttonMemChange.Size = new System.Drawing.Size(100, 23);
            this.buttonMemChange.TabIndex = 3;
            this.buttonMemChange.Text = "Change";
            this.buttonMemChange.UseVisualStyleBackColor = true;
            this.buttonMemChange.Click += new System.EventHandler(this.buttonMemChange_Click);
            // 
            // buttonMemLoad
            // 
            this.buttonMemLoad.Location = new System.Drawing.Point(126, 46);
            this.buttonMemLoad.Name = "buttonMemLoad";
            this.buttonMemLoad.Size = new System.Drawing.Size(100, 23);
            this.buttonMemLoad.TabIndex = 4;
            this.buttonMemLoad.Text = "Load";
            this.buttonMemLoad.UseVisualStyleBackColor = true;
            this.buttonMemLoad.Click += new System.EventHandler(this.buttonMemLoad_Click);
            // 
            // Form_Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 11F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(890, 563);
            this.Controls.Add(this.groupBoxMemChange);
            this.Controls.Add(this.buttonSend);
            this.Controls.Add(this.textBoxTerminal);
            this.Controls.Add(this.groupBoxCharset);
            this.Controls.Add(this.buttonMemStartUpdate);
            this.Controls.Add(this.numericUpDownMemStartValue);
            this.Controls.Add(this.labelMemStartDesc);
            this.Controls.Add(this.textBoxMemory);
            this.Controls.Add(this.buttonStop);
            this.Controls.Add(this.buttonStart);
            this.Controls.Add(this.buttonReset);
            this.Controls.Add(this.buttonStep);
            this.Controls.Add(this.buttonCycle);
            this.Controls.Add(this.groupBoxCPU);
            this.Controls.Add(this.pictureBoxScreen);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form_Main";
            this.Text = "View6502 Screen";
            this.Load += new System.EventHandler(this.Form_Main_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxScreen)).EndInit();
            this.groupBoxCPU.ResumeLayout(false);
            this.groupBoxCPU.PerformLayout();
            this.tableLayoutPanelCPU.ResumeLayout(false);
            this.tableLayoutPanelCPU.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMemStartValue)).EndInit();
            this.groupBoxCharset.ResumeLayout(false);
            this.groupBoxCharset.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPosValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxChar)).EndInit();
            this.groupBoxMemChange.ResumeLayout(false);
            this.groupBoxMemChange.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMemChangeAddress)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMemChangeValue)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxScreen;
        private System.Windows.Forms.Timer timerClock;
        private System.Windows.Forms.GroupBox groupBoxCPU;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelCPU;
        private System.Windows.Forms.Label labelADesc;
        private System.Windows.Forms.Label labelAValue;
        private System.Windows.Forms.Label labelXDesc;
        private System.Windows.Forms.Label labelXValue;
        private System.Windows.Forms.Label labelYDesc;
        private System.Windows.Forms.Label labelYValue;
        private System.Windows.Forms.Label labelPCDesc;
        private System.Windows.Forms.Label labelPCValue;
        private System.Windows.Forms.Label labelSPDesc;
        private System.Windows.Forms.Label labelSPValue;
        private System.Windows.Forms.Label labelSRDesc;
        private System.Windows.Forms.Label labelSRValue;
        private System.Windows.Forms.Button buttonCycle;
        private System.Windows.Forms.Button buttonStep;
        private System.Windows.Forms.Button buttonReset;
        private System.Windows.Forms.Label labelCurInst;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.TextBox textBoxMemory;
        private System.Windows.Forms.Label labelMemStartDesc;
        private System.Windows.Forms.NumericUpDown numericUpDownMemStartValue;
        private System.Windows.Forms.Button buttonMemStartUpdate;
        private System.Windows.Forms.GroupBox groupBoxCharset;
        private System.Windows.Forms.NumericUpDown numericUpDownPosValue;
        private System.Windows.Forms.Label labelPosDesc;
        private System.Windows.Forms.Label labelCharDesc;
        private System.Windows.Forms.PictureBox pictureBoxChar;
        private System.Windows.Forms.TextBox textBoxTerminal;
        private System.Windows.Forms.Button buttonSend;
        private System.Windows.Forms.GroupBox groupBoxMemChange;
        private System.Windows.Forms.Button buttonMemLoad;
        private System.Windows.Forms.Button buttonMemChange;
        private System.Windows.Forms.NumericUpDown numericUpDownMemChangeValue;
        private System.Windows.Forms.Label labelColon;
        private System.Windows.Forms.NumericUpDown numericUpDownMemChangeAddress;
    }
}

