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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.colIndex = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colOffset = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colOpCode = new System.Windows.Forms.DataGridViewLinkColumn();
            this.colOperand = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.iInstructionBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.rbTextView = new System.Windows.Forms.RadioButton();
            this.rbDetailView = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.cbShowOffset = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.iInstructionBindingSource)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.richTextBox1.AutoWordSelection = true;
            this.richTextBox1.BackColor = System.Drawing.SystemColors.Window;
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox1.DetectUrls = false;
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBox1.Location = new System.Drawing.Point(0, 24);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(584, 537);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            this.richTextBox1.WordWrap = false;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.AliceBlue;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dataGridView1.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView1.AutoGenerateColumns = false;
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridView1.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dataGridView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridView1.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.dataGridView1.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colIndex,
            this.colOffset,
            this.colOpCode,
            this.colOperand});
            this.dataGridView1.DataSource = this.iInstructionBindingSource;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 24);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersVisible = false;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dataGridView1.RowsDefaultCellStyle = dataGridViewCellStyle6;
            this.dataGridView1.RowTemplate.Height = 18;
            this.dataGridView1.ShowEditingIcon = false;
            this.dataGridView1.Size = new System.Drawing.Size(584, 537);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            // 
            // colIndex
            // 
            this.colIndex.DataPropertyName = "Index";
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.SteelBlue;
            this.colIndex.DefaultCellStyle = dataGridViewCellStyle2;
            this.colIndex.FillWeight = 50F;
            this.colIndex.HeaderText = "#";
            this.colIndex.Name = "colIndex";
            this.colIndex.ReadOnly = true;
            this.colIndex.Width = 39;
            // 
            // colOffset
            // 
            this.colOffset.DataPropertyName = "Offset";
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.DimGray;
            dataGridViewCellStyle3.Format = "X4";
            this.colOffset.DefaultCellStyle = dataGridViewCellStyle3;
            this.colOffset.FillWeight = 60F;
            this.colOffset.HeaderText = "Offset";
            this.colOffset.Name = "colOffset";
            this.colOffset.ReadOnly = true;
            this.colOffset.Width = 60;
            // 
            // colOpCode
            // 
            this.colOpCode.ActiveLinkColor = System.Drawing.Color.White;
            this.colOpCode.DataPropertyName = "OpCode";
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.Blue;
            this.colOpCode.DefaultCellStyle = dataGridViewCellStyle4;
            this.colOpCode.HeaderText = "OpCode";
            this.colOpCode.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.colOpCode.Name = "colOpCode";
            this.colOpCode.ReadOnly = true;
            this.colOpCode.Width = 52;
            // 
            // colOperand
            // 
            this.colOperand.DataPropertyName = "Operand";
            dataGridViewCellStyle5.ForeColor = System.Drawing.Color.DimGray;
            this.colOperand.DefaultCellStyle = dataGridViewCellStyle5;
            this.colOperand.HeaderText = "Operand";
            this.colOperand.Name = "colOperand";
            this.colOperand.ReadOnly = true;
            this.colOperand.Width = 73;
            // 
            // iInstructionBindingSource
            // 
            this.iInstructionBindingSource.DataSource = typeof(ILReader.Readers.IInstruction);
            // 
            // rbTextView
            // 
            this.rbTextView.AutoSize = true;
            this.rbTextView.Checked = true;
            this.rbTextView.Location = new System.Drawing.Point(52, 4);
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
            this.rbDetailView.Location = new System.Drawing.Point(104, 4);
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
            this.label1.Location = new System.Drawing.Point(13, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "View:";
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.Controls.Add(this.cbShowOffset);
            this.panel1.Controls.Add(this.rbTextView);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.rbDetailView);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(584, 24);
            this.panel1.TabIndex = 5;
            // 
            // cbShowOffset
            // 
            this.cbShowOffset.AutoSize = true;
            this.cbShowOffset.Checked = true;
            this.cbShowOffset.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbShowOffset.Dock = System.Windows.Forms.DockStyle.Right;
            this.cbShowOffset.Location = new System.Drawing.Point(500, 0);
            this.cbShowOffset.Margin = new System.Windows.Forms.Padding(0);
            this.cbShowOffset.Name = "cbShowOffset";
            this.cbShowOffset.Size = new System.Drawing.Size(84, 24);
            this.cbShowOffset.TabIndex = 4;
            this.cbShowOffset.Text = "Show Offset";
            this.cbShowOffset.UseVisualStyleBackColor = true;
            this.cbShowOffset.CheckedChanged += new System.EventHandler(this.cbShowOffset_CheckedChanged);
            // 
            // InstructionsWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 561);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "InstructionsWindow";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "InstructionsWindow";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.iInstructionBindingSource)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.BindingSource iInstructionBindingSource;
        private System.Windows.Forms.RadioButton rbTextView;
        private System.Windows.Forms.RadioButton rbDetailView;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.DataGridViewTextBoxColumn colIndex;
        private System.Windows.Forms.DataGridViewTextBoxColumn colOffset;
        private System.Windows.Forms.DataGridViewLinkColumn colOpCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colOperand;
        private System.Windows.Forms.CheckBox cbShowOffset;
    }
}