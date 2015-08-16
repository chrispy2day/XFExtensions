using System;
using System.IO;
using System.Threading.Tasks;

namespace MetaMediaPlugin.Abstractions
{
    public class MediaFile : IDisposable
    {
        private readonly string _name;
        private readonly string _path;
        private readonly Func<Stream> _previewStreamGetter;
        private readonly Func<Stream> _fullStreamGetter;
        private readonly Action<bool> _dispose;

        public string FileName { get { return _name; } }
        public string FilePath { get { return _path; } }
        public Stream GetPreviewStream()
        {
            if (isDisposed)
                throw new ObjectDisposedException(null);
            return _previewStreamGetter();
        }
        public Stream GetFullStream()
        {
            if (isDisposed)
                throw new ObjectDisposedException(null);
            return _fullStreamGetter();
        }

        public MediaFile(string name, 
                         string path, 
                         Func<Stream> getPreviewStream, 
                         Func<Stream> getFullStream, 
                         Action<bool> dispose = null)
        {
            _name = name;
            _path = path;
            _previewStreamGetter = getPreviewStream;
            _fullStreamGetter = getFullStream;
            _dispose = dispose;
        }

        #region IDisposable implementation

        private bool isDisposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (isDisposed)
                return;

            isDisposed = true;
            if (_dispose != null)
                _dispose(disposing);
        }

        ~MediaFile()
        {
            Dispose(false);
        }

        #endregion

    }
}
