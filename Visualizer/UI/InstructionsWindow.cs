using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ILReader.Readers;

namespace ILReader.Visualizer.UI {
    public partial class InstructionsWindow : Form {
        public InstructionsWindow() {
            InitializeComponent();
        }
        public InstructionsWindow(IEnumerable<IInstruction> instructions)
            : this() {
            AppendLines(instructions);
            //
            iInstructionBindingSource.DataSource = instructions.ToList();
        }
        void AppendLines(IEnumerable<IInstruction> instructions, bool showOffset = true) {
            richTextBox1.ResetText();
            foreach(var instruction in instructions)
                AppendLine(richTextBox1, instruction.Text, instruction.OpCode.ToString(), showOffset);
        }
        static void AppendLine(RichTextBox rtb, string line, string keyword, bool showOffset = true) {
            if(showOffset) {
                rtb.SelectionStart = rtb.TextLength;
                rtb.SelectionLength = 0;
                rtb.SelectionColor = Color.Gray;
                rtb.AppendText(line.Substring(0, 8));
            }
            rtb.SelectionStart = rtb.TextLength;
            rtb.SelectionLength = 0;
            rtb.SelectionColor = Color.Blue;
            rtb.AppendText(keyword);
            //
            rtb.SelectionColor = rtb.ForeColor;
            rtb.SelectionStart = rtb.TextLength;
            rtb.SelectionLength = 0;
            rtb.AppendText(line.Substring(line.IndexOf(keyword) + keyword.Length));
            //
            rtb.AppendText(System.Environment.NewLine);
        }
        void textView_CheckedChanged(object sender, System.EventArgs e) {
            richTextBox1.BringToFront();
        }
        void detailView_CheckedChanged(object sender, System.EventArgs e) {
            dataGridView1.BringToFront();
        }
        void cbShowOffset_CheckedChanged(object sender, System.EventArgs e) {
            colOffset.Visible = cbShowOffset.Checked;
            AppendLines((IEnumerable<IInstruction>)iInstructionBindingSource.DataSource, cbShowOffset.Checked);
        }
        //
        const string uriFormat = @"https://msdn.microsoft.com/en-us/library/system.reflection.emit.opcodes.{0}(v=vs.110).aspx";
        void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) {
            if(e.ColumnIndex == colOpCode.Index) {
                var opCode = (System.Reflection.Emit.OpCode)dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                OpenUrl(new System.Uri(string.Format(uriFormat, opCode.ToString()), System.UriKind.Absolute));
            }
        }
        static void OpenUrl(System.Uri uri) {
            try { System.Diagnostics.Process.Start(uri.AbsoluteUri); }
            catch { }
        }
    }
}