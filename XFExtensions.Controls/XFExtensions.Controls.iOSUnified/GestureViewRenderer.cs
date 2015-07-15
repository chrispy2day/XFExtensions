using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using XFExtensions.Controls.iOSUnified;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using XFExtensions.Controls.Abstractions;


[assembly: ExportRenderer(typeof(GestureView), typeof(GestureViewRenderer))]
namespace XFExtensions.Controls.iOSUnified
{
    public class GestureViewRenderer : ViewRenderer
    {
        private UISwipeGestureRecognizer _swipeDown;
        private UIGestureRecognizer _swipeUp;
        private UIGestureRecognizer _swipeLeft;
        private UIGestureRecognizer _swipeRight;

        private UITapGestureRecognizer _singleTap;
        private UITapGestureRecognizer _doubleTap;

        private UIPinchGestureRecognizer _pinch;

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.View> e)
        {
            base.OnElementChanged(e);

            // get the element
            var gestureView = (GestureView)this.Element;

            // setup the swipe actions
            _swipeDown = new UISwipeGestureRecognizer(() =>
            {
                gestureView.OnSwipeDown();
            })
            {
                Direction = UISwipeGestureRecognizerDirection.Down
            };

            _swipeUp = new UISwipeGestureRecognizer(() =>
            {
                gestureView.OnSwipeUp();
            })
            {
                Direction = UISwipeGestureRecognizerDirection.Up
            };

            _swipeLeft = new UISwipeGestureRecognizer(() =>
            {
                gestureView.OnSwipeLeft();
            })
            {
                Direction = UISwipeGestureRecognizerDirection.Left
            };

            _swipeRight = new UISwipeGestureRecognizer(() =>
            {
                gestureView.OnSwipeRight();
            })
            {
                Direction = UISwipeGestureRecognizerDirection.Right
            };

            // setup the tap gesture
            _singleTap = new UITapGestureRecognizer(() =>
            {
                gestureView.OnSingleTap();
            })
            {
                NumberOfTapsRequired = 1
            };
            _doubleTap = new UITapGestureRecognizer(() =>
            {
                gestureView.OnDoubleTap();
            })
            {
                NumberOfTapsRequired = 2
            };

            // setup the pinch gesture
            _pinch = new UIPinchGestureRecognizer(() =>
            {
                gestureView.PinchScale = _pinch.Scale;
                gestureView.PinchVelocity = _pinch.Velocity;
                switch (_pinch.State)
                {
                    case UIGestureRecognizerState.Began:
                        gestureView.CurrentPinchState = GestureView.PinchGestureState.Started;
                        break;
                    case UIGestureRecognizerState.Changed:
                        gestureView.CurrentPinchState = GestureView.PinchGestureState.Continuing;
                        break;
                    case UIGestureRecognizerState.Ended:
                        gestureView.CurrentPinchState = GestureView.PinchGestureState.Ended;
                        break;
                }
                gestureView.OnPinch();
            });

            // remove the gesture when the control is removed
            if (e.NewElement == null)
            {
                if (_swipeDown != null)
                    this.RemoveGestureRecognizer(_swipeDown);
                if (_swipeLeft != null)
                    this.RemoveGestureRecognizer(_swipeLeft);
                if (_swipeRight != null)
                    this.RemoveGestureRecognizer(_swipeRight);
                if (_swipeUp != null)
                    this.RemoveGestureRecognizer(_swipeUp);
                if (_doubleTap != null)
                    this.RemoveGestureRecognizer(_doubleTap);
                if (_singleTap != null)
                    this.RemoveGestureRecognizer(_singleTap);
                if (_pinch != null)
                    this.RemoveGestureRecognizer(_pinch);
            }

            // only add the gesture when the control is not being reused
            if (e.OldElement == null)
            {
                this.AddGestureRecognizer(_swipeDown);
                this.AddGestureRecognizer(_swipeLeft);
                this.AddGestureRecognizer(_swipeRight);
                this.AddGestureRecognizer(_swipeUp);
                this.AddGestureRecognizer(_doubleTap);
                this.AddGestureRecognizer(_singleTap);
                this.AddGestureRecognizer(_pinch);
            }
        }

        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_swipeDown != null)
                {
                    _swipeDown.Dispose();
                    _swipeDown = null;
                }
                if (_swipeLeft != null)
                {
                    _swipeLeft.Dispose();
                    _swipeLeft = null;
                }
                if (_swipeRight != null)
                {
                    _swipeRight.Dispose();
                    _swipeRight = null;
                }
                if (_swipeUp != null)
                {
                    _swipeUp.Dispose();
                    _swipeUp = null;
                }
                if (_doubleTap != null)
                {
                    _doubleTap.Dispose();
                    _doubleTap = null;
                }
                if (_singleTap != null)
                {
                    _singleTap.Dispose();
                    _singleTap = null;
                }
                if (_pinch != null)
                {
                    _pinch.Dispose();
                    _pinch = null;
                }
            }
            // don't try and dispose the base - it will try and dispose of the native control that we didn't create
            //base.Dispose(disposing);
        }
    }
}