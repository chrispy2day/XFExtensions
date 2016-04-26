using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Foundation;
using Photos;
using UIKit;
using AssetsLibrary;
using System.Runtime.InteropServices;
using MetaMediaPlugin.Abstractions;

namespace MetaMediaPlugin
{
    public class MediaFile : IMediaFile
    {
        private nfloat _deviceWidth;
        private nfloat _deviceHeight;
        private string _path;

        public MediaFile()
        {
            var screenSize = UIScreen.MainScreen.Bounds; // this is size in points, not pixels
            var density = UIScreen.MainScreen.Scale; // this is the density of pixels to points
            _deviceWidth = screenSize.Width * density; // in true pixels
            _deviceHeight = screenSize.Height * density;
        }

        private bool iOS8Plus
        {
            get
            {
                return UIDevice.CurrentDevice.CheckSystemVersion(8, 0);
            }
        }

        #region IMediaFile implementation

        public async Task<Stream> GetFullFileStreamAsync()
        {
            Debug.WriteLine("MediaFile: getting full file stream");
            if (string.IsNullOrWhiteSpace(Path))
                throw new InvalidDataException("Invalid media path.");
            else if (iOS8Plus)
                return await GetStreamPHAsync(Path);
            else
                return await GetAssetStream(Path);
        }

        public async Task<Stream> GetPreviewFileStreamAsync(float width = 0f, float height = 0f)
        {
            return await GetFullFileStreamAsync(); // TODO: Provide real implementation
        }

        public string FileName { get; private set; }

        public string Path
        {
            get
            {
                return _path;
            }
            internal set
            {
                Debug.WriteLine("MediaFile: setting the path");
                _path = value;
                if (string.IsNullOrWhiteSpace(Path))
                    FileName = string.Empty;
                else if (iOS8Plus)
                    FileName = GetFileNamePHAsync(Path).Result;
                else
                    FileName = GetFileNameAssetAsync(Path).Result;
            }
        }

        #endregion

        #region Photo Framework Methods

        private static PHAsset GetAssetPH(NSUrl assetUrl)
        {
            var assets = PHAsset.FetchAssets(new NSUrl[] { assetUrl }, null);
            if (assets.Count == 0)
                throw new NullReferenceException("Unable to find the specified asset.");
            var asset = (PHAsset)assets[0];
            return asset;
        }

        private async Task<string> GetFileNamePHAsync(string path)
        {
            Debug.WriteLine("MediaFile: starting GetFileNamePHAsync");
            var nameTCS = new TaskCompletionSource<string>();
            using (var assetUrl = new NSUrl(path))
            {
                using (var asset = GetAssetPH(assetUrl))
                {
                    using (var manager = new PHImageManager())
                    {
                        Debug.WriteLine("MediaFile: requesting image data for {0}", (object)path);

                        manager.RequestImageData(asset,
                            new PHImageRequestOptions { Synchronous = true },
                            (data, dataUti, orientation, info) =>
                            {
                                Debug.WriteLine("MediaFile: image data request returned");
                                var imageName = "Unknown";
                                var fileUrl = info["PHImageFileURLKey"].ToString();
                                if (!string.IsNullOrWhiteSpace(fileUrl))
                                {
                                    var slash = fileUrl.LastIndexOf('/');
                                    if (slash > -1)
                                        imageName = fileUrl.Substring(slash + 1);
                                }
                                Debug.WriteLine("MediaFile: image name set to {0}", (object)imageName);
                                nameTCS.SetResult(imageName);
                            });
                        Debug.WriteLine("MediaFile: awaiting image name");
                        await nameTCS.Task;
                    }
                }
            }
            var name = nameTCS.Task.Result;
            Debug.WriteLine("MediaFile: image name returned {0}", (object)name);
            return name;
        }

        private async Task<Stream> GetStreamPHAsync(string path)
        {
            Debug.WriteLine("MediaFile: GetStreamPH entered with path = {0}", (object)path);
            var streamCompletion = new TaskCompletionSource<Stream>();
            using (var assetUrl = new NSUrl(path))
            {
                using (var asset = GetAssetPH(assetUrl))
                {
                    var imageDataTCS = new TaskCompletionSource<NSData>();
                    NSData imgData;
                    using (var manager = new PHImageManager())
                    {
                        manager.RequestImageData(
                            asset,
                            null,
                            (data, dataUti, orientation, info) =>
                            {
                                Debug.WriteLine("MediaFile: data is {0} bytes", data.Length);
                                imageDataTCS.SetResult(data);
                            });
                        Debug.WriteLine("MediaFile: waiting for imgData");
                        imgData = await imageDataTCS.Task.ConfigureAwait(false);
                    }
                    Debug.WriteLine("MediaFile: imgData is {0} bytes", imgData.Length);
                    streamCompletion.SetResult(imgData.AsStream());
                }
            }
            Debug.WriteLine("MediaFile: waiting for stream");
            var stream = await streamCompletion.Task;
            Debug.WriteLine("MediaFile: stream returned with length {0}", stream.Length);
            return stream;
        }

        #endregion

        #region ALAssets Methods

        private async Task<ALAssetRepresentation> GetAssetDefaultRepAsync(string path)
        {
            // get the default representation of the asset
            var library = new ALAssetsLibrary();
            var dRepTCS = new TaskCompletionSource<ALAssetRepresentation>();
            using (var assetUrl = new NSUrl(path))
            {
                library.AssetForUrl(assetUrl,
                    asset => dRepTCS.SetResult(asset.DefaultRepresentation),
                    error => dRepTCS.SetException(new Exception(error.LocalizedFailureReason)));
            }
            var rep = await dRepTCS.Task.ConfigureAwait(false);
            return rep;
        }

        private async Task<Stream> GetAssetStream(string path)
        {
            Debug.WriteLine("MediaFile: GetAssetStream entered with path = {0}", (object)path);

            using (var rep = await GetAssetDefaultRepAsync(path))
            {
                // now some really ugly code to copy that as a byte array
                var size = (uint)rep.Size;
                //byte[] imgData = new byte[size];
                IntPtr buffer = Marshal.AllocHGlobal((int)size);
                NSError bError;
                rep.GetBytes(buffer, 0, (uint)size, out bError);
                //Marshal.Copy(buffer, imgData, 0, imgData.Length);
                var imgData = NSData.FromBytes(buffer, (uint)size);
                Marshal.FreeHGlobal(buffer);
                return imgData.AsStream();
            }
        }

        private async Task<string> GetFileNameAssetAsync(string path)
        {
            Debug.WriteLine("MediaFile: GetFileNameAssetAsync entered with path = {0}", (object)path);

            using (var rep = await GetAssetDefaultRepAsync(path))
            {
                return rep.Filename;
            }
        }

        #endregion
    }
}

