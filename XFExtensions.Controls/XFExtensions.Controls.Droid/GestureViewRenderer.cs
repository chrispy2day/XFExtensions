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

[assembly: ExportRenderer(typeof(GestureView), typeof(GestureViewRenderer))]
namespace XFExtensions.Controls.Droid
{
    #region Gesture View Renderer

    public class GestureViewRenderer : ViewRenderer
    {
        private readonly DroidGestureViewTouchListener _listener;
        private readonly GestureDetector _detector;

        public GestureViewRenderer()
        {
            _listener = new DroidGestureViewTouchListener();
            _detector = new GestureDetector(_listener);
        }


        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.View> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement == null)
            {
                this.GenericMotion -= HandleGenericMotion;
                this.Touch -= HandleTouch;
                _listener.OnSwipeLeft -= HandleOnSwipeLeft;
                _listener.OnSwipeRight -= HandleOnSwipeRight;
                _listener.OnSwipeUp -= HandleOnSwipeTop;
                _listener.OnSwipeDown -= HandleOnSwipeDown;
            }

            if (e.OldElement == null)
            {
                this.GenericMotion += HandleGenericMotion;
                this.Touch += HandleTouch;
                _listener.OnSwipeLeft += HandleOnSwipeLeft;
                _listener.OnSwipeRight += HandleOnSwipeRight;
                _listener.OnSwipeUp += HandleOnSwipeTop;
                _listener.OnSwipeDown += HandleOnSwipeDown;
            }
        }

        void HandleTouch(object sender, TouchEventArgs e)
        {
            _detector.OnTouchEvent(e.Event);
        }

        void HandleGenericMotion(object sender, GenericMotionEventArgs e)
        {
            _detector.OnTouchEvent(e.Event);
        }

        void HandleOnSwipeLeft(object sender, EventArgs e)
        {
            GestureView _gi = (GestureView)this.Element;
            _gi.OnSwipeLeft();
        }

        void HandleOnSwipeRight(object sender, EventArgs e)
        {
            GestureView _gi = (GestureView)this.Element;
            _gi.OnSwipeRight();
        }

        void HandleOnSwipeTop(object sender, EventArgs e)
        {
            GestureView _gi = (GestureView)this.Element;
            _gi.OnSwipeUp();
        }

        void HandleOnSwipeDown(object sender, EventArgs e)
        {
            GestureView _gi = (GestureView)this.Element;
            _gi.OnSwipeDown();
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

        public override bool OnSingleTapUp(MotionEvent e)
        {
            var handler = SingleTap;
            if (handler != null)
                handler(this, new EventArgs());
            return base.OnSingleTapUp(e);
        }
    }

    #endregion
}