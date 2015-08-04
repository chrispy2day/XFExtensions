using System.Diagnostics;
using System.Windows.Input;
using MetaMediaPlugin;
using PropertyChanged;
using Xamarin.Forms;

namespace XFExtensions.Samples.ViewModels
{
    [ImplementPropertyChanged]
    public class MediaVM
    {
        public ICommand ChoosePhotoCommand { get; private set; }

        public MediaVM()
        {
            ChoosePhotoCommand = new Command(async (_) => 
                {
                    Debug.WriteLine("Starting the Pick Photo Intent");
                    var pic = await MetaMedia.Current.PickPhotoAsync();
                    Debug.WriteLine("Picture returned is {0}", (pic == null) ? "empty" : "valid");
                    if (pic != null)
                        SelectedImage = ImageSource.FromStream(() =>
                            {
                                var stream = pic.GetStream();
                                pic.Dispose();
                                return stream;
                            });
                }, 
                o => MetaMedia.Current.IsPickPhotoSupported);
        }

        public ImageSource SelectedImage { get; set; }
    }
}
