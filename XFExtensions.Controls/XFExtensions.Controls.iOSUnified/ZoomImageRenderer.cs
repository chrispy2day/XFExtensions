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

[assembly: ExportRenderer(typeof(ZoomImage), typeof(ZoomImageRenderer))]
namespace XFExtensions.Controls.iOSUnified
{
    public class ZoomImageRenderer : ViewRenderer<ZoomImage, UIScrollView>
    {
        private ZoomImage _zoomImage;
        private UIScrollView _scrollView;
        private UIImageView _imageView;
        private ImageRenderer _imageRenderer;

        protected override void OnElementChanged(ElementChangedEventArgs<ZoomImage> e)
        {
            if (this.Control == null && e.NewElement != null)
            {
                // setup the control to be a scroll view with an image in it
                _zoomImage = e.NewElement;
                var webSource = (UriImageSource) _zoomImage.Source;

                // for testing purposes create the image view myself
                //var url = webSource.Uri.ToString();
                //var imageView = new UIImageView(UIImage.LoadFromData(NSData.FromUrl(NSUrl.FromString(url))));

                // prepare the image view
                _imageRenderer = new ImageRenderer();
                _imageRenderer.SetElement(_zoomImage);
                _imageView = _imageRenderer.Control;
                // make sure to size the image view to the image it contains
                _imageView.SizeToFit();
                //imageView.AutoresizingMask = UIViewAutoresizing.All;

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

        private void SetZoomToAspect()
        {
            // the min and max zoom provided by the zoom control will be based on whatever initial scale is determined here
            // so 10X max will be 10 x original zoom and similiarly for min zoom

            // get the scale for each dimension
            var wScale = _scrollView.Frame.Width / _imageView.Image.Size.Width;
            var hScale = _scrollView.Frame.Height / _imageView.Image.Size.Height;
            nfloat scale;

            switch (_zoomImage.Aspect)
            {
                case Aspect.AspectFill:
                case Aspect.Fill:
                    // fill the view, so scale to the larger of the two scales
                    scale = (nfloat)Math.Max(wScale, hScale);
                    break;
                default:
                    // fit the full image, so scale to the smaller of the two scales
                    scale = (nfloat)Math.Min(wScale, hScale);
                    break;
            }

            _scrollView.MinimumZoomScale = (nfloat)_zoomImage.MinZoom * scale;
            _scrollView.MaximumZoomScale = (nfloat)_zoomImage.MaxZoom * scale;

            _scrollView.SetZoomScale(scale, true);
        }

        private void SetupZoom()
        {
            if (_scrollView == null || _zoomImage == null || _imageView == null)
                return;

            // set the scale
            SetZoomToAspect();

            // enable / disable zooming
            if (_zoomImage.ZoomEnabled)
            {
                // enable the pinch gesture recongnizer for default functionality
                _scrollView.PinchGestureRecognizer.Enabled = true;
            }
            else
            {
                // reset the image to normal size and position
                SetZoomToAspect();

                // disable pinch gesture so other controls can listen for it
                _scrollView.PinchGestureRecognizer.Enabled = false;
            }

            SetNeedsDisplay();
            //this.SetNeedsLayout();
        }

        private CancellationTokenSource _propertyUpdateCancel;
        protected override async void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == ZoomImage.WidthProperty.PropertyName)
            {
                var width = this.Element.Width;
            }

            if (_propertyUpdateCancel != null)
            {
                _propertyUpdateCancel.Cancel();
            }
            _propertyUpdateCancel = new CancellationTokenSource();
            try
            {
                await Task.Delay(100, _propertyUpdateCancel.Token);
                SetupZoom();
            }
            catch (TaskCanceledException)
            {
                // another update occurred so we don't need to do anything
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

