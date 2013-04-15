﻿namespace GraphAlgorithmDemo
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
            this.layoutStartStopBTN = new System.Windows.Forms.Button();
            this.layoutShuffleBTN = new System.Windows.Forms.Button();
            this.layoutOnNodeMovedCHK = new System.Windows.Forms.CheckBox();
            this.layoutPausedCHK = new System.Windows.Forms.CheckBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.graphPanel = new GraphForms.GraphPanel();
            this.layoutParamsGrid = new System.Windows.Forms.PropertyGrid();
            this.layoutParamsLBL = new System.Windows.Forms.Label();
            this.layoutGRP.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // layoutAlgCMB
            // 
            this.layoutAlgCMB.FormattingEnabled = true;
            this.layoutAlgCMB.Location = new System.Drawing.Point(100, 13);
            this.layoutAlgCMB.Name = "layoutAlgCMB";
            this.layoutAlgCMB.Size = new System.Drawing.Size(135, 21);
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
            this.layoutGRP.Size = new System.Drawing.Size(538, 43);
            this.layoutGRP.TabIndex = 3;
            this.layoutGRP.TabStop = false;
            this.layoutGRP.Text = "Layout Settings";
            // 
            // layoutStartStopBTN
            // 
            this.layoutStartStopBTN.Location = new System.Drawing.Point(437, 11);
            this.layoutStartStopBTN.Name = "layoutStartStopBTN";
            this.layoutStartStopBTN.Size = new System.Drawing.Size(37, 23);
            this.layoutStartStopBTN.TabIndex = 3;
            this.layoutStartStopBTN.Text = "Start";
            this.layoutStartStopBTN.UseVisualStyleBackColor = true;
            this.layoutStartStopBTN.Click += new System.EventHandler(this.layoutStartStopClick);
            // 
            // layoutShuffleBTN
            // 
            this.layoutShuffleBTN.Location = new System.Drawing.Point(480, 11);
            this.layoutShuffleBTN.Name = "layoutShuffleBTN";
            this.layoutShuffleBTN.Size = new System.Drawing.Size(50, 23);
            this.layoutShuffleBTN.TabIndex = 4;
            this.layoutShuffleBTN.Text = "Shuffle";
            this.layoutShuffleBTN.UseVisualStyleBackColor = true;
            this.layoutShuffleBTN.Click += new System.EventHandler(this.layoutShuffleClick);
            // 
            // layoutOnNodeMovedCHK
            // 
            this.layoutOnNodeMovedCHK.AutoSize = true;
            this.layoutOnNodeMovedCHK.Checked = true;
            this.layoutOnNodeMovedCHK.CheckState = System.Windows.Forms.CheckState.Checked;
            this.layoutOnNodeMovedCHK.Location = new System.Drawing.Point(241, 15);
            this.layoutOnNodeMovedCHK.Name = "layoutOnNodeMovedCHK";
            this.layoutOnNodeMovedCHK.Size = new System.Drawing.Size(128, 17);
            this.layoutOnNodeMovedCHK.TabIndex = 6;
            this.layoutOnNodeMovedCHK.Text = "Start on Node Moved";
            this.layoutOnNodeMovedCHK.UseVisualStyleBackColor = true;
            this.layoutOnNodeMovedCHK.CheckedChanged += new System.EventHandler(this.layoutOnNodeMovedCheckedChanged);
            // 
            // layoutPausedCHK
            // 
            this.layoutPausedCHK.AutoSize = true;
            this.layoutPausedCHK.Location = new System.Drawing.Point(375, 15);
            this.layoutPausedCHK.Name = "layoutPausedCHK";
            this.layoutPausedCHK.Size = new System.Drawing.Size(56, 17);
            this.layoutPausedCHK.TabIndex = 7;
            this.layoutPausedCHK.Text = "Pause";
            this.layoutPausedCHK.UseVisualStyleBackColor = true;
            this.layoutPausedCHK.CheckedChanged += new System.EventHandler(this.layoutPausedCheckedChanged);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(12, 62);
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
            // layoutParamsLBL
            // 
            this.layoutParamsLBL.AutoSize = true;
            this.layoutParamsLBL.Location = new System.Drawing.Point(3, 4);
            this.layoutParamsLBL.Name = "layoutParamsLBL";
            this.layoutParamsLBL.Size = new System.Drawing.Size(98, 13);
            this.layoutParamsLBL.TabIndex = 1;
            this.layoutParamsLBL.Text = "Layout Parameters:";
            // 
            // DemoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(678, 546);
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
    }
}

