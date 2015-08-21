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
using com.refractored.monodroidtoolkit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using XFExtensions.Controls.Abstractions;
using XFExtensions.Controls.Droid;

[assembly: ExportRenderer(typeof(ZoomImage), typeof(ZoomImageRenderer))]
namespace XFExtensions.Controls.Droid
{
    public class ZoomImageRenderer : ImageRenderer
    {
        private ImageView _normalImage;
        private ScaleImageView _scaleImage;

        protected async override void OnElementChanged(ElementChangedEventArgs<Image> e)
        {
            base.OnElementChanged(e);
            _normalImage = Control;

            var zoomImage = (ZoomImage)e.NewElement;
            if (zoomImage == null || !zoomImage.ZoomEnabled)
                return;

            // use the scale image control instead
            _scaleImage = await CreateScaleView(zoomImage.Source);
            SetNativeControl(_scaleImage);
        }

        protected async override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            var zoomImage = (ZoomImage)Element;

            if (e.PropertyName == ZoomImage.ZoomEnabledProperty.PropertyName)
            {
                if (zoomImage.ZoomEnabled)
                {
                    // create a scale image if one doesn't already exist
                    if (_scaleImage == null)
                        _scaleImage = await CreateScaleView(zoomImage.Source);
                    SetNativeControl(_scaleImage);
                }
                else
                {
                    SetNativeControl(_normalImage);

                    // remove the scale image
                    if (_scaleImage != null)
                    {
                        _scaleImage.Dispose();
                        _scaleImage = null;
                    }
                }
            }
        }

        private async Task<ScaleImageView> CreateScaleView(ImageSource source)
        {
            var scale = new ScaleImageView(Context, null);
            var handler = new ImageLoaderSourceHandler();
            var image = await handler.LoadImageAsync(source, Context);
            scale.SetImageBitmap(image);
            return scale;
        }
    }
}