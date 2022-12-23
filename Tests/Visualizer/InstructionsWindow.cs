namespace ILReader.Visualizer.Tests {
    using ILReader.Visualizer.UI;
    using NUnit.Framework;

    [TestFixture]
    public class InstructionsWindowTests {
        [Test, Explicit]
        public void Show() {
            var m = typeof(InstructionsWindowTests).GetMethod("Show");
            var cfg = Configuration.Resolve(m);
            var source = cfg.GetReader(m) as Dump.ISupportDump;
            using(var ms = new System.IO.MemoryStream()) {
                source.Dump(ms);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                cfg = Configuration.Resolve(ms);
                var reader = cfg.GetReader(ms);
                using(var window = new InstructionsWindow(reader)) {
                    window.Text = reader.Name;
                    window.ShowDialog();
                }
            }
        }
    }
}