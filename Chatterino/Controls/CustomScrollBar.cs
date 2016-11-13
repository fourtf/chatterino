using System;
using System.Collections.Concurrent;
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
        const int minThumbHeight = 10;


        // events
        public event EventHandler<CustomScrollBarEventArgs> Scroll;


        // properties
        private double max;
        object maxLock = new object();

        public double Maximum
        {
            get { lock (maxLock) return max; }
            set { lock (maxLock) max = value; Value = Value; updateScroll(); Invalidate(); }
        }

        private double min;
        object minLock = new object();

        public double Minimum
        {
            get { lock (minLock) return min; }
            set { lock (minLock) min = value; updateScroll(); Invalidate(); }
        }

        private double large;
        object largeLock = new object();

        public double LargeChange
        {
            get { lock (largeLock) return large; }
            set { lock (largeLock) large = value; Value = Value; updateScroll(); Invalidate(); }
        }

        private double small;
        object smallLock = new object();

        public double SmallChange
        {
            get { lock (smallLock) return small; }
            set { lock (smallLock) small = value; updateScroll(); Invalidate(); }
        }

        private double value;

        public double Value
        {
            get { return value; }
            set
            {
                this.value = value < 0 ? 0 : (value > max - large ? max - large : value);
                updateScroll();
                Update();
            }
        }

        object highlightLock = new object();

        List<ScrollBarHighlight> highlights = new List<ScrollBarHighlight>();

        public void RemoveHighlightsWhere(Func<ScrollBarHighlight, bool> match)
        {
            lock (highlightLock)
            {
                highlights.RemoveAll(new Predicate<ScrollBarHighlight>(match));
            }
        }

        public void UpdateHighlights(Action<ScrollBarHighlight> func)
        {
            lock (highlightLock)
            {
                foreach (ScrollBarHighlight highlight in highlights)
                {
                    func(highlight);
                }
            }
        }

        public void AddHighlight(double position, Color color, double height = 1, object tag = null)
        {
            lock (highlightLock)
            {
                highlights.Add(new ScrollBarHighlight(position, color, height, tag));
            }

            Invalidate();
        }

        static CustomScrollBar()
        {
            App.ColorSchemeChanged += (s, e) => colors.Clear();
        }

        // ctor
        public CustomScrollBar()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

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
            trackHeight = Height - buttonSize - buttonSize - minThumbHeight - 1;
            thumbHeight = (int)(large / max * trackHeight) + minThumbHeight;
            thumbOffset = (int)(value / max * trackHeight) + 1;
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

        static ConcurrentDictionary<Color, SolidBrush> colors = new ConcurrentDictionary<Color, SolidBrush>();


        // drawing
        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                var g = e.Graphics;

                if (Enabled)
                {
                    // top button
                    //g.FillRectangle(mOverIndex == 0 ? App.ColorScheme.ScrollbarThumbSelected : App.ColorScheme.ScrollbarThumb
                    //    , 0, 0, buttonSize, buttonSize);

                    // top button triangle
                    g.FillPolygon(App.ColorScheme.ScrollbarThumbSelected,
                        new Point[] {
                    new Point(buttonSize * 3 / 4, buttonSize * 5 / 8),
                    new Point(buttonSize / 4,     buttonSize * 5 / 8),
                    new Point(buttonSize / 2,     buttonSize * 3 / 8),
                        });

                    g.FillRectangle(App.ColorScheme.ChatBackground, 0, Height - buttonSize - 1, Width, 1);

                    // bottom button
                    //g.FillRectangle(mOverIndex == 4 ? App.ColorScheme.ScrollbarThumbSelected : App.ColorScheme.ScrollbarThumb
                    //    , 0, Height - buttonSize, buttonSize, buttonSize);

                    // bottom button triangle
                    g.FillPolygon(App.ColorScheme.ScrollbarThumbSelected,
                        new Point[] {
                    new Point(buttonSize / 4,     Height - buttonSize * 5 / 8),
                    new Point(buttonSize * 3 / 4, Height - buttonSize * 5 / 8),
                    new Point(buttonSize / 2,     Height - buttonSize * 3 / 8),
                        });

                    // draw thumb
                    g.FillRectangle(mOverIndex == 2 ? App.ColorScheme.ScrollbarThumbSelected : App.ColorScheme.ScrollbarThumb
                        , 0, buttonSize + thumbOffset, buttonSize, thumbHeight);

                    if (Height != 0 && Maximum != 0)
                    {
                        var h = (Height - buttonSize - buttonSize - minThumbHeight);

                        lock (highlights)
                        {
                            foreach (var highlight in highlights)
                            {

                                SolidBrush brush = colors.GetOrAdd(highlight.Color, (x) =>
                                {
                                    Color bg = (App.ColorScheme.ChatBackground as SolidBrush)?.Color ?? Color.Black;

                                    Color n = Color.FromArgb(
                                        (bg.R + x.R) / 2,
                                        (bg.G + x.G) / 2,
                                        (bg.B + x.B) / 2);

                                    return new SolidBrush(n);
                                });

                                var y = (int)(h * highlight.Position / Maximum);

                                int a = 0;

                                if (y > thumbOffset)
                                {
                                    if (y > thumbOffset + thumbHeight - minThumbHeight)
                                    {
                                        y += minThumbHeight - 3;
                                    }
                                    else
                                    {
                                        a += 2;
                                        y = (int)((y - thumbOffset) * (thumbHeight / ((double)thumbHeight - (minThumbHeight - 3)))) + thumbOffset;
                                    }
                                }

                                y += buttonSize;

                                g.FillRectangle(brush, Width / 2 - 2, y, 4, a + Math.Max(4, (int)(h * highlight.Height / Maximum)));
                            }
                        }
                    }
                }
            }
            catch { }
        }
    }
}

