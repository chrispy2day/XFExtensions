using System.Threading.Tasks;

namespace MetaMediaPlugin.Abstractions
{
    public interface IMediaService
    {
        bool IsCameraAvailable { get; }
        bool IsTakePhotoSupported { get; }
        bool IsPickPhotoSupported { get; }
        Task<IMediaFile> PickPhotoAsync();
        Task<IMediaFile> TakePhotoAsync();
    }
}