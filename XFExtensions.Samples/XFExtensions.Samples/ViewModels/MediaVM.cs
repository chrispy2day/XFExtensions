using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MetaMediaPlugin;
using Xamarin.Forms;
using PropertyChanged;

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
                    var pic = await MetaMedia.Current.PickPhotoAsync();
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
