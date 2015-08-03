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
        private readonly Context _context;
        private int _requestId = 0;
        private TaskCompletionSource<MediaFile> _tcs;

        public MediaImplementation()
        {
            _context = Android.App.Application.Context;
            IsCameraAvailable = _context.PackageManager.HasSystemFeature(PackageManager.FeatureCamera);
        }

        public bool IsCameraAvailable { get; private set; }
        public bool IsTakePhotoSupported { get { return IsCameraAvailable; } }
        public bool IsPickPhotoSupported { get { return true;  } }
        public bool IsTakeVideoSupported { get { return IsCameraAvailable; } }
        public bool IsPickVideoSupported { get { return true; } }

        private void UpdateRequestId()
		{
			if (_requestId == Int32.MaxValue)
				_requestId = 0;
			else
				_requestId++;
		}

        private Intent CreateMetaMediaIntent(string mediaType, string mediaAction)
        {
            var metaMediaIntent = new Intent(_context, typeof(MetaMediaActivity));
            metaMediaIntent.PutExtra(MetaMediaActivity.ExtraMediaType, mediaType);
            metaMediaIntent.PutExtra(MetaMediaActivity.ExtraMediaAction, mediaAction);
            metaMediaIntent.SetFlags(ActivityFlags.NewTask);
            return metaMediaIntent;
        }

        private Task<MediaFile> GetMediaAsync(string mediaType, string mediaAction)
        {
            UpdateRequestId();

            var tcs = new TaskCompletionSource<MediaFile>(_requestId);
            // compare exchange compares param1 and param2, if equal then param1 replaces param0.  Returns org param0 value
            // here tcs will never be null, so this is a thread safe way to say
            // if (_tcs != null) throw...
            if (Interlocked.CompareExchange(ref _tcs, tcs, null) != null)
                throw new InvalidOperationException("Only one operation can be active at a time.");

            // start our own activity which is just a middle man to the default device activities for taking / selecting media
            _context.StartActivity(CreateMetaMediaIntent(mediaType, mediaAction));
            EventHandler<MediaPickedEventArgs> handler = null;
            handler = (sender, args) =>
            {
                // clear _tcs and remove the previous handler if there was one
                var taskSource = Interlocked.Exchange(ref _tcs, null);
                MetaMediaActivity.MediaPicked -= handler;

                // do nothing if this is not the response we are waiting for
                if (args.RequestId != _requestId)
                    return;

                // if error or cancel then no result, otherwise return the media
                if (args.Error != null || args.IsCanceled)
                    taskSource.SetResult(null);
                else
                    taskSource.SetResult(args.Media);
            };

            // assign the handler and return the task
            MetaMediaActivity.MediaPicked += handler;
            return tcs.Task;
        }

        public Task<MediaFile> PickPhotoAsync()
        {
            return GetMediaAsync(MetaMediaActivity.PhotoMediaType, MetaMediaActivity.SelectMediaAction);
        }

        public Task<MediaFile> TakePhotoAsync(StoreCameraMediaOptions options)
        {
            throw new NotImplementedException();
        }

        public Task<MediaFile> PickVideoAsync()
        {
            throw new NotImplementedException();
        }

        public Task<MediaFile> TakeVideoAsync(StoreVideoOptions options)
        {
            throw new NotImplementedException();
        }
    }
}