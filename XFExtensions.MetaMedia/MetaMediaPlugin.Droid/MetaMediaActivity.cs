using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Locations;
using Android.Media;
using Android.OS;
using Android.Provider;
using Java.IO;

using Environment = Android.OS.Environment;
using Uri = Android.Net.Uri;
using Android.Database;

namespace MetaMediaPlugin
{
    [Activity]
    public class MetaMediaActivity : Activity, ILocationListener
    {
        internal const string ExtraMediaAction = "action";
        internal const string ExtraMediaType = "mediatype";
        internal const string ExtraRequestId = "request_id";
        internal const string ExtraPhotosDir = "photos_subdir";
        internal const string SelectMediaAction = "select media";
        internal const string CreateMediaAction = "create media";
        internal const string PhotoMediaType = "photo";
        internal const string VideoMediaType = "video";

        internal static event EventHandler<MediaPickedEventArgs> MediaPicked;

        private int _requestId;
        private string _mediaAction;
        private string _mediaType;
        private string _photoSubDir;
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
            _photoSubDir = bundle.GetString(ExtraPhotosDir);

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
                StartTakePhotoIntent();
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
                    catch (Exception e)
                    {
                        OnMediaPicked(new MediaPickedEventArgs(requestCode, e));
                    }
                }
                else
                    OnMediaPicked(new MediaPickedEventArgs(requestCode, new Exception("Activity returned an unknown / unexpected result code.")));
            }
            Finish();
        }

        private static void OnMediaPicked(MediaPickedEventArgs e)
        {
            var picked = MediaPicked;
            if (picked != null)
                picked(null, e);
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
            string path = GetPath(data.Data);
            //            if (Build.VERSION.SdkInt < BuildVersionCodes.Kitkat) // less than version 19 - KitKat
            //                path = GetPathToImage(data.Data);
            //            else
            //                path = GetPathToImageApi19(data.Data);
            if (string.IsNullOrWhiteSpace(path))
                throw new InvalidOperationException("Unable to retrieve the path to the image.");

            var name = path.Substring(path.LastIndexOf("/") + 1);
            //var stream = Forms.Context.ContentResolver.OpenInputStream(data.Data);
            var file = new MediaFile { FileName = name, Path = path };
            OnMediaPicked(new MediaPickedEventArgs(requestCode, false, file));
        }

        // found API version variations at http://hmkcode.com/android-display-selected-image-and-its-real-path/
        //        private string GetPathToImageApi19(Uri uri)
        //        {
        //            string path = uri.Path;
        //            if (!DocumentsContract.IsDocumentUri(Application.Context, uri))
        //                return path;
        //            string wholeId = DocumentsContract.GetDocumentId(uri);
        //            string id = wholeId.Split(new[] {':'})[1];
        //            string[] column = {MediaStore.Images.Media.InterfaceConsts.Data};
        //            string sel = MediaStore.Images.Media.InterfaceConsts.Id + "=?";
        //            using (var cursor = ContentResolver.Query(MediaStore.Images.Media.ExternalContentUri, column, sel,
        //                new string[] {id}, null))
        //            {
        //                int columnIndex = cursor.GetColumnIndex(column[0]);
        //                if (cursor.MoveToFirst())
        //                    path = cursor.GetString(columnIndex);
        //                cursor.Close();
        //            }
        //            return path;
        //        }
        //
        //        private string GetPathToImage(Uri uri)
        //        {
        //            string path = uri.Path;
        //            // The projection contains the columns we want to return in our query.
        //            string[] projection = new[] { MediaStore.Images.Media.InterfaceConsts.Data };
        //            using (var cursor = ContentResolver.Query(uri, projection, null, null, null))
        //            {
        //                if (cursor != null && cursor.MoveToFirst())
        //                {
        //                    int columnIndex = cursor.GetColumnIndexOrThrow(Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data);
        //                    path = cursor.GetString(columnIndex);
        //                }
        //            }
        //            return path;
        //        }

        // found this method at http://stackoverflow.com/questions/31715181/android-get-image-path
        // and converted it for Xamarin.  Covers more options than the previous and seems to be working well.
        public string GetPath(Uri uri)
        {
            bool isKitKat = Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat;
            // DocumentProvider
            if (isKitKat && DocumentsContract.IsDocumentUri(Application.Context, uri))
            {
                // ExternalStorageProvider
                if (IsExternalStorageDocument(uri))
                {
                    string docId = DocumentsContract.GetDocumentId(uri);
                    string[] split = docId.Split(new char[] { ':' });
                    string type = split[0];
                    if (type.ToLower() == "primary")
                        return Environment.ExternalStorageDirectory + "/" + split[1];
                }
                // DownloadsProvider
                else if (IsDownloadsDocument(uri))
                {
                    string id = DocumentsContract.GetDocumentId(uri);
                    Uri contentUri = ContentUris.WithAppendedId(Uri.Parse("content://downloads/public_downloads"), long.Parse(id));
                    return GetDataColumn(Application.Context, contentUri, null, null);
                }
                // MediaProvider
                else if (IsMediaDocument(uri))
                {
                    string docId = DocumentsContract.GetDocumentId(uri);
                    string[] split = docId.Split(new char[] { ':' });
                    string type = split[0];
                    Uri contentUri = null;
                    if (type == "image")
                        contentUri = MediaStore.Images.Media.ExternalContentUri;
                    else if (type == "video")
                        contentUri = MediaStore.Video.Media.ExternalContentUri;
                    else if (type == "audio")
                        contentUri = MediaStore.Audio.Media.ExternalContentUri;
                    string selection = "_id=?";
                    string[] selectionArgs = new String[] { split[1] };
                    return GetDataColumn(Application.Context, contentUri, selection, selectionArgs);
                }
            }
            // MediaStore (and general)
            else if (uri.Scheme.ToLower() == "content")
            {
                // Return the remote address
                if (IsGooglePhotosUri(uri))
                    return uri.LastPathSegment;
                return GetDataColumn(Application.Context, uri, null, null);
            }
            // File
            else if (uri.Scheme.ToLower() == "file")
                return uri.Path;
            return null;
        }

        public static string GetDataColumn(Context context, Uri uri, String selection, String[] selectionArgs)
        {
            ICursor cursor = null;
            string column = "_data";
            string[] projection = { column };
            try
            {
                cursor = context.ContentResolver.Query(uri, projection, selection, selectionArgs, null);
                if (cursor != null && cursor.MoveToFirst())
                {
                    int index = cursor.GetColumnIndexOrThrow(column);
                    return cursor.GetString(index);
                }
            }
            finally
            {
                if (cursor != null)
                    cursor.Close();
            }
            return null;
        }

        public static bool IsExternalStorageDocument(Uri uri)
        {
            return uri.Authority == "com.android.externalstorage.documents";
        }

        public static bool IsDownloadsDocument(Uri uri)
        {
            return uri.Authority == "com.android.providers.downloads.documents";
        }

        public static bool IsMediaDocument(Uri uri)
        {
            return uri.Authority == "com.android.providers.media.documents";
        }

        public static bool IsGooglePhotosUri(Uri uri)
        {
            return uri.Authority == "com.google.android.apps.photos.content";
        }

        #endregion

        #region Take Photo Code

        private void CreatePhotoDir()
        {
            try
            {
                _dir = new File(Environment.GetExternalStoragePublicDirectory(Environment.DirectoryPictures), _photoSubDir);
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
            // see if the photo already contains geo coords
            var exif = new ExifInterface(_file.Path);
            if (!string.IsNullOrWhiteSpace(exif.GetAttribute(ExifInterface.TagGpsLatitude)))
                return;

            RequestCurrentLocation();
            var location = await _locationTCS.Task;

            try
            {
                int num1Lat = (int)Math.Floor(location.Latitude);
                int num2Lat = (int)Math.Floor((location.Latitude - num1Lat) * 60);
                double num3Lat = (location.Latitude - ((double)num1Lat + ((double)num2Lat / 60))) * 3600000;

                int num1Lon = (int)Math.Floor(location.Longitude);
                int num2Lon = (int)Math.Floor((location.Longitude - num1Lon) * 60);
                double num3Lon = (location.Longitude - ((double)num1Lon + ((double)num2Lon / 60))) * 3600000;

                exif.SetAttribute(ExifInterface.TagGpsLatitude, num1Lat + "/1," + num2Lat + "/1," + num3Lat + "/1000");
                exif.SetAttribute(ExifInterface.TagGpsLongitude, num1Lon + "/1," + num2Lon + "/1," + num3Lon + "/1000");


                if (location.Latitude > 0)
                {
                    exif.SetAttribute(ExifInterface.TagGpsLatitudeRef, "N");
                }
                else
                {
                    exif.SetAttribute(ExifInterface.TagGpsLatitudeRef, "S");
                }

                if (location.Longitude > 0)
                {
                    exif.SetAttribute(ExifInterface.TagGpsLongitudeRef, "E");
                }
                else
                {
                    exif.SetAttribute(ExifInterface.TagGpsLongitudeRef, "W");
                }

                exif.SaveAttributes();
            }
            catch (Java.IO.IOException)
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
            var path = _file.Path;
            var name = path.Substring(path.LastIndexOf("/"));
            var file = new MediaFile { FileName = name, Path = path };
            OnMediaPicked(new MediaPickedEventArgs(requestCode, false, file));
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