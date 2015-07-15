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

            PinchCommand = new Command((_) => GestureMessage = "Pinch Received!");
        }
        public ICommand SwipeUpCommand { get; private set; }
        public ICommand SwipeDownCommand { get; private set; }
        public ICommand SwipeLeftCommand { get; private set; }
        public ICommand SwipeRightCommand { get; private set; }
        public ICommand SingleTapCommand { get; private set; }
        public ICommand DoubleTapCommand { get; private set; }
        public ICommand PinchCommand { get; private set; }
        public string GestureMessage { get; set; }
    }
}
