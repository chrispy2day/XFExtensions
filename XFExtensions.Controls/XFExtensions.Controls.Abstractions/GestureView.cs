using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace XFExtensions.Controls.Abstractions
{
    public class GestureView : ContentView
    {
        #region Swipes

        public event EventHandler SwipeDown;
        public event EventHandler SwipeUp;
        public event EventHandler SwipeLeft;
        public event EventHandler SwipeRight;

        public void OnSwipeDown()
        {
            if (SwipeDownCommand != null && SwipeDownCommand.CanExecute(null))
                SwipeDownCommand.Execute(null);

            EventHandler handler = SwipeDown;
            if (handler != null)
                handler(this, new EventArgs());
        }

        public void OnSwipeUp()
        {
            if (SwipeUpCommand != null && SwipeUpCommand.CanExecute(null))
                SwipeUpCommand.Execute(null);

            EventHandler handler = SwipeUp;
            if (handler != null)
                handler(this, new EventArgs());
        }

        public void OnSwipeLeft()
        {
            if (SwipeLeftCommand != null && SwipeLeftCommand.CanExecute(null))
                SwipeLeftCommand.Execute(null);

            EventHandler handler = SwipeLeft;
            if (handler != null)
                handler(this, new EventArgs());
        }

        public void OnSwipeRight()
        {
            if (SwipeRightCommand != null && SwipeRightCommand.CanExecute(null))
                SwipeRightCommand.Execute(null);

            EventHandler handler = SwipeRight;
            if (handler != null)
                handler(this, new EventArgs());
        }

        public static readonly BindableProperty SwipeRightCommandProperty = BindableProperty.Create<GestureView, ICommand>(
            p => p.SwipeRightCommand, default(ICommand), BindingMode.Default);
        public ICommand SwipeRightCommand
        {
            get { return (ICommand) GetValue(SwipeRightCommandProperty); }
            set { SetValue(SwipeRightCommandProperty, value); }
        }

        public static readonly BindableProperty SwipeLeftCommandProperty = BindableProperty.Create<GestureView, ICommand>(
            p => p.SwipeLeftCommand, default(ICommand), BindingMode.Default);
        public ICommand SwipeLeftCommand
        {
            get { return (ICommand)GetValue(SwipeLeftCommandProperty); }
            set { SetValue(SwipeLeftCommandProperty, value); }
        }

        public static readonly BindableProperty SwipeUpCommandProperty = BindableProperty.Create<GestureView, ICommand>(
            p => p.SwipeUpCommand, default(ICommand), BindingMode.Default);
        public ICommand SwipeUpCommand
        {
            get { return (ICommand)GetValue(SwipeUpCommandProperty); }
            set { SetValue(SwipeUpCommandProperty, value); }
        }

        public static readonly BindableProperty SwipeDownCommandProperty = BindableProperty.Create<GestureView, ICommand>(
            p => p.SwipeDownCommand, default(ICommand), BindingMode.Default);
        public ICommand SwipeDownCommand
        {
            get { return (ICommand)GetValue(SwipeDownCommandProperty); }
            set { SetValue(SwipeDownCommandProperty, value); }
        }

        #endregion

        #region Taps

        public event EventHandler SingleTap;
        public event EventHandler DoubleTap;

        public static readonly BindableProperty SingleTapCommandProperty = BindableProperty.Create<GestureView, ICommand>(
            p => p.SingleTapCommand, default(ICommand), BindingMode.Default);
        public ICommand SingleTapCommand
        {
            get { return (ICommand)GetValue(SingleTapCommandProperty); }
            set { SetValue(SingleTapCommandProperty, value); }
        }

        public static readonly BindableProperty DoubleTapCommandProperty = BindableProperty.Create<GestureView, ICommand>(
            p => p.DoubleTapCommand, default(ICommand), BindingMode.Default);
        public ICommand DoubleTapCommand
        {
            get { return (ICommand)GetValue(DoubleTapCommandProperty); }
            set { SetValue(DoubleTapCommandProperty, value); }
        }

        public void OnSingleTap()
        {
            if (SingleTapCommand != null && SingleTapCommand.CanExecute(null))
                SingleTapCommand.Execute(null);

            EventHandler handler = SingleTap;
            if (handler != null)
                handler(this, new EventArgs());
        }

        public void OnDoubleTap()
        {
            if (DoubleTapCommand != null && DoubleTapCommand.CanExecute(null))
                DoubleTapCommand.Execute(null);

            EventHandler handler = DoubleTap;
            if (handler != null)
                handler(this, new EventArgs());
        }

        #endregion

        #region Pinch

        // pinch event
        public event EventHandler Pinched;
        public enum PinchGestureState
        {
            Started, Continuing, Ended
        }
        public PinchGestureState CurrentPinchState { get; set; }

        public static BindableProperty PinchScaleProperty = BindableProperty.Create<GestureView, double>(
            p => p.PinchScale, 0.0, BindingMode.Default);

        public static readonly BindableProperty PinchCommandProperty = BindableProperty.Create<GestureView, ICommand>(
            p => p.PinchCommand, default(ICommand), BindingMode.Default);

        public ICommand PinchCommand
        {
            get { return (ICommand)GetValue(PinchCommandProperty); }
            set { SetValue(PinchCommandProperty, value); }
        }

        public double PinchScale
        {
            get { return (double)GetValue(PinchScaleProperty); }
            set { SetValue(PinchScaleProperty, value); }
        }

        public double PinchVelocity { get; set; }

        public void OnPinch()
        {
            if (PinchCommand != null && PinchCommand.CanExecute(null))
                PinchCommand.Execute(null);

            EventHandler handler = Pinched;
            if (Pinched != null)
                Pinched(this, new EventArgs());
        }

        #endregion
    }
}
