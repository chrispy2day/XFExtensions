using System.Threading.Tasks;

namespace MetaMediaPlugin.Abstractions
{
    public interface IMediaService
    {
        bool IsCameraAvailable { get; }
        bool IsTakePhotoSupported { get; }
        bool IsPickPhotoSupported { get; }

        /// <summary>
        /// Specify the photo directory to use for your app.  
        /// In iOS, this has no effect.
        /// In Android, this will be the name of the subdirectory within the shared photos directory.
        /// </summary>
        /// <value>The photos directory.</value>
        string PhotosDirectory { get; set; } // this is only used in Android to specify the sub-directory in photos that your app uses
        Task<IMediaFile> PickPhotoAsync();
        Task<IMediaFile> TakePhotoAsync();
    }
}