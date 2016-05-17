using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ILReader.Readers;
using System;

namespace ILReader.Visualizer.UI {
    public partial class CodeBox : UserControl {
        const int DefaultFontIndex = 1;
        float currentFontSize;
        readonly static float[] fontSizes = new float[] { 
            6, 8.25f, 10, 12, 14, 16, 18, 20, 22, 24, 28, 32, 36, 40, 48, 56, 72
        };
        readonly static Font[] fonts = new Font[fontSizes.Length];
        int? codeSize;
        public int CodeSize {
            get { return codeSize.HasValue ? codeSize.Value : Array.FindIndex(fontSizes, fs => fs >= currentFontSize); }
            set {
                if(value == CodeSize)
                    return;
                if(value < 0 || value >= fontSizes.Length)
                    return;
                if(rtbCode.TextLength > 0)
                    SetFont(rtbCode, value);
                else
                    codeSize = value;
            }
        }
        public CodeBox() {
            InitializeComponent();
            fonts[DefaultFontIndex] = rtbCode.Font;
            currentFontSize = fontSizes[DefaultFontIndex];
            rtbCode.MouseWheel += rtbCode_MouseWheel;
            rtbCode.KeyDown += rtbCode_KeyDown;
        }
        public void AppendLines(IEnumerable<IInstruction> instructions, bool showOffset = true, bool showBytes = false) {
            rtbCode.BeginUpdate();
            rtbCode.ResetText();
            var ILReader = instructions as Readers.IILReader;
            if(ILReader != null && ILReader.Metadata.Any()) {
                AppendLines(ILReader.Metadata);
                rtbCode.AppendText(System.Environment.NewLine);
            }
            int bytesLength = instructions.Any() ? instructions.Max(i => i.Bytes.Length) : 0;
            foreach(var instruction in instructions)
                AppendLine(rtbCode, instruction.Text, instruction.Bytes, bytesLength, instruction.OpCode.ToString(), showOffset, showBytes);
            rtbCode.EndUpdate();
            //
            if(codeSize.HasValue) {
                SetFont(rtbCode, codeSize.Value);
                codeSize = null;
            }
        }
        void AppendLines(IEnumerable<Readers.IMetadataItem> metadata, int offset = 0) {
            foreach(var meta in metadata) {
                if(meta.HasChildren) {
                    AppendLine(rtbCode, meta.Name, "(" + System.Environment.NewLine, offset, false);
                    AppendLines(meta.Children, offset + 4);
                    AppendLine(rtbCode, null, ")", offset);
                }
                else AppendLine(rtbCode, meta.Name, meta.Value, offset);
            }
        }
        static void AppendLine(RichTextBox rtb, string meta, object value, int offset = 0, bool newLine = true) {
            if(meta != null) {
                SetColor(rtb, Color.Green);
                rtb.AppendText(meta.PadLeft(offset + meta.Length));
            }
            if(value != null) {
                SetColor(rtb, Color.DarkBlue);
                rtb.AppendText((meta != null) ? (" " + value.ToString()) : value.ToString());
            }
            if(newLine) rtb.AppendText(System.Environment.NewLine);
        }
        static void AppendLine(RichTextBox rtb, string line, byte[] bytes, int bytesLength, string keyword, bool showOffset, bool showBytes) {
            if(showOffset) {
                SetColor(rtb, Color.Gray);
                rtb.AppendText(line.Substring(0, line.IndexOf(':') + 2));
            }
            if(showBytes) {
                SetColor(rtb, Color.DarkGreen);
                string strBytes = GetStrBytes(bytes);
                rtb.AppendText(strBytes.PadRight(bytesLength * 2 + 1));
            }
            SetColor(rtb, Color.Blue);
            rtb.AppendText(keyword);
            SetColor(rtb, rtb.ForeColor);
            rtb.AppendText(line.Substring(line.IndexOf(keyword) + keyword.Length));
            rtb.AppendText(System.Environment.NewLine);
        }
        static void SetColor(RichTextBox rtb, Color color) {
            rtb.SelectionStart = rtb.TextLength;
            rtb.SelectionLength = 0;
            rtb.SelectionColor = color;
        }
        static void SetFont(RichTextBox rtb, Font font) {
            rtb.BeginUpdate();
            rtb.Select(0, rtb.TextLength);
            rtb.SelectionFont = font;
            rtb.SelectionLength = 0;
            rtb.EndUpdate();
        }
        void SetFont(RichTextBox rtb, int index) {
            if(index != -1) {
                currentFontSize = fontSizes[index];
                if(fonts[index] == null)
                    fonts[index] = new System.Drawing.Font(fonts[DefaultFontIndex].FontFamily, currentFontSize, FontStyle.Regular, GraphicsUnit.Point, 0);
                SetFont(rtb, fonts[index]);
            }
        }
        static string GetStrBytes(byte[] bytes) {
            string[] sBytes = new string[bytes.Length];
            for(int i = 0; i < sBytes.Length; i++)
                sBytes[i] = string.Format("{0:X2}", bytes[i]);
            return string.Join(string.Empty, sBytes);
        }
        void contextMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            if(e.ClickedItem == menuItemCopy) {
                try { Clipboard.SetText(rtbCode.SelectedText, TextDataFormat.UnicodeText); }
                catch { }
            }
            if(e.ClickedItem == menuItemZoomReset)
                ZoomReset();
            if(e.ClickedItem == menuItemZoomIn)
                ZoomIn();
            if(e.ClickedItem == menuItemZoomOut)
                ZoomOut();
        }
        void rtbCode_KeyDown(object sender, KeyEventArgs e) {
            if(e.Control && e.KeyCode == Keys.Add)
                ZoomIn();
            if(e.Control && e.KeyCode == Keys.Subtract)
                ZoomOut();
        }
        void rtbCode_MouseWheel(object sender, MouseEventArgs e) {
            if((Control.ModifierKeys & Keys.Control) == Keys.Control) {
                if(e.Delta > 0)
                    ZoomIn();
                else
                    ZoomOut();
            }
        }
        void ZoomReset() {
            SetFont(rtbCode, DefaultFontIndex);
        }
        void ZoomIn() {
            SetFont(rtbCode, System.Array.FindIndex(fontSizes, fs => fs > currentFontSize));
        }
        void ZoomOut() {
            SetFont(rtbCode, System.Array.FindLastIndex(fontSizes, fs => fs < currentFontSize));
        }
    }
    //
    static class RichTextBoxExtension {
        const int WM_SETREDRAW = 0x0b;
        public static void BeginUpdate(this RichTextBox rtb) {
            SendMessage(rtb.Handle, WM_SETREDRAW, (IntPtr)0, IntPtr.Zero);
        }
        public static void EndUpdate(this RichTextBox rtb) {
            SendMessage(rtb.Handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
            rtb.Invalidate();
        }
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
    }
}