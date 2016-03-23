#if DEBUGTEST
namespace ILReader.Visualizer.Tests {
    using ILReader.Visualizer.UI;
    using NUnit.Framework;

    [TestFixture]
    public class InstructionsWindowTests {
        [Test, Explicit]
        public void Show() {
            var cfg = Configuration.Standard;
            var reader = cfg.GetReader(typeof(InstructionsWindowTests).GetMethod("Show"));
            using(var window = new InstructionsWindow(reader)) {
                window.Text = "Show";
                window.ShowDialog();
            }
        }
    }
}
#endif