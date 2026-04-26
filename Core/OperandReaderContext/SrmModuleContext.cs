namespace ILReader.Context {
    using System;
    using System.IO;
    using System.Reflection.Metadata;
    using System.Reflection.PortableExecutable;
    // Manages the lifetime of PEReader, MetadataReader, and the source stream.
    // SrmOperandReaderContext borrows (does not own) an instance of this class.
    // SrmConfiguration creates, owns, and disposes one instance per ForAssembly() call.
    internal sealed class SrmModuleContext : IDisposable {
        readonly PEReader peReader;
        readonly Stream stream;
        bool disposed;
        public SrmModuleContext(Stream peStream) {
            if(peStream == null)
                throw new ArgumentNullException(nameof(peStream));
            stream = peStream;
            // PrefetchEntireImage copies the full image into a managed buffer immediately.
            // LeaveOpen prevents PEReader from closing the stream so we control its lifetime.
            peReader = new PEReader(peStream, PEStreamOptions.PrefetchEntireImage | PEStreamOptions.LeaveOpen);
            MetadataReader = peReader.GetMetadataReader();
        }
        public PEReader PEReader => peReader;
        public MetadataReader MetadataReader { get; }
        public void Dispose() {
            if(disposed)
                return;
            disposed = true;
            peReader.Dispose();
            stream.Dispose();
        }
    }
}