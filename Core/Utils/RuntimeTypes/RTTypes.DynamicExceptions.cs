using System;
using System.Collections.Generic;
using System.Reflection;
using ILReader.Readers;
using BF = System.Reflection.BindingFlags;

namespace ILReader {
    // __ExceptionInfo (internal CLR type) stores one try-block with all its handler clauses.
    // The fields are read via standard FieldInfo reflection so that a null DynamicResolverType
    // (or missing fields on a future runtime) degrades gracefully to "no exception handlers"
    // rather than throwing at class-initialisation time.
    static partial class RTTypes {
        internal readonly record struct DynamicExceptionClause(
            ExceptionHandlerType HandlerType,
            int TryStart,
            int TryEnd,
            int FilterStart,    // valid only when HandlerType == Filter
            int HandlerStart,
            int HandlerEnd,
            Type CatchType);    // non-null only for Catch handlers
        // ── __ExceptionInfo field accessors ─────────────────────────────────────────
        const BF NonPublicInstance = BF.Instance | BF.NonPublic;
        static readonly FieldInfo fi_m_exceptions = DynamicResolverType?.GetField("m_exceptions", NonPublicInstance);
        static readonly Type EIType = fi_m_exceptions?.FieldType.GetElementType();
        static readonly FieldInfo fi_ei_startAddr = EIType?.GetField("m_startAddr", NonPublicInstance);
        static readonly FieldInfo fi_ei_endAddr = EIType?.GetField("m_endAddr", NonPublicInstance);
        static readonly FieldInfo fi_ei_filterAddr = EIType?.GetField("m_filterAddr", NonPublicInstance);
        static readonly FieldInfo fi_ei_catchAddr = EIType?.GetField("m_catchAddr", NonPublicInstance);
        static readonly FieldInfo fi_ei_catchEndAddr = EIType?.GetField("m_catchEndAddr", NonPublicInstance);
        static readonly FieldInfo fi_ei_type = EIType?.GetField("m_type", NonPublicInstance);
        static readonly FieldInfo fi_ei_catchClass = EIType?.GetField("m_catchClass", NonPublicInstance);
        static readonly FieldInfo fi_ei_currentCatch = EIType?.GetField("m_currentCatch", NonPublicInstance);
        //
        internal static IEnumerable<DynamicExceptionClause> GetDynamicExceptionClauses(object resolver) {
            if(fi_m_exceptions is null || resolver is null)
                return null;
            var exceptions = fi_m_exceptions.GetValue(resolver) as Array;
            if(exceptions is null || exceptions.Length == 0)
                return null;
            return EnumerateDynamicExceptionClauses(exceptions);
        }
        static IEnumerable<DynamicExceptionClause> EnumerateDynamicExceptionClauses(Array exceptions) {
            foreach(var exInfo in exceptions) {
                int currentCatch = (int)fi_ei_currentCatch.GetValue(exInfo);
                int startAddr = (int)fi_ei_startAddr.GetValue(exInfo);
                int endAddr = (int)fi_ei_endAddr.GetValue(exInfo);
                int[] filterAddr = (int[])fi_ei_filterAddr.GetValue(exInfo);
                int[] catchAddr = (int[])fi_ei_catchAddr.GetValue(exInfo);
                int[] catchEndAddr = (int[])fi_ei_catchEndAddr.GetValue(exInfo);
                int[] types = (int[])fi_ei_type.GetValue(exInfo);
                Type[] catchClass = (Type[])fi_ei_catchClass.GetValue(exInfo);
                for(int i = 0; i < currentCatch; i++) {
                    // filterAddr contains unresolved label sentinels for non-Filter handlers — only use for Filter type
                    int filterStart = types[i] == (int)ExceptionHandlerType.Filter
                        ? filterAddr[i] : 0;
                    yield return new DynamicExceptionClause(
                        (ExceptionHandlerType)types[i],
                        startAddr, endAddr, filterStart,
                        catchAddr[i], catchEndAddr[i],
                        catchClass[i]);
                }
            }
        }
    }
}