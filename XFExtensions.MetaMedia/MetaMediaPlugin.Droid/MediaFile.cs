using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Media;

using MetaMediaPlugin.Abstractions;

using Uri = Android.Net.Uri;
using Stream = System.IO.Stream;

namespace MetaMediaPlugin
{
    public class MediaFile : IMediaFile
    {
        private readonly int _screenWidth;
        private readonly int _screenHeight;

        public MediaFile()
        {
            // get the device size
            var size = Android.App.Application.Context.Resources.DisplayMetrics;

            // use the screen height in density-independent pixels (smaller file)
            _screenWidth = (int)Math.Round(size.WidthPixels / size.Density);
            _screenHeight = (int)Math.Round(size.HeightPixels / size.Density);
        }

        #region IMediaFile implementation

        public Task<Stream> GetFullFileStreamAsync()
        {
            if (string.IsNullOrWhiteSpace(Path))
                throw new InvalidOperationException("Invalid media path.");

            var tcs = new TaskCompletionSource<Stream>();
            tcs.SetResult(System.IO.File.OpenRead(Path));
            return tcs.Task;
        }

        public Task<Stream> GetPreviewFileStreamAsync(float width = 0f, float height = 0f)
        {
            if (string.IsNullOrWhiteSpace(Path))
                throw new InvalidOperationException("Invalid media path.");
            return Task.Run(new Func<Stream>(() =>
            {
                var img = LoadAndResizeBitmap();
                var stream = new MemoryStream();
                img.Compress(Bitmap.CompressFormat.Jpeg, 80, stream);
                stream.Position = 0;
                return stream;
            }));
        }

        public string FileName { get; internal set; }

        public string Path { get; internal set; }

        #endregion

        private Bitmap LoadAndResizeBitmap()
        {
            if (Path.StartsWith("http"))
            {
                //var webImage = GetImageBitmapFromUrl(Path);
                //return Bitmap.CreateScaledBitmap(webImage, _screenWidth, _screenHeight, false);
                return GetImageBitmapFromUrl(Path);
            }

            // First we get the the dimensions of the file on disk
            BitmapFactory.Options options = new BitmapFactory.Options { InJustDecodeBounds = true };
            BitmapFactory.DecodeFile(Path, options);

            // Next we calculate the ratio that we need to resize the image by
            // in order to fit the requested dimensions.
            int outHeight = options.OutHeight;
            int outWidth = options.OutWidth;
            int inSampleSize = 1;

            if (outHeight > _screenHeight || outWidth > _screenWidth)
            {
                inSampleSize = outWidth > outHeight
                               ? outHeight / _screenHeight
                               : outWidth / _screenWidth;
            }

            // Now we will load the image and have BitmapFactory resize it for us.
            options.InSampleSize = inSampleSize;
            options.InJustDecodeBounds = false;
            Bitmap resizedBitmap = BitmapFactory.DecodeFile(Path, options);

            // Images are being saved in landscape, so rotate them back to portrait if they were taken in portrait
            Matrix mtx = new Matrix();
            ExifInterface exif = new ExifInterface(Path);
            string orientation = exif.GetAttribute(ExifInterface.TagOrientation);
            switch (orientation)
            {
                case "6": // portrait
                    mtx.PreRotate(90);
                    resizedBitmap = Bitmap.CreateBitmap(resizedBitmap, 0, 0, resizedBitmap.Width, resizedBitmap.Height, mtx, false);
                    mtx.Dispose();
                    mtx = null;
                    break;
                case "1": // landscape
                    break;
                default:
                    mtx.PreRotate(90);
                    resizedBitmap = Bitmap.CreateBitmap(resizedBitmap, 0, 0, resizedBitmap.Width, resizedBitmap.Height, mtx, false);
                    mtx.Dispose();
                    mtx = null;
                    break;
            }

            return resizedBitmap;
        }

        private Bitmap GetImageBitmapFromUrl(string url)
        {
            Bitmap imageBitmap = null;

            using (var webClient = new WebClient())
            {
                try
                {
                    var imageBytes = webClient.DownloadData(url);
                    if (imageBytes != null && imageBytes.Length > 0)
                    {
                        imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                    }
                }
                catch (Exception err)
                {
                    var msg = err.Message;
                }
            }

            return imageBitmap;
        }
    }
}

