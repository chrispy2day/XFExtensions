using System;
using System.IO;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.Locations;
using Android.Media;
using Android.OS;
using Android.Provider;

using Java.IO;

using Xamarin.Forms;

using MetaMediaPlugin.Abstractions;



using Environment = Android.OS.Environment;
using Uri = Android.Net.Uri;
using File = Java.IO.File;

namespace MetaMediaPlugin
{
    [Activity]
    public class MetaMediaActivity : Activity, ILocationListener
    {
        internal const string ExtraMediaAction = "action";
        internal const string ExtraMediaType = "mediatype";
        internal const string ExtraRequestId = "request_id";
        internal const string SelectMediaAction = "select media";
        internal const string CreateMediaAction = "create media";
        internal const string PhotoMediaType = "photo";
        internal const string VideoMediaType = "video";

        internal static event EventHandler<MediaPickedEventArgs> MediaPicked;

        private int _requestId;
        private string _mediaAction;
        private string _mediaType;
        private File _dir;
        private File _file;

        #region Activity Methods

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Bundle bundle = (savedInstanceState ?? Intent.Extras);
            _requestId = bundle.GetInt(ExtraRequestId, 0);
            _mediaAction = bundle.GetString(ExtraMediaAction);
            _mediaType = bundle.GetString(ExtraMediaType);

            if (_mediaAction == SelectMediaAction && _mediaType == PhotoMediaType)
                StartPickPhotoIntent();
            else if (_mediaAction == CreateMediaAction && _mediaType == PhotoMediaType)
            {
                CreatePhotoDir();
                if (_dir == null)
                {
                    OnMediaPicked(new MediaPickedEventArgs(_requestId, new System.IO.IOException("Unable to create photo directory.")));
                    Finish();
                    return;
                }
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutInt(ExtraRequestId, _requestId);
            outState.PutString(ExtraMediaAction, _mediaAction);
            outState.PutString(ExtraMediaType, _mediaType);

            base.OnSaveInstanceState(outState);
        }

        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == _requestId)
            {
                if (resultCode == Result.Canceled)
                    OnMediaPicked(new MediaPickedEventArgs(requestCode, new System.OperationCanceledException()));
                else if (resultCode == Result.Ok)
                {
                    try
                    {
                        if (_mediaAction == SelectMediaAction && _mediaType == PhotoMediaType)
                            HandlePickPhotoResults(requestCode, data);
                        else if (_mediaAction == CreateMediaAction && _mediaType == PhotoMediaType)
                            await HandleTakePhotoResultsAsync(requestCode);
                    }
                    catch(Exception e)
                    {
                        OnMediaPicked(new MediaPickedEventArgs(requestCode, e));
                    }
                }
                else
                    OnMediaPicked(new MediaPickedEventArgs(requestCode, new Exception("Activity returned an unknown / unexpected result code.")));
            }
            Finish();
        }

        #endregion

        #region Pick Photo Code

        private void StartPickPhotoIntent()
        {
            var imagePicker = new Intent();
            imagePicker.SetAction(Intent.ActionGetContent);
            imagePicker.SetType("image/*");
            StartActivityForResult(Intent.CreateChooser(imagePicker, "Choose Image"), _requestId);
        }

        private void HandlePickPhotoResults(int requestCode, Intent data)
        {
            var stream = Forms.Context.ContentResolver.OpenInputStream(data.Data);
            var imgBytes = ReadFully(stream);
            stream.Close();
            stream.Dispose();
            var file = new MediaFile {
                Media = imgBytes
            };
            OnMediaPicked(new MediaPickedEventArgs(requestCode, false, file));
        }

        #endregion

        #region Take Photo Code

        private void CreatePhotoDir()
        {
            try
            {
                _dir = new File(Environment.GetExternalStoragePublicDirectory(Environment.DirectoryPictures), "FormsImagePicker");
                if (!_dir.Exists())
                    _dir.Mkdirs();
            }
            catch
            {
                _dir = null;
            }
        }

        private void StartTakePhotoIntent()
        {
            var intent = new Intent(MediaStore.ActionImageCapture);
            var now = DateTime.Now;
            _file = new File(_dir, string.Format("im_{0}{1}{2}_{3}{4}{5}.jpg", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second));
            intent.PutExtra(MediaStore.ExtraOutput, Uri.FromFile(_file));
            StartActivityForResult(intent, _requestId);
        }

        private async Task GeoTagPhotoAsync()
        {
            RequestCurrentLocation();
            var location = await _locationTCS.Task;

            try 
            {
                var exif = new ExifInterface(_file.Path);
                int num1Lat = (int)Math.Floor(location.Latitude);
                int num2Lat = (int)Math.Floor((location.Latitude - num1Lat) * 60);
                double num3Lat = (location.Latitude - ((double)num1Lat+((double)num2Lat/60))) * 3600000;

                int num1Lon = (int)Math.Floor(location.Longitude);
                int num2Lon = (int)Math.Floor((location.Longitude - num1Lon) * 60);
                double num3Lon = (location.Longitude - ((double)num1Lon+((double)num2Lon/60))) * 3600000;

                exif.SetAttribute(ExifInterface.TagGpsLatitude, num1Lat+"/1,"+num2Lat+"/1,"+num3Lat+"/1000");
                exif.SetAttribute(ExifInterface.TagGpsLongitude, num1Lon+"/1,"+num2Lon+"/1,"+num3Lon+"/1000");


                if (location.Latitude > 0) {
                    exif.SetAttribute(ExifInterface.TagGpsLatitudeRef, "N"); 
                } else {
                    exif.SetAttribute(ExifInterface.TagGpsLatitudeRef, "S");
                }

                if (location.Longitude > 0) {
                    exif.SetAttribute(ExifInterface.TagGpsLongitudeRef, "E");    
                } else {
                    exif.SetAttribute(ExifInterface.TagGpsLongitudeRef, "W");
                }

                exif.SaveAttributes();
            } 
            catch (Java.IO.IOException e) 
            {
                // location will not be available on this image, but continue
            } 
        }

        private async Task HandleTakePhotoResultsAsync(int requestCode)
        {
            await GeoTagPhotoAsync();
            // Make it available in the gallery
            Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
            Uri contentUri = Uri.FromFile(_file);
            mediaScanIntent.SetData(contentUri);
            SendBroadcast(mediaScanIntent);
            // return the file
            var stream = Forms.Context.ContentResolver.OpenInputStream(contentUri);
            var imgBytes = ReadFully(stream);
            stream.Close();
            stream.Dispose();
            var file = new MediaFile { Media = imgBytes };
            OnMediaPicked(new MediaPickedEventArgs(requestCode, false, file));
        }

        #endregion

        #region Helpers

        private static void OnMediaPicked (MediaPickedEventArgs e)
        {
            var picked = MediaPicked;
            if (picked != null)
                picked (null, e);
        }

        private static byte[] ReadFully(System.IO.Stream input)
        {
            byte[] buffer = new byte[16*1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        #endregion

        #region Location Monitoring

        private TaskCompletionSource<Location> _locationTCS;

        private void RequestCurrentLocation()
        {
            _locationTCS = new TaskCompletionSource<Location>();
            var lm = (LocationManager)GetSystemService(Context.LocationService);
            lm.RequestSingleUpdate(new Criteria { Accuracy = Android.Locations.Accuracy.Fine }, this, MainLooper);
        }


        public void OnLocationChanged(Location location)
        {
            _locationTCS.SetResult(location);
        }

        public void OnProviderDisabled(string provider)
        {

        }

        public void OnProviderEnabled(string provider)
        {

        }

        public void OnStatusChanged(string provider, Availability status, Android.OS.Bundle extras)
        {
        }

        #endregion
    }
}