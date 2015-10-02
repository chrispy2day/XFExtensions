﻿/*
 * Copyright (C) 2013 @JamesMontemagno http://www.montemagno.com http://www.refractored.com
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * Ported from mic: https://github.com/matabii/scale-imageview-android/blob/master/src/com/matabii/dev/scaleimageview/ScaleImageView.java
 */

using System;

using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Android.Widget;
using XFExtensions.Controls.Abstractions;

namespace XFExtensions.Controls.Droid
{

    public class ScaleImageViewGestureDetector : GestureDetector.SimpleOnGestureListener
    {
        private readonly ScaleImageView m_ScaleImageView;
        public ScaleImageViewGestureDetector(ScaleImageView imageView)
        {
            m_ScaleImageView = imageView;
        }

        public override bool OnDown(MotionEvent e)
        {
            return true;
        }

        public override bool OnDoubleTap(MotionEvent e)
        {
            m_ScaleImageView.TapZoomTo((int)e.GetX(), (int)e.GetY());
            m_ScaleImageView.Cutting();
            return true;
        }
    }

    public class ScaleImageView : ImageView, View.IOnTouchListener
    {
        private Context m_Context;

        private Matrix m_Matrix = new Matrix();
        private float[] m_MatrixValues = new float[9];
        private int m_Width;
        private int m_Height;
        private int m_IntrinsicWidth;
        private int m_IntrinsicHeight;
        private float m_Scale;
        private float m_MinScale;
        private float m_MaxScale = 4.0f;
        private float m_PreviousDistance;
        private int m_PreviousMoveX;
        private int m_PreviousMoveY;

        private bool m_IsScaling;
        private GestureDetector m_GestureDetector;

        public ScaleImageView(Context context, IAttributeSet attrs) :
        base(context, attrs)
        {
            m_Context = context;
            Initialize();
        }

        public ScaleImageView(Context context, IAttributeSet attrs, int defStyle) :
        base(context, attrs, defStyle)
        {
            m_Context = context;
            Initialize();
        }

        public ZoomImage ZoomImage { get; set; }

        public override void SetImageBitmap(Bitmap bm)
        {
            base.SetImageBitmap(bm);
            this.Initialize();
        }

        public override void SetImageResource(int resId)
        {
            base.SetImageResource(resId);
            this.Initialize();
        }

        public void Initialize()
        {
            this.SetScaleType(ScaleType.Matrix);

            if (Drawable != null)
            {
                m_IntrinsicWidth = Drawable.IntrinsicWidth;
                m_IntrinsicHeight = Drawable.IntrinsicHeight;
                this.SetOnTouchListener(this);
            }

            m_GestureDetector = new GestureDetector(m_Context, new ScaleImageViewGestureDetector(this));
        }

        protected override bool SetFrame(int l, int t, int r, int b)
        {
            // set the view size variables
            m_Width = r - l;
            m_Height = b - t;

            // make sure there is a drawable size to continue
            if (m_IntrinsicWidth > 1 || m_IntrinsicHeight > 1)
            {
                // ensure starting from a clean matrix to prevent multiple calls from throwing off the scale
                m_Matrix.Reset();

                // calculate the scale that should be used
                var hScale = m_Width / (float)m_IntrinsicWidth;
                var vScale = m_Height / (float)m_IntrinsicHeight;
                if (ZoomImage.Aspect == Xamarin.Forms.Aspect.AspectFit)
                    m_Scale = (float)Math.Min(hScale, vScale);
                else
                    m_Scale = (float)Math.Max(hScale, vScale);

                // set the min and max scales
                m_MinScale = m_Scale * (float)ZoomImage.MinZoom;
                m_MaxScale = m_Scale * (float)ZoomImage.MaxZoom;

                // perform the zoom
                ZoomTo(m_Scale, m_Width / 2, m_Height / 2);
                Cutting();
            }
            return base.SetFrame(l, t, r, b);
        }

        private float GetValue(Matrix matrix, int whichValue)
        {
            matrix.GetValues(m_MatrixValues);
            return m_MatrixValues[whichValue];
        }

        public float Scale
        {
            get { return this.GetValue(m_Matrix, Matrix.MscaleX); }
        }

        public float TranslateX
        {
            get { return this.GetValue(m_Matrix, Matrix.MtransX); }
        }

        public float TranslateY
        {
            get { return this.GetValue(m_Matrix, Matrix.MtransY); }
        }

        public void TapZoomTo(int x, int y)
        {
            if (ZoomImage.ZoomEnabled && ZoomImage.DoubleTapToZoomEnabled)
            {
                // if at max, zoom out
                if ((m_MaxScale - Scale) < 0.1f)
                    ZoomTo(1 / (float)ZoomImage.TapZoomScale, x, y);
                else
                    ZoomTo((float)ZoomImage.TapZoomScale, x, y);
            }
        }

        public void ZoomTo(float scale, int x, int y)
        {
            if (Scale * scale < m_MinScale)
            {
                scale = m_MinScale / Scale;
            }
            else
            {
                if (scale >= 1 && Scale * scale > m_MaxScale)
                {
                    scale = m_MaxScale / Scale;
                }
            }
            m_Matrix.PostScale(scale, scale);
            //move to center
            m_Matrix.PostTranslate(-(m_Width * scale - m_Width) / 2, -(m_Height * scale - m_Height) / 2);

            //move x and y distance
            m_Matrix.PostTranslate(-(x - (m_Width / 2)) * scale, 0);
            m_Matrix.PostTranslate(0, -(y - (m_Height / 2)) * scale);
            ImageMatrix = m_Matrix;
        }

        public void Cutting()
        {
            var width = (int)(m_IntrinsicWidth * Scale);
            var height = (int)(m_IntrinsicHeight * Scale);
            if (TranslateX < -(width - m_Width))
            {
                m_Matrix.PostTranslate(-(TranslateX + width - m_Width), 0);
            }

            if (TranslateX > 0)
            {
                m_Matrix.PostTranslate(-TranslateX, 0);
            }

            if (TranslateY < -(height - m_Height))
            {
                m_Matrix.PostTranslate(0, -(TranslateY + height - m_Height));
            }

            if (TranslateY > 0)
            {
                m_Matrix.PostTranslate(0, -TranslateY);
            }

            if (width < m_Width)
            {
                m_Matrix.PostTranslate((m_Width - width) / 2, 0);
            }

            if (height < m_Height)
            {
                m_Matrix.PostTranslate(0, (m_Height - height) / 2);
            }

            ImageMatrix = m_Matrix;
        }

        private float Distance(float x0, float x1, float y0, float y1)
        {
            var x = x0 - x1;
            var y = y0 - y1;
            return FloatMath.Sqrt(x * x + y * y);
        }

        private float DispDistance()
        {
            return FloatMath.Sqrt(m_Width * m_Width + m_Height * m_Height);
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            if (m_GestureDetector.OnTouchEvent(e))
            {
                m_PreviousMoveX = (int)e.GetX();
                m_PreviousMoveY = (int)e.GetY();
                return true;
            }

            var touchCount = e.PointerCount;
            switch (e.Action)
            {
                case MotionEventActions.Down:
                case MotionEventActions.Pointer1Down:
                case MotionEventActions.Pointer2Down:
                    {
                        if (touchCount >= 2)
                        {
                            if (ZoomImage.ZoomEnabled)
                            {
                                var distance = this.Distance(e.GetX(0), e.GetX(1), e.GetY(0), e.GetY(1));
                                m_PreviousDistance = distance;
                                m_IsScaling = true;
                            }
                        }
                    }
                    break;

                case MotionEventActions.Move:
                    {
                        if (touchCount >= 2 && m_IsScaling)
                        {
                            if (ZoomImage.ZoomEnabled)
                            {
                                var distance = this.Distance(e.GetX(0), e.GetX(1), e.GetY(0), e.GetY(1));
                                var scale = (distance - m_PreviousDistance) / this.DispDistance();
                                m_PreviousDistance = distance;
                                scale += 1;
                                scale = scale * scale;
                                this.ZoomTo(scale, m_Width / 2, m_Height / 2);
                                this.Cutting();
                            }
                        }
                        else if (!m_IsScaling)
                        {
                            if (ZoomImage.ScrollEnabled)
                            {
                                var distanceX = m_PreviousMoveX - (int)e.GetX();
                                var distanceY = m_PreviousMoveY - (int)e.GetY();
                                m_PreviousMoveX = (int)e.GetX();
                                m_PreviousMoveY = (int)e.GetY();

                                m_Matrix.PostTranslate(-distanceX, -distanceY);
                                this.Cutting();
                            }
                        }
                    }
                    break;
                case MotionEventActions.Up:
                case MotionEventActions.Pointer1Up:
                case MotionEventActions.Pointer2Up:
                    {
                        if (touchCount <= 1)
                        {
                            m_IsScaling = false;
                        }
                    }
                    break;
            }
            return true;
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            return OnTouchEvent(e);
        }
    }
}
