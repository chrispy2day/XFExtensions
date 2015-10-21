using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.App.Usage;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using XFExtensions.Controls.Abstractions;
using XFExtensions.Controls.Droid;
using System.Threading.Tasks;


[assembly: ExportRenderer(typeof(GestureView), typeof(GestureViewRenderer))]
namespace XFExtensions.Controls.Droid
{
    #region Gesture View Renderer

    public class GestureViewRenderer : ViewRenderer
    {
        private readonly DroidGestureViewTouchListener _gestureListener;
        private readonly GestureDetector _gestureDetector;
        private readonly DroidGestureViewScaleListener _scaleListener;
        private readonly ScaleGestureDetector _scaleDetector;

        public GestureViewRenderer()
        {
            _gestureListener = new DroidGestureViewTouchListener();
            _gestureDetector = new GestureDetector(_gestureListener);
            _scaleListener = new DroidGestureViewScaleListener();
            _scaleDetector = new ScaleGestureDetector(Context, _scaleListener);
        }

        protected override async void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.View> e)
        {
            //base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                this.GenericMotion += HandleGenericMotion;
                this.Touch += HandleTouch;
                _gestureListener.SwipeLeft += HandleOnSwipeLeft;
                _gestureListener.SwipeRight += HandleOnSwipeRight;
                _gestureListener.SwipeUp += HandleOnSwipeTop;
                _gestureListener.SwipeDown += HandleOnSwipeDown;

                _gestureListener.DoubleTap += HandleOnDoubleTap;
                _gestureListener.SingleTap += HandleOnSingleTap;
                _gestureListener.LongPress += HandleLongPress;

                _scaleListener.Pinch += HandlePinch;
            }
            else
            {
                this.GenericMotion -= HandleGenericMotion;
                this.Touch -= HandleTouch;
                _gestureListener.SwipeLeft -= HandleOnSwipeLeft;
                _gestureListener.SwipeRight -= HandleOnSwipeRight;
                _gestureListener.SwipeUp -= HandleOnSwipeTop;
                _gestureListener.SwipeDown -= HandleOnSwipeDown;

                _gestureListener.DoubleTap -= HandleOnDoubleTap;
                _gestureListener.SingleTap -= HandleOnSingleTap;

                _scaleListener.Pinch -= HandlePinch;
            }
        }

        void HandleTouch(object sender, TouchEventArgs e)
        {
            _scaleDetector.OnTouchEvent(e.Event);
            if (!_scaleDetector.IsInProgress)
                _gestureDetector.OnTouchEvent(e.Event);
        }

        void HandleGenericMotion(object sender, GenericMotionEventArgs e)
        {
            _gestureDetector.OnTouchEvent(e.Event);
        }

        void HandleOnSwipeLeft(object sender, EventArgs e)
        {
            GestureView gi = (GestureView)this.Element;
            gi.OnSwipeLeft();
        }

        void HandleOnSwipeRight(object sender, EventArgs e)
        {
            GestureView gi = (GestureView)this.Element;
            gi.OnSwipeRight();
        }

        void HandleOnSwipeTop(object sender, EventArgs e)
        {
            GestureView gi = (GestureView)this.Element;
            gi.OnSwipeUp();
        }

        void HandleOnSwipeDown(object sender, EventArgs e)
        {
            GestureView gi = (GestureView)this.Element;
            gi.OnSwipeDown();
        }

        void HandleOnDoubleTap(object sender, EventArgs e)
        {
            GestureView gi = (GestureView)this.Element;
            gi.OnDoubleTap();
        }

        void HandleOnSingleTap(object sender, EventArgs e)
        {
            GestureView gi = (GestureView)this.Element;
            gi.OnSingleTap();
        }

        void HandleLongPress(object sender, EventArgs e)
        {
            GestureView gi = (GestureView)this.Element;
            gi.OnLongPress();
        }

        private void HandlePinch(object sender, PinchEventArgs e)
        {
            GestureView gi = (GestureView)this.Element;
            if (e.InProgress)
                gi.CurrentPinchState = (gi.CurrentPinchState == GestureView.PinchGestureState.Started)
                    ? GestureView.PinchGestureState.Continuing
                    : GestureView.PinchGestureState.Started;
            else
                gi.CurrentPinchState = GestureView.PinchGestureState.Ended;
            gi.PinchScale = e.ScaleFactor;
            gi.OnPinch();
        }
    }

    #endregion

    #region Gesture Listeners

    public class DroidGestureViewTouchListener : GestureDetector.SimpleOnGestureListener
    {
        private static int SWIPE_THRESHOLD = 100;
        private static int SWIPE_VELOCITY_THRESHOLD = 100;

        public event EventHandler SwipeDown;
        public event EventHandler SwipeUp;
        public event EventHandler SwipeLeft;
        public event EventHandler SwipeRight;

        public event EventHandler SingleTap;
        public event EventHandler DoubleTap;
        public event EventHandler LongPress;

        public override bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            float diffY = e2.GetY() - e1.GetY();
            float diffX = e2.GetX() - e1.GetX();

            EventHandler handler = null;

            if (Math.Abs(diffX) > Math.Abs(diffY))
            {
                if (Math.Abs(diffX) > SWIPE_THRESHOLD && Math.Abs(velocityX) > SWIPE_VELOCITY_THRESHOLD)
                    handler = diffX > 0 ? SwipeRight : SwipeLeft;
            }
            else if (Math.Abs(diffY) > SWIPE_THRESHOLD && Math.Abs(velocityY) > SWIPE_VELOCITY_THRESHOLD)
                handler = diffY > 0 ? SwipeDown : SwipeUp;

            if (handler != null)
                handler(this, new EventArgs());
            return true;
        }

        public override bool OnDoubleTap(MotionEvent e)
        {
            var handler = DoubleTap;
            if (handler != null)
                handler(this, new EventArgs());
            return base.OnDoubleTap(e);
        }

        public override bool OnDoubleTapEvent(MotionEvent e)
        {
            var handler = DoubleTap;
            if (handler != null)
                handler(this, new EventArgs());
            return base.OnDoubleTapEvent(e);
        }

        public override bool OnSingleTapConfirmed(MotionEvent e)
        {
            var handler = SingleTap;
            if (handler != null)
                handler(this, new EventArgs());
            return base.OnSingleTapConfirmed(e);
        }

        public override void OnLongPress(MotionEvent e)
        {
            var handler = this.LongPress;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }

    public class DroidGestureViewScaleListener : ScaleGestureDetector.SimpleOnScaleGestureListener
    {
        public event EventHandler<PinchEventArgs> Pinch;
        public override bool OnScale(ScaleGestureDetector detector)
        {
            var handler = Pinch;
            if (handler != null)
                handler(this, new PinchEventArgs
                    {
                        ScaleFactor = detector.ScaleFactor, 
                        InProgress = detector.IsInProgress,
                        GestureTime = TimeSpan.FromMilliseconds(detector.TimeDelta)
                    });
            return true;
        }
    }

    #endregion

    #region Custom Event Args

    public class PinchEventArgs : EventArgs
    {
        public float ScaleFactor { get; set; }
        public bool InProgress { get; set; }
        public TimeSpan GestureTime { get; set; }
    }

    #endregion
}