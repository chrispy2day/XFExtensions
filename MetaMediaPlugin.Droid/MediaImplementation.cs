using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Content.PM;
using Android.Provider;
using MetaMediaPlugin.Abstractions;

namespace MetaMediaPlugin
{
    public class MediaImplementation : IMedia
    {
        private int _requestId = 0;
        private readonly Context _context;
        private TaskCompletionSource<MediaFile> _tcs;

        public MediaImplementation()
        {
            _context = Android.App.Application.Context;
            IsCameraAvailable = _context.PackageManager.HasSystemFeature(PackageManager.FeatureCamera);
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

        public bool IsCameraAvailable { get; private set; }
        public bool IsTakePhotoSupported { get { return IsCameraAvailable; } }
        public bool IsPickPhotoSupported { get { return true;  } }
        public bool IsTakeVideoSupported { get { return IsCameraAvailable; } }
        public bool IsPickVideoSupported { get { return true; } }

        private Intent CreateMetaMediaIntent(string mediaType, string mediaAction)
        {
            _requestId++;
            var metaMediaIntent = new Intent(_context, typeof(MetaMediaActivity));
            metaMediaIntent.PutExtra(MetaMediaActivity.ExtraRequestId, _requestId);
            metaMediaIntent.PutExtra(MetaMediaActivity.ExtraMediaType, mediaType);
            metaMediaIntent.PutExtra(MetaMediaActivity.ExtraMediaAction, mediaAction);
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

        public Task<MediaFile> PickPhotoAsync()
        {
            return GetMediaAsync(MetaMediaActivity.PhotoMediaType, MetaMediaActivity.SelectMediaAction);
        }

        public Task<MediaFile> TakePhotoAsync()
        {
            return GetMediaAsync(MetaMediaActivity.PhotoMediaType, MetaMediaActivity.CreateMediaAction);
        }

        public Task<MediaFile> PickVideoAsync()
        {
            throw new NotImplementedException();
        }

        public Task<MediaFile> TakeVideoAsync()
        {
            throw new NotImplementedException();
        }
    }
}