namespace GraphAlgorithmDemo
{
    partial class DemoForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.layoutAlgCMB = new System.Windows.Forms.ComboBox();
            this.layoutAlgLBL = new System.Windows.Forms.Label();
            this.layoutGRP = new System.Windows.Forms.GroupBox();
            this.layoutPausedCHK = new System.Windows.Forms.CheckBox();
            this.layoutOnNodeMovedCHK = new System.Windows.Forms.CheckBox();
            this.layoutShuffleBTN = new System.Windows.Forms.Button();
            this.layoutStartStopBTN = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.graphPanel = new GraphForms.GraphPanel();
            this.layoutParamsLBL = new System.Windows.Forms.Label();
            this.layoutParamsGrid = new System.Windows.Forms.PropertyGrid();
            this.styleAlgGRP = new System.Windows.Forms.GroupBox();
            this.styleAlgReversedCHK = new System.Windows.Forms.CheckBox();
            this.styleAlgDirectedCHK = new System.Windows.Forms.CheckBox();
            this.styleAlgTestBTN = new System.Windows.Forms.Button();
            this.styleAlgCMB = new System.Windows.Forms.ComboBox();
            this.graphCreatorGRP = new System.Windows.Forms.GroupBox();
            this.createGraphBTN = new System.Windows.Forms.Button();
            this.graphCreatorCMB = new System.Windows.Forms.ComboBox();
            this.graphStyleGRP = new System.Windows.Forms.GroupBox();
            this.graphStyleResetOnCreateCHK = new System.Windows.Forms.CheckBox();
            this.edgeAngNUM = new System.Windows.Forms.NumericUpDown();
            this.edgeAngLBL = new System.Windows.Forms.Label();
            this.nodeRadNUM = new System.Windows.Forms.NumericUpDown();
            this.nodeRadLBL = new System.Windows.Forms.Label();
            this.shortPathGRP = new System.Windows.Forms.GroupBox();
            this.shortPathAlgCMB = new System.Windows.Forms.ComboBox();
            this.shortPathOnOffBTN = new System.Windows.Forms.Button();
            this.shortPathDirectedCHK = new System.Windows.Forms.CheckBox();
            this.shortPathReversedCHK = new System.Windows.Forms.CheckBox();
            this.layoutGRP.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.styleAlgGRP.SuspendLayout();
            this.graphCreatorGRP.SuspendLayout();
            this.graphStyleGRP.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.edgeAngNUM)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nodeRadNUM)).BeginInit();
            this.shortPathGRP.SuspendLayout();
            this.SuspendLayout();
            // 
            // layoutAlgCMB
            // 
            this.layoutAlgCMB.FormattingEnabled = true;
            this.layoutAlgCMB.Location = new System.Drawing.Point(100, 13);
            this.layoutAlgCMB.Name = "layoutAlgCMB";
            this.layoutAlgCMB.Size = new System.Drawing.Size(175, 21);
            this.layoutAlgCMB.TabIndex = 1;
            this.layoutAlgCMB.SelectedIndexChanged += new System.EventHandler(this.layoutAlgSelectedIndexChanged);
            // 
            // layoutAlgLBL
            // 
            this.layoutAlgLBL.AutoSize = true;
            this.layoutAlgLBL.Location = new System.Drawing.Point(6, 16);
            this.layoutAlgLBL.Name = "layoutAlgLBL";
            this.layoutAlgLBL.Size = new System.Drawing.Size(88, 13);
            this.layoutAlgLBL.TabIndex = 2;
            this.layoutAlgLBL.Text = "Layout Algorithm:";
            // 
            // layoutGRP
            // 
            this.layoutGRP.Controls.Add(this.layoutPausedCHK);
            this.layoutGRP.Controls.Add(this.layoutOnNodeMovedCHK);
            this.layoutGRP.Controls.Add(this.layoutShuffleBTN);
            this.layoutGRP.Controls.Add(this.layoutStartStopBTN);
            this.layoutGRP.Controls.Add(this.layoutAlgLBL);
            this.layoutGRP.Controls.Add(this.layoutAlgCMB);
            this.layoutGRP.Location = new System.Drawing.Point(12, 12);
            this.layoutGRP.Name = "layoutGRP";
            this.layoutGRP.Size = new System.Drawing.Size(583, 43);
            this.layoutGRP.TabIndex = 3;
            this.layoutGRP.TabStop = false;
            this.layoutGRP.Text = "Layout Settings";
            // 
            // layoutPausedCHK
            // 
            this.layoutPausedCHK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.layoutPausedCHK.AutoSize = true;
            this.layoutPausedCHK.Location = new System.Drawing.Point(420, 15);
            this.layoutPausedCHK.Name = "layoutPausedCHK";
            this.layoutPausedCHK.Size = new System.Drawing.Size(56, 17);
            this.layoutPausedCHK.TabIndex = 7;
            this.layoutPausedCHK.Text = "Pause";
            this.layoutPausedCHK.UseVisualStyleBackColor = true;
            this.layoutPausedCHK.CheckedChanged += new System.EventHandler(this.layoutPausedCheckedChanged);
            // 
            // layoutOnNodeMovedCHK
            // 
            this.layoutOnNodeMovedCHK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.layoutOnNodeMovedCHK.AutoSize = true;
            this.layoutOnNodeMovedCHK.Location = new System.Drawing.Point(286, 15);
            this.layoutOnNodeMovedCHK.Name = "layoutOnNodeMovedCHK";
            this.layoutOnNodeMovedCHK.Size = new System.Drawing.Size(128, 17);
            this.layoutOnNodeMovedCHK.TabIndex = 6;
            this.layoutOnNodeMovedCHK.Text = "Start on Node Moved";
            this.layoutOnNodeMovedCHK.UseVisualStyleBackColor = true;
            this.layoutOnNodeMovedCHK.CheckedChanged += new System.EventHandler(this.layoutOnNodeMovedCheckedChanged);
            // 
            // layoutShuffleBTN
            // 
            this.layoutShuffleBTN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.layoutShuffleBTN.Location = new System.Drawing.Point(525, 11);
            this.layoutShuffleBTN.Name = "layoutShuffleBTN";
            this.layoutShuffleBTN.Size = new System.Drawing.Size(50, 23);
            this.layoutShuffleBTN.TabIndex = 4;
            this.layoutShuffleBTN.Text = "Shuffle";
            this.layoutShuffleBTN.UseVisualStyleBackColor = true;
            this.layoutShuffleBTN.Click += new System.EventHandler(this.layoutShuffleClick);
            // 
            // layoutStartStopBTN
            // 
            this.layoutStartStopBTN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.layoutStartStopBTN.Location = new System.Drawing.Point(482, 11);
            this.layoutStartStopBTN.Name = "layoutStartStopBTN";
            this.layoutStartStopBTN.Size = new System.Drawing.Size(37, 23);
            this.layoutStartStopBTN.TabIndex = 3;
            this.layoutStartStopBTN.Text = "Start";
            this.layoutStartStopBTN.UseVisualStyleBackColor = true;
            this.layoutStartStopBTN.Click += new System.EventHandler(this.layoutStartStopClick);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(12, 179);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.graphPanel);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.layoutParamsLBL);
            this.splitContainer1.Panel2.Controls.Add(this.layoutParamsGrid);
            this.splitContainer1.Size = new System.Drawing.Size(654, 409);
            this.splitContainer1.SplitterDistance = 422;
            this.splitContainer1.TabIndex = 4;
            // 
            // graphPanel
            // 
            this.graphPanel.Location = new System.Drawing.Point(4, 4);
            this.graphPanel.MinimumSize = new System.Drawing.Size(400, 400);
            this.graphPanel.Name = "graphPanel";
            this.graphPanel.Scene = null;
            this.graphPanel.Size = new System.Drawing.Size(414, 400);
            this.graphPanel.TabIndex = 0;
            // 
            // layoutParamsLBL
            // 
            this.layoutParamsLBL.AutoSize = true;
            this.layoutParamsLBL.Location = new System.Drawing.Point(3, 4);
            this.layoutParamsLBL.Name = "layoutParamsLBL";
            this.layoutParamsLBL.Size = new System.Drawing.Size(98, 13);
            this.layoutParamsLBL.TabIndex = 1;
            this.layoutParamsLBL.Text = "Layout Parameters:";
            // 
            // layoutParamsGrid
            // 
            this.layoutParamsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.layoutParamsGrid.Location = new System.Drawing.Point(3, 20);
            this.layoutParamsGrid.Name = "layoutParamsGrid";
            this.layoutParamsGrid.Size = new System.Drawing.Size(222, 384);
            this.layoutParamsGrid.TabIndex = 0;
            // 
            // styleAlgGRP
            // 
            this.styleAlgGRP.Controls.Add(this.styleAlgReversedCHK);
            this.styleAlgGRP.Controls.Add(this.styleAlgDirectedCHK);
            this.styleAlgGRP.Controls.Add(this.styleAlgTestBTN);
            this.styleAlgGRP.Controls.Add(this.styleAlgCMB);
            this.styleAlgGRP.Location = new System.Drawing.Point(13, 62);
            this.styleAlgGRP.Name = "styleAlgGRP";
            this.styleAlgGRP.Size = new System.Drawing.Size(390, 49);
            this.styleAlgGRP.TabIndex = 5;
            this.styleAlgGRP.TabStop = false;
            this.styleAlgGRP.Text = "Style Algorithm";
            // 
            // styleAlgReversedCHK
            // 
            this.styleAlgReversedCHK.AutoSize = true;
            this.styleAlgReversedCHK.Enabled = false;
            this.styleAlgReversedCHK.Location = new System.Drawing.Point(266, 21);
            this.styleAlgReversedCHK.Name = "styleAlgReversedCHK";
            this.styleAlgReversedCHK.Size = new System.Drawing.Size(72, 17);
            this.styleAlgReversedCHK.TabIndex = 3;
            this.styleAlgReversedCHK.Text = "Reversed";
            this.styleAlgReversedCHK.UseVisualStyleBackColor = true;
            this.styleAlgReversedCHK.CheckedChanged += new System.EventHandler(this.styleAlgReversedCheckedChanged);
            // 
            // styleAlgDirectedCHK
            // 
            this.styleAlgDirectedCHK.AutoSize = true;
            this.styleAlgDirectedCHK.Enabled = false;
            this.styleAlgDirectedCHK.Location = new System.Drawing.Point(194, 21);
            this.styleAlgDirectedCHK.Name = "styleAlgDirectedCHK";
            this.styleAlgDirectedCHK.Size = new System.Drawing.Size(66, 17);
            this.styleAlgDirectedCHK.TabIndex = 2;
            this.styleAlgDirectedCHK.Text = "Directed";
            this.styleAlgDirectedCHK.UseVisualStyleBackColor = true;
            this.styleAlgDirectedCHK.CheckedChanged += new System.EventHandler(this.styleAlgUndirectedCheckedChanged);
            // 
            // styleAlgTestBTN
            // 
            this.styleAlgTestBTN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.styleAlgTestBTN.Location = new System.Drawing.Point(347, 17);
            this.styleAlgTestBTN.Name = "styleAlgTestBTN";
            this.styleAlgTestBTN.Size = new System.Drawing.Size(37, 23);
            this.styleAlgTestBTN.TabIndex = 1;
            this.styleAlgTestBTN.Text = "Test";
            this.styleAlgTestBTN.UseVisualStyleBackColor = true;
            this.styleAlgTestBTN.Click += new System.EventHandler(this.styleAlgTestClick);
            // 
            // styleAlgCMB
            // 
            this.styleAlgCMB.FormattingEnabled = true;
            this.styleAlgCMB.Location = new System.Drawing.Point(6, 19);
            this.styleAlgCMB.Name = "styleAlgCMB";
            this.styleAlgCMB.Size = new System.Drawing.Size(182, 21);
            this.styleAlgCMB.TabIndex = 0;
            this.styleAlgCMB.SelectedValueChanged += new System.EventHandler(this.styleAlgSelectedValueChanged);
            // 
            // graphCreatorGRP
            // 
            this.graphCreatorGRP.Controls.Add(this.createGraphBTN);
            this.graphCreatorGRP.Controls.Add(this.graphCreatorCMB);
            this.graphCreatorGRP.Location = new System.Drawing.Point(409, 62);
            this.graphCreatorGRP.Name = "graphCreatorGRP";
            this.graphCreatorGRP.Size = new System.Drawing.Size(257, 49);
            this.graphCreatorGRP.TabIndex = 6;
            this.graphCreatorGRP.TabStop = false;
            this.graphCreatorGRP.Text = "Graph Creator";
            // 
            // createGraphBTN
            // 
            this.createGraphBTN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.createGraphBTN.Location = new System.Drawing.Point(204, 17);
            this.createGraphBTN.Name = "createGraphBTN";
            this.createGraphBTN.Size = new System.Drawing.Size(47, 23);
            this.createGraphBTN.TabIndex = 1;
            this.createGraphBTN.Text = "Create";
            this.createGraphBTN.UseVisualStyleBackColor = true;
            this.createGraphBTN.Click += new System.EventHandler(this.createGraphClick);
            // 
            // graphCreatorCMB
            // 
            this.graphCreatorCMB.FormattingEnabled = true;
            this.graphCreatorCMB.Location = new System.Drawing.Point(6, 19);
            this.graphCreatorCMB.Name = "graphCreatorCMB";
            this.graphCreatorCMB.Size = new System.Drawing.Size(192, 21);
            this.graphCreatorCMB.TabIndex = 0;
            // 
            // graphStyleGRP
            // 
            this.graphStyleGRP.Controls.Add(this.graphStyleResetOnCreateCHK);
            this.graphStyleGRP.Controls.Add(this.edgeAngNUM);
            this.graphStyleGRP.Controls.Add(this.edgeAngLBL);
            this.graphStyleGRP.Controls.Add(this.nodeRadNUM);
            this.graphStyleGRP.Controls.Add(this.nodeRadLBL);
            this.graphStyleGRP.Location = new System.Drawing.Point(12, 118);
            this.graphStyleGRP.Name = "graphStyleGRP";
            this.graphStyleGRP.Size = new System.Drawing.Size(360, 49);
            this.graphStyleGRP.TabIndex = 7;
            this.graphStyleGRP.TabStop = false;
            this.graphStyleGRP.Text = "Graph Style";
            // 
            // graphStyleResetOnCreateCHK
            // 
            this.graphStyleResetOnCreateCHK.AutoSize = true;
            this.graphStyleResetOnCreateCHK.Checked = true;
            this.graphStyleResetOnCreateCHK.CheckState = System.Windows.Forms.CheckState.Checked;
            this.graphStyleResetOnCreateCHK.Location = new System.Drawing.Point(249, 21);
            this.graphStyleResetOnCreateCHK.Name = "graphStyleResetOnCreateCHK";
            this.graphStyleResetOnCreateCHK.Size = new System.Drawing.Size(105, 17);
            this.graphStyleResetOnCreateCHK.TabIndex = 4;
            this.graphStyleResetOnCreateCHK.Text = "Reset On Create";
            this.graphStyleResetOnCreateCHK.UseVisualStyleBackColor = true;
            // 
            // edgeAngNUM
            // 
            this.edgeAngNUM.Location = new System.Drawing.Point(208, 20);
            this.edgeAngNUM.Maximum = new decimal(new int[] {
            90,
            0,
            0,
            0});
            this.edgeAngNUM.Name = "edgeAngNUM";
            this.edgeAngNUM.Size = new System.Drawing.Size(35, 20);
            this.edgeAngNUM.TabIndex = 3;
            this.edgeAngNUM.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.edgeAngNUM.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.edgeAngNUM.ValueChanged += new System.EventHandler(this.edgeAngValueChanged);
            // 
            // edgeAngLBL
            // 
            this.edgeAngLBL.AutoSize = true;
            this.edgeAngLBL.Location = new System.Drawing.Point(137, 22);
            this.edgeAngLBL.Name = "edgeAngLBL";
            this.edgeAngLBL.Size = new System.Drawing.Size(65, 13);
            this.edgeAngLBL.TabIndex = 2;
            this.edgeAngLBL.Text = "Edge Angle:";
            // 
            // nodeRadNUM
            // 
            this.nodeRadNUM.DecimalPlaces = 1;
            this.nodeRadNUM.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.nodeRadNUM.Location = new System.Drawing.Point(84, 20);
            this.nodeRadNUM.Maximum = new decimal(new int[] {
            25,
            0,
            0,
            0});
            this.nodeRadNUM.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.nodeRadNUM.Name = "nodeRadNUM";
            this.nodeRadNUM.Size = new System.Drawing.Size(47, 20);
            this.nodeRadNUM.TabIndex = 1;
            this.nodeRadNUM.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.nodeRadNUM.Value = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.nodeRadNUM.ValueChanged += new System.EventHandler(this.nodeRadValueChanged);
            // 
            // nodeRadLBL
            // 
            this.nodeRadLBL.AutoSize = true;
            this.nodeRadLBL.Location = new System.Drawing.Point(6, 22);
            this.nodeRadLBL.Name = "nodeRadLBL";
            this.nodeRadLBL.Size = new System.Drawing.Size(72, 13);
            this.nodeRadLBL.TabIndex = 0;
            this.nodeRadLBL.Text = "Node Radius:";
            // 
            // shortPathGRP
            // 
            this.shortPathGRP.Controls.Add(this.shortPathReversedCHK);
            this.shortPathGRP.Controls.Add(this.shortPathDirectedCHK);
            this.shortPathGRP.Controls.Add(this.shortPathOnOffBTN);
            this.shortPathGRP.Controls.Add(this.shortPathAlgCMB);
            this.shortPathGRP.Location = new System.Drawing.Point(378, 118);
            this.shortPathGRP.Name = "shortPathGRP";
            this.shortPathGRP.Size = new System.Drawing.Size(288, 49);
            this.shortPathGRP.TabIndex = 8;
            this.shortPathGRP.TabStop = false;
            this.shortPathGRP.Text = "Shortest Path";
            // 
            // shortPathAlgCMB
            // 
            this.shortPathAlgCMB.FormattingEnabled = true;
            this.shortPathAlgCMB.Location = new System.Drawing.Point(6, 19);
            this.shortPathAlgCMB.Name = "shortPathAlgCMB";
            this.shortPathAlgCMB.Size = new System.Drawing.Size(91, 21);
            this.shortPathAlgCMB.TabIndex = 0;
            this.shortPathAlgCMB.SelectedValueChanged += new System.EventHandler(this.shortPathAlgSelectedValueChanged);
            // 
            // shortPathOnOffBTN
            // 
            this.shortPathOnOffBTN.Location = new System.Drawing.Point(251, 17);
            this.shortPathOnOffBTN.Name = "shortPathOnOffBTN";
            this.shortPathOnOffBTN.Size = new System.Drawing.Size(31, 23);
            this.shortPathOnOffBTN.TabIndex = 1;
            this.shortPathOnOffBTN.Text = "Off";
            this.shortPathOnOffBTN.UseVisualStyleBackColor = true;
            this.shortPathOnOffBTN.Click += new System.EventHandler(this.shortPathOnOffClick);
            // 
            // shortPathDirectedCHK
            // 
            this.shortPathDirectedCHK.AutoSize = true;
            this.shortPathDirectedCHK.Location = new System.Drawing.Point(103, 21);
            this.shortPathDirectedCHK.Name = "shortPathDirectedCHK";
            this.shortPathDirectedCHK.Size = new System.Drawing.Size(66, 17);
            this.shortPathDirectedCHK.TabIndex = 2;
            this.shortPathDirectedCHK.Text = "Directed";
            this.shortPathDirectedCHK.UseVisualStyleBackColor = true;
            this.shortPathDirectedCHK.CheckedChanged += new System.EventHandler(this.shortPathDirectedCheckedChanged);
            // 
            // shortPathReversedCHK
            // 
            this.shortPathReversedCHK.AutoSize = true;
            this.shortPathReversedCHK.Location = new System.Drawing.Point(175, 21);
            this.shortPathReversedCHK.Name = "shortPathReversedCHK";
            this.shortPathReversedCHK.Size = new System.Drawing.Size(72, 17);
            this.shortPathReversedCHK.TabIndex = 3;
            this.shortPathReversedCHK.Text = "Reversed";
            this.shortPathReversedCHK.UseVisualStyleBackColor = true;
            this.shortPathReversedCHK.CheckedChanged += new System.EventHandler(this.shortPathReversedCheckedChanged);
            // 
            // DemoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(678, 600);
            this.Controls.Add(this.shortPathGRP);
            this.Controls.Add(this.graphStyleGRP);
            this.Controls.Add(this.graphCreatorGRP);
            this.Controls.Add(this.styleAlgGRP);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.layoutGRP);
            this.Name = "DemoForm";
            this.Text = "Graph Algorithm Demo";
            this.Load += new System.EventHandler(this.DemoForm_Load);
            this.layoutGRP.ResumeLayout(false);
            this.layoutGRP.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            this.styleAlgGRP.ResumeLayout(false);
            this.styleAlgGRP.PerformLayout();
            this.graphCreatorGRP.ResumeLayout(false);
            this.graphStyleGRP.ResumeLayout(false);
            this.graphStyleGRP.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.edgeAngNUM)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nodeRadNUM)).EndInit();
            this.shortPathGRP.ResumeLayout(false);
            this.shortPathGRP.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox layoutAlgCMB;
        private System.Windows.Forms.Label layoutAlgLBL;
        private System.Windows.Forms.GroupBox layoutGRP;
        private System.Windows.Forms.Button layoutStartStopBTN;
        private System.Windows.Forms.CheckBox layoutPausedCHK;
        private System.Windows.Forms.CheckBox layoutOnNodeMovedCHK;
        private System.Windows.Forms.Button layoutShuffleBTN;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private GraphForms.GraphPanel graphPanel;
        private System.Windows.Forms.PropertyGrid layoutParamsGrid;
        private System.Windows.Forms.Label layoutParamsLBL;
        private System.Windows.Forms.GroupBox styleAlgGRP;
        private System.Windows.Forms.Button styleAlgTestBTN;
        private System.Windows.Forms.ComboBox styleAlgCMB;
        private System.Windows.Forms.CheckBox styleAlgDirectedCHK;
        private System.Windows.Forms.CheckBox styleAlgReversedCHK;
        private System.Windows.Forms.GroupBox graphCreatorGRP;
        private System.Windows.Forms.Button createGraphBTN;
        private System.Windows.Forms.ComboBox graphCreatorCMB;
        private System.Windows.Forms.GroupBox graphStyleGRP;
        private System.Windows.Forms.NumericUpDown nodeRadNUM;
        private System.Windows.Forms.Label nodeRadLBL;
        private System.Windows.Forms.NumericUpDown edgeAngNUM;
        private System.Windows.Forms.Label edgeAngLBL;
        private System.Windows.Forms.CheckBox graphStyleResetOnCreateCHK;
        private System.Windows.Forms.GroupBox shortPathGRP;
        private System.Windows.Forms.ComboBox shortPathAlgCMB;
        private System.Windows.Forms.Button shortPathOnOffBTN;
        private System.Windows.Forms.CheckBox shortPathDirectedCHK;
        private System.Windows.Forms.CheckBox shortPathReversedCHK;
    }
}

