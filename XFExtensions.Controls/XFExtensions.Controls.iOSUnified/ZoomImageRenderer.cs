using System;
using Xamarin.Forms;
using XFExtensions.Controls.Abstractions;
using XFExtensions.Controls.iOSUnified;
using Xamarin.Forms.Platform.iOS;
using UIKit;
using CoreGraphics;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using System.Net.Http;
using System.Net;

[assembly: ExportRenderer(typeof(ZoomImage), typeof(ZoomImageRenderer))]
namespace XFExtensions.Controls.iOSUnified
{
    public class ZoomImageRenderer : ViewRenderer<ZoomImage, UIScrollView>
    {
        private ZoomImage _zoomImage;
        private UIScrollView _scrollView;
        private UIImageView _imageView;
        private ImageRenderer _imageRenderer;
        private nfloat _baseScalingFactor;

        protected override void OnElementChanged(ElementChangedEventArgs<ZoomImage> e)
        {
            if (this.Control == null && e.NewElement != null)
            {
                // setup the control to be a scroll view with an image in it
                _zoomImage = e.NewElement;

                // for testing purposes create the image view myself
//                var wc = new WebClient();
//                byte[] imageBytes = wc.DownloadData("http://octodex.github.com/images/Professortocat_v2.png");
//                var iosData = NSData.FromArray(imageBytes);
//                _imageView = new UIImageView(UIImage.LoadFromData(iosData));

                var webSource = _zoomImage.Source as UriImageSource;
                if (webSource != null)
                {
                    var url = webSource.Uri.ToString();
                    var data = NSData.FromUrl(NSUrl.FromString(url));
                    _imageView = new UIImageView(UIImage.LoadFromData(data));
                }
                else
                {
                    // use a file source
                }

                // prepare the image view
//                _imageRenderer = new ImageRenderer();
//                _imageRenderer.SetElement(_zoomImage);
//                _imageView = _imageRenderer.Control;
                // make sure to size the image view to the image it contains
                _imageView.SizeToFit();

                // create the scroll view
                _scrollView = new UIScrollView
                {
                    ClipsToBounds = true,
                    BackgroundColor = UIColor.Red,
                    ContentMode = _zoomImage.Aspect.ToUIViewContentMode(),
                    ContentSize = _imageView.Frame.Size,
                    ScrollEnabled = _zoomImage.ScrollEnabled
                };
                // add the image view to it
                _scrollView.AddSubview(_imageView);
                // setup the zooming and double tap
                _scrollView.ViewForZoomingInScrollView += (view) => _imageView;
                _scrollView.AddGestureRecognizer(
                    new UITapGestureRecognizer((gest) =>
                    {
                        if (_zoomImage.DoubleTapToZoomEnabled)
                        {
                                var location = gest.LocationOfTouch(0, _scrollView);
                                _scrollView.ZoomToRect(GenerateZoomRect(_scrollView, (float)_zoomImage.TapZoomScale, location), true);
                        }
                    }) 
                    { NumberOfTapsRequired = 2 }
                );
                this.SetNativeControl(_scrollView);
            }

            base.OnElementChanged(e);
        }

        private void SetZoomToAspect(bool reapplyCurrentScale = false)
        {
            // the min and max zoom provided by the zoom control will be based on whatever initial scale is determined here
            // so 10X max will be 10 x original zoom and similiarly for min zoom

            if (_scrollView == null || _imageView == null || _imageView.Image == null)
                return;

            // if the scroll view doesn't have any size, just exit
            if (_scrollView.Frame.Width == 0 || _scrollView.Frame.Height == 0)
                return;

            if (_baseScalingFactor == 0)
                reapplyCurrentScale = false;
            
            // if reapplying the current scale, hold on to what it currently is without the base scaling factor (which may change)
            nfloat oldScale = 0;
            if (reapplyCurrentScale)
                oldScale = _scrollView.ZoomScale / _baseScalingFactor;

            // get the scale for each dimension
            var wScale = _scrollView.Frame.Width / _imageView.Image.Size.Width;
            var hScale = _scrollView.Frame.Height / _imageView.Image.Size.Height;

            // determine the base scaling factor to use based on the requested aspect
            switch (_zoomImage.Aspect)
            {
                case Aspect.AspectFill:
                case Aspect.Fill:
                    // fill the view, so scale to the larger of the two scales
                    _baseScalingFactor = (nfloat)Math.Max(wScale, hScale);
                    break;
                default:
                    // fit the full image, so scale to the smaller of the two scales
                    _baseScalingFactor = (nfloat)Math.Min(wScale, hScale);
                    break;
            }

            // assign the min and max zooms based on the user request and base scaling factor
            _scrollView.MinimumZoomScale = (nfloat)_zoomImage.MinZoom * _baseScalingFactor;
            _scrollView.MaximumZoomScale = (nfloat)_zoomImage.MaxZoom * _baseScalingFactor;

            // center image when filling the screen
            var widthDiff = (_imageView.Bounds.Width * _baseScalingFactor) - _scrollView.Bounds.Width;
            var heightDiff = (_imageView.Bounds.Height * _baseScalingFactor) - _scrollView.Bounds.Height;
            _scrollView.ContentOffset = new CGPoint(
                Math.Max(widthDiff / 2, 0), 
                Math.Max(heightDiff / 2, 0));
            // center the image in the scroll when image is smaller than the scroll view
            var inset = new UIEdgeInsets();
            if (widthDiff < 0)
                inset.Left = (nfloat)Math.Abs(widthDiff) / 2;
            if (heightDiff < 0)
                inset.Top = (nfloat)Math.Abs(heightDiff) / 2;
            _scrollView.ContentInset = inset;

            // set the current scale
            if (reapplyCurrentScale)
                _scrollView.SetZoomScale(oldScale * _baseScalingFactor, true);
            else
                _scrollView.SetZoomScale(_baseScalingFactor, true);
        }

        protected override async void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == ZoomImage.CurrentZoomProperty.PropertyName)
            {
                var scale = (nfloat)_zoomImage.Scale * _baseScalingFactor;
                _scrollView.SetZoomScale(scale, true);
            }

            if (e.PropertyName == ZoomImage.WidthProperty.PropertyName 
                || e.PropertyName == ZoomImage.HeightProperty.PropertyName)
            {
                await Task.Delay(50); // give a short delay for changes to be applied to the frame
                SetZoomToAspect(true); // reapply the current scale
                SetNeedsDisplay();
            }

            if (e.PropertyName == ZoomImage.AspectProperty.PropertyName)
            {
                SetZoomToAspect();
            }

            if (e.PropertyName == ZoomImage.MaxZoomProperty.PropertyName)
            {
                _scrollView.MaximumZoomScale = (nfloat)_zoomImage.MaxZoom * _baseScalingFactor;
            }
            if (e.PropertyName == ZoomImage.MinZoomProperty.PropertyName)
            {
                _scrollView.MaximumZoomScale = (nfloat)_zoomImage.MinZoom * _baseScalingFactor;
            }
            if (e.PropertyName == ZoomImage.ScrollEnabledProperty.PropertyName)
            {
                _scrollView.ScrollEnabled = _zoomImage.ScrollEnabled;
            }
            if (e.PropertyName == ZoomImage.ZoomEnabledProperty.PropertyName)
            {
                _scrollView.PinchGestureRecognizer.Enabled = _zoomImage.ZoomEnabled;
                // if zoom is disabled, return to aspect view
                if (!_zoomImage.ZoomEnabled)
                    SetZoomToAspect();
            }
        }

        private CGRect GenerateZoomRect(UIScrollView scrollView, float scaleFactor, CGPoint point)
        {
            nfloat scale;
            if (scrollView.ZoomScale < scrollView.MaximumZoomScale)
            {
                // not at max zoom so zoom in
                scale = (nfloat)Math.Min(scrollView.ZoomScale * scaleFactor, scrollView.MaximumZoomScale);
            }
            else
            {
                // already at max zoom so zoom out
                scale = (nfloat)Math.Max(scrollView.ZoomScale / scaleFactor, scrollView.MinimumZoomScale);
            }

            // note that the point location is from the top left of the image and is measured in the scaled size
            CGRect zoomRect = new CGRect
                {
                    Height = scrollView.Frame.Height / scale,
                    Width = scrollView.Frame.Width / scale,
                    X = (point.X / scrollView.ZoomScale) - (scrollView.Frame.Width / (scale * 2.0f)), // half the width
                    Y = (point.Y / scrollView.ZoomScale) - (scrollView.Frame.Height / (scale * 2.0f)) // half the height
                };

            return zoomRect;
        }
    }
}

