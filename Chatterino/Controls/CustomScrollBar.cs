using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Chatterino.Controls
{
    public class CustomScrollBar : Control
    {
        const int buttonSize = 16;
        const int minThumbHeight = 12;


        // events
        public event EventHandler<CustomScrollBarEventArgs> Scroll;


        // properties
        private double max;

        public double Maximum
        {
            get { return max; }
            set { max = value; updateScroll(); Invalidate(); }
        }

        private double min;

        public double Minimum
        {
            get { return min; }
            set { min = value; updateScroll(); Invalidate(); }
        }

        private double large;

        public double LargeChange
        {
            get { return large; }
            set { large = value; updateScroll(); Invalidate(); }
        }

        private double small;

        public double SmallChange
        {
            get { return small; }
            set { small = value; updateScroll(); Invalidate(); }
        }

        private double value;

        public double Value
        {
            get { return value; }
            set
            {
                this.value = value < 0 ? 0 : (value > max - large ? max - large : value);
                updateScroll();
                Invalidate();
            }
        }


        // ctor
        public CustomScrollBar()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            new VScrollBar().Scroll += (s, e) => { };
            Width = buttonSize;
        }

        int thumbHeight = 10;
        int thumbOffset = 0;
        int trackHeight = 100;


        // scrolling
        protected override void OnSizeChanged(EventArgs e)
        {
            updateScroll();

            base.OnSizeChanged(e);
        }

        void updateScroll()
        {
            trackHeight = Height - buttonSize - buttonSize - minThumbHeight;
            thumbHeight = (int)(large / max * trackHeight) + minThumbHeight;
            thumbOffset = (int)(value / max * trackHeight);
        }


        // mouse
        Point lastMousePosition = Point.Empty;

        // -1 = none, 0 = top button, 1 = top track, 2 = thumb, 3 = bottom track, 4 = bottom button
        int mOverIndex = -1;
        int mDownIndex = -1;

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (mDownIndex == -1)
            {
                int oldMOverIndex = mOverIndex;

                if (e.Y < buttonSize)
                    mOverIndex = 0;
                else if (e.Y < buttonSize + thumbOffset)
                    mOverIndex = 1;
                else if (e.Y < buttonSize + thumbOffset + thumbHeight)
                    mOverIndex = 2;
                else if (e.Y < Height - buttonSize)
                    mOverIndex = 3;
                else
                    mOverIndex = 4;

                if (oldMOverIndex != mOverIndex)
                    Invalidate();
            }
            else
            {
                if (mOverIndex == 2)
                {
                    int deltaY = e.Y - lastMousePosition.Y;

                    double oldValue = value;
                    Value += (double)deltaY / trackHeight * max;

                    if (oldValue != value)
                        Scroll?.Invoke(this, new CustomScrollBarEventArgs(oldValue, value));
                }
            }

            lastMousePosition = e.Location;

            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            mOverIndex = -1;
            Invalidate();

            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Y < buttonSize)
                mDownIndex = 0;
            else if (e.Y < buttonSize + thumbOffset)
                mDownIndex = 1;
            else if (e.Y < buttonSize + thumbOffset + thumbHeight)
                mDownIndex = 2;
            else if (e.Y < Height - buttonSize)
                mDownIndex = 3;
            else
                mDownIndex = 4;

            Invalidate();

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            mDownIndex = -1;

            if (e.Y < buttonSize)
            {
                if (mOverIndex == 0)
                {
                    double oldValue = value;
                    Value -= SmallChange;

                    if (oldValue != value)
                        Scroll?.Invoke(this, new CustomScrollBarEventArgs(oldValue, value));
                }
            }
            else if (e.Y < buttonSize + thumbOffset)
            {
                if (mOverIndex == 1)
                {
                    double oldValue = value;
                    Value -= SmallChange;

                    if (oldValue != value)
                        Scroll?.Invoke(this, new CustomScrollBarEventArgs(oldValue, value));
                }
            }
            else if (e.Y < buttonSize + thumbOffset + thumbHeight)
            {
                //if (mOverIndex == 2)
                //    ;
            }
            else if (e.Y < Height - buttonSize)
            {
                if (mOverIndex == 3)
                {
                    double oldValue = value;
                    Value += SmallChange;

                    if (oldValue != value)
                        Scroll?.Invoke(this, new CustomScrollBarEventArgs(oldValue, value));
                }
            }
            else
            {
                if (mOverIndex == 4)
                {
                    double oldValue = value;
                    Value += SmallChange;

                    if (oldValue != value)
                        Scroll?.Invoke(this, new CustomScrollBarEventArgs(oldValue, value));
                }
            }

            base.OnMouseUp(e);
        }


        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(App.ColorScheme.ChatBackground, e.ClipRectangle);
        }

        // drawing
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;

            if (Enabled)
            {
                // top button
                g.FillRectangle(mOverIndex == 0 ? App.ColorScheme.ScrollbarThumbSelected : App.ColorScheme.ScrollbarThumb
                    , 0, 0, buttonSize, buttonSize);
                
                // top button triangle
                g.FillPolygon(Brushes.Black,
                    new Point[] {
                    new Point(buttonSize * 3 / 4, buttonSize * 5 / 8),
                    new Point(buttonSize / 4,     buttonSize * 5 / 8),
                    new Point(buttonSize / 2,     buttonSize * 3 / 8),
                    });

                // bottom button
                g.FillRectangle(mOverIndex == 4 ? App.ColorScheme.ScrollbarThumbSelected : App.ColorScheme.ScrollbarThumb
                    , 0, Height - buttonSize, buttonSize, buttonSize);

                // bottom button triangle
                g.FillPolygon(Brushes.Black,
                    new Point[] {
                    new Point(buttonSize / 4,     Height - buttonSize * 5 / 8),
                    new Point(buttonSize * 3 / 4, Height - buttonSize * 5 / 8),
                    new Point(buttonSize / 2,     Height - buttonSize * 3 / 8),
                    });

                // draw thumb
                g.FillRectangle(mOverIndex == 2 ? App.ColorScheme.ScrollbarThumbSelected : App.ColorScheme.ScrollbarThumb
                    , 0, buttonSize + thumbOffset, buttonSize, thumbHeight);
            }
        }
    }
}

