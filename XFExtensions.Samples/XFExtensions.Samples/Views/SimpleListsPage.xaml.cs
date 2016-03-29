using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace XFExtensions.Samples
{
    public partial class SimpleListsPage : ContentPage
    {
        public SimpleListsPage()
        {
            InitializeComponent();
            BindingContext = new SimpleListVM();
        }
    }
}

