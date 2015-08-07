using System;
using UIKit;
using Foundation;

namespace TweetStation
{
    //
    // A static class that will reuse the UIImagePickerController
    // as iPhoneOS has a crash if multiple UIImagePickerController are created
    //   http://stackoverflow.com/questions/487173
    // (Follow the links)
    //
    public static class Camera
    {
        static UIImagePickerController picker;
        static Action<NSDictionary> _callback;

        static void Init ()
        {
            if (picker != null)
                return;

            picker = new UIImagePickerController ();
            picker.Delegate = new CameraDelegate ();
        }

        class CameraDelegate : UIImagePickerControllerDelegate {
            public override void FinishedPickingMedia (UIImagePickerController picker, NSDictionary info)
            {
                var cb = _callback;
                _callback = null;

                //picker.DismissModalViewController(true);
                picker.DismissViewController(true, null);
                cb (info);
            }
        }

        public static void TakePicture (UIViewController parent, Action<NSDictionary> callback)
        {
            Init ();
            picker.SourceType = UIImagePickerControllerSourceType.Camera;
            _callback = callback;
            //parent.PresentModalViewController (picker, true);
            picker.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
            parent.PresentViewController(picker, true, null);
        }

        public static void SelectPicture (UIViewController parent, Action<NSDictionary> callback)
        {
            Init ();
            picker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
            _callback = callback;
            //parent.PresentModalViewController (picker, true);
            picker.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
            parent.PresentViewController(picker, true, null);
        }
    }
}