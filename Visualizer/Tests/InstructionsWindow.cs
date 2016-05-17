#if DEBUGTEST
namespace ILReader.Visualizer.Tests {
    using ILReader.Visualizer.UI;
    using NUnit.Framework;

    [TestFixture]
    public class InstructionsWindowTests {
        [Test, Explicit]
        public void Show() {
            var m = typeof(InstructionsWindowTests).GetMethod("Show");
            var cfg = Configuration.Resolve(m);
            var reader = cfg.GetReader(m);
            using(var window = new InstructionsWindow(reader)) {
                window.Text = "Show";
                window.ShowDialog();
            }
        }
    }
}
#endif