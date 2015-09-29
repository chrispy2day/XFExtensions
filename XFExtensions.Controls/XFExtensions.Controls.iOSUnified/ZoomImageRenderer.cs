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
        private ImageRenderer _imageRenderer;

        protected override void OnElementChanged(ElementChangedEventArgs<ZoomImage> e)
        {
            if (this.Control == null && e.NewElement != null)
            {
                // setup the control to be a scroll view with an image in it
                var zoomImage = e.NewElement;
                var webSource = (UriImageSource) zoomImage.Source;

                // for testing purposes create the image view myself
                //var url = webSource.Uri.ToString();
                //var imageView = new UIImageView(UIImage.LoadFromData(NSData.FromUrl(NSUrl.FromString(url))));

                _imageRenderer = new ImageRenderer();
                _imageRenderer.SetElement(zoomImage);
                var imageView = _imageRenderer.Control;
                imageView.SizeToFit();
                //imageView.AutoresizingMask = UIViewAutoresizing.All;
                var scroll = new UIScrollView
                {
                    ClipsToBounds = true,
                    BackgroundColor = UIColor.Red,
                    ContentMode = zoomImage.Aspect.ToUIViewContentMode(),
                    ContentSize = imageView.Frame.Size
                };
                scroll.AddSubview(imageView);
                scroll.ViewForZoomingInScrollView += (view) => imageView;
                
                //var scroll = new UIScrollView { BackgroundColor = UIColor.Red, AutoresizingMask = UIViewAutoresizing.All };
                //scroll.ContentSize = imageView.Image.Size;
                //scroll.AddSubview(imageView);
                //scroll.MinimumZoomScale = 1;
                //scroll.MaximumZoomScale = 5;
                //scroll.ViewForZoomingInScrollView += view => imageView;
                
                this.SetNativeControl(scroll);
            }

            base.OnElementChanged(e);
        }

        private void SetupZoom()
        {
            var scroll = this.Control;
            var zoomImage = this.Element;
            if (scroll == null || zoomImage == null)
                return;

            var imageView = (UIImageView)scroll.Subviews[0];
            if (imageView == null)
                return;

            //scroll.ContentMode = UIViewContentMode.ScaleAspectFit;
            //imageView.SizeToFit();
            //imageView.Frame = scroll.Bounds; //new CGRect(0, 0, imageView.Image.Size.Width, imageView.Image.Size.Height);
            //imageView.ContentMode = zoomImage.Aspect.ToUIViewContentMode();
            //scroll.ContentSize = imageView.Frame.Size; //imageView.Image.Size;

            // set the scale
            var wScale = scroll.Frame.Width / imageView.Image.Size.Width;
            var hScale = scroll.Frame.Height / imageView.Image.Size.Height;

            //scroll.SetZoomScale(wScale, true);
            //scroll.ZoomScale = wScale;

            //scroll.ContentMode = zoomImage.Aspect.ToUIViewContentMode();
            //imageView.Frame = new CGRect(0, 0, imageView.Image.Size.Width, imageView.Image.Size.Height);
            //scroll.ContentSize = new CGSize(imageView.Image.CGImage.Width, imageView.Image.CGImage.Height);

            // set the size and aspect
            //scroll.ContentSize = new CGSize(zoomImage.WidthRequest, zoomImage.HeightRequest);
            //scroll.ContentMode = zoomImage.Aspect.ToUIViewContentMode();

            // set the zoom scale
            scroll.MinimumZoomScale = wScale;//(float)zoomImage.MinZoom;
            scroll.MaximumZoomScale = (float)zoomImage.MaxZoom;

            scroll.ViewForZoomingInScrollView += ViewForZoomingInScrollView;
            scroll.PinchGestureRecognizer.Enabled = true;

            // enable / disable zooming
            if (zoomImage.ZoomEnabled)
            {
                //scroll.ViewForZoomingInScrollView += ViewForZoomingInScrollView;
                // enable the pinch gesture recongnizer for default functionality
                scroll.PinchGestureRecognizer.Enabled = true;
            }
            else
            {
                // reset the image to normal size and position
                scroll.SetZoomScale(wScale, true);//1.0f, true);

                // turn off scrolling if it was previously enabled by removing the delegate
//                if (scroll.ViewForZoomingInScrollView != null)
//                    scroll.ViewForZoomingInScrollView -= ViewForZoomingInScrollView;
                // disable pinch gesture so other controls can listen for it
                scroll.PinchGestureRecognizer.Enabled = false;
            }

            // enable / disable double tap to zoom (note that zooming must be enabled for this to actually zoom, regardless of this setting)
            var tapGesture = scroll.GestureRecognizers.SingleOrDefault(g => g is UITapGestureRecognizer);
            if (tapGesture != null)
                scroll.RemoveGestureRecognizer(tapGesture); // always remove this and then readd it if necessary to pick up scale changes
            if (zoomImage.DoubleTapToZoomEnabled)
            {
                scroll.AddGestureRecognizer(
                    new UITapGestureRecognizer((gest) =>
                        {
                            var location = gest.LocationOfTouch(0, scroll);
                            scroll.ZoomToRect(GenerateZoomRect(scroll, (float)zoomImage.TapZoomScale, location), true);
                        }) { NumberOfTapsRequired = 2 }
                );
            }
            SetNeedsDisplay();
            //this.SetNeedsLayout();
        }

        private UIView ViewForZoomingInScrollView(UIScrollView scrollView)
        {
            return scrollView.Subviews[0];
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

