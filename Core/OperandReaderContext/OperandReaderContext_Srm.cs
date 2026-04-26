namespace ILReader.Context {
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Reflection.Metadata;
    using System.Reflection.Metadata.Ecma335;
    using System.Text;
    using ILReader.Readers;

    sealed class SrmOperandReaderContext : IOperandReaderContext {
        readonly SrmModuleContext module;
        readonly string name;
        readonly byte[] ilBytes;
        readonly int maxStackSize;
        readonly ImmutableArray<ExceptionRegion> exceptionRegions;
        readonly Dictionary<int, string> catchTypeNames = new();
        readonly ParameterSymbol[] parameters;
        readonly LocalVariableSymbol[] variables;
        int exceptionRegionIndex;
        public SrmOperandReaderContext(SrmModuleContext module, int metadataToken) {
            this.module = module ?? throw new ArgumentNullException(nameof(module));
            var entityHandle = MetadataTokens.EntityHandle(metadataToken);
            if(entityHandle.Kind != HandleKind.MethodDefinition) {
                throw new ArgumentException(
                    $"Token 0x{metadataToken:X8} is {entityHandle.Kind}, not MethodDefinition.",
                    nameof(metadataToken));
            }
            Initialize((MethodDefinitionHandle)entityHandle,
                out name, out ilBytes, out maxStackSize, out exceptionRegions,
                out parameters, out variables);
        }
        public SrmOperandReaderContext(SrmModuleContext module, string typeName, string methodName) {
            this.module = module ??
                throw new ArgumentNullException(nameof(module));
            var handle = FindMethod(module.MetadataReader, typeName, methodName);
            if(handle.IsNil)
                throw new MissingMethodException(typeName, methodName);
            Initialize(handle,
                out name, out ilBytes, out maxStackSize, out exceptionRegions,
                out parameters, out variables);
            this.name = methodName;
        }
        public SrmOperandReaderContext(SrmModuleContext module, string typeName,
            string methodName, string signature) {
            this.module = module ?? throw new ArgumentNullException(nameof(module));
            var handle = FindMethodBySignature(module.MetadataReader, typeName, methodName, signature);
            if(handle.IsNil)
                throw new MissingMethodException(typeName, methodName);
            Initialize(handle,
                out name, out ilBytes, out maxStackSize, out exceptionRegions,
                out parameters, out variables);
            this.name = methodName;
        }
        void Initialize(MethodDefinitionHandle handle,
            out string outName, out byte[] outIL, out int outMaxStack,
            out ImmutableArray<ExceptionRegion> outRegions,
            out ParameterSymbol[] outParams, out LocalVariableSymbol[] outVars) {
            var reader = module.MetadataReader;
            var methodDef = reader.GetMethodDefinition(handle);
            outName = reader.GetString(methodDef.Name);
            var body = module.PEReader.GetMethodBody(methodDef.RelativeVirtualAddress);
            outMaxStack = body.MaxStack;
            outIL = body.GetILBytes() ?? Array.Empty<byte>();
            outRegions = body.ExceptionRegions;
            foreach(var region in outRegions) {
                if(region.Kind == ExceptionRegionKind.Catch && !region.CatchType.IsNil)
                    catchTypeNames[region.HandlerOffset] = GetCatchTypeName(reader, region.CatchType);
            }
            var methodSig = methodDef.DecodeSignature(TypeNameProvider.Instance, default(Unit));
            outParams = BuildParameters(reader, methodDef.GetParameters(), methodSig.ParameterTypes);
            outVars = body.LocalSignature.IsNil
                ? Array.Empty<LocalVariableSymbol>()
                : BuildLocals(reader, body.LocalSignature);
        }
        static ParameterSymbol[] BuildParameters(MetadataReader reader,
            ParameterHandleCollection paramHandles, ImmutableArray<string> paramTypes) {
            var list = new List<ParameterSymbol>(paramTypes.Length);
            int sigIdx = 0;
            foreach(var ph in paramHandles) {
                var p = reader.GetParameter(ph);
                if(p.SequenceNumber == 0)
                    continue;  // return-value entry
                string typeName = sigIdx < paramTypes.Length ? paramTypes[sigIdx] : "System.Object";
                list.Add(new ParameterSymbol(p.SequenceNumber - 1, reader.GetString(p.Name), typeName));
                sigIdx++;
            }
            for(int i = list.Count; i < paramTypes.Length; i++)
                list.Add(new ParameterSymbol(i, string.Empty, paramTypes[i]));
            return list.ToArray();
        }
        static LocalVariableSymbol[] BuildLocals(MetadataReader reader,
            StandaloneSignatureHandle sigHandle) {
            var localSig = reader.GetStandaloneSignature(sigHandle);
            var types = localSig.DecodeLocalSignature(TypeNameProvider.Instance, default(Unit));
            var result = new LocalVariableSymbol[types.Length];
            for(int i = 0; i < types.Length; i++)
                result[i] = new LocalVariableSymbol(i, types[i]);
            return result;
        }
        public OperandReaderContextType Type => OperandReaderContextType.Method;
        public string Name => name;
        public byte[] GetIL() => ilBytes;
        public object This => null;
        public object this[byte index, bool argument = false] {
            get {
                if(argument)
                    return index < parameters.Length ? (object)parameters[index] : null;
                return index < variables.Length ? (object)variables[index] : null;
            }
        }
        public object this[short index, bool argument = false] {
            get {
                if(argument)
                    return index >= 0 && index < parameters.Length ? (object)parameters[index] : null;
                return index >= 0 && index < variables.Length ? (object)variables[index] : null;
            }
        }
        public IMetadataSymbol ResolveMethod(int token)
            => ResolveSymbol(module.MetadataReader, token);
        public IMetadataSymbol ResolveField(int token)
            => ResolveSymbol(module.MetadataReader, token);
        public IMetadataSymbol ResolveType(int token)
            => ResolveSymbol(module.MetadataReader, token);
        public IMetadataSymbol ResolveMember(int token)
            => ResolveSymbol(module.MetadataReader, token);
        public string ResolveString(int token) {
            return module.MetadataReader.GetUserString(MetadataTokens.UserStringHandle(token));
        }
        public byte[] ResolveSignature(int token) {
            var handle = MetadataTokens.StandaloneSignatureHandle(token);
            var sig = module.MetadataReader.GetStandaloneSignature(handle);
            return module.MetadataReader.GetBlobBytes(sig.Signature);
        }
        public bool ResolveExceptionHandler(Func<int, IInstruction> getInstruction,
            out ExceptionHandler handler) {
            handler = null;
            if(exceptionRegionIndex >= exceptionRegions.Length)
                return false;
            var region = exceptionRegions[exceptionRegionIndex++];
            catchTypeNames.TryGetValue(region.HandlerOffset, out string catchType);
            var type = (ExceptionHandlerType)(int)region.Kind;
            var offsets = new int[] {
                region.TryOffset,
                region.TryOffset + region.TryLength,
                region.FilterOffset,
                region.HandlerOffset,
                region.HandlerOffset + region.HandlerLength
            };
            handler = new ExceptionHandler(getInstruction, type, catchType, offsets);
            return true;
        }
        public IEnumerable<IMetadataItem> GetMetadata() {
            yield return new MetadataItem(name, null);
            yield return new MetadataItem(".codeSize", ilBytes.Length);
            yield return new MetadataItem(".maxStack", maxStackSize);
        }
        public void Dump(System.IO.Stream stream) {
            /* SRM contexts do not support binary dump */
        }
        static IMetadataSymbol ResolveSymbol(MetadataReader reader, int token) {
            var handle = MetadataTokens.EntityHandle(token);
            return handle.Kind switch {
                HandleKind.MethodDefinition => ResolveMethodDef(reader, (MethodDefinitionHandle)handle),
                HandleKind.MemberReference => ResolveMemberRef(reader, (MemberReferenceHandle)handle),
                HandleKind.MethodSpecification => ResolveMethodSpec(reader, (MethodSpecificationHandle)handle),
                HandleKind.FieldDefinition => ResolveFieldDef(reader, (FieldDefinitionHandle)handle),
                HandleKind.TypeDefinition => ResolveTypeDef(reader, (TypeDefinitionHandle)handle),
                HandleKind.TypeReference => ResolveTypeRef(reader, (TypeReferenceHandle)handle),
                HandleKind.TypeSpecification => ResolveTypeSpec(reader, (TypeSpecificationHandle)handle),
                _ => new SrmSymbol_Signature(handle)
            };
        }
        static IMetadataSymbol ResolveMethodDef(MetadataReader reader, MethodDefinitionHandle handle) {
            var methodDef = reader.GetMethodDefinition(handle);
            string mName = reader.GetString(methodDef.Name);
            var sig = methodDef.DecodeSignature(TypeNameProvider.Instance, default(Unit));
            var parms = BuildParametersFromTypes(sig.ParameterTypes);
            var (typeName, asmName, isNested, enclosing) = ResolveTypeDefInfo(reader, methodDef.GetDeclaringType());
            string fullName = BuildMethodFullName(sig.ReturnType, mName, sig.ParameterTypes);
            return new SrmSymbol_Method(mName, fullName, typeName, asmName,
                sig.ReturnType, parms, isNested, enclosing, handle);
        }
        static IMetadataSymbol ResolveMemberRef(MetadataReader reader, MemberReferenceHandle handle) {
            var memberRef = reader.GetMemberReference(handle);
            string mName = reader.GetString(memberRef.Name);
            var (typeName, asmName) = ResolveParentTypeName(reader, memberRef.Parent);
            if(memberRef.GetKind() == MemberReferenceKind.Method) {
                var sig = memberRef.DecodeMethodSignature(TypeNameProvider.Instance, default(Unit));
                var parms = BuildParametersFromTypes(sig.ParameterTypes);
                string full = BuildMethodFullName(sig.ReturnType, mName, sig.ParameterTypes);
                return new SrmSymbol_Method(mName, full, typeName, asmName,
                    sig.ReturnType, parms, false, Array.Empty<string>(), handle);
            }
            else {
                var fieldTypeName = memberRef.DecodeFieldSignature(TypeNameProvider.Instance, default(Unit));
                string full = $"{fieldTypeName} {mName}";
                return new SrmSymbol_Field(mName, full, fieldTypeName, typeName, asmName, handle);
            }
        }
        static IMetadataSymbol ResolveMethodSpec(MetadataReader reader, MethodSpecificationHandle handle) {
            var spec = reader.GetMethodSpecification(handle);
            return spec.Method.Kind switch {
                HandleKind.MethodDefinition => ResolveMethodDef(reader, (MethodDefinitionHandle)spec.Method),
                HandleKind.MemberReference => ResolveMemberRef(reader, (MemberReferenceHandle)spec.Method),
                _ => new SrmSymbol_Signature(handle)
            };
        }
        static IMetadataSymbol ResolveFieldDef(MetadataReader reader, FieldDefinitionHandle handle) {
            var fieldDef = reader.GetFieldDefinition(handle);
            string fName = reader.GetString(fieldDef.Name);
            string fieldType = fieldDef.DecodeSignature(TypeNameProvider.Instance, default(Unit));
            var (typeName, asmName, _, _) = ResolveTypeDefInfo(reader, fieldDef.GetDeclaringType());
            string full = $"{fieldType} {fName}";
            return new SrmSymbol_Field(fName, full, fieldType, typeName, asmName, handle);
        }
        static IMetadataSymbol ResolveTypeDef(MetadataReader reader, TypeDefinitionHandle handle) {
            var (typeName, asmName, isNested, enclosing) = ResolveTypeDefInfo(reader, handle);
            int dot = typeName.LastIndexOf('.');
            string sn = dot >= 0 ? typeName.Substring(dot + 1) : typeName;
            return new SrmSymbol_Type(sn, typeName, asmName, isNested, enclosing, handle);
        }
        static IMetadataSymbol ResolveTypeRef(MetadataReader reader, TypeReferenceHandle handle) {
            var typeRef = reader.GetTypeReference(handle);
            string ns = reader.GetString(typeRef.Namespace);
            string name = reader.GetString(typeRef.Name);
            string full = string.IsNullOrEmpty(ns) ? name : ns + "." + name;
            string asm = ResolveTypeRefAssemblyName(reader, handle);
            return new SrmSymbol_Type(name, full, asm, false, Array.Empty<string>(), handle);
        }
        static IMetadataSymbol ResolveTypeSpec(MetadataReader reader, TypeSpecificationHandle handle) {
            string typeName = reader.GetTypeSpecification(handle)
                                    .DecodeSignature(TypeNameProvider.Instance, default(Unit));
            return new SrmSymbol_Type(typeName, typeName, string.Empty, false, Array.Empty<string>(), handle);
        }
        // Returns (fullTypeName, assemblyName, isNested, enclosingTypeNames[])
        static (string typeName, string asmName, bool isNested, string[] enclosing)
            ResolveTypeDefInfo(MetadataReader reader, TypeDefinitionHandle handle) {
            if(handle.IsNil)
                return (string.Empty, string.Empty, false, Array.Empty<string>());
            var typeDef = reader.GetTypeDefinition(handle);
            string ns = reader.GetString(typeDef.Namespace);
            string name = reader.GetString(typeDef.Name);
            string simple = string.IsNullOrEmpty(ns) ? name : ns + "." + name;
            bool isNested = typeDef.IsNested;
            string asm = GetSelfAssemblyName(reader);
            if(!isNested)
                return (simple, asm, false, Array.Empty<string>());
            // Build "Outer+Middle+Inner" chain for nested types (matches Type.FullName format)
            var chain = new List<string>();
            var parent = typeDef.GetDeclaringType();
            while(!parent.IsNil) {
                var pd = reader.GetTypeDefinition(parent);
                string pns = reader.GetString(pd.Namespace);
                string pn = reader.GetString(pd.Name);
                chain.Add(string.IsNullOrEmpty(pns) ? pn : pns + "." + pn);
                parent = pd.IsNested ? pd.GetDeclaringType() : default;
            }
            var sb = new StringBuilder();
            for(int i = chain.Count - 1; i >= 0; i--) {
                if(sb.Length > 0) sb.Append('+');
                sb.Append(chain[i]);
            }
            sb.Append('+').Append(name);
            return (sb.ToString(), asm, true, chain.ToArray());
        }
        static (string typeName, string asmName) ResolveParentTypeName(MetadataReader reader, EntityHandle parent) =>
            parent.Kind switch {
                HandleKind.TypeReference =>
                    (TypeNameProvider.Instance.GetTypeFromReference(reader, (TypeReferenceHandle)parent, 0),
                     ResolveTypeRefAssemblyName(reader, (TypeReferenceHandle)parent)),
                HandleKind.TypeDefinition =>
                    (TypeNameProvider.Instance.GetTypeFromDefinition(reader, (TypeDefinitionHandle)parent, 0),
                     GetSelfAssemblyName(reader)),
                HandleKind.TypeSpecification =>
                    (reader.GetTypeSpecification((TypeSpecificationHandle)parent)
                           .DecodeSignature(TypeNameProvider.Instance, default(Unit)),
                     string.Empty),
                _ => ("?", "?")
            };

        static string GetCatchTypeName(MetadataReader reader, EntityHandle catchType) =>
            catchType.Kind switch {
                HandleKind.TypeDefinition =>
                    TypeNameProvider.Instance.GetTypeFromDefinition(reader, (TypeDefinitionHandle)catchType, 0),
                HandleKind.TypeReference =>
                    TypeNameProvider.Instance.GetTypeFromReference(reader, (TypeReferenceHandle)catchType, 0),
                HandleKind.TypeSpecification =>
                    reader.GetTypeSpecification((TypeSpecificationHandle)catchType)
                          .DecodeSignature(TypeNameProvider.Instance, default(Unit)),
                _ => null
            };
        static string ResolveTypeRefAssemblyName(MetadataReader reader, TypeReferenceHandle handle) {
            var typeRef = reader.GetTypeReference(handle);
            if(typeRef.ResolutionScope.Kind == HandleKind.AssemblyReference) {
                var asmRef = reader.GetAssemblyReference((AssemblyReferenceHandle)typeRef.ResolutionScope);
                string name = reader.GetString(asmRef.Name);
                string culture = reader.GetString(asmRef.Culture);
                var pkBytes = reader.GetBlobBytes(asmRef.PublicKeyOrToken);
                string pkHex = BitConverter.ToString(pkBytes).Replace("-", string.Empty).ToLowerInvariant();
                return $"{name}, Version={asmRef.Version}, Culture={(string.IsNullOrEmpty(culture) ? "neutral" : culture)}, PublicKeyToken={pkHex}";
            }
            if(typeRef.ResolutionScope.Kind == HandleKind.TypeReference)
                return ResolveTypeRefAssemblyName(reader, (TypeReferenceHandle)typeRef.ResolutionScope);
            return string.Empty;
        }
        static string GetSelfAssemblyName(MetadataReader reader) {
            var assemDef = reader.GetAssemblyDefinition();
            string name = reader.GetString(assemDef.Name);
            string culture = reader.GetString(assemDef.Culture);
            return $"{name}, Version={assemDef.Version}, Culture={(string.IsNullOrEmpty(culture) ? "neutral" : culture)}, PublicKeyToken=null";
        }
        internal static MethodDefinitionHandle FindMethod(MetadataReader reader,
            string typeName, string methodName) {
            foreach(var typeHandle in reader.TypeDefinitions) {
                if(!TypeNameMatches(reader, typeHandle, typeName))
                    continue;
                foreach(var mHandle in reader.GetTypeDefinition(typeHandle).GetMethods()) {
                    if(reader.GetString(reader.GetMethodDefinition(mHandle).Name) == methodName)
                        return mHandle;
                }
            }
            return default;
        }
        internal static MethodDefinitionHandle FindMethodBySignature(MetadataReader reader,
            string typeName, string methodName, string signature) {
            foreach(var typeHandle in reader.TypeDefinitions) {
                if(!TypeNameMatches(reader, typeHandle, typeName))
                    continue;
                foreach(var mHandle in reader.GetTypeDefinition(typeHandle).GetMethods()) {
                    var md = reader.GetMethodDefinition(mHandle);
                    if(reader.GetString(md.Name) != methodName)
                        continue;
                    var sig = md.DecodeSignature(TypeNameProvider.Instance, default(Unit));
                    string full = BuildMethodFullName(sig.ReturnType, methodName, sig.ParameterTypes);
                    if(full == signature)
                        return mHandle;
                }
            }
            return default;
        }
        internal static IEnumerable<IMethodSymbol> FindMethods(MetadataReader reader,
            string typeName, string methodName) {
            var results = new List<IMethodSymbol>();
            foreach(var typeHandle in reader.TypeDefinitions) {
                if(!TypeNameMatches(reader, typeHandle, typeName))
                    continue;
                foreach(var mHandle in reader.GetTypeDefinition(typeHandle).GetMethods()) {
                    var md = reader.GetMethodDefinition(mHandle);
                    if(reader.GetString(md.Name) != methodName)
                        continue;
                    results.Add((IMethodSymbol)ResolveMethodDef(reader, mHandle));
                }
            }
            return results;
        }
        static bool TypeNameMatches(MetadataReader reader, TypeDefinitionHandle handle, string typeName) {
            var typeDef = reader.GetTypeDefinition(handle);
            string ns = reader.GetString(typeDef.Namespace);
            string tn = reader.GetString(typeDef.Name);
            string full = string.IsNullOrEmpty(ns) ? tn : ns + "." + tn;
            if(full == typeName)
                return true;
            // Handle nested types: "Outer+Middle+Inner" format (matches Type.FullName)
            if(typeDef.IsNested) {
                var (nestedFull, _, _, _) = ResolveTypeDefInfo(reader, handle);
                return nestedFull == typeName;
            }
            return false;
        }
        static string BuildMethodFullName(string returnType, string name, ImmutableArray<string> paramTypes) {
            var sb = new StringBuilder(64);
            sb.Append(returnType).Append(' ').Append(name).Append('(');
            for(int i = 0; i < paramTypes.Length; i++) {
                if(i > 0) sb.Append(", ");
                sb.Append(paramTypes[i]);
            }
            sb.Append(')');
            return sb.ToString();
        }
        static ParameterSymbol[] BuildParametersFromTypes(ImmutableArray<string> paramTypes) {
            var result = new ParameterSymbol[paramTypes.Length];
            for(int i = 0; i < paramTypes.Length; i++)
                result[i] = new ParameterSymbol(i, string.Empty, paramTypes[i]);
            return result;
        }
    }
}