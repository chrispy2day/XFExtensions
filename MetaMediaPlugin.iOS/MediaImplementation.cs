using System;
using MetaMediaPlugin.Abstractions;

namespace MetaMediaPlugin
{
    public class MediaImplementation : IMedia
    {
        public MediaImplementation()
        {
        }

        #region IMedia implementation

        public System.Threading.Tasks.Task<MediaFile> PickPhotoAsync()
        {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task<MediaFile> TakePhotoAsync()
        {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task<MediaFile> PickVideoAsync()
        {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task<MediaFile> TakeVideoAsync()
        {
            throw new NotImplementedException();
        }

        public bool IsCameraAvailable
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsTakePhotoSupported
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsPickPhotoSupported
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsTakeVideoSupported
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsPickVideoSupported
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}

