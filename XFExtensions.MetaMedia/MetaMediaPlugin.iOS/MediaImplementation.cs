using System;
using MetaMediaPlugin.Abstractions;
using UIKit;
using System.Linq;
using Foundation;
using AssetsLibrary;
using System.Threading.Tasks;
using CoreLocation;
using MobileCoreServices;
using System.Diagnostics;

namespace MetaMediaPlugin
{
    public class MediaImplementation : IMediaService
    {
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

        public bool IsPickPhotoSupported
        {
            get
            {
                var avaialbleLibraryMedia = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary) ?? new string[0];
                return avaialbleLibraryMedia.Contains(UTType.Image);
            }
        }

        public async Task<IMediaFile> PickPhotoAsync()
        {
            Debug.WriteLine("PickPhotoAsync: starting");
            // setup a task to get the results back from the delegate
            var mediaInfoTask = new TaskCompletionSource<NSDictionary>();

            // setup a UI Picker to show photos
            var picker = new UIImagePickerController();
            picker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
            picker.MediaTypes = new string[] { UTType.Image };
            picker.AllowsEditing = false;
            picker.Delegate = new MediaDelegate { InfoTask = mediaInfoTask };
            // iPad should display the picker in a popover
            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
                picker.ModalInPopover = true;
            var vc = GetViewController();
            Debug.WriteLine("PickPhotoAsync: displaying photo picker");
            vc.PresentViewController(picker, true, null);
            // wait to get the results back
            Debug.WriteLine("PickPhotoAsync: waiting for selected photo");
            var info = await mediaInfoTask.Task;
            var assetUrl = info[UIImagePickerController.ReferenceUrl] as NSUrl;
            Debug.WriteLine("PickPhotoAsync: photo selected {0}", assetUrl);
            var mediaFile = new MediaFile {Path = assetUrl.ToString()};
            Debug.WriteLine("PickPhotoAsync: returning created media file");
            return mediaFile;
        }

        public async Task<IMediaFile> TakePhotoAsync()
        {
            Debug.WriteLine("TakePhotoAsync: starting");
            // setup a task to get the results back from the delegate
            var mediaInfoTask = new TaskCompletionSource<NSDictionary>();

            // setup a UI Picker to display camera
            var picker = new UIImagePickerController();
            picker.SourceType = UIImagePickerControllerSourceType.Camera;
            picker.MediaTypes = new string[] { UTType.Image };
            picker.AllowsEditing = false;
            picker.Delegate = new MediaDelegate { InfoTask = mediaInfoTask };

            // display the picker
            Debug.WriteLine("TakePhotoAsync: displaying the camera");
            var vc = GetViewController();
            vc.PresentViewController(picker, true, null);

            // wait for a result
            Debug.WriteLine("TakePhotoAsync: waiting for photo from camera");
            var info = await mediaInfoTask.Task;
            var assetLocation = await SavePhotoWithLocationAsync(info);

            // create and return a media file
            Debug.WriteLine("TakePhotoAsync: picture taken & stored at {0}", assetLocation);
            var mediaFile = new MediaFile {Path = assetLocation.ToString()};
            Debug.WriteLine("TakePhotoAsync: returning created media file");
            return mediaFile;
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
                        _locationTCS.TrySetResult(locationArgs.Locations[locationArgs.Locations.Length - 1]);
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
    }
}

