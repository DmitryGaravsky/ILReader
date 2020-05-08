#if DEBUGTEST
namespace ILReader.Visualizer.Tests {
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;
    using Microsoft.VisualStudio.DebuggerVisualizers;
    using NUnit.Framework;
    using MA = System.Reflection.MethodAttributes;

    #region TestClasses
    class Foo {
        internal static int c = 4;
        public int Sum(int a, int b) {
            return a + b;
        }
    }
    public abstract class Bar {
        public abstract int Sum(int a, int b);
    }
    class FooBar {
        int val;
        public FooBar()
            : this(42) {
        }
        public FooBar(int value) {
            this.val = value;
        }
        public int Sum(int a, int b, int c, int d, int e) {
            return val + a + b + c + d + e;
        }
        public int Value {
            get { return val; }
        }
    }
    static class TestHelper {
        internal static void ShowVisualizer(object objectToVisualize) {
            VisualizerDevelopmentHost visualizerHost = new VisualizerDevelopmentHost(objectToVisualize,
                typeof(ILReader.DebuggerVisualizer.DebuggerSide), typeof(ILReader.DebuggerVisualizer.ILDumpObjectSource));
            visualizerHost.ShowVisualizer();
        }
    }
    #endregion TestClasses
    //
    [TestFixture]
    public class MethodVisualizer_Tests {
        [Test, Explicit]
        public void Show1() {
            object objectToVisualize = typeof(Foo).GetMethod("Sum");
            TestHelper.ShowVisualizer(objectToVisualize);
        }
        [Test, Explicit]
        public void Show2() {
            object objectToVisualize = typeof(InstructionsWindowTests).GetMethod("Show");
            TestHelper.ShowVisualizer(objectToVisualize);
        }
        [Test, Explicit]
        public void Show3() {
            object objectToVisualize = typeof(FooBar).GetMethod("Sum");
            TestHelper.ShowVisualizer(objectToVisualize);
        }
        [Test, Explicit]
        public void Show4() {
            object objectToVisualize = typeof(bool).GetMethod("GetType");
            TestHelper.ShowVisualizer(objectToVisualize);
        }
        [Test, Explicit]
        public void Show5() {
            DynamicMethod dm = new DynamicMethod("m", null, null);
            var dmGenType = dm.GetILGenerator().GetType();
            object objectToVisualize = dmGenType.GetMethod("BeginExceptFilterBlock");
            TestHelper.ShowVisualizer(objectToVisualize);
        }
        [Test, Explicit]
        public void Show6() {
            object objectToVisualize = typeof(FooBar).GetConstructor(Type.EmptyTypes);
            TestHelper.ShowVisualizer(objectToVisualize);
        }
        [Test, Explicit]
        public void Show7() {
            object objectToVisualize = typeof(FooBar).GetConstructor(new Type[] { typeof(int) });
            TestHelper.ShowVisualizer(objectToVisualize);
        }
    }
    [TestFixture]
    public class DelegateVisualizer_Tests {
        [Test, Explicit]
        public void Show1() {
            object objectToVisualize = CreateSum();
            TestHelper.ShowVisualizer(objectToVisualize);
        }
        [Test, Explicit]
        public void Show2() {
            object objectToVisualize = CreateSum().Method;
            TestHelper.ShowVisualizer(objectToVisualize);
        }
        Delegate CreateSum() { return new Func<Foo, int>(x => x.Sum(2, 2)); }
    }
    [TestFixture]
    public class DynamicMethodVisualizer_Tests {
        [Test, Explicit]
        public void Show1() {
            object objectToVisualize = CreateSum1();
            TestHelper.ShowVisualizer(objectToVisualize);
        }
        [Test, Explicit]
        public void Show2() {
            object objectToVisualize = CreateSum1()
                .CreateDelegate(typeof(Func<int, int, int>));
            TestHelper.ShowVisualizer(objectToVisualize);
        }
        [Test, Explicit]
        public void Show3() {
            object objectToVisualize = CreateSum2();
            TestHelper.ShowVisualizer(objectToVisualize);
        }
        [Test, Explicit]
        public void Show4() {
            object objectToVisualize = CreateGetter()
                .CreateDelegate(typeof(Func<object, object>));
            TestHelper.ShowVisualizer(objectToVisualize);
        }
        [Test, Explicit]
        public void Show5() {
            object objectToVisualize = CreateGetter();
            TestHelper.ShowVisualizer(objectToVisualize);
        }
        static DynamicMethod CreateSum1() {
            DynamicMethod m = new DynamicMethod("Sum", typeof(int), new Type[] { typeof(int), typeof(int) });
            var ilgen = m.GetILGenerator();
            ilgen.Emit(OpCodes.Ldarg_1);
            ilgen.Emit(OpCodes.Ldarg_2);
            ilgen.Emit(OpCodes.Add);
            ilgen.Emit(OpCodes.Ret);
            return m;
        }
        static DynamicMethod CreateSum2() {
            DynamicMethod m = new DynamicMethod("Sum", typeof(int), Type.EmptyTypes);
            var ilgen = m.GetILGenerator();
            var loc1 = ilgen.DeclareLocal(typeof(int));
            var loc2 = ilgen.DeclareLocal(typeof(int));
            ilgen.Emit(OpCodes.Nop);
            ilgen.Emit(OpCodes.Ldc_I4_S, 25);
            ilgen.Emit(OpCodes.Stloc_0, loc1);
            ilgen.Emit(OpCodes.Ldc_I4_S, 36);
            ilgen.Emit(OpCodes.Stloc_1, loc2);
            ilgen.Emit(OpCodes.Ldloc_0);
            ilgen.Emit(OpCodes.Ldloc_1);
            ilgen.Emit(OpCodes.Add);
            ilgen.Emit(OpCodes.Ret);
            return m;
        }
        static DynamicMethod CreateGetter() {
            var property = typeof(FooBar).GetProperty("Value");
            var getMethod = property.GetGetMethod();
            var m = new DynamicMethod(String.Empty, typeof(object), new Type[] { typeof(object) }, property.DeclaringType, true);
            var ilGen = m.GetILGenerator();
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Castclass, property.DeclaringType);
            ilGen.Emit(getMethod.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, getMethod);
            if(property.PropertyType.IsValueType)
                ilGen.Emit(OpCodes.Box, property.PropertyType);
            ilGen.Emit(OpCodes.Ret);
            return m;
        }
    }
    [TestFixture]
    public class ExpressionDynamicMethodVisualizer_Tests {
        [Test, Explicit]
        public void Show1() {
            object objectToVisualize = CreateSum1();
            TestHelper.ShowVisualizer(objectToVisualize);
        }
        [Test, Explicit]
        public void Show2() {
            object objectToVisualize = CreateSum2();
            TestHelper.ShowVisualizer(objectToVisualize);
        }
        static Func<int, int, int> CreateSum1() {
            var pa = Expression.Parameter(typeof(int), "a");
            var pb = Expression.Parameter(typeof(int), "b");
            return Expression.Lambda<Func<int, int, int>>(Expression.Add(pa, pb), pa, pb).Compile();
        }
        static Expression<Func<object, string>> fooExpr =
            x => "a" + ((Foo)x).Sum(2, Foo.c).ToString() + "b";
        static Func<object, string> CreateSum2() {
            return fooExpr.Compile();
        }
    }
    [TestFixture]
    public class MethodBuilderVisualizer_Tests {
        [Test, Explicit]
        public void Show() {
            object objectToVisualize = CreateSum();
            TestHelper.ShowVisualizer(objectToVisualize);
        }
        static object CreateSum() {
            var dynAsmName = new AssemblyName("Bar.Dynamic." + Guid.NewGuid().ToString());
            var asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(dynAsmName, AssemblyBuilderAccess.Run);
            var moduleBuilder = asmBuilder.DefineDynamicModule(dynAsmName.FullName);
            //
            Type type = typeof(Bar);
            var typeBuilder = moduleBuilder.DefineType("Darth" + type.Name, TypeAttributes.Public, type);
            var m = DoMethodOverride(typeBuilder, "Sum", (mb, ilgen) => {
                ilgen.Emit(OpCodes.Ldarg_0);
                ilgen.Emit(OpCodes.Ldarg_1);
                ilgen.Emit(OpCodes.Add);
                ilgen.Emit(OpCodes.Ret);
            });
            return typeBuilder.CreateType().GetMethod("Sum");
        }
        static object DoMethodOverride(TypeBuilder typebuilder, string methodName, Action<MethodBuilder, ILGenerator> gen) {
            var method = typebuilder.BaseType.GetMethod(methodName);
            var m = typebuilder.DefineMethod(method.Name,
                method.Attributes & ~MA.Abstract,
                method.CallingConvention, method.ReturnType,
                method.GetParameters().Select(p => p.ParameterType).ToArray());
            gen(m, m.GetILGenerator());
            typebuilder.DefineMethodOverride(m, method);
            return m;
        }
    }
}
#endif