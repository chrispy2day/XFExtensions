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
using MobileCoreServices;

namespace MetaMediaPlugin
{
    public class MediaImplementation : IMedia
    {
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

        #region Pick Photo

        public bool IsPickPhotoSupported
        {
            get
            {
                var avaialbleLibraryMedia = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary) ?? new string[0];
                return avaialbleLibraryMedia.Contains(UTType.Image);
            }
        }

        public async Task<MediaFile> PickPhotoAsync()
        {
            var mediaInfoTask = new TaskCompletionSource<NSDictionary>();

            var picker = new UIImagePickerController();
            picker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
            picker.MediaTypes = new string[] {UTType.Image};
            picker.AllowsEditing = false;
            picker.Delegate = new MediaDelegate { InfoTask = mediaInfoTask };
            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
                picker.ModalInPopover = true;

            var vc = GetViewController();
            vc.PresentViewController(picker, true, null);

            var info = await mediaInfoTask.Task;
            var assetUrl = info[UIImagePickerController.ReferenceUrl] as NSUrl;

            return await GetMediaFileForAssetAsync(assetUrl);
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

                var imgData = await imgTCS.Task.ConfigureAwait(false);
                var imgBytes = StreamHelpers.ReadFully(imgData.AsStream());
                imgData.Dispose();
                return new MediaFile { FileName = imageName, MediaBytes = imgBytes };
            }
            else
            {
                // get the default representation of the asset
                var dRepTCS = new TaskCompletionSource<ALAssetRepresentation>();
                using (var library = new ALAssetsLibrary())
                {
                    library.AssetForUrl(
                        assetUrl,
                        (asset) => dRepTCS.SetResult(asset.DefaultRepresentation),
                        (error) => dRepTCS.SetException(new Exception(error.LocalizedFailureReason))
                    );
                }
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
                var imgBytes = StreamHelpers.ReadFully(imgData.AsStream());
                imgData.Dispose();
                return new MediaFile { FileName = rep.Filename, MediaBytes = imgBytes };
            }
        }

        #endregion

        #region Take Photo

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
                return availableCameraMedia.Contains(UTType.Image);
            }
        }

        public async Task<MediaFile> TakePhotoAsync()
        {
            var mediaInfoTask = new TaskCompletionSource<NSDictionary>();

            var picker = new UIImagePickerController();
            picker.SourceType = UIImagePickerControllerSourceType.Camera;
            picker.MediaTypes = new string[] {UTType.Image};
            picker.AllowsEditing = false;
            picker.Delegate = new MediaDelegate { InfoTask = mediaInfoTask };

            var vc = GetViewController();
            vc.PresentViewController(picker, true, null);

            var info = await mediaInfoTask.Task;
            var assetLocation = await SavePhotoWithLocationAsync(info);
            return await GetMediaFileForAssetAsync(assetLocation);
        }

        private async Task<NSUrl> SavePhotoWithLocationAsync(NSDictionary info)
        {
            var image = (UIImage)info[UIImagePickerController.EditedImage];
            if (image == null)
                image = (UIImage)info[UIImagePickerController.OriginalImage];

            var metadata = info[UIImagePickerController.MediaMetadata] as NSDictionary;
            var newMetadata = new NSMutableDictionary(metadata);
            if (!newMetadata.ContainsKey(ImageIO.CGImageProperties.GPSDictionary))
            {
                var gpsData = await BuildGPSDataAsync();
                if (gpsData != null)
                    newMetadata.Add(ImageIO.CGImageProperties.GPSDictionary, gpsData);
            }

            // save to camera roll with metadata
            var assetUrlTCS = new TaskCompletionSource<NSUrl>();
            using (var library = new ALAssetsLibrary())
            {
                library.WriteImageToSavedPhotosAlbum(image.CGImage, newMetadata, (newAssetUrl, error) =>
                    {
                        // any additional processing can go here
                        if (error == null)
                            assetUrlTCS.SetResult(newAssetUrl);
                        else
                            assetUrlTCS.SetException(new Exception(error.LocalizedFailureReason));
                    });
            }
            return await assetUrlTCS.Task;
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
                ImageIO.CGImageProperties.GPSAltitude, Math.Abs(location.Altitude),
                ImageIO.CGImageProperties.GPSDateStamp, DateTime.UtcNow.ToString("yyyy:MM:dd"),
                ImageIO.CGImageProperties.GPSTimeStamp, DateTime.UtcNow.ToString("HH:mm:ss.ff")
            );
            return gpsData;
        }

        #endregion





        #region IMedia implementation


        public System.Threading.Tasks.Task<MediaFile> PickVideoAsync()
        {
            throw new NotImplementedException();
        }

        public System.Threading.Tasks.Task<MediaFile> TakeVideoAsync()
        {
            throw new NotImplementedException();
        }

        public bool IsTakeVideoSupported
        {
            get
            {
                var availableCameraMedia = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.Camera) ?? new string[0];
                return availableCameraMedia.Contains(UTType.Movie);
            }
        }

        public bool IsPickVideoSupported
        {
            get
            {
                var avaialbleLibraryMedia = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary) ?? new string[0];
                return avaialbleLibraryMedia.Contains(UTType.Movie);
            }
        }

        #endregion
    }
}

