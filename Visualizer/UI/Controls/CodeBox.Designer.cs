namespace ILReader.Visualizer.UI {
    partial class CodeBox {
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.rtbCode = new System.Windows.Forms.RichTextBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuItemCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemZoomReset = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemZoomIn = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemZoomOut = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // rtbCode
            // 
            this.rtbCode.AutoWordSelection = true;
            this.rtbCode.BackColor = System.Drawing.SystemColors.Window;
            this.rtbCode.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbCode.ContextMenuStrip = this.contextMenuStrip1;
            this.rtbCode.DetectUrls = false;
            this.rtbCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbCode.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtbCode.Location = new System.Drawing.Point(8, 8);
            this.rtbCode.Name = "rtbCode";
            this.rtbCode.ReadOnly = true;
            this.rtbCode.Size = new System.Drawing.Size(284, 284);
            this.rtbCode.TabIndex = 1;
            this.rtbCode.Text = "";
            this.rtbCode.WordWrap = false;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemCopy,
            this.menuItemZoomReset,
            this.menuItemZoomIn,
            this.menuItemZoomOut});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(223, 92);
            this.contextMenuStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.contextMenu_ItemClicked);
            // 
            // menuItemCopy
            // 
            this.menuItemCopy.Name = "menuItemCopy";
            this.menuItemCopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.menuItemCopy.Size = new System.Drawing.Size(222, 22);
            this.menuItemCopy.Text = "Copy";
            // 
            // menuItemZoomReset
            // 
            this.menuItemZoomReset.Name = "menuItemZoomReset";
            this.menuItemZoomReset.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D0)));
            this.menuItemZoomReset.Size = new System.Drawing.Size(222, 22);
            this.menuItemZoomReset.Text = "Zoom (100%)";
            // 
            // menuItemZoomIn
            // 
            this.menuItemZoomIn.Name = "menuItemZoomIn";
            this.menuItemZoomIn.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Oemplus)));
            this.menuItemZoomIn.Size = new System.Drawing.Size(222, 22);
            this.menuItemZoomIn.Text = "Zoom In";
            // 
            // menuItemZoomOut
            // 
            this.menuItemZoomOut.Name = "menuItemZoomOut";
            this.menuItemZoomOut.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.OemMinus)));
            this.menuItemZoomOut.Size = new System.Drawing.Size(222, 22);
            this.menuItemZoomOut.Text = "Zoom Out";
            // 
            // CodeBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.rtbCode);
            this.Name = "CodeBox";
            this.Padding = new System.Windows.Forms.Padding(8);
            this.Size = new System.Drawing.Size(300, 300);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtbCode;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem menuItemCopy;
        private System.Windows.Forms.ToolStripMenuItem menuItemZoomReset;
        private System.Windows.Forms.ToolStripMenuItem menuItemZoomIn;
        private System.Windows.Forms.ToolStripMenuItem menuItemZoomOut;
    }
}
