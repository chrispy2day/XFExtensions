using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace XFExtensions.Samples
{
    public partial class ZoomPage : ContentPage
    {
        public ZoomPage()
        {
            InitializeComponent();
            BindingContext = new ZoomVM();
        }
    }
}

