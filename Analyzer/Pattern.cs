namespace ILReader.Analyzer {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class MatchPattern<T> {
        protected readonly Func<int, T[], bool> matchFunc;
        protected MatchPattern(Func<T, Func<T, bool>> getMatch, params T[] elements)
            : this(elements.Select(e => getMatch(e)).ToArray()) {
        }
        protected MatchPattern(params Func<T, bool>[] matches) {
            matchFunc = (start, elements) => {
                List<T> foundElements = null; List<int> foundIndexes = null;
                if(start >= elements.Length)
                    start = 0;
                foreach(var match in matches) {
                    int index = Array.FindIndex(elements, start, element => match(element));
                    if(index == -1)
                        return false;
                    start = OnMatch(elements[index], index, ref foundElements, ref foundIndexes);
                }
                this.result = foundElements.ToArray();
                this.indexes = foundIndexes.ToArray();
                return (result.Length > 0);
            };
        }
        int OnMatch(T element, int index, ref List<T> foundElements, ref List<int> foundIndexes) {
            if(!startIndex.HasValue)
                startIndex = index;
            int start = index;
            if(foundElements == null) {
                foundElements = new List<T>();
                foundIndexes = new List<int>();
            }
            foundIndexes.Add(index);
            foundElements.Add(element);
            return start;
        }
        //
        int? startIndex;
        public int StartIndex {
            get { return startIndex.GetValueOrDefault(-1); }
        }
        readonly static T[] EmptyResult = new T[] { };
        T[] result;
        public T[] Result {
            get { return result ?? EmptyResult; }
        }
        readonly static int[] EmptyIndexes = new int[] { };
        int[] indexes;
        public int[] Indexes {
            get { return indexes ?? EmptyIndexes; }
        }
        public bool Success {
            get { return startIndex.HasValue && (result != null) && (indexes != null); }
        }
        public int Reset() {
            int start = startIndex.GetValueOrDefault(-1);
            ResetCore();
            return start;
        }
        protected void ResetCore() {
            this.startIndex = null;
            this.result = null;
            this.indexes = null;
        }
    }
}