using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ILReader.Readers;

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
                else {
                    currentFontSize = fontSizes[value];
                    codeSize = value;
                }
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
                rtbCode.AppendText(Environment.NewLine);
            }
            int bytesLength = instructions.Any() && showBytes ? instructions.Max(i => i.Bytes.Length) : 0;
            Dictionary<int, string> exceptionBlocks = new Dictionary<int, string>();
            if(ILReader != null) {
                for(int i = 0; i < ILReader.ExceptionHandlers.Length; i++) {
                    var eh = ILReader.ExceptionHandlers[i];
                    PrepareExceptionBlockLine(exceptionBlocks, eh.TryStart.Index, ".try {", i, bytesLength);
                    PrepareExceptionBlockLine(exceptionBlocks, eh.TryEnd.Index, "}  // end .try", i, bytesLength);
                    if(eh.IsFinally)
                        PrepareExceptionBlockLine(exceptionBlocks, eh.HandlerStart.Index, "finally {", i, bytesLength);
                    if(eh.IsFault)
                        PrepareExceptionBlockLine(exceptionBlocks, eh.HandlerStart.Index, "fault {", i, bytesLength);
                    if(eh.IsCatch)
                        PrepareExceptionBlockLine(exceptionBlocks, eh.HandlerStart.Index, "catch " + eh.CatchType + " {", i, bytesLength);
                    if(eh.IsFilter)
                        PrepareExceptionBlockLine(exceptionBlocks, eh.FilterStart.Index, "filter {", i, bytesLength);
                    PrepareExceptionBlockLine(exceptionBlocks, eh.HandlerEnd.Index, "} // end handler", i, bytesLength);
                }
            }
            foreach(var instruction in instructions) {
                rtbCode.SelectionFont = GetFont(CodeSize, currentFontSize);
                string ebLines;
                if(exceptionBlocks.TryGetValue(instruction.Index, out ebLines))
                    AppendExceptionBlockLines(ebLines, instruction.Depth - 1);
                if(instruction.OpCode == System.Reflection.Emit.OpCodes.Ldstr)
                    AppendLdSrtLine(rtbCode, instruction, bytesLength, showOffset, showBytes);
                else
                    AppendLine(rtbCode, instruction, bytesLength, showOffset, showBytes);
            }
            rtbCode.EndUpdate();
            if(codeSize.HasValue) {
                SetFont(rtbCode, codeSize.Value);
                codeSize = null;
            }
            rtbCode.Select(0, 0);
            rtbCode.ScrollToCaret();
        }
        void AppendExceptionBlockLines(string exceptionBlock, int depth) {
            var lines = exceptionBlock.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            for(int i = 0; i < lines.Length; i++) {
                SetColor(rtbCode, Color.DeepSkyBlue);
                rtbCode.AppendText(lines[i].Substring(0, 6));
                if(depth > 0)
                    rtbCode.AppendText(new string(' ', depth * 2));
                SetColor(rtbCode, Color.DarkGreen);
                rtbCode.AppendText(lines[i].Substring(6));
                rtbCode.AppendText(Environment.NewLine);
            }
        }
        static void PrepareExceptionBlockLine(Dictionary<int, string> exceptionBlocks, int index, string text, int i, int bytesLength) {
            var blockLine = ("@" + i.ToString("X2")).PadRight(bytesLength * 2 + (bytesLength > 0 ? 1 : 0) + 8) + text + Environment.NewLine;
            string line;
            if(!exceptionBlocks.TryGetValue(index, out line))
                exceptionBlocks.Add(index, blockLine);
            else
                exceptionBlocks[index] = line + blockLine;
        }
        void AppendLines(IEnumerable<IMetadataItem> metadata, int offset = 0) {
            foreach(var meta in metadata) {
                rtbCode.SelectionFont = GetFont(CodeSize, currentFontSize);
                if(meta.HasChildren) {
                    AppendLine(rtbCode, meta.Name, "(" + Environment.NewLine, offset, false);
                    AppendLines(meta.Children, offset + 4);
                    rtbCode.SelectionFont = GetFont(CodeSize, currentFontSize);
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
            if(newLine) rtb.AppendText(Environment.NewLine);
        }
        static void AppendLdSrtLine(RichTextBox rtb, IInstruction instruction, int bytesLength, bool showOffset, bool showBytes) {
            if(showOffset) {
                string line = instruction.Text;
                SetColor(rtb, Color.Gray);
                rtb.AppendText(line.Substring(0, line.IndexOf(':') + 2));
            }
            if(showBytes) {
                SetColor(rtb, Color.DarkGreen);
                string strBytes = GetStrBytes(instruction.Bytes);
                rtb.AppendText(strBytes.PadRight(bytesLength * 2 + 1));
            }
            if(instruction.Depth > 0)
                rtb.AppendText(new string(' ', instruction.Depth * 2));
            SetColor(rtb, Color.Blue);
            rtb.AppendText(System.Reflection.Emit.OpCodes.Ldstr.ToString());
            SetColor(rtb, Color.DarkRed);
            string value = (string)instruction.Operand;
            rtb.AppendText(" \"" + value + "\"");
            rtb.AppendText(Environment.NewLine);
        }
        static void AppendLine(RichTextBox rtb, IInstruction instruction, int bytesLength, bool showOffset, bool showBytes) {
            string line = instruction.Text;
            if(showOffset) {
                SetColor(rtb, Color.Gray);
                rtb.AppendText(line.Substring(0, line.IndexOf(':') + 2));
            }
            if(showBytes) {
                SetColor(rtb, Color.DarkGreen);
                string strBytes = GetStrBytes(instruction.Bytes);
                rtb.AppendText(strBytes.PadRight(bytesLength * 2 + 1));
            }
            if(instruction.Depth > 0)
                rtb.AppendText(new string(' ', instruction.Depth * 2));
            SetColor(rtb, Color.Blue);
            string keyword = instruction.OpCode.ToString();
            rtb.AppendText(keyword);
            SetColor(rtb, rtb.ForeColor);
            var value = line.Substring(line.IndexOf(keyword) + keyword.Length);
            AppendHighlight(rtb, value, Color.DarkBlue, rtb.ForeColor, Color.Gray);
            rtb.AppendText(Environment.NewLine);
        }
        static void AppendHighlight(RichTextBox rtb, string codeLine, Color keywordColor, Color textColor) {
            codeLine.Highlight(
                keyword => {
                    SetColor(rtb, keywordColor);
                    rtb.AppendText(keyword);
                },
                text => {
                    SetColor(rtb, textColor);
                    rtb.AppendText(text);
                });
        }
        static void AppendHighlight(RichTextBox rtb, string codeLine, Color keywordColor, Color textColor, Color specialColor) {
            codeLine.Highlight(
                keyword => {
                    SetColor(rtb, keywordColor);
                    rtb.AppendText(keyword);
                },
                text => {
                    SetColor(rtb, textColor);
                    rtb.AppendText(text);
                },
                special => {
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
                SetFont(rtb, GetFont(index, currentFontSize));
            }
        }
        static Font GetFont(int index, float size) {
            if(fonts[index] == null)
                fonts[index] = new Font(fonts[DefaultFontIndex].FontFamily, size, FontStyle.Regular, GraphicsUnit.Point, 0);
            return fonts[index];
        }
        static string GetStrBytes(byte[] bytes) {
            string[] sBytes = new string[bytes.Length];
            for(int i = 0; i < sBytes.Length; i++)
                sBytes[i] = bytes[i].ToString("X2");
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
            SetFont(rtbCode, Array.FindIndex(fontSizes, fs => fs > currentFontSize));
        }
        void ZoomOut() {
            SetFont(rtbCode, Array.FindLastIndex(fontSizes, fs => fs < currentFontSize));
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
        static readonly char[] splitChars = new char[] { ' ' };
        static internal void Highlight(this string codeLine, Action<string> appendKeyword, Action<string> append, Action<string> appendSpecial = null) {
            var words = codeLine.ToLowerInvariant().Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
            var keywords = Array.FindAll(words, w => {
                var wl = w.ToLowerInvariant();
                return Array.IndexOf(DefaultKeywords, wl) != -1 || wl.Contains("@this") || wl.Contains("@arg.") || wl.Contains("@loc.");
            });
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