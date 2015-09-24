using System;
using System.Collections.Generic;

using Xamarin.Forms;
using System.Threading.Tasks;

namespace XFExtensions.Samples
{
    public partial class ZoomPage : ContentPage
    {
        public ZoomPage()
        {
            InitializeComponent();
            BindingContext = new ZoomVM();
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            while (enableButton.Height < 10)
                await Task.Delay(50);
            var underButton = enableButton.Y + enableButton.Height + 10;
            zoomableImage.HeightRequest = this.Height - underButton;
            zoomableImage.WidthRequest = this.Width;
        }
    }
}

