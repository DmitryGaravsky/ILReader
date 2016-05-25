namespace ILReader.Visualizer.UI {
    partial class InstructionsWindow {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if(disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.rbTextView = new System.Windows.Forms.RadioButton();
            this.rbDetailView = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.imgBox = new System.Windows.Forms.PictureBox();
            this.cbShowBytes = new System.Windows.Forms.CheckBox();
            this.cbShowOffset = new System.Windows.Forms.CheckBox();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.codeBox = new ILReader.Visualizer.UI.CodeBox();
            this.detailBox = new ILReader.Visualizer.UI.Controls.DetailBox();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgBox)).BeginInit();
            this.SuspendLayout();
            // 
            // rbTextView
            // 
            this.rbTextView.AutoSize = true;
            this.rbTextView.Checked = true;
            this.rbTextView.Location = new System.Drawing.Point(81, 12);
            this.rbTextView.Name = "rbTextView";
            this.rbTextView.Size = new System.Drawing.Size(46, 17);
            this.rbTextView.TabIndex = 1;
            this.rbTextView.TabStop = true;
            this.rbTextView.Text = "Text";
            this.rbTextView.UseVisualStyleBackColor = true;
            this.rbTextView.CheckedChanged += new System.EventHandler(this.textView_CheckedChanged);
            // 
            // rbDetailView
            // 
            this.rbDetailView.AutoSize = true;
            this.rbDetailView.Location = new System.Drawing.Point(133, 12);
            this.rbDetailView.Name = "rbDetailView";
            this.rbDetailView.Size = new System.Drawing.Size(57, 17);
            this.rbDetailView.TabIndex = 2;
            this.rbDetailView.Text = "Details";
            this.rbDetailView.UseVisualStyleBackColor = true;
            this.rbDetailView.CheckedChanged += new System.EventHandler(this.detailView_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label1.Location = new System.Drawing.Point(42, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "View:";
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.Controls.Add(this.imgBox);
            this.panel1.Controls.Add(this.cbShowBytes);
            this.panel1.Controls.Add(this.cbShowOffset);
            this.panel1.Controls.Add(this.rbTextView);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.rbDetailView);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(8);
            this.panel1.Size = new System.Drawing.Size(384, 40);
            this.panel1.TabIndex = 5;
            // 
            // imgBox
            // 
            this.imgBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.imgBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.imgBox.Location = new System.Drawing.Point(8, 8);
            this.imgBox.Name = "imgBox";
            this.imgBox.Size = new System.Drawing.Size(24, 24);
            this.imgBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.imgBox.TabIndex = 6;
            this.imgBox.TabStop = false;
            this.imgBox.Click += new System.EventHandler(this.imgBox_Click);
            // 
            // cbShowBytes
            // 
            this.cbShowBytes.AutoSize = true;
            this.cbShowBytes.Dock = System.Windows.Forms.DockStyle.Right;
            this.cbShowBytes.Location = new System.Drawing.Point(210, 8);
            this.cbShowBytes.Margin = new System.Windows.Forms.Padding(0);
            this.cbShowBytes.Name = "cbShowBytes";
            this.cbShowBytes.Size = new System.Drawing.Size(82, 24);
            this.cbShowBytes.TabIndex = 5;
            this.cbShowBytes.Text = "Show Bytes";
            this.cbShowBytes.UseVisualStyleBackColor = true;
            this.cbShowBytes.CheckedChanged += new System.EventHandler(this.cbShowBytes_CheckedChanged);
            // 
            // cbShowOffset
            // 
            this.cbShowOffset.AutoSize = true;
            this.cbShowOffset.Checked = true;
            this.cbShowOffset.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbShowOffset.Dock = System.Windows.Forms.DockStyle.Right;
            this.cbShowOffset.Location = new System.Drawing.Point(292, 8);
            this.cbShowOffset.Margin = new System.Windows.Forms.Padding(0);
            this.cbShowOffset.Name = "cbShowOffset";
            this.cbShowOffset.Size = new System.Drawing.Size(84, 24);
            this.cbShowOffset.TabIndex = 4;
            this.cbShowOffset.Text = "Show Offset";
            this.cbShowOffset.UseVisualStyleBackColor = true;
            this.cbShowOffset.CheckedChanged += new System.EventHandler(this.cbShowOffset_CheckedChanged);
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // codeBox
            // 
            this.codeBox.BackColor = System.Drawing.SystemColors.Window;
            this.codeBox.CodeSize = 1;
            this.codeBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.codeBox.Location = new System.Drawing.Point(0, 40);
            this.codeBox.Name = "codeBox";
            this.codeBox.Padding = new System.Windows.Forms.Padding(8);
            this.codeBox.Size = new System.Drawing.Size(384, 321);
            this.codeBox.TabIndex = 0;
            // 
            // detailBox
            // 
            this.detailBox.BytesVisible = false;
            this.detailBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.detailBox.Location = new System.Drawing.Point(0, 0);
            this.detailBox.Name = "detailBox";
            this.detailBox.OffsetVisible = true;
            this.detailBox.Padding = new System.Windows.Forms.Padding(8);
            this.detailBox.Size = new System.Drawing.Size(384, 361);
            this.detailBox.TabIndex = 6;
            // 
            // InstructionsWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 361);
            this.Controls.Add(this.codeBox);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.detailBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Location = new System.Drawing.Point(100, 100);
            this.Name = "InstructionsWindow";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "InstructionsWindow";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.window_FormClosing);
            this.Load += new System.EventHandler(this.window_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private CodeBox codeBox;
        private System.Windows.Forms.RadioButton rbTextView;
        private System.Windows.Forms.RadioButton rbDetailView;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox cbShowOffset;
        private Controls.DetailBox detailBox;
        private System.Windows.Forms.CheckBox cbShowBytes;
        private System.Windows.Forms.PictureBox imgBox;
        private System.Windows.Forms.ImageList imageList1;
    }
}