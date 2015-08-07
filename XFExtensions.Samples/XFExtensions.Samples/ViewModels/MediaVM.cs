using System.Diagnostics;
using System.Windows.Input;
using MetaMediaPlugin;
using PropertyChanged;
using Xamarin.Forms;
using System.IO;

namespace XFExtensions.Samples.ViewModels
{
    [ImplementPropertyChanged]
    public class MediaVM
    {
        public ICommand ChoosePhotoCommand { get; private set; }
        public ICommand TakePhotoCommand { get; private set; }

        public MediaVM()
        {
            ChoosePhotoCommand = new Command(async (_) => 
                {
                    Debug.WriteLine("Starting the Pick Photo Intent");
                    using (var pic = await MetaMedia.Current.PickPhotoAsync())
                    {
                        Debug.WriteLine("Picture returned is {0}", (pic == null) ? "empty" : "valid");
                        if (pic != null)
                        {
                            SelectedImage = ImageSource.FromStream(() => pic.MediaStream);
                            pic.MediaStream.Dispose();
                        }
                    }
                }, 
                o => MetaMedia.Current.IsPickPhotoSupported);
            TakePhotoCommand = new Command(async (_) =>
                {
                    using (var pic = await MetaMedia.Current.TakePhotoAsync())
                    {
                        if (pic != null)
                            SelectedImage = ImageSource.FromStream(() => pic.MediaStream);
                    }
                },
                o => MetaMedia.Current.IsTakePhotoSupported);
        }

        public ImageSource SelectedImage { get; set; }
    }
}
