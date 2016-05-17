namespace ILReader.Readers {
    using System.Collections.Generic;

    class MetadataItem : Readers.IMetadataItem {
        internal MetadataItem(string name, object value) {
            this.name = name;
            this.value = value;
        }
        readonly string name;
        public string Name {
            get { return name; }
        }
        readonly object value;
        public object Value {
            get { return value; }
        }
        //
        public bool HasChildren {
            get { return value is IEnumerable<IMetadataItem>; }
        }
        public IEnumerable<IMetadataItem> Children {
            get { return value as IEnumerable<IMetadataItem>; }
        }
    }
}