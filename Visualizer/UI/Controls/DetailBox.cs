using System.Collections.Generic;
using System.Windows.Forms;

namespace ILReader.Visualizer.UI.Controls {
    public partial class DetailBox : UserControl {
        public DetailBox() {
            InitializeComponent();
        }
        public object DataSource {
            get { return dataGridView1.DataSource; }
            set { dataGridView1.DataSource = value; }
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
                        sBytes[i] = string.Format("{0:X2}", bytes[i]);
                    e.Value = string.Join(" ", sBytes);
                }
            }
        }
        const string uriFormat = @"https://msdn.microsoft.com/en-us/library/system.reflection.emit.opcodes.{0}(v=vs.110).aspx";
        void dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e) {
            if(e.ColumnIndex == colOpCode.Index) {
                var opCode = (System.Reflection.Emit.OpCode)dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                OpenUrl(new System.Uri(string.Format(uriFormat, GetOpcodeName(opCode)), System.UriKind.Absolute));
            }
        }
        static void OpenUrl(System.Uri uri) {
            try { System.Diagnostics.Process.Start(uri.AbsoluteUri); }
            catch { }
        }
        static IDictionary<System.Reflection.Emit.OpCode, string> opCodeNames;
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