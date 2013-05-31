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
            this.layoutGRP.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.styleAlgGRP.SuspendLayout();
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
            this.layoutOnNodeMovedCHK.Checked = true;
            this.layoutOnNodeMovedCHK.CheckState = System.Windows.Forms.CheckState.Checked;
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
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(12, 125);
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
            this.styleAlgTestBTN.Location = new System.Drawing.Point(342, 17);
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
            // DemoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(678, 546);
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
    }
}

