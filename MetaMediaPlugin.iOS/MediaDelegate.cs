using UIKit;
using System.Threading.Tasks;
using Foundation;

namespace MetaMediaPlugin
{
    public class MediaDelegate : UIImagePickerControllerDelegate
    {
        public TaskCompletionSource<NSDictionary> InfoTask { get; set; }
        public override void FinishedPickingMedia(UIImagePickerController picker, NSDictionary info)
        {
            if (InfoTask != null)
                InfoTask.SetResult(info);
            DismissPicker(picker);
        }

        public override void Canceled(UIImagePickerController picker)
        {
            DismissPicker(picker);
        }

        private void DismissPicker(UIImagePickerController picker)
        {
            picker.DismissViewController(true, null);
            picker.Dispose();
        }
    }
}

