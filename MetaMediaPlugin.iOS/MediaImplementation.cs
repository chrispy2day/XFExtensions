using System;
using MetaMediaPlugin.Abstractions;
using UIKit;
using System.Linq;
using Foundation;
using AssetsLibrary;
using System.Threading.Tasks;
using CoreLocation;
using Photos;
using System.Runtime.InteropServices;

namespace MetaMediaPlugin
{
    public class MediaImplementation : IMedia
    {
        private const string TypeImage = "public.image";
        private const string TypeMovie = "public.movie";
        private TaskCompletionSource<MediaFile> _tcs;
        private ALAssetsLibrary _library;

        public MediaImplementation()
        {
            _library = new ALAssetsLibrary();
        }

        #region IMedia implementation

        static UIViewController GetViewController()
        {
            var vc = UIApplication.SharedApplication
                .Windows.OrderByDescending(w => w.WindowLevel)
                .FirstOrDefault(w => w.RootViewController != null)
                .RootViewController;
            while (vc.PresentedViewController != null)
                vc = vc.PresentedViewController;
            return vc;
        }

        public async Task<MediaFile> PickPhotoAsync()
        {
            if (!IsPickPhotoSupported)
                throw new NotSupportedException();

            if (_tcs != null)
                throw new InvalidOperationException("Only one operation can be active at a time.");
            _tcs = new TaskCompletionSource<MediaFile>();

            var rvc = GetViewController();
            if (rvc == null)
                _tcs.SetException(new InvalidOperationException("Unable to identify the current view controller."));
            else
            {
                var assetUrlTCS = new TaskCompletionSource<NSUrl>();
                TweetStation.Camera.SelectPicture(rvc, (dict) =>
                    {
                        // get the assetUrl for the selected image
                        assetUrlTCS.SetResult(dict[UIImagePickerController.ReferenceUrl] as NSUrl);
                    });
                var assetUrl = await assetUrlTCS.Task;
                var file = await GetMediaFileForAssetAsync(assetUrl);
                _tcs.SetResult(file);
            }
            var result = await _tcs.Task;
            _tcs = null;
            return result;
        }

        public async Task<MediaFile> TakePhotoAsync()
        {
            if (_tcs != null)
                throw new InvalidOperationException("Only one operation can be active at a time.");
            _tcs = new TaskCompletionSource<MediaFile>();

            var rvc = GetViewController();
            if (rvc == null)
                _tcs.SetException(new InvalidOperationException("Unable to identify the current view controller."));
            else
            {
                var assetUrlTCS = new TaskCompletionSource<NSUrl>();
                TweetStation.Camera.TakePicture(rvc, async (dict) =>
                    {
                        var photo = dict.ValueForKey(UIImagePickerController.OriginalImage) as UIImage;
                        var meta = dict.ValueForKey(UIImagePickerController.MediaMetadata) as NSDictionary;
                        var newMetadata = new NSMutableDictionary(meta);
                        if (!newMetadata.ContainsKey(ImageIO.CGImageProperties.GPSDictionary))
                        {
                            var gpsData = await BuildGPSDataAsync();
                            if (gpsData != null)
                                newMetadata.Add(ImageIO.CGImageProperties.GPSDictionary, gpsData);
                        }

                        // This bit of code saves to the Photo Album with metadata
                        _library.WriteImageToSavedPhotosAlbum(photo.CGImage, newMetadata, (assetUrl, error) =>
                            {
                                // any additional processing can go here
                                if (error == null)
                                    assetUrlTCS.SetResult(assetUrl);
                                else
                                    assetUrlTCS.SetException(new Exception(error.LocalizedFailureReason));
                            });
                    });
                var file = await GetMediaFileForAssetAsync(await assetUrlTCS.Task);
                _tcs.SetResult(file);
            }
            var result = await _tcs.Task;
            _tcs = null;
            return result;
        }

        private async Task<MediaFile> GetMediaFileForAssetAsync(NSUrl assetUrl)
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                var assets = PHAsset.FetchAssets(new NSUrl[] { assetUrl }, null);
                if (assets.Count == 0)
                    throw new NullReferenceException("Unable to find the specified asset.");
                var asset = (PHAsset)assets[0];
                var imgMgr = new PHImageManager();

                var imgTCS = new TaskCompletionSource<NSData>();
                var imageName = "Unknown";
                imgMgr.RequestImageData(asset, null, (data, uti, imageOrientation, dictionary) =>
                    {
                        var fileUrl = dictionary["PHImageFileURLKey"].ToString();
                        if (!string.IsNullOrWhiteSpace(fileUrl))
                        {
                            var slash = fileUrl.LastIndexOf('/');
                            if (slash > -1)
                                imageName = fileUrl.Substring(slash + 1);
                        }
                        imgTCS.SetResult(data);
                    });

                var img = await imgTCS.Task.ConfigureAwait(false);
                return new MediaFile { Name = imageName, MediaStream = img.AsStream() };
            }
            else
            {
                // get the default representation of the asset
                var dRepTCS = new TaskCompletionSource<ALAssetRepresentation>();
                _library.AssetForUrl(
                    assetUrl,
                    (asset) => dRepTCS.SetResult(asset.DefaultRepresentation),
                    (error) => dRepTCS.SetException(new Exception(error.LocalizedFailureReason))
                );
                var rep = await dRepTCS.Task.ConfigureAwait(false);

                // now some really ugly code to copy that as a byte array
                var size = (uint)rep.Size;
                //byte[] imgData = new byte[size];
                IntPtr buffer = Marshal.AllocHGlobal((int)size);
                NSError bError;
                rep.GetBytes(buffer, 0, (uint)size, out bError);
                //Marshal.Copy(buffer, imgData, 0, imgData.Length);
                var imgData = NSData.FromBytes(buffer, (uint)size);
                Marshal.FreeHGlobal(buffer);

                return new MediaFile { Name = rep.Filename, MediaStream = imgData.AsStream() };
            }
        }

        CLLocationManager _locationManager;
        TaskCompletionSource<CLLocation> _locationTCS;
        private async Task<NSDictionary> BuildGPSDataAsync()
        {
            // setup the location manager
            if (_locationManager == null)
            {
                _locationManager = new CLLocationManager();
                if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
                    _locationManager.RequestWhenInUseAuthorization();
                _locationManager.DesiredAccuracy = 1; // in meters
            }

            // setup a task for getting the current location and a callback for receiving the location
            _locationTCS = new TaskCompletionSource<CLLocation>();
            _locationManager.LocationsUpdated += (sender, locationArgs) =>
                {
                    if (locationArgs.Locations.Length > 0)
                    {
                        _locationManager.StopUpdatingLocation();
                        _locationTCS.SetResult(locationArgs.Locations[locationArgs.Locations.Length - 1]);
                    }
                };
            // start location monitoring
            _locationManager.StartUpdatingLocation();

            // create a timeout and location task to ensure we don't wait forever
            var timeoutTask = System.Threading.Tasks.Task.Delay(5000); // 5 second wait
            var locationTask = _locationTCS.Task;

            // setup a date formatter
            var dateFormatter = new NSDateFormatter();
            dateFormatter.TimeZone = new NSTimeZone("UTC");
            dateFormatter.DateFormat = "HH:mm:ss.SS";

            // try and set a location based on whatever task ends first
            CLLocation location;
            var completeTask = await System.Threading.Tasks.Task.WhenAny(locationTask, timeoutTask);
            if (completeTask == locationTask && completeTask.Status == TaskStatus.RanToCompletion)
            {
                // use the location result
                location = locationTask.Result;
            }
            else
            {
                // timeout - stop the location manager and try and use the last location
                _locationManager.StopUpdatingLocation();
                location = _locationManager.Location;
            }

            if (location == null)
                return null;

            var gpsData = new NSDictionary(
                ImageIO.CGImageProperties.GPSLatitude, Math.Abs(location.Coordinate.Latitude),
                ImageIO.CGImageProperties.GPSLatitudeRef, (location.Coordinate.Latitude >= 0) ? "N" : "S",
                ImageIO.CGImageProperties.GPSLongitude, Math.Abs(location.Coordinate.Longitude),
                ImageIO.CGImageProperties.GPSLongitudeRef, (location.Coordinate.Longitude >= 0) ? "E" : "W",
                ImageIO.CGImageProperties.GPSTimeStamp, dateFormatter.StringFor(location.Timestamp),
                ImageIO.CGImageProperties.GPSAltitude, Math.Abs(location.Altitude),
                ImageIO.CGImageProperties.GPSDateStamp, DateTime.UtcNow.ToString("yyyy:MM:dd"),
                ImageIO.CGImageProperties.GPSDateStamp, DateTime.UtcNow.ToString("HH:mm:ss.ff")
            );
            return gpsData;
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
                return UIImagePickerController.IsSourceTypeAvailable(UIImagePickerControllerSourceType.Camera);
            }
        }

        public bool IsTakePhotoSupported
        {
            get
            {
                var availableCameraMedia = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.Camera) ?? new string[0];
                return availableCameraMedia.Contains(TypeImage);
            }
        }

        public bool IsPickPhotoSupported
        {
            get
            {
                var avaialbleLibraryMedia = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary) ?? new string[0];
                return avaialbleLibraryMedia.Contains(TypeImage);
            }
        }

        public bool IsTakeVideoSupported
        {
            get
            {
                var availableCameraMedia = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.Camera) ?? new string[0];
                return availableCameraMedia.Contains(TypeMovie);
            }
        }

        public bool IsPickVideoSupported
        {
            get
            {
                var avaialbleLibraryMedia = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary) ?? new string[0];
                return avaialbleLibraryMedia.Contains(TypeMovie);
            }
        }

        #endregion
    }
}

