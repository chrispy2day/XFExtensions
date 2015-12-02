using System;
using System.Windows.Input;
using Xamarin.Forms;
using PropertyChanged;
using System.Collections.Generic;
using ModernHttpClient;
using System.Net;
using System.Net.Http;

namespace XFExtensions.Samples
{
    [ImplementPropertyChanged]
    public class ZoomVM
    {
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
                new ImageResource{ ImageFrom = ImageOrigin.Uri, ImageSourceText = "http://yeahsoup.s3-us-west-2.amazonaws.com/wp-content/uploads/2015/05/img1114.jpg"},
                new ImageResource{ ImageFrom = ImageOrigin.Stream, ImageSourceText = "http://i.telegraph.co.uk/multimedia/archive/02262/A124CE_2262003b.jpg"},
                new ImageResource{ ImageFrom = ImageOrigin.Uri, ImageSourceText = "http://i.telegraph.co.uk/multimedia/archive/01476/chimp_1476818c.jpg"},
                new ImageResource{ ImageFrom = ImageOrigin.Stream, ImageSourceText = "http://i.huffpost.com/gen/1490756/images/o-CHIMPANZEE-facebook.jpg"},
                new ImageResource{ ImageFrom = ImageOrigin.Uri, ImageSourceText = "http://static1.squarespace.com/static/523b539be4b0a75330f9c8ce/55a55614e4b01d30adbfe144/55a557ace4b0632463d49108/1436909257139/babyowl.jpg?format=1000w"},
                new ImageResource{ ImageFrom = ImageOrigin.Uri, ImageSourceText = "http://www.rantlifestyle.com/wp-content/uploads/2014/06/schattigebabydier11.jpg"},
                new ImageResource{ ImageFrom = ImageOrigin.Stream, ImageSourceText = "http://ww2.valdosta.edu/~kaletour/bb1.jpg"},
                new ImageResource{ ImageFrom = ImageOrigin.Uri, ImageSourceText = "https://36.media.tumblr.com/5c493da746cc1c1f438ae304591244c4/tumblr_n9a78n99Ea1tvs3v3o1_500.jpg"},
                new ImageResource{ ImageFrom = ImageOrigin.Uri, ImageSourceText = "http://streetloop.com/wp-content/uploads/2014/07/Baby-animals-looking-like-their-parents25.jpg"}
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

        public bool EnableZoom { get; set; }
        public bool EnableScroll { get {return EnableZoom; }}
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

        public ImageSource Image { get; set; }

        public string ImageComesFrom { get; set; }

        public ICommand ToggleZoomCommand { get; private set; }
        public ICommand ChangeImageCommand { get; private set; }
    }
}

