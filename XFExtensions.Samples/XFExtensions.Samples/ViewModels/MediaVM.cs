using System.Diagnostics;
using System.Windows.Input;
using MetaMediaPlugin;
using PropertyChanged;
using Xamarin.Forms;
using System.IO;
using System;
using MetaMediaPlugin.Abstractions;

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
                    var pic = await MetaMedia.Current.PickPhotoAsync();
                    if (pic != null)
                    {
                        var stream = await pic.GetPreviewFileStreamAsync();
                        SelectedImage = ImageSource.FromStream(() => stream);
                    }
                }, 
                o => MetaMedia.Current.IsPickPhotoSupported);
            
            TakePhotoCommand = new Command(async (_) =>
                {
                    var pic = await MetaMedia.Current.TakePhotoAsync();
                    if (pic != null)
                    {
                        var stream = await pic.GetPreviewFileStreamAsync();
                        SelectedImage = ImageSource.FromStream(() => stream);
                    }
                },
                o => MetaMedia.Current.IsTakePhotoSupported);
        }

        public ImageSource SelectedImage { get; set; }
    }
}
