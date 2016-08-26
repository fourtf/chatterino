using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xwt;
using Xwt.Drawing;

namespace Chatterino.Desktop.Widgets
{
    public class ScrollWidget : Canvas
    {
        const int buttonSize = 16;
        const int minThumbHeight = 10;


        // events
        public event EventHandler<ScrollEventArgs> Scroll;


        // properties
        private double max;
        object maxLock = new object();

        public double Maximum
        {
            get { lock (maxLock) return max; }
            set { lock (maxLock) max = value; Value = Value; updateScroll(); QueueDraw(); }
        }

        private double min;
        object minLock = new object();

        public double Minimum
        {
            get { lock (minLock) return min; }
            set { lock (minLock) min = value; updateScroll(); QueueDraw(); }
        }

        private double large;
        object largeLock = new object();

        public double LargeChange
        {
            get { lock (largeLock) return large; }
            set { lock (largeLock) large = value; Value = Value; updateScroll(); QueueDraw(); }
        }

        private double small;
        object smallLock = new object();

        public double SmallChange
        {
            get { lock (smallLock) return small; }
            set { lock (smallLock) small = value; updateScroll(); QueueDraw(); }
        }

        private double value;

        public double Value
        {
            get { return value; }
            set
            {
                this.value = value < 0 ? 0 : (value > max - large ? max - large : value);
                updateScroll();
                QueueDraw();
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

        public void AddHighlight(double position, Color color, double height = 1)
        {
            lock (highlightLock)
            {
                highlights.Add(new ScrollBarHighlight(position, color, height));
            }
        }

        // ctor
        public ScrollWidget()
        {
            WidthRequest = buttonSize;
        }

        int thumbHeight = 10;
        int thumbOffset = 0;
        int trackHeight = 100;


        // scrolling
        protected override void OnBoundsChanged()
        {
            updateScroll();

            base.OnBoundsChanged();
        }

        void updateScroll()
        {
            trackHeight = (int)Bounds.Height - buttonSize - buttonSize - minThumbHeight - 1;
            thumbHeight = (int)(large / max * trackHeight) + minThumbHeight;
            thumbOffset = (int)(value / max * trackHeight) + 1;
        }


        // mouse
        Point lastMousePosition = Point.Zero;

        // -1 = none, 0 = top button, 1 = top track, 2 = thumb, 3 = bottom track, 4 = bottom button
        int mOverIndex = -1;
        int mDownIndex = -1;

        protected override void OnMouseMoved(MouseMovedEventArgs e)
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
                else if (e.Y < Bounds.Height - buttonSize)
                    mOverIndex = 3;
                else
                    mOverIndex = 4;

                if (oldMOverIndex != mOverIndex)
                    QueueDraw();
            }
            else
            {
                if (mOverIndex == 2)
                {
                    double deltaY = e.Y - lastMousePosition.Y;

                    double oldValue = value;
                    Value += deltaY / trackHeight * max;

                    if (oldValue != value)
                        Scroll?.Invoke(this, new ScrollEventArgs(oldValue, value));
                }
            }

            lastMousePosition = e.Position;

            base.OnMouseMoved(e);
        }

        protected override void OnMouseExited(EventArgs e)
        {
            //if (mDownIndex == -1)
            {
                mOverIndex = -1;
            }
            QueueDraw();

            base.OnMouseExited(e);
        }

        protected override void OnButtonPressed(ButtonEventArgs e)
        {
            if (e.Y < buttonSize)
                mDownIndex = 0;
            else if (e.Y < buttonSize + thumbOffset)
                mDownIndex = 1;
            else if (e.Y < buttonSize + thumbOffset + thumbHeight)
                mDownIndex = 2;
            else if (e.Y < Bounds.Height - buttonSize)
                mDownIndex = 3;
            else
                mDownIndex = 4;

            QueueDraw();

            base.OnButtonPressed(e);
        }

        protected override void OnButtonReleased(ButtonEventArgs e)
        {
            mDownIndex = -1;

            if (e.Y < buttonSize)
            {
                if (mOverIndex == 0)
                {
                    double oldValue = value;
                    Value -= SmallChange;

                    if (oldValue != value)
                        Scroll?.Invoke(this, new ScrollEventArgs(oldValue, value));
                }
            }
            else if (e.Y < buttonSize + thumbOffset)
            {
                if (mOverIndex == 1)
                {
                    double oldValue = value;
                    Value -= SmallChange;

                    if (oldValue != value)
                        Scroll?.Invoke(this, new ScrollEventArgs(oldValue, value));
                }
            }
            else if (e.Y < buttonSize + thumbOffset + thumbHeight)
            {
                //if (mOverIndex == 2)
                //    ;
            }
            else if (e.Y < Bounds.Height - buttonSize)
            {
                if (mOverIndex == 3)
                {
                    double oldValue = value;
                    Value += SmallChange;

                    if (oldValue != value)
                        Scroll?.Invoke(this, new ScrollEventArgs(oldValue, value));
                }
            }
            else
            {
                if (mOverIndex == 4)
                {
                    double oldValue = value;
                    Value += SmallChange;

                    if (oldValue != value)
                        Scroll?.Invoke(this, new ScrollEventArgs(oldValue, value));
                }
            }

            base.OnButtonReleased(e);
        }


        // drawing
        protected override void OnDraw(Context ctx, Rectangle dirtyRect)
        {
            ctx.SetColor(App.ColorScheme.ChatBackground);
            ctx.Rectangle(dirtyRect);
            ctx.Fill();

            if (Sensitive)
            {
                // top button
                //ctx.SetColor(mOverIndex == 0 ? App.ColorScheme.ScrollbarThumbSelected : App.ColorScheme.ScrollbarThumb);
                //ctx.Rectangle(0, 0, buttonSize, buttonSize);
                //ctx.Fill();

                // top button triangle
                ctx.SetColor(App.ColorScheme.ScrollbarThumb);
                ctx.NewPath();
                ctx.MoveTo(buttonSize * 3 / 4, buttonSize * 5 / 8);
                ctx.LineTo(buttonSize / 4, buttonSize * 5 / 8);
                ctx.LineTo(buttonSize / 2, buttonSize * 3 / 8);
                ctx.ClosePath();
                ctx.Fill();

                //g.FillRectangle(App.ColorScheme.ChatBackground, 0, Height - buttonSize - 1, Width, 1);

                // bottom button
                //ctx.SetColor(mOverIndex == 4 ? App.ColorScheme.ScrollbarThumbSelected : App.ColorScheme.ScrollbarThumb);
                //ctx.Rectangle(0, Bounds.Height - buttonSize, buttonSize, buttonSize);
                //ctx.Fill();

                // bottom button triangle
                ctx.SetColor(App.ColorScheme.ScrollbarThumb);
                ctx.NewPath();
                ctx.MoveTo(buttonSize / 4,     Bounds.Height - buttonSize * 5 / 8);
                ctx.LineTo(buttonSize * 3 / 4, Bounds.Height - buttonSize * 5 / 8);
                ctx.LineTo(buttonSize / 2,     Bounds.Height - buttonSize * 3 / 8);
                ctx.ClosePath();
                ctx.Fill();

                // draw thumb
                ctx.SetColor(mOverIndex == 2 ? App.ColorScheme.ScrollbarThumbSelected : App.ColorScheme.ScrollbarThumb);
                ctx.Rectangle(0, buttonSize + thumbOffset, buttonSize, thumbHeight);
                ctx.Fill();

                if (Bounds.Height != 0 && Maximum != 0)
                {
                    var h = (Bounds.Height - buttonSize - buttonSize - minThumbHeight);

                    lock (highlights)
                    {
                        foreach (var highlight in highlights)
                        {
                            Color c = highlight.Color;

                            var y = (int)(h * highlight.Position / Maximum);

                            if (y > thumbOffset)
                            {
                                if (y > thumbOffset + thumbHeight - minThumbHeight)
                                {
                                    y += minThumbHeight - 3;
                                }
                                else
                                {
                                    y = (int)((y - thumbOffset) * (thumbHeight / ((double)thumbHeight - (minThumbHeight - 3)))) + thumbOffset;
                                }
                            }

                            y += buttonSize;

                            ctx.SetColor(c);
                            ctx.Rectangle(0, y, 4, Math.Max(3, (int)(h * highlight.Height / Maximum)));
                            ctx.Fill();
                        }
                    }
                }
            }
        }
    }
}
