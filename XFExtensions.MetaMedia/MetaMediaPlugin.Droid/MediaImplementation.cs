using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Content.PM;
using MetaMediaPlugin.Abstractions;

namespace MetaMediaPlugin
{
    public class MediaImplementation : IMediaService
    {
        private readonly Context _context;
        private int _requestId = 0;
        private TaskCompletionSource<MediaFile> _tcs;

        public MediaImplementation()
        {
            _context = Android.App.Application.Context;
            MetaMediaActivity.MediaPicked += (sender, e) =>
            {
                if (e.Cancelled)
                    _tcs.SetCanceled();
                else if (e.Failed)
                    _tcs.SetException(e.Error);
                else
                    _tcs.SetResult(e.Media);
            };
        }

        #region IMediaService implementation

        public bool IsCameraAvailable
        {
            get
            {
                return _context.PackageManager.HasSystemFeature(PackageManager.FeatureCamera);
            }
        }

        public bool IsTakePhotoSupported { get { return IsCameraAvailable; } }

        public bool IsPickPhotoSupported { get { return true; } }

        public async Task<IMediaFile> PickPhotoAsync()
        {
            return await GetMediaAsync(MetaMediaActivity.PhotoMediaType, MetaMediaActivity.SelectMediaAction);
        }

        public async Task<IMediaFile> TakePhotoAsync()
        {
            return await GetMediaAsync(MetaMediaActivity.PhotoMediaType, MetaMediaActivity.CreateMediaAction);
        }

        public string PhotosDirectory { get; set; }

        #endregion

        #region Shared

        private Intent CreateMetaMediaIntent(string mediaType, string mediaAction)
        {
            _requestId++;
            var metaMediaIntent = new Intent(_context, typeof(MetaMediaActivity));
            metaMediaIntent.PutExtra(MetaMediaActivity.ExtraRequestId, _requestId);
            metaMediaIntent.PutExtra(MetaMediaActivity.ExtraMediaType, mediaType);
            metaMediaIntent.PutExtra(MetaMediaActivity.ExtraMediaAction, mediaAction);
            metaMediaIntent.PutExtra(MetaMediaActivity.ExtraPhotosDir, PhotosDirectory);
            metaMediaIntent.SetFlags(ActivityFlags.NewTask);
            return metaMediaIntent;
        }

        private async Task<MediaFile> GetMediaAsync(string mediaType, string mediaAction)
        {
            if (_tcs != null)
                throw new InvalidOperationException("Only one operation can be active at a time.");
            _tcs = new TaskCompletionSource<MediaFile>();

            // start our own activity which is just a middle man to the default device activities for taking / selecting media
            _context.StartActivity(CreateMetaMediaIntent(mediaType, mediaAction));
            var media = await _tcs.Task;
            _tcs = null;
            return media;
        }

        #endregion
    }
}