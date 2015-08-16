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
                    Debug.WriteLine("Starting the Pick Photo Intent");
                    MediaFile pic = null;
                    try
                    {
                        pic = await MetaMedia.Current.PickPhotoAsync();
                    }
                    catch(Exception e)
                    {
                        var message = e.Message;
                    }
                    Debug.WriteLine("Picture returned is {0}", (pic == null) ? "empty" : "valid");
                    if (pic != null)
                    {
                        try
                        {
                            using (var stream = pic.GetPreviewStream())
                                SelectedImage = ImageSource.FromStream(() => stream);
                        }
                        catch(Exception e)
                        {
                            var message = e.Message;
                        }
                    }
                }, 
                o => MetaMedia.Current.IsPickPhotoSupported);
            
            TakePhotoCommand = new Command(async (_) =>
                {
                    var pic = await MetaMedia.Current.TakePhotoAsync();
                    if (pic != null)
                    {
                        using (var stream = pic.GetPreviewStream())
                        {
                            SelectedImage = ImageSource.FromStream(() => stream);
                        }
                    }
                },
                o => MetaMedia.Current.IsTakePhotoSupported);
        }

        public ImageSource SelectedImage { get; set; }
    }
}
