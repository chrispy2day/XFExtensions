using System;
using System.IO;

namespace MetaMediaPlugin.Abstractions
{
    /// <summary>
    /// Media file representations
    /// </summary>
    public sealed class MediaFile
      : IDisposable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path"></param>
        /// <param name="streamGetter"></param>
        /// <param name="deletePathOnDispose"></param>
        /// <param name="dispose"></param>
        public MediaFile(string name, string path, Func<Stream> streamGetter, bool deletePathOnDispose = false, Action<bool> dispose = null)
        {
            this.dispose = dispose;
            this.streamGetter = streamGetter;
            this.name = name;
            this.path = path;
            this.deletePathOnDispose = deletePathOnDispose;
        }
        /// <summary>
        /// Path to file
        /// </summary>
        public string Path
        {
            get
            {
                if (this.isDisposed)
                    throw new ObjectDisposedException(null);

                return this.path;
            }
        }
        /// <summary>
        /// The local file name on the system.
        /// </summary>
        public string FileName
        {
            get
            {
                if (this.isDisposed)
                    throw new ObjectDisposedException(null);
                return this.name;
            }
        }
        /// <summary>
        /// Get stream if available
        /// </summary>
        /// <returns></returns>
        public Stream GetStream()
        {
            if (this.isDisposed)
                throw new ObjectDisposedException(null);

            return this.streamGetter();
        }
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool isDisposed;
        private readonly Action<bool> dispose;
        private readonly Func<Stream> streamGetter;
        private readonly string path;
        private readonly bool deletePathOnDispose;
        private readonly string name;

        private void Dispose(bool disposing)
        {
            if (this.isDisposed)
                return;

            this.isDisposed = true;
            if (this.dispose != null)
                this.dispose(disposing);
        }
        /// <summary>
        /// 
        /// </summary>
        ~MediaFile()
        {
            Dispose(false);
        }
    }
}
