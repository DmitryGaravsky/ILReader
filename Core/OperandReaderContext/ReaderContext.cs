namespace ILReader.Context {
    using System.Linq;
    using System.Reflection;

    class OperandReaderContext : IOperandReaderContext {
        public OperandReaderContext(MethodBase method)
            : this(method, method.GetMethodBody()) {
        }
        public OperandReaderContext(MethodBase method, MethodBody methodBody)
            : this(method.Module, methodBody.LocalVariables.ToArray()) {
        }
        public OperandReaderContext(Module module, LocalVariableInfo[] variables) {
            this.moduleCore = module;
            this.variablesCore = variables;
        }
        readonly Module moduleCore;
        public Module Module {
            get { return moduleCore; }
        }
        readonly LocalVariableInfo[] variablesCore;
        public LocalVariableInfo[] Variables {
            get { return variablesCore; }
        }
    }
}