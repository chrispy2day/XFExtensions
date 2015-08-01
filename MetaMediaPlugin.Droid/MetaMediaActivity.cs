using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;

namespace MetaMediaPlugin
{
    [Activity]
    public class MetaMediaActivity : Activity
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

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Bundle bundle = (savedInstanceState ?? Intent.Extras);
            _requestId = bundle.GetInt(ExtraRequestId, 0);
            _mediaAction = bundle.GetString(ExtraMediaAction);
            _mediaType = bundle.GetString(ExtraMediaType);

            if (_mediaAction == SelectMediaAction && _mediaType == PhotoMediaType)
                StartPickPhotoIntent();
        }

        private void StartPickPhotoIntent()
        {
            Intent = new Intent();
            Intent.SetType("image/*");
            Intent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(Intent, "Select Photo"), _requestId);
        }

        /// <summary>
        /// This event will fire when the Droid media intents to take or choose media return.
        /// </summary>
        /// <param name="requestCode"></param>
        /// <param name="resultCode"></param>
        /// <param name="data"></param>
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            Task<MediaPickedEventArgs> cont;
            if (resultCode == Result.Canceled)
                cont = Task.FromResult(new MediaPickedEventArgs(requestCode, isCanceled: true));
            else
            {
                cont = GetMediaFileAsync(requestCode, data);
            }
        }

        private Task<MediaPickedEventArgs> GetMediaFileAsync(int requestCode, Intent data)
        {
            if (_mediaAction == SelectMediaAction && _mediaType == PhotoMediaType)
            {
                // get the file

            }
        }
    }
}