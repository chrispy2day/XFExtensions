using System;
using System.IO;

namespace MetaMediaPlugin.Abstractions
{
    public class MediaFile : IDisposable
    {
        public Stream MediaStream { get; set; }
        public string Name { get; set; }

        #region IDisposable implementation

        private bool isDisposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (this.isDisposed)
                return;

            if (disposing)
                MediaStream.Dispose();

            this.isDisposed = true;
        }

        ~MediaFile()
        {
            Dispose(false);
        }

        #endregion
    }
}
