using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ILReader.Visualizer.UI.Controls {
    public partial class DetailBox : UserControl {
        public DetailBox() {
            InitializeComponent();
        }
        public object DataSource {
            get { return dataGridView.DataSource; }
            set {
                dataGridView.DataSource = ((IEnumerable<Readers.IInstruction>)value).ToList();
                ShowIssues(value as ILReader.Readers.IILReader);
            }
        }
        void ShowIssues(Readers.IILReader reader) {
            var nopColor = Color.FromArgb(200, 200, 255);
            ShowIssues(reader, nopColor, Analyzer.Nop.Instance);
            var issueColor = Color.FromArgb(255, 200, 255);
            ShowIssues(reader, issueColor, Analyzer.Box.Instance);
            ShowIssues(reader, issueColor, Analyzer.Unbox.Instance);
            dataGridView.ClearSelection();
        }
        void ShowIssues(Readers.IILReader reader, Color color, Analyzer.ILPattern pattern) {
            if(reader == null)
                return;
            while(pattern.Match(reader, false))
                dataGridView.Rows[pattern.StartIndex].Cells[colOpCode.Name].Style.BackColor = color;
            pattern.Reset();
        }
        public bool OffsetVisible {
            get { return colOffset.Visible; }
            set { colOffset.Visible = value; }
        }
        public bool BytesVisible {
            get { return colBytes.Visible; }
            set { colBytes.Visible = value; }
        }
        //
        void dataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e) {
            if(e.ColumnIndex == colBytes.Index) {
                byte[] bytes = e.Value as byte[];
                if(bytes != null) {
                    string[] sBytes = new string[bytes.Length];
                    for(int i = 0; i < sBytes.Length; i++)
                        sBytes[i] = bytes[i].ToString("X2");
                    e.Value = string.Join(" ", sBytes);
                }
            }
        }
        const string uriFormat = @"https://msdn.microsoft.com/en-us/library/system.reflection.emit.opcodes.{0}(v=vs.110).aspx";
        void dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e) {
            if(e.ColumnIndex == colOpCode.Index && e.RowIndex > 0) {
                var opCode = (System.Reflection.Emit.OpCode)dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                OpenUrl(new System.Uri(string.Format(uriFormat, GetOpcodeName(opCode)), System.UriKind.Absolute));
            }
        }
        static void OpenUrl(System.Uri uri) {
            try { System.Diagnostics.Process.Start(uri.AbsoluteUri); }
            catch { }
        }
        static Dictionary<System.Reflection.Emit.OpCode, string> opCodeNames;
        static string GetOpcodeName(System.Reflection.Emit.OpCode opCode) {
            if(opCodeNames == null) {
                opCodeNames = new Dictionary<System.Reflection.Emit.OpCode, string>();
                var fields = typeof(System.Reflection.Emit.OpCodes).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                for(int i = 0; i < fields.Length; i++)
                    opCodeNames.Add((System.Reflection.Emit.OpCode)fields[i].GetValue(null), fields[i].Name);
            }
            return opCodeNames[opCode];
        }
    }
}