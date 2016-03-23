#if DEBUGTEST
namespace ILReader.Visualizer.Tests {
    using System;
    using Microsoft.VisualStudio.DebuggerVisualizers;
    using NUnit.Framework;

    #region TestClasses
    class Foo {
        public int Sum(int a, int b) {
            return a + b;
        }
    }
    #endregion TestClasses
    [TestFixture]
    public class MethodVisualizer_Tests {
        object objectToVisualize = typeof(Foo).GetMethod("Sum");
        [Test, Explicit]
        public void Show() {
            VisualizerDevelopmentHost visualizerHost = new VisualizerDevelopmentHost(objectToVisualize,
                typeof(ILReader.MethodVisualizer.DebuggerSide));
            visualizerHost.ShowVisualizer();
        }
    }
    [TestFixture]
    public class DelegateVisualizer_Tests {
        object objectToVisualize = new Func<Foo, int>(x => x.Sum(2, 2));
        [Test, Explicit]
        public void Show() {
            VisualizerDevelopmentHost visualizerHost = new VisualizerDevelopmentHost(objectToVisualize,
                typeof(ILReader.DelegateVisualizer.DebuggerSide));
            visualizerHost.ShowVisualizer();
        }
    }
}
#endif