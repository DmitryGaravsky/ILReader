namespace ILReader {
    using System;

    sealed class LazyRef<T> 
        where T : class {
        readonly Func<T> create;
        public LazyRef(Func<T> create) {
            this.create = create;
        }
        T value;
        public T Value {
            get { return value ?? (value = create()); }
        }
    }
}