using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using PropertyChanged;
using Xamarin.Forms;

namespace XFExtensions.Samples.ViewModels
{
    [ImplementPropertyChanged]
    public class EchoGesturesVM
    {
        public EchoGesturesVM()
        {
            GestureMessage = "No gestures received.";

            SwipeUpCommand = new Command((_) => GestureMessage = "Swipe Up Received!");
            SwipeDownCommand = new Command((_) => GestureMessage = "Swipe Down Received!");
            SwipeLeftCommand = new Command((_) => GestureMessage = "Swipe Left Received!");
            SwipeRightCommand = new Command((_) => GestureMessage = "Swipe Right Received!");
            
            SingleTapCommand = new Command((_) => GestureMessage = "Single Tap Received!");
            DoubleTapCommand = new Command((_) => GestureMessage = "Double Tap Received!");
            LongPressCommand = new Command((_) => GestureMessage = "Long Press Received!");
            InnerSingleTapCommand = new Command((_) => 
                {
                    GestureMessage = "Inner Single Tap Received!";
                    InnerTapTime = DateTime.Now.ToString("hh:mm:ss tt");
                });

            PinchCommand = new Command((_) => GestureMessage = "Pinch Received!");

            ButtonClickCommand = new Command((_) => GestureMessage = "Button Clicked!");
        }
        public ICommand SwipeUpCommand { get; private set; }
        public ICommand SwipeDownCommand { get; private set; }
        public ICommand SwipeLeftCommand { get; private set; }
        public ICommand SwipeRightCommand { get; private set; }

        public ICommand SingleTapCommand { get; private set; }
        public ICommand DoubleTapCommand { get; private set; }
        public ICommand LongPressCommand { get; private set; }
        public ICommand InnerSingleTapCommand { get; private set; }

        public ICommand PinchCommand { get; private set; }

        public ICommand ButtonClickCommand { get; private set; }

        public string GestureMessage { get; set; }

        public string InnerTapTime { get; set; }
    }
}
