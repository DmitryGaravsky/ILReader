namespace ILReader.Readers {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    //
    abstract class ReflectionSymbol : IMetadataSymbol {
        protected ReflectionSymbol(
            MetadataSymbolKind kind, string name, string fullName,
            string declaringType, string assemblyName, object source) {
            Kind = kind;
            Name = name;
            FullName = fullName;
            DeclaringType = declaringType;
            AssemblyName = assemblyName;
            Source = source;
        }
        protected object Source { get; }
        public MetadataSymbolKind Kind { get; }
        public string Name { get; }
        public string FullName { get; }
        public string DeclaringType { get; }
        public string AssemblyName { get; }
        public TSource GetSource<TSource>() {
            if(Source is TSource s)
                return s;
            throw new InvalidCastException($"Source is {Source?.GetType().Name}, not {typeof(TSource).Name}");
        }
        public bool TryGetSource<TSource>(out TSource source) {
            if(Source is TSource s) {
                source = s;
                return true;
            }
            source = default;
            return false;
        }
        public override string ToString() {
            return Source?.ToString() ?? Name;
        }
        public static IMethodSymbol FromMethod(MethodBase m)
            => new ReflectionSymbol_Method(m);
        public static IFieldSymbol FromField(FieldInfo f)
            => new ReflectionSymbol_Field(f);
        public static ITypeSymbol FromType(Type t)
            => new ReflectionSymbol_Type(t);
        public static IMetadataSymbol FromMember(MemberInfo m) {
            if(m is MethodBase mb)
                return FromMethod(mb);
            if(m is FieldInfo fi)
                return FromField(fi);
            return new ReflectionSymbol_Member(m);
        }
        // Builds an innermost-first array of enclosing type full-names for a nested type.
        // "OuterVictim.InnerVictim" → ["ILReader.Tests.Outer", "ILReader.Tests.SrmEngine_Tests"]
        protected static string[] BuildEnclosingTypeNames(Type t) {
            if(t == null || !t.IsNested)
                return Array.Empty<string>();
            var chain = new List<string>();
            var parent = t.DeclaringType;
            while(parent != null) {
                chain.Add(parent.FullName ?? parent.Name);
                parent = parent.DeclaringType;
            }
            return chain.ToArray();
        }
    }
    sealed class ReflectionSymbol_Method : ReflectionSymbol, IMethodSymbol {
        public ReflectionSymbol_Method(MethodBase m)
            : base(MetadataSymbolKind.Method,
                   m.Name,
                   BuildFullName(m),
                   m.DeclaringType?.FullName ?? m.DeclaringType?.Name,
                   m.DeclaringType?.Assembly.FullName ?? string.Empty,
                   m) {
            ReturnTypeName = m is MethodInfo mi
                ? mi.ReturnType.FullName ?? mi.ReturnType.Name
                : "System.Void";
            Parameters = m.GetParameters()
                .Select((p, i) => new ParameterSymbol(i, p.Name ?? string.Empty,
                    p.ParameterType.FullName ?? p.ParameterType.Name))
                .ToArray();
            IsDeclaringTypeNested = m.DeclaringType?.IsNested ?? false;
            EnclosingTypeNames = BuildEnclosingTypeNames(m.DeclaringType);
        }
        public string ReturnTypeName { get; }
        public ParameterSymbol[] Parameters { get; }
        public bool IsDeclaringTypeNested { get; }
        public string[] EnclosingTypeNames { get; }
        //
        static string BuildFullName(MethodBase m) {
            string ret = m is MethodInfo mi
                ? (mi.ReturnType.FullName ?? mi.ReturnType.Name)
                : "System.Void";
            string @params = string.Join(", ", m.GetParameters()
                .Select(p => p.ParameterType.FullName ?? p.ParameterType.Name));
            return $"{ret} {m.Name}({@params})";
        }
    }
    sealed class ReflectionSymbol_Field : ReflectionSymbol, IFieldSymbol {
        public ReflectionSymbol_Field(FieldInfo f)
            : base(MetadataSymbolKind.Field,
                   f.Name,
                   $"{f.FieldType.FullName ?? f.FieldType.Name} {f.Name}",
                   f.DeclaringType?.FullName ?? f.DeclaringType?.Name,
                   f.DeclaringType?.Assembly.FullName ?? string.Empty,
                   f) {
            FieldTypeName = f.FieldType.FullName ?? f.FieldType.Name;
        }
        public string FieldTypeName { get; }
    }
    sealed class ReflectionSymbol_Type : ReflectionSymbol, ITypeSymbol {
        public ReflectionSymbol_Type(Type t)
            : base(MetadataSymbolKind.Type,
                   t.Name,
                   t.FullName ?? t.Name,
                   null,
                   t.Assembly.FullName ?? string.Empty,
                   t) {
            IsNested = t.IsNested;
            EnclosingTypeNames = BuildEnclosingTypeNames(t);
        }
        public bool IsNested { get; }
        public string[] EnclosingTypeNames { get; }
    }
    sealed class ReflectionSymbol_Member : ReflectionSymbol {
        public ReflectionSymbol_Member(MemberInfo m)
            : base(MetadataSymbolKind.Member,
                   m.Name,
                   m.ToString() ?? m.Name,
                   m.DeclaringType?.FullName ?? m.DeclaringType?.Name,
                   m.DeclaringType?.Assembly.FullName ?? string.Empty,
                   m) {
        }
    }
}