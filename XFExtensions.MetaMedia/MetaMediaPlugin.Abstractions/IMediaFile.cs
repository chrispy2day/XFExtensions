using System.IO;
using System.Threading.Tasks;

namespace MetaMediaPlugin.Abstractions
{
    public interface IMediaFile
    {
        string FileName { get; }
        string Path { get; }
        Task<Stream> GetFullFileStreamAsync();
        Task<Stream> GetPreviewFileStreamAsync(float width = 0, float height = 0);
    }
}