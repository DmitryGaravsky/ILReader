namespace ILReader.Core.Tests {
    using System.Reflection;
    using ILReader.Readers;
    using NUnit.Framework;

    [TestFixture]
    public class MetadataSymbol_Tests {
        static readonly MethodInfo toString =
            typeof(string).GetMethod("ToString", System.Type.EmptyTypes);
        static readonly FieldInfo stringEmpty =
            typeof(string).GetField("Empty");
        [Test]
        public void ReflectionSymbol_Method_Kind_And_Name() {
            var sym = ReflectionSymbol.FromMethod(toString);
            Assert.AreEqual(MetadataSymbolKind.Method, sym.Kind);
            Assert.AreEqual("ToString", sym.Name);
        }
        [Test]
        public void ReflectionSymbol_Method_DeclaringType() {
            var sym = ReflectionSymbol.FromMethod(toString);
            Assert.AreEqual("System.String", sym.DeclaringType);
        }
        [Test]
        public void ReflectionSymbol_GetSource_Returns_Original() {
            var sym = ReflectionSymbol.FromMethod(toString);
            Assert.AreSame(toString, sym.GetSource<MethodBase>());
        }
        [Test]
        public void ReflectionSymbol_GetSource_WrongType_Throws() {
            var sym = ReflectionSymbol.FromMethod(toString);
            Assert.Throws<System.InvalidCastException>(() => sym.GetSource<FieldInfo>());
        }
        [Test]
        public void ReflectionSymbol_TryGetSource_CorrectType_True() {
            var sym = ReflectionSymbol.FromMethod(toString);
            Assert.IsTrue(sym.TryGetSource<MethodBase>(out var result));
            Assert.AreSame(toString, result);
        }
        [Test]
        public void ReflectionSymbol_TryGetSource_WrongType_False() {
            var sym = ReflectionSymbol.FromMethod(toString);
            Assert.IsFalse(sym.TryGetSource<FieldInfo>(out _));
        }
        [Test]
        public void ReflectionSymbol_ToString_Delegates_To_Source() {
            var sym = ReflectionSymbol.FromMethod(toString);
            Assert.AreEqual(toString.ToString(), sym.ToString());
        }
        [Test]
        public void ReflectionSymbol_Field_Kind_And_Name() {
            var sym = ReflectionSymbol.FromField(stringEmpty);
            Assert.AreEqual(MetadataSymbolKind.Field, sym.Kind);
            Assert.AreEqual("Empty", sym.Name);
        }
        [Test]
        public void ReflectionSymbol_Type_DeclaringType_IsNull() {
            var sym = ReflectionSymbol.FromType(typeof(string));
            Assert.AreEqual(MetadataSymbolKind.Type, sym.Kind);
            Assert.IsNull(sym.DeclaringType);
        }
        [Test]
        public void StringSymbol_Kind_And_Name() {
            var sym = new StringSymbol(MetadataSymbolKind.Method, "void Foo.Bar()");
            Assert.AreEqual(MetadataSymbolKind.Method, sym.Kind);
            Assert.AreEqual("void Foo.Bar()", sym.Name);
        }
        [Test]
        public void StringSymbol_GetSource_Returns_String() {
            var sym = new StringSymbol(MetadataSymbolKind.Field, "int Foo.x");
            Assert.AreEqual("int Foo.x", sym.GetSource<string>());
        }
        [Test]
        public void StringSymbol_DeclaringType_IsNull() {
            var sym = new StringSymbol(MetadataSymbolKind.Type, "Foo");
            Assert.IsNull(sym.DeclaringType);
        }
        [Test]
        public void StringSymbol_ToString_Returns_Value() {
            var sym = new StringSymbol(MetadataSymbolKind.Member, "Foo.Bar");
            Assert.AreEqual("Foo.Bar", sym.ToString());
        }
        [Test]
        public void ReflectionSymbol_Method_FullName_NotEmpty() {
            var sym = ReflectionSymbol.FromMethod(toString);
            Assert.IsNotEmpty(sym.FullName);
        }
        [Test]
        public void ReflectionSymbol_Method_AssemblyName_NotEmpty() {
            var sym = ReflectionSymbol.FromMethod(toString);
            Assert.IsNotEmpty(sym.AssemblyName);
        }
        [Test]
        public void ReflectionSymbol_Method_IsIMethodSymbol() {
            var sym = ReflectionSymbol.FromMethod(toString);
            Assert.IsInstanceOf<IMethodSymbol>(sym);
        }
        [Test]
        public void ReflectionSymbol_Field_IsIFieldSymbol() {
            var sym = ReflectionSymbol.FromField(stringEmpty);
            Assert.IsInstanceOf<IFieldSymbol>(sym);
        }
        [Test]
        public void IMethodSymbol_ReturnTypeName_ToString_IsString() {
            var sym = (IMethodSymbol)ReflectionSymbol.FromMethod(toString);
            Assert.AreEqual("System.String", sym.ReturnTypeName);
        }
        [Test]
        public void IMethodSymbol_Parameters_Empty_ForNoArgMethod() {
            var sym = (IMethodSymbol)ReflectionSymbol.FromMethod(toString);
            Assert.IsEmpty(sym.Parameters);
        }
        [Test]
        public void IFieldSymbol_FieldTypeName_IsString() {
            var sym = (IFieldSymbol)ReflectionSymbol.FromField(stringEmpty);
            Assert.AreEqual("System.String", sym.FieldTypeName);
        }
        [Test]
        public void ParameterSymbol_Record_Equality() {
            var a = new ParameterSymbol(0, "x", "System.Int32");
            var b = new ParameterSymbol(0, "x", "System.Int32");
            Assert.AreEqual(a, b);
        }
        [Test]
        public void LocalVariableSymbol_Record_Equality() {
            var a = new LocalVariableSymbol(0, "System.Int32");
            var b = new LocalVariableSymbol(0, "System.Int32");
            Assert.AreEqual(a, b);
        }
        static readonly System.Type stringType = typeof(string);
        static readonly System.Type innerType  =
            typeof(System.Collections.Generic.List<>).GetNestedType("Enumerator");
        [Test]
        public void ReflectionSymbol_Type_IsITypeSymbol() {
            var sym = ReflectionSymbol.FromType(stringType);
            Assert.IsInstanceOf<ITypeSymbol>(sym);
        }
        [Test]
        public void ReflectionSymbol_Type_NonNested_IsNested_False() {
            var sym = (ITypeSymbol)ReflectionSymbol.FromType(stringType);
            Assert.IsFalse(sym.IsNested);
        }
        [Test]
        public void ReflectionSymbol_Type_NonNested_EnclosingTypeNames_Empty() {
            var sym = (ITypeSymbol)ReflectionSymbol.FromType(stringType);
            Assert.IsEmpty(sym.EnclosingTypeNames);
        }
        [Test]
        public void ReflectionSymbol_Type_Nested_IsNested_True() {
            var sym = (ITypeSymbol)ReflectionSymbol.FromType(innerType);
            Assert.IsTrue(sym.IsNested);
        }
        [Test]
        public void ReflectionSymbol_Type_Nested_EnclosingTypeNames_ContainsParent() {
            var sym = (ITypeSymbol)ReflectionSymbol.FromType(innerType);
            Assert.IsNotEmpty(sym.EnclosingTypeNames);
            Assert.IsTrue(sym.EnclosingTypeNames[0].Contains("List"),
                $"Expected 'List' in EnclosingTypeNames[0] but got: {sym.EnclosingTypeNames[0]}");
        }
    }
}