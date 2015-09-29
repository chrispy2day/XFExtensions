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
    }
}

