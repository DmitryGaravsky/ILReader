namespace ILReader.Context {
    using System.Reflection;

    class OperandReaderContext : IOperandReaderContext {
        public OperandReaderContext(Module module, LocalVariableInfo[] variables) {
            this.moduleCore = module;
            this.variablesCore = variables;
        }
        Module moduleCore;
        public Module Module {
            get { return moduleCore; }
        }
        LocalVariableInfo[] variablesCore;
        public LocalVariableInfo[] Variables {
            get { return variablesCore; }
        }
    }
}