using System;
using System.Windows.Input;
using Xamarin.Forms;
using PropertyChanged;
using System.Collections.Generic;

namespace XFExtensions.Samples
{
    [ImplementPropertyChanged]
    public class ZoomVM
    {
        private List<string> _imageUrls;
        private Random _indexGenerator;

        public ZoomVM()
        {
            _indexGenerator = new Random();
            _imageUrls = new List<string>
            {
                "http://yeahsoup.s3-us-west-2.amazonaws.com/wp-content/uploads/2015/05/img1114.jpg",
                "http://i.telegraph.co.uk/multimedia/archive/02262/A124CE_2262003b.jpg",
                "http://i.telegraph.co.uk/multimedia/archive/01476/chimp_1476818c.jpg",
                "http://i.huffpost.com/gen/1490756/images/o-CHIMPANZEE-facebook.jpg",
                "http://static1.squarespace.com/static/523b539be4b0a75330f9c8ce/55a55614e4b01d30adbfe144/55a557ace4b0632463d49108/1436909257139/babyowl.jpg?format=1000w",
                "http://go-grafix.com/data/wallpapers/4/baby-animals-517582-1440x1080-hq-dsk-wallpapers.jpg",
                "http://www.rantlifestyle.com/wp-content/uploads/2014/06/schattigebabydier11.jpg",
                "http://ww2.valdosta.edu/~kaletour/bb1.jpg",
                "https://36.media.tumblr.com/5c493da746cc1c1f438ae304591244c4/tumblr_n9a78n99Ea1tvs3v3o1_500.jpg",
                "http://streetloop.com/wp-content/uploads/2014/07/Baby-animals-looking-like-their-parents25.jpg"
            };
            
            ToggleZoomCommand = new Command((_) => 
            {
                EnableZoom = !EnableZoom;
            });
            ChangeImageCommand = new Command((_) =>
            {
                var index = _indexGenerator.Next(0, _imageUrls.Count - 1);
                Image = new UriImageSource() { Uri = new Uri(_imageUrls[index])};
            });

            // initialize the image
            //ChangeImageCommand.Execute(null);
            Image = new UriImageSource() { Uri = new Uri(_imageUrls[0])};
        }

        public bool EnableZoom { get; set; }
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

        public ICommand ToggleZoomCommand { get; private set; }
        public ICommand ChangeImageCommand { get; private set; }
    }
}

