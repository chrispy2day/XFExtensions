using System;
using System.Windows.Input;
using Xamarin.Forms;
using System.Collections.Generic;
using ModernHttpClient;
using System.Net.Http;
using System.ComponentModel;

namespace XFExtensions.Samples
{
    public class ZoomVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void TriggerPropertyChanged (string propertyName)
        {
            var propertyChanged = PropertyChanged;
            if (propertyChanged != null)
                propertyChanged (this, new PropertyChangedEventArgs (propertyName));
        }
        
        private List<ImageResource> _imageUrls;
        private Random _indexGenerator;

        private enum ImageOrigin { Uri, Stream };
        private struct ImageResource
        {
            public ImageOrigin ImageFrom { get; set; }
            public string ImageSourceText { get; set; }
        }

        public ZoomVM()
        {
            _indexGenerator = new Random();
            _imageUrls = new List<ImageResource>
            {
                new ImageResource{ ImageFrom = ImageOrigin.Uri, ImageSourceText = "http://static.boredpanda.com/blog/wp-content/uuuploads/cute-baby-animals/cute-baby-animals-10.jpg"},
                new ImageResource{ ImageFrom = ImageOrigin.Stream, ImageSourceText = "http://static.boredpanda.com/blog/wp-content/uuuploads/cute-baby-animals/cute-baby-animals-13.jpg"},
                new ImageResource{ ImageFrom = ImageOrigin.Uri, ImageSourceText = "http://yesserver.space.swri.edu/yes2013/personal/emilyklotzbach/tiger.jpg"},
                new ImageResource{ ImageFrom = ImageOrigin.Stream, ImageSourceText = "http://img.brainjet.com/slides/2/8/7/0/1/2/2870120722/afcb62c33c69ff2449a72be7d44919911df1432b.jpeg"},
                new ImageResource{ ImageFrom = ImageOrigin.Uri, ImageSourceText = "http://amazinganimalstories.com/wp-content/uploads/2013/10/cute-baby-animal-pictures-002-015.jpg"},
                new ImageResource{ ImageFrom = ImageOrigin.Uri, ImageSourceText = "http://65.media.tumblr.com/f2caa3272b7015889d6e9e88b6448b84/tumblr_o3zws59Nd31qi4ucgo1_540.jpg"},
                new ImageResource{ ImageFrom = ImageOrigin.Stream, ImageSourceText = "http://amazinganimalstories.com/wp-content/uploads/2013/10/cute-baby-animal-pictures-002-018.jpg"},
                new ImageResource{ ImageFrom = ImageOrigin.Uri, ImageSourceText = "http://static.boredpanda.com/blog/wp-content/uploads/2014/12/cutest-baby-animals-3__605.jpg"},
                new ImageResource{ ImageFrom = ImageOrigin.Uri, ImageSourceText = "http://www.stylemotivation.com/wp-content/uploads/2013/07/cute-baby-animals-3.jpg"}
            };
            
            ToggleZoomCommand = new Command((_) => 
            {
                EnableZoom = !EnableZoom;
            });
            ChangeImageCommand = new Command(async (_) =>
            {
                var index = _indexGenerator.Next(0, _imageUrls.Count - 1);
                var resource = _imageUrls[index];
                ImageComesFrom = Enum.GetName(typeof(ImageOrigin), resource.ImageFrom);
                if (resource.ImageFrom == ImageOrigin.Uri)
                    Image = new UriImageSource() { Uri = new Uri(resource.ImageSourceText)};
                else if (resource.ImageFrom == ImageOrigin.Stream)
                {
                    // this would normally be something off the device, but any stream will do so we'll just manually get a stream to the online image
                    var client = new HttpClient(new NativeMessageHandler());
                    var stream = await client.GetStreamAsync(resource.ImageSourceText);
                    Image = ImageSource.FromStream(() => stream);
                }
                else
                    throw new NotSupportedException($"Unable to load image of type {ImageComesFrom} with value {resource.ImageSourceText}.");
            });

            // enable zoom should initially be false
            EnableZoom = false;

            // initialize the image
            ChangeImageCommand.Execute(null);
        }

        private bool _enableZoom;
        public bool EnableZoom
        {
            get { return _enableZoom; }
            set 
            {
                if (value == _enableZoom)
                    return;
                _enableZoom = value;
                TriggerPropertyChanged (nameof (EnableZoom));
                TriggerPropertyChanged (nameof (EnableScroll));
                TriggerPropertyChanged (nameof (ToggleZoomText));
            }
        }
            
        public bool EnableScroll { get {return _enableZoom; }}
        
        public string ToggleZoomText
        {
            get
            {
                return (EnableZoom) ? "Disable Zoom" : "Enable Zoom";
            }
        }

        public Aspect ImageAspect
        {
            get
            {
                if (EnableZoom)
                    return Aspect.AspectFit;
                else
                    return Aspect.AspectFill;
            }
        }

        private ImageSource _image;
        public ImageSource Image 
        { 
            get { return _image; }
            set
            {
                _image = value;
                TriggerPropertyChanged (nameof (Image));
            }
        }

        private string _imageComesFrom;
        public string ImageComesFrom
        {
            get { return _imageComesFrom; }
            set
            {
                if (value == _imageComesFrom)
                    return;
                _imageComesFrom = value;
                TriggerPropertyChanged (nameof (ImageComesFrom));
            }
        }

        public ICommand ToggleZoomCommand { get; private set; }
        public ICommand ChangeImageCommand { get; private set; }
    }
}

