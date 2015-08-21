using System;
using System.Windows.Input;
using Xamarin.Forms;
using PropertyChanged;

namespace XFExtensions.Samples
{
    [ImplementPropertyChanged]
    public class ZoomVM
    {
        public ZoomVM()
        {
            ToggleZoomCommand = new Command((_) => 
            {
                EnableZoom = !EnableZoom;
            });
        }

        public bool EnableZoom { get; set; }
        public string ToggleZoomText
        {
            get
            {
                return (EnableZoom) ? "Disable Zoom" : "Enable Zoom";
            }
        }
        public ICommand ToggleZoomCommand { get; private set; }
    }
}

