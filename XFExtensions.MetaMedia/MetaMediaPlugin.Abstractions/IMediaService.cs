using System.Threading.Tasks;

namespace MetaMediaPlugin.Abstractions
{
    public interface IMediaService
    {
        bool IsCameraAvailable { get; }
        bool IsTakePhotoSupported { get; }
        bool IsPickPhotoSupported { get; }
        string PhotosDirectory { get; set; } // this is only used in Android to specify the sub-directory in photos that your app uses
        Task<IMediaFile> PickPhotoAsync();
        Task<IMediaFile> TakePhotoAsync();
    }
}