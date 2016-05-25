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
            foreach(var instruction in instructions) {
                if(instruction.OpCode == System.Reflection.Emit.OpCodes.Ldstr)
                    AppendLdSrtLine(rtbCode, instruction.Text, instruction.Bytes, bytesLength, (string)instruction.Operand, showOffset, showBytes);
                else
                    AppendLine(rtbCode, instruction.Text, instruction.Bytes, bytesLength, instruction.OpCode.ToString(), showOffset, showBytes);
            }
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
                var metaName = meta.PadLeft(offset + meta.Length);
                AppendHighlight(rtb, metaName, Color.Blue, Color.Green);
            }
            if(value != null) {
                var metaValue = (meta != null) ? (" " + value.ToString()) : value.ToString();
                AppendHighlight(rtb, metaValue, Color.Blue, Color.DarkBlue);
            }
            if(newLine) rtb.AppendText(System.Environment.NewLine);
        }
        static void AppendLdSrtLine(RichTextBox rtb, string line, byte[] bytes, int bytesLength, string value, bool showOffset, bool showBytes) {
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
            rtb.AppendText(System.Reflection.Emit.OpCodes.Ldstr.ToString());
            SetColor(rtb, Color.DarkCyan);
            rtb.AppendText(" \"" + value + "\"");
            rtb.AppendText(System.Environment.NewLine);
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
            var value = line.Substring(line.IndexOf(keyword) + keyword.Length);
            AppendHighlight(rtb, value, Color.DarkBlue, rtb.ForeColor, Color.Gray);
            rtb.AppendText(System.Environment.NewLine);
        }
        static void AppendHighlight(RichTextBox rtb, string codeLine, Color keywordColor, Color textColor) {
            codeLine.Highlight(
                keyword =>
                {
                    SetColor(rtb, keywordColor);
                    rtb.AppendText(keyword);
                },
                text =>
                {
                    SetColor(rtb, textColor);
                    rtb.AppendText(text);
                });
        }
        static void AppendHighlight(RichTextBox rtb, string codeLine, Color keywordColor, Color textColor, Color specialColor) {
            codeLine.Highlight(
                keyword =>
                {
                    SetColor(rtb, keywordColor);
                    rtb.AppendText(keyword);
                },
                text =>
                {
                    SetColor(rtb, textColor);
                    rtb.AppendText(text);
                },
                special =>
                {
                    SetColor(rtb, specialColor);
                    rtb.AppendText(special);
                });
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
    static class HightlightCodeExtension {
        static readonly string[] DefaultKeywords = new string[] { 
            "void", "instance", "static", "noinlining", "il", "dynamic", ".ctor",
            "object", "byte", "bool", "char", "int", "long", "decimal", "float", "double", "string", 
        };

        static internal void Highlight(this string codeLine, Action<string> appendKeyword, Action<string> append, Action<string> appendSpecial = null) {
            var words = codeLine.ToLowerInvariant().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var keywords = Array.FindAll(words,
                w => (Array.IndexOf(DefaultKeywords, w.ToLowerInvariant()) != -1 ||
                    w.ToLowerInvariant().Contains("@this") ||
                    w.ToLowerInvariant().Contains("@arg.") ||
                    w.ToLowerInvariant().Contains("@loc."))
                );
            int startIndex = 0; int endIndex = 0;
            if(keywords.Length > 0) {
                var line = codeLine.ToLowerInvariant();
                for(int i = 0; i < keywords.Length; i++) {
                    string keyword = keywords[i];
                    int index = FindWord(line, startIndex, keyword);
                    if(index != -1 && index - endIndex > 0)
                        append(codeLine.Substring(endIndex, index - endIndex));
                    if(index != -1) {
                        endIndex = index + keyword.Length;
                        startIndex = endIndex;
                    }
                    if(keyword.StartsWith("(@")) {
                        append("(");
                        (appendSpecial ?? appendKeyword)(keyword.Substring(1, keyword.Length - 1));
                    }
                    else appendKeyword(keyword);
                }
                if(codeLine.Length - endIndex > 0)
                    append(codeLine.Substring(endIndex, codeLine.Length - endIndex));
            }
            else append(codeLine);
        }
        static int FindWord(string line, int startIndex, string keyword) {
            int index = line.IndexOf(keyword, startIndex, StringComparison.InvariantCulture);
            if(index < 0)
                return -1;
            bool fromStart = (index > 0 && line[index - 1] == ' ') || (index == 0);
            bool fromEnd = (index + keyword.Length < line.Length && line[index + keyword.Length] == ' ') || (index + keyword.Length == line.Length);
            return (fromStart || fromEnd) ? index : FindWord(line, index + keyword.Length, keyword);
        }
    }
}