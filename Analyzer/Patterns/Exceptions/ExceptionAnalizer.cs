namespace ILReader.Analyzer {
    using System;
    using System.Reflection;

    static class ExceptionAnalizer {
        static readonly Type ExceptionType = typeof(System.Exception);
        internal static bool IsException(ConstructorInfo cInfo) {
            return (cInfo != null) && ExceptionType.IsAssignableFrom(cInfo.DeclaringType);
        }
        internal static bool IsException<TException>(ConstructorInfo cInfo)
            where TException : Exception {
            return (cInfo != null) && typeof(TException).IsAssignableFrom(cInfo.DeclaringType);
        }
    }
}