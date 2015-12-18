using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using XFExtensions.Controls.Abstractions;
using XFExtensions.Controls.Droid;

[assembly: ExportRenderer(typeof(ZoomImage), typeof(ZoomImageRenderer))]
namespace XFExtensions.Controls.Droid
{
    public class ZoomImageRenderer : ImageRenderer
    {
        private ZoomImage _zoomImage;
        private ScaleImageView _scaleImage;

        protected async override void OnElementChanged(ElementChangedEventArgs<Image> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                _zoomImage = (ZoomImage)e.NewElement;

                // create the scale image and set it as the native control so it's available
                _scaleImage = new ScaleImageView(Context, null);
                _scaleImage.ZoomImage = _zoomImage;
                SetNativeControl(_scaleImage);
                await LoadImage();
            }
        }

        protected async override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == ZoomImage.AspectProperty.PropertyName
                || e.PropertyName == ZoomImage.HeightProperty.PropertyName
                || e.PropertyName == ZoomImage.WidthProperty.PropertyName)
            {
                _scaleImage.ZoomToAspect();
            }
            else if (e.PropertyName == ZoomImage.SourceProperty.PropertyName)
            {
                await LoadImage();
                _scaleImage.ZoomToAspect();
            }
            else if (e.PropertyName == ZoomImage.CurrentZoomProperty.PropertyName)
            {
                _scaleImage.ZoomFromCurrentZoom();
            }
            else if (e.PropertyName == ZoomImage.MaxZoomProperty.PropertyName)
            {
                _scaleImage.UpdateMaxScaleFromZoomImage();
            }
            else if (e.PropertyName == ZoomImage.MinZoomProperty.PropertyName)
            {
                _scaleImage.UpdateMinScaleFromZoomImage();
            }
        }

        private async Task LoadImage()
        {
            var image = await (new ImageLoaderSourceHandler()).LoadImageAsync(_zoomImage.Source, Context);
            // would seem like the below would offer more flexibility, but actually is causing an exception
//            if (image == null)
//                image = await (new StreamImagesourceHandler()).LoadImageAsync(_zoomImage.Source, Context);
//            if (image == null)
//                image = await (new FileImageSourceHandler()).LoadImageAsync(_zoomImage.Source, Context);
            _scaleImage.SetImageBitmap(image);
        }
    }
}