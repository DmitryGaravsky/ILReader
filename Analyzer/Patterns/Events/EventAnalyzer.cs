namespace ILReader.Analyzer {
    using System;
    using System.Reflection;

    static class EventAnalyzer {
        static readonly Type DelegateType = typeof(Delegate);
        internal static bool IsDelegate(ConstructorInfo cInfo) {
            return (cInfo != null) && DelegateType.IsAssignableFrom(cInfo.DeclaringType);
        }
        internal static bool IsAddEvent(MethodInfo mInfo) {
            return (mInfo != null) && mInfo.IsSpecialName && mInfo.Name.StartsWith("add_");
        }
        internal static bool IsRemoveEvent(MethodInfo mInfo) {
            return (mInfo != null) && mInfo.IsSpecialName && mInfo.Name.StartsWith("remove_");
        }
        internal static bool IsRaiseEvent(MethodInfo mInfo) {
            return (mInfo != null) && mInfo.IsSpecialName && mInfo.Name.StartsWith("raise_");
        }
    }
}