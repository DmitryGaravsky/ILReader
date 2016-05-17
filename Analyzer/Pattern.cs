namespace ILReader.Analyzer {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class MatchPattern<T> {
        readonly Func<T[], bool> matchFunc;
        protected MatchPattern(Func<T, Func<T, bool>> getMatch, params T[] elements)
            : this(elements.Select(e => getMatch(e)).ToArray()) {
        }
        protected MatchPattern(params Func<T, bool>[] matches) {
            matchFunc = (elements) =>
            {
                List<T> foundElements = new List<T>();
                List<int> foundIndexes = new List<int>();
                int start = 0;
                foreach(var match in matches) {
                    int index = Array.FindIndex(elements, start, element => match(element));
                    if(index == -1) 
                        return false;
                    if(!startIndex.HasValue)
                        startIndex = index;
                    start = index;
                    foundIndexes.Add(index);
                    foundElements.Add(elements[index]);
                }
                this.result = foundElements.ToArray();
                this.indexes = foundIndexes.ToArray();
                return result.Length > 0;
            };
        }
        protected bool Match(T[] elements) {
            Reset();
            return matchFunc(elements);
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
        public void Reset() {
            this.startIndex = null;
            this.result = null;
            this.indexes = null;
        }
    }
}