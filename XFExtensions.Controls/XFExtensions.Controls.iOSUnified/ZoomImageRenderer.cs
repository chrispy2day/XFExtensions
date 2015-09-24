using System;
using Xamarin.Forms;
using XFExtensions.Controls.Abstractions;
using XFExtensions.Controls.iOSUnified;
using Xamarin.Forms.Platform.iOS;
using UIKit;
using CoreGraphics;
using System.ComponentModel;
using System.Linq;

[assembly: ExportRenderer(typeof(ZoomImage), typeof(ZoomImageRenderer))]
namespace XFExtensions.Controls.iOSUnified
{
    public class ZoomImageRenderer : ViewRenderer<ZoomImage, UIScrollView>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<ZoomImage> e)
        {
            if (this.Control == null && e.NewElement != null)
            {
                // setup the control to be a scroll view with an image in it
                var zoomImage = e.NewElement;
                var imageRenderer = new ImageRenderer();
                imageRenderer.SetElement(zoomImage);
                var imageView = imageRenderer.Control;
                imageView.AutoresizingMask = UIViewAutoresizing.All;
                var scroll = new UIScrollView(CGRect.Empty)
                    {
                        ClipsToBounds = true
                    };
                scroll.AddSubview(imageView);
                SetupZoom(scroll, zoomImage);
                this.SetNativeControl(scroll);
            }
            base.OnElementChanged(e);
        }

        private void SetupZoom(UIScrollView scroll, ZoomImage zoomImage)
        {
            if (scroll == null || zoomImage == null)
                return;

            // set the size and aspect
            scroll.ContentSize = new CGSize(zoomImage.WidthRequest, zoomImage.HeightRequest);
            scroll.ContentMode = zoomImage.Aspect.ToUIViewContentMode();

            // set the zoom scale
            scroll.MinimumZoomScale = (float)zoomImage.MinZoom;
            scroll.MaximumZoomScale = (float)zoomImage.MaxZoom;

            // enable / disable zooming
            if (zoomImage.ZoomEnabled && scroll.ViewForZoomingInScrollView == null)
            {
                scroll.ViewForZoomingInScrollView += ViewForZoomingInScrollView;
                // enable the pinch gesture recongnizer for default functionality
                scroll.PinchGestureRecognizer.Enabled = true;
            }
            else
            {
                // reset the image to normal size and position
                scroll.SetZoomScale(1.0f, true);

                // turn off scrolling if it was previously enabled by removing the delegate
                if (scroll.ViewForZoomingInScrollView != null)
                    scroll.ViewForZoomingInScrollView -= ViewForZoomingInScrollView;
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
        }

        private UIView ViewForZoomingInScrollView(UIScrollView scrollView)
        {
            return scrollView.Subviews[0];
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            SetupZoom(this.Control, this.Element);
            SetNeedsDisplay();
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

