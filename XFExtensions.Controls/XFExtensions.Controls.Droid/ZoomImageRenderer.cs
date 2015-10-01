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
            //base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                _zoomImage = (ZoomImage)e.NewElement;

                // create the scale image and set it as the native control so it's available
                _scaleImage = new ScaleImageView(Context, null);
                SetNativeControl(_scaleImage);
                await LoadImage();
            }
        }

        protected async override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
        }

        private async Task LoadImage()
        {
            var handler = new ImageLoaderSourceHandler();
            var image = await handler.LoadImageAsync(_zoomImage.Source, Context);
            _scaleImage.SetImageBitmap(image);
        }
    }
}