namespace ILReader.Monads {
    static class @MayBe {
        /// <summary>@Monad(T): If not null</summary>
        [System.Diagnostics.DebuggerStepThrough]
        internal static TResult @Get<T, TResult>(this T @this, System.Func<T, TResult> @get, TResult defaultValue = default(TResult)) {
            return (@this != null && @get != null) ? @get(@this) : defaultValue;
        }
        /// <summary>@Monad(T): If not null</summary>
        [System.Diagnostics.DebuggerStepThrough]
        internal static T @Do<T>(this T @this, System.Action<T> @do) {
            if(@this != null && @do != null) @do(@this);
            return @this;
        }
    }
}