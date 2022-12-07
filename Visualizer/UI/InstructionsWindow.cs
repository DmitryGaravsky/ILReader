namespace ILReader.Visualizer.UI {
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    using ILReader.Readers;

    public partial class InstructionsWindow : Form {
        public InstructionsWindow() {
            InitializeComponent();
        }
        IILReader ILReader;
        public InstructionsWindow(IEnumerable<IInstruction> reader)
            : this() {
            ILReader = reader as IILReader;
            if(ILReader == null)
                LoadSettingsAndUpdate(reader);
        }
        void window_FormClosing(object sender, FormClosingEventArgs e) {
            SaveSettings();
        }
        void window_Load(object sender, System.EventArgs e) {
            if(ILReader != null)
                LoadSettingsAndUpdate(ILReader);
        }
        void imgBox_Click(object sender, System.EventArgs e) {
            using(var about = new About()) 
                about.ShowDialog();
        }
        void textView_CheckedChanged(object sender, System.EventArgs e) {
            codeBox.BringToFront();
        }
        void detailView_CheckedChanged(object sender, System.EventArgs e) {
            detailBox.BringToFront();
        }
        void cbShowBytes_CheckedChanged(object sender, System.EventArgs e) {
            detailBox.BytesVisible = cbShowBytes.Checked;
            UpdateCodeBox(cbShowOffset.Checked, cbShowBytes.Checked);
        }
        void cbShowOffset_CheckedChanged(object sender, System.EventArgs e) {
            detailBox.OffsetVisible = cbShowOffset.Checked;
            UpdateCodeBox(cbShowOffset.Checked, cbShowBytes.Checked);
        }
        void LoadSettingsAndUpdate(IEnumerable<IInstruction> instructions) {
            LoadSettings();
            LoadImage(instructions as IILReader);
            UpdateCodeBox(instructions, cbShowOffset.Checked, cbShowBytes.Checked);
            detailBox.DataSource = instructions;
        }
        void LoadImage(IILReader ILReader) {
            imgBox.Image = LoadImage("method");
            if(ILReader == null)
                return;
            var methodDesc = ILReader.Metadata.FirstOrDefault();
            if(methodDesc != null) {
                if(methodDesc.Name.Contains(".ctor"))
                    imgBox.Image = LoadImage("constructor");
                if(methodDesc.Name.Contains("dynamic"))
                    imgBox.Image = LoadImage("dynmethod");
            }
        }
        Image LoadImage(string imageName) {
            var asm = typeof(InstructionsWindow).Assembly;
            using(var s = asm.GetManifestResourceStream("ILReader.Visualizer.Images." + imageName)) 
                return Image.FromStream(s);
        }
        void UpdateCodeBox(bool showOffset, bool showBytes) {
            UpdateCodeBox(ILReader ?? GetInstructions(), showOffset, showBytes);
        }
        void UpdateCodeBox(IEnumerable<IInstruction> instructions, bool showOffset, bool showBytes) {
            codeBox.AppendLines(instructions ?? GetInstructions(), showOffset, showBytes);
        }
        IEnumerable<IInstruction> GetInstructions() {
            return (IEnumerable<IInstruction>)detailBox.DataSource;
        }
        #region Settings
        void LoadSettings() {
            var settings = Properties.Settings.Default;
            detailBox.BytesVisible = cbShowBytes.Checked = settings.ShowBytes;
            detailBox.OffsetVisible = cbShowOffset.Checked = settings.ShowOffset;
            codeBox.CodeSize = settings.CodeSize;
            Bounds = new Rectangle(settings.Left, settings.Top, settings.Width, settings.Height);
            WindowState = (FormWindowState)settings.WindowState;
        }
        void SaveSettings() {
            var settings = Properties.Settings.Default;
            settings.WindowState = (int)WindowState;
            var bounds = (WindowState != FormWindowState.Normal) ? RestoreBounds : Bounds;
            settings.Left = bounds.Left;
            settings.Top = bounds.Top;
            settings.Width = bounds.Width;
            settings.Height = bounds.Height;
            settings.ShowBytes = cbShowBytes.Checked;
            settings.ShowOffset = cbShowOffset.Checked;
            settings.CodeSize = codeBox.CodeSize;
            settings.Save();
        }
        #endregion Settings
    }
}