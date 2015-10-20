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
        private Stream _previewStream;

        public MediaVM()
        {
            FullStreamLength = PreviewStreamLength = "Unknown - no image selected yet.";
            ChoosePhotoCommand = new Command(async (_) => 
                {
                    var pic = await MetaMedia.Current.PickPhotoAsync();
                    if (pic != null)
                    {
                        _previewStream = await pic.GetPreviewFileStreamAsync();
                        PreviewStreamLength = CalculateLength(_previewStream.Length);
                        SelectedImage = ImageSource.FromStream(() => _previewStream); // image source will dispose of stream when it's done

                        using (var stream = await pic.GetFullFileStreamAsync())
                        {
                            FullStreamLength = CalculateLength(stream.Length);
                        }
                    }
                }, 
                o => MetaMedia.Current.IsPickPhotoSupported);
            
            TakePhotoCommand = new Command(async (_) =>
                {
                    var pic = await MetaMedia.Current.TakePhotoAsync();
                    if (pic != null)
                    {
                        _previewStream = await pic.GetPreviewFileStreamAsync();
                        PreviewStreamLength = CalculateLength(_previewStream.Length);
                        SelectedImage = ImageSource.FromStream(() => _previewStream); // image source will dispose of stream when it's done
                        using (var stream = await pic.GetFullFileStreamAsync())
                        {
                            FullStreamLength = CalculateLength(stream.Length);
                        }
                    }
                },
                o => MetaMedia.Current.IsTakePhotoSupported);

            NewPicCommand = new Command((_) =>
                {
                });
        }

        public ICommand ChoosePhotoCommand { get; private set; }
        public ICommand TakePhotoCommand { get; private set; }
        public ICommand NewPicCommand { get; private set; }

        private string CalculateLength(long length)
        {
            var scaledLength = (double)length;
            var scale = 0;
            while (scaledLength > 1024)
            {
                scaledLength /= 1024;
                scale++;
            }
            string unit;
            switch (scale)
            {
                case 0:
                    unit = "B";
                    break;
                case 1:
                    unit = "KB";
                    break;
                case 2:
                    unit = "MB";
                    break;
                case 3:
                    unit = "GB";
                    break;
                default:
                    unit = "Err - too big";
                    break;
            }
            return string.Format("{0:F2} {1}", scaledLength, unit);
        }

        public ImageSource SelectedImage { get; set; }

        public string PreviewStreamLength { get; set; }
        public string FullStreamLength {get; set; }
    }
}
