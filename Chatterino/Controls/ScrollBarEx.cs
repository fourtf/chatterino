namespace CustomScrollBar
{
    using Chatterino;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    /// <summary>
    /// A custom scrollbar control.
    /// </summary>
    //[Designer(typeof(Design.ScrollBarControlDesigner))]
    [DefaultEvent("Scroll")]
    [DefaultProperty("Value")]
    public class ScrollBarEx : Control
    {
        #region fields

        /// <summary>
        /// Redraw const.
        /// </summary>
        private const int SETREDRAW = 11;

        /// <summary>
        /// Indicates many changes to the scrollbar are happening, so stop painting till finished.
        /// </summary>
        private bool inUpdate;

        /// <summary>
        /// The scrollbar orientation - horizontal / vertical.
        /// </summary>
        private ScrollBarOrientation orientation = ScrollBarOrientation.Vertical;

        /// <summary>
        /// The scroll orientation in scroll events.
        /// </summary>
        private ScrollOrientation scrollOrientation = ScrollOrientation.VerticalScroll;

        /// <summary>
        /// The clicked channel rectangle.
        /// </summary>
        private Rectangle clickedBarRectangle;

        /// <summary>
        /// The thumb rectangle.
        /// </summary>
        private Rectangle thumbRectangle;

        /// <summary>
        /// The top arrow rectangle.
        /// </summary>
        private Rectangle topArrowRectangle;

        /// <summary>
        /// The bottom arrow rectangle.
        /// </summary>
        private Rectangle bottomArrowRectangle;

        /// <summary>
        /// The channel rectangle.
        /// </summary>
        private Rectangle channelRectangle;

        /// <summary>
        /// Indicates if top arrow was clicked.
        /// </summary>
        private bool topArrowClicked;

        /// <summary>
        /// Indicates if bottom arrow was clicked.
        /// </summary>
        private bool bottomArrowClicked;

        /// <summary>
        /// Indicates if channel rectangle above the thumb was clicked.
        /// </summary>
        private bool topBarClicked;

        /// <summary>
        /// Indicates if channel rectangle under the thumb was clicked.
        /// </summary>
        private bool bottomBarClicked;

        /// <summary>
        /// Indicates if the thumb was clicked.
        /// </summary>
        private bool thumbClicked;

        /// <summary>
        /// The state of the thumb.
        /// </summary>
        private ScrollBarState thumbState = ScrollBarState.Normal;

        /// <summary>
        /// The state of the top arrow.
        /// </summary>
        private ScrollBarArrowButtonState topButtonState = ScrollBarArrowButtonState.UpNormal;

        /// <summary>
        /// The state of the bottom arrow.
        /// </summary>
        private ScrollBarArrowButtonState bottomButtonState = ScrollBarArrowButtonState.DownNormal;

        /// <summary>
        /// The scrollbar value minimum.
        /// </summary>
        private int minimum;

        /// <summary>
        /// The scrollbar value maximum.
        /// </summary>
        private int maximum = 100;

        /// <summary>
        /// The small change value.
        /// </summary>
        private int smallChange = 1;

        /// <summary>
        /// The large change value.
        /// </summary>
        private int largeChange = 10;

        /// <summary>
        /// The value of the scrollbar.
        /// </summary>
        private int value;

        /// <summary>
        /// The width of the thumb.
        /// </summary>
        private int thumbWidth = 15;

        /// <summary>
        /// The height of the thumb.
        /// </summary>
        private int thumbHeight;

        /// <summary>
        /// The width of an arrow.
        /// </summary>
        private int arrowWidth = 15;

        /// <summary>
        /// The height of an arrow.
        /// </summary>
        private int arrowHeight = 17;

        /// <summary>
        /// The bottom limit for the thumb bottom.
        /// </summary>
        private int thumbBottomLimitBottom;

        /// <summary>
        /// The bottom limit for the thumb top.
        /// </summary>
        private int thumbBottomLimitTop;

        /// <summary>
        /// The top limit for the thumb top.
        /// </summary>
        private int thumbTopLimit;

        /// <summary>
        /// The current position of the thumb.
        /// </summary>
        private int thumbPosition;

        /// <summary>
        /// The track position.
        /// </summary>
        private int trackPosition;

        /// <summary>
        /// The progress timer for moving the thumb.
        /// </summary>
        private Timer progressTimer = new Timer();

        /// <summary>
        /// The border color.
        /// </summary>
        private Color borderColor = Color.FromArgb(93, 140, 201);

        /// <summary>
        /// The border color in disabled state.
        /// </summary>
        private Color disabledBorderColor = Color.Gray;

        #region context menu items

        /// <summary>
        /// Context menu strip.
        /// </summary>
        private ContextMenuStrip contextMenu;

        /// <summary>
        /// Container for components.
        /// </summary>
        private IContainer components;

        /// <summary>
        /// Menu item.
        /// </summary>
        private ToolStripMenuItem tsmiScrollHere;

        /// <summary>
        /// Menu separator.
        /// </summary>
        private ToolStripSeparator toolStripSeparator1;

        /// <summary>
        /// Menu item.
        /// </summary>
        private ToolStripMenuItem tsmiTop;

        /// <summary>
        /// Menu item.
        /// </summary>
        private ToolStripMenuItem tsmiBottom;

        /// <summary>
        /// Menu separator.
        /// </summary>
        private ToolStripSeparator toolStripSeparator2;

        /// <summary>
        /// Menu item.
        /// </summary>
        private ToolStripMenuItem tsmiLargeUp;

        /// <summary>
        /// Menu item.
        /// </summary>
        private ToolStripMenuItem tsmiLargeDown;

        /// <summary>
        /// Menu separator.
        /// </summary>
        private ToolStripSeparator toolStripSeparator3;

        /// <summary>
        /// Menu item.
        /// </summary>
        private ToolStripMenuItem tsmiSmallUp;

        /// <summary>
        /// Menu item.
        /// </summary>
        private ToolStripMenuItem tsmiSmallDown;

        #endregion

        #endregion

        #region constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ScrollBarEx"/> class.
        /// </summary>
        public ScrollBarEx()
        {
            // sets the control styles of the control
            SetStyle(
                  ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw
                  /*| ControlStyles.Selectable*/ | ControlStyles.AllPaintingInWmPaint
                  | ControlStyles.UserPaint, true);

            // initializes the context menu
            this.InitializeComponent();

            this.Width = 19;
            this.Height = 200;

            // sets the scrollbar up
            this.SetUpScrollBar();

            // timer for clicking and holding the mouse button
            // over/below the thumb and on the arrow buttons
            this.progressTimer.Interval = 20;
            this.progressTimer.Tick += this.ProgressTimerTick;

            // no image margin in context menu
            this.contextMenu.ShowImageMargin = false;
            this.ContextMenuStrip = this.contextMenu;
        }

        #endregion

        #region events
        /// <summary>
        /// Occurs when the scrollbar scrolled.
        /// </summary>
        [Category("Behavior")]
        [Description("Is raised, when the scrollbar was scrolled.")]
        public event ScrollEventHandler Scroll;
        #endregion

        #region properties

        /// <summary>
        /// Gets or sets the orientation.
        /// </summary>
        [Category("Layout")]
        [Description("Gets or sets the orientation.")]
        [DefaultValue(ScrollBarOrientation.Vertical)]
        public ScrollBarOrientation Orientation
        {
            get
            {
                return this.orientation;
            }

            set
            {
                // no change - return
                if (value == this.orientation)
                {
                    return;
                }

                this.orientation = value;

                // change text of context menu entries
                this.ChangeContextMenuItems();

                // save scroll orientation for scroll event
                this.scrollOrientation = value == ScrollBarOrientation.Vertical ?
                   ScrollOrientation.VerticalScroll : ScrollOrientation.HorizontalScroll;

                // only in DesignMode switch width and height
                if (this.DesignMode)
                {
                    this.Size = new Size(this.Height, this.Width);
                }

                // sets the scrollbar up
                this.SetUpScrollBar();
            }
        }

        /// <summary>
        /// Gets or sets the minimum value.
        /// </summary>
        [Category("Behavior")]
        [Description("Gets or sets the minimum value.")]
        [DefaultValue(0)]
        public int Minimum
        {
            get
            {
                return this.minimum;
            }

            set
            {
                // no change or value invalid - return
                if (this.minimum == value || value < 0 || value >= this.maximum)
                {
                    return;
                }

                this.minimum = value;

                // current value less than new minimum value - adjust
                if (this.value < value)
                {
                    this.value = value;
                }

                // is current large change value invalid - adjust
                if (this.largeChange > this.maximum - this.minimum)
                {
                    this.largeChange = this.maximum - this.minimum;
                }

                this.SetUpScrollBar();

                // current value less than new minimum value - adjust
                if (this.value < value)
                {
                    this.Value = value;
                }
                else
                {
                    // current value is valid - adjust thumb position
                    this.ChangeThumbPosition(this.GetThumbPosition());

                    this.Refresh();
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum value.
        /// </summary>
        [Category("Behavior")]
        [Description("Gets or sets the maximum value.")]
        [DefaultValue(100)]
        public int Maximum
        {
            get
            {
                return this.maximum;
            }

            set
            {
                // no change or new max. value invalid - return
                if (value == this.maximum || value < 1 || value <= this.minimum)
                {
                    return;
                }

                this.maximum = value;

                // is large change value invalid - adjust
                if (this.largeChange > this.maximum - this.minimum)
                {
                    this.largeChange = this.maximum - this.minimum;
                }

                this.SetUpScrollBar();

                // is current value greater than new maximum value - adjust
                if (this.value > value)
                {
                    this.Value = this.maximum;
                }
                else
                {
                    // current value is valid - adjust thumb position
                    this.ChangeThumbPosition(this.GetThumbPosition());

                    this.Refresh();
                }
            }
        }

        /// <summary>
        /// Gets or sets the small change amount.
        /// </summary>
        [Category("Behavior")]
        [Description("Gets or sets the small change value.")]
        [DefaultValue(1)]
        public int SmallChange
        {
            get
            {
                return this.smallChange;
            }

            set
            {
                // no change or new small change value invalid - return
                if (value == this.smallChange || value < 1 || value >= this.largeChange)
                {
                    return;
                }

                this.smallChange = value;

                this.SetUpScrollBar();
            }
        }

        /// <summary>
        /// Gets or sets the large change amount.
        /// </summary>
        [Category("Behavior")]
        [Description("Gets or sets the large change value.")]
        [DefaultValue(10)]
        public int LargeChange
        {
            get
            {
                return this.largeChange;
            }

            set
            {
                // no change or new large change value is invalid - return
                if (value == this.largeChange || value < this.smallChange || value < 2)
                {
                    return;
                }

                // if value is greater than scroll area - adjust
                if (value > this.maximum - this.minimum)
                {
                    this.largeChange = this.maximum - this.minimum;
                }
                else
                {
                    // set new value
                    this.largeChange = value;
                }

                this.SetUpScrollBar();
            }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        [Category("Behavior")]
        [Description("Gets or sets the current value.")]
        [DefaultValue(0)]
        public int Value
        {
            get
            {
                return this.value;
            }

            set
            {
                // no change or invalid value - return
                if (this.value == value || value < this.minimum || value > this.maximum)
                {
                    return;
                }

                this.value = value;

                // adjust thumb position
                this.ChangeThumbPosition(this.GetThumbPosition());

                // raise scroll event
                this.OnScroll(new ScrollEventArgs(ScrollEventType.ThumbPosition, -1, this.value, this.scrollOrientation));

                this.Refresh();
            }
        }

        /// <summary>
        /// Gets or sets the border color.
        /// </summary>
        [Category("Appearance")]
        [Description("Gets or sets the border color.")]
        [DefaultValue(typeof(Color), "93, 140, 201")]
        public Color BorderColor
        {
            get
            {
                return this.borderColor;
            }

            set
            {
                this.borderColor = value;

                this.Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the border color in disabled state.
        /// </summary>
        [Category("Appearance")]
        [Description("Gets or sets the border color in disabled state.")]
        [DefaultValue(typeof(Color), "Gray")]
        public Color DisabledBorderColor
        {
            get
            {
                return this.disabledBorderColor;
            }

            set
            {
                this.disabledBorderColor = value;

                this.Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the opacity of the context menu (from 0 - 1).
        /// </summary>
        [Category("Appearance")]
        [Description("Gets or sets the opacity of the context menu (from 0 - 1).")]
        [DefaultValue(typeof(double), "1")]
        public double Opacity
        {
            get
            {
                return this.contextMenu.Opacity;
            }

            set
            {
                // no change - return
                if (value == this.contextMenu.Opacity)
                {
                    return;
                }

                this.contextMenu.AllowTransparency = value != 1;

                this.contextMenu.Opacity = value;
            }
        }

        #endregion

        #region methods

        #region public methods

        /// <summary>
        /// Prevents the drawing of the control until <see cref="EndUpdate"/> is called.
        /// </summary>
        public void BeginUpdate()
        {
            SendMessage(this.Handle, SETREDRAW, false, 0);
            this.inUpdate = true;
        }

        /// <summary>
        /// Ends the updating process and the control can draw itself again.
        /// </summary>
        public void EndUpdate()
        {
            SendMessage(this.Handle, SETREDRAW, true, 0);
            this.inUpdate = false;
            this.SetUpScrollBar();
            this.Refresh();
        }

        #endregion

        #region protected methods

        /// <summary>
        /// Raises the <see cref="Scroll"/> event.
        /// </summary>
        /// <param name="e">The <see cref="ScrollEventArgs"/> that contains the event data.</param>
        protected virtual void OnScroll(ScrollEventArgs e)
        {
            // if event handler is attached - raise scroll event
            if (this.Scroll != null)
            {
                this.Scroll(this, e);
            }
        }

        /// <summary>
        /// Paints the background of the control.
        /// </summary>
        /// <param name="e">A <see cref="PaintEventArgs"/> that contains information about the control to paint.</param>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // no painting here
        }

        /// <summary>
        /// Paints the control.
        /// </summary>
        /// <param name="e">A <see cref="PaintEventArgs"/> that contains information about the control to paint.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            // sets the smoothing mode to none
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

            // save client rectangle
            Rectangle rect = ClientRectangle;

            e.Graphics.FillRectangle(App.ColorScheme.ScrollbarBG, rect);

            // adjust the rectangle
            if (this.orientation == ScrollBarOrientation.Vertical)
            {
                rect.X++;
                rect.Y += this.arrowHeight + 1;
                rect.Width -= 2;
                rect.Height -= (this.arrowHeight * 2) + 2;
            }
            else
            {
                rect.X += this.arrowWidth + 1;
                rect.Y++;
                rect.Width -= (this.arrowWidth * 2) + 2;
                rect.Height -= 2;
            }

            // draws the background
            ScrollBarExRenderer.DrawBackground(
               e.Graphics,
               ClientRectangle,
               this.orientation);

            // draws the track
            ScrollBarExRenderer.DrawTrack(
               e.Graphics,
               rect,
               ScrollBarState.Normal,
               this.orientation);

            // draw thumb and grip
            ScrollBarExRenderer.DrawThumb(
               e.Graphics,
               this.thumbRectangle,
               this.thumbState,
               this.orientation);

            if (this.Enabled)
            {
                ScrollBarExRenderer.DrawThumbGrip(
                   e.Graphics,
                   this.thumbRectangle,
                   this.orientation);
            }

            // draw arrows
            ScrollBarExRenderer.DrawArrowButton(
               e.Graphics,
               this.topArrowRectangle,
               this.topButtonState,
               true,
               this.orientation);

            ScrollBarExRenderer.DrawArrowButton(
               e.Graphics,
               this.bottomArrowRectangle,
               this.bottomButtonState,
               false,
               this.orientation);

            // check if top or bottom bar was clicked
            if (this.topBarClicked)
            {
                if (this.orientation == ScrollBarOrientation.Vertical)
                {
                    this.clickedBarRectangle.Y = this.thumbTopLimit;
                    this.clickedBarRectangle.Height =
                       this.thumbRectangle.Y - this.thumbTopLimit;
                }
                else
                {
                    this.clickedBarRectangle.X = this.thumbTopLimit;
                    this.clickedBarRectangle.Width =
                       this.thumbRectangle.X - this.thumbTopLimit;
                }

                ScrollBarExRenderer.DrawTrack(
                   e.Graphics,
                   this.clickedBarRectangle,
                   ScrollBarState.Pressed,
                   this.orientation);
            }
            else if (this.bottomBarClicked)
            {
                if (this.orientation == ScrollBarOrientation.Vertical)
                {
                    this.clickedBarRectangle.Y = this.thumbRectangle.Bottom + 1;
                    this.clickedBarRectangle.Height =
                       this.thumbBottomLimitBottom - this.clickedBarRectangle.Y + 1;
                }
                else
                {
                    this.clickedBarRectangle.X = this.thumbRectangle.Right + 1;
                    this.clickedBarRectangle.Width =
                       this.thumbBottomLimitBottom - this.clickedBarRectangle.X + 1;
                }

                ScrollBarExRenderer.DrawTrack(
                   e.Graphics,
                   this.clickedBarRectangle,
                   ScrollBarState.Pressed,
                   this.orientation);
            }

            // draw border
            //using (Pen pen = new Pen(
            //   (this.Enabled ? this.borderColor : this.disabledBorderColor)))
            //{
            //    e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
            //}
        }

        /// <summary>
        /// Raises the MouseDown event.
        /// </summary>
        /// <param name="e">A <see cref="MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            //this.Focus();

            if (e.Button == MouseButtons.Left)
            {
                // prevents showing the context menu if pressing the right mouse
                // button while holding the left
                this.ContextMenuStrip = null;

                Point mouseLocation = e.Location;

                if (this.thumbRectangle.Contains(mouseLocation))
                {
                    this.thumbClicked = true;
                    this.thumbPosition = this.orientation == ScrollBarOrientation.Vertical ? mouseLocation.Y - this.thumbRectangle.Y : mouseLocation.X - this.thumbRectangle.X;
                    this.thumbState = ScrollBarState.Pressed;

                    Invalidate(this.thumbRectangle);
                }
                else if (this.topArrowRectangle.Contains(mouseLocation))
                {
                    this.topArrowClicked = true;
                    this.topButtonState = ScrollBarArrowButtonState.UpPressed;

                    this.Invalidate(this.topArrowRectangle);

                    this.ProgressThumb(true);
                }
                else if (this.bottomArrowRectangle.Contains(mouseLocation))
                {
                    this.bottomArrowClicked = true;
                    this.bottomButtonState = ScrollBarArrowButtonState.DownPressed;

                    this.Invalidate(this.bottomArrowRectangle);

                    this.ProgressThumb(true);
                }
                else
                {
                    this.trackPosition =
                       this.orientation == ScrollBarOrientation.Vertical ?
                          mouseLocation.Y : mouseLocation.X;

                    if (this.trackPosition <
                       (this.orientation == ScrollBarOrientation.Vertical ?
                          this.thumbRectangle.Y : this.thumbRectangle.X))
                    {
                        this.topBarClicked = true;
                    }
                    else
                    {
                        this.bottomBarClicked = true;
                    }

                    this.ProgressThumb(true);
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                this.trackPosition =
                   this.orientation == ScrollBarOrientation.Vertical ? e.Y : e.X;
            }
        }

        /// <summary>
        /// Raises the MouseUp event.
        /// </summary>
        /// <param name="e">A <see cref="MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Left)
            {
                this.ContextMenuStrip = this.contextMenu;

                if (this.thumbClicked)
                {
                    this.thumbClicked = false;
                    this.thumbState = ScrollBarState.Normal;

                    this.OnScroll(new ScrollEventArgs(
                       ScrollEventType.EndScroll,
                       -1,
                       this.value,
                       this.scrollOrientation)
                    );
                }
                else if (this.topArrowClicked)
                {
                    this.topArrowClicked = false;
                    this.topButtonState = ScrollBarArrowButtonState.UpNormal;
                    this.StopTimer();
                }
                else if (this.bottomArrowClicked)
                {
                    this.bottomArrowClicked = false;
                    this.bottomButtonState = ScrollBarArrowButtonState.DownNormal;
                    this.StopTimer();
                }
                else if (this.topBarClicked)
                {
                    this.topBarClicked = false;
                    this.StopTimer();
                }
                else if (this.bottomBarClicked)
                {
                    this.bottomBarClicked = false;
                    this.StopTimer();
                }

                Invalidate();
            }
        }

        /// <summary>
        /// Raises the MouseEnter event.
        /// </summary>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            this.bottomButtonState = ScrollBarArrowButtonState.DownActive;
            this.topButtonState = ScrollBarArrowButtonState.UpActive;
            this.thumbState = ScrollBarState.Active;

            Invalidate();
        }

        /// <summary>
        /// Raises the MouseLeave event.
        /// </summary>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            this.ResetScrollStatus();
        }

        /// <summary>
        /// Raises the MouseMove event.
        /// </summary>
        /// <param name="e">A <see cref="MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // moving and holding the left mouse button
            if (e.Button == MouseButtons.Left)
            {
                // Update the thumb position, if the new location is within the bounds.
                if (this.thumbClicked)
                {
                    int oldScrollValue = this.value;

                    this.topButtonState = ScrollBarArrowButtonState.UpActive;
                    this.bottomButtonState = ScrollBarArrowButtonState.DownActive;
                    int pos = this.orientation == ScrollBarOrientation.Vertical ?
                       e.Location.Y : e.Location.X;

                    // The thumb is all the way to the top
                    if (pos <= (this.thumbTopLimit + this.thumbPosition))
                    {
                        this.ChangeThumbPosition(this.thumbTopLimit);

                        this.value = this.minimum;
                    }
                    else if (pos >= (this.thumbBottomLimitTop + this.thumbPosition))
                    {
                        // The thumb is all the way to the bottom
                        this.ChangeThumbPosition(this.thumbBottomLimitTop);

                        this.value = this.maximum;
                    }
                    else
                    {
                        // The thumb is between the ends of the track.
                        this.ChangeThumbPosition(pos - this.thumbPosition);

                        int pixelRange, thumbPos, arrowSize;

                        // calculate the value - first some helper variables
                        // dependent on the current orientation
                        if (this.orientation == ScrollBarOrientation.Vertical)
                        {
                            pixelRange = this.Height - (2 * this.arrowHeight) - this.thumbHeight;
                            thumbPos = this.thumbRectangle.Y;
                            arrowSize = this.arrowHeight;
                        }
                        else
                        {
                            pixelRange = this.Width - (2 * this.arrowWidth) - this.thumbWidth;
                            thumbPos = this.thumbRectangle.X;
                            arrowSize = this.arrowWidth;
                        }

                        float perc = 0f;

                        if (pixelRange != 0)
                        {
                            // percent of the new position
                            perc = (float)(thumbPos - arrowSize) / (float)pixelRange;
                        }

                        // the new value is somewhere between max and min, starting
                        // at min position
                        this.value = Convert.ToInt32((perc * (this.maximum - this.minimum)) + this.minimum);
                    }

                    // raise scroll event if new value different
                    if (oldScrollValue != this.value)
                    {
                        this.OnScroll(new ScrollEventArgs(ScrollEventType.ThumbTrack, oldScrollValue, this.value, this.scrollOrientation));

                        this.Refresh();
                    }
                }
            }
            else if (!this.ClientRectangle.Contains(e.Location))
            {
                this.ResetScrollStatus();
            }
            else if (e.Button == MouseButtons.None) // only moving the mouse
            {
                if (this.topArrowRectangle.Contains(e.Location))
                {
                    this.topButtonState = ScrollBarArrowButtonState.UpHot;

                    this.Invalidate(this.topArrowRectangle);
                }
                else if (this.bottomArrowRectangle.Contains(e.Location))
                {
                    this.bottomButtonState = ScrollBarArrowButtonState.DownHot;

                    Invalidate(this.bottomArrowRectangle);
                }
                else if (this.thumbRectangle.Contains(e.Location))
                {
                    this.thumbState = ScrollBarState.Hot;

                    this.Invalidate(this.thumbRectangle);
                }
                else if (this.ClientRectangle.Contains(e.Location))
                {
                    this.topButtonState = ScrollBarArrowButtonState.UpActive;
                    this.bottomButtonState = ScrollBarArrowButtonState.DownActive;
                    this.thumbState = ScrollBarState.Active;

                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Performs the work of setting the specified bounds of this control.
        /// </summary>
        /// <param name="x">The new x value of the control.</param>
        /// <param name="y">The new y value of the control.</param>
        /// <param name="width">The new width value of the control.</param>
        /// <param name="height">The new height value of the control.</param>
        /// <param name="specified">A bitwise combination of the <see cref="BoundsSpecified"/> values.</param>
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            // only in design mode - constrain size
            if (this.DesignMode)
            {
                if (this.orientation == ScrollBarOrientation.Vertical)
                {
                    if (height < (2 * this.arrowHeight) + 10)
                    {
                        height = (2 * this.arrowHeight) + 10;
                    }

                    width = 19;
                }
                else
                {
                    if (width < (2 * this.arrowWidth) + 10)
                    {
                        width = (2 * this.arrowWidth) + 10;
                    }

                    height = 19;
                }
            }

            base.SetBoundsCore(x, y, width, height, specified);

            if (this.DesignMode)
            {
                this.SetUpScrollBar();
            }
        }

        /// <summary>
        /// Raises the <see cref="System.Windows.Forms.Control.SizeChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            this.SetUpScrollBar();
        }

        /// <summary>
        /// Processes a dialog key.
        /// </summary>
        /// <param name="keyData">One of the <see cref="System.Windows.Forms.Keys"/> values that represents the key to process.</param>
        /// <returns>true, if the key was processed by the control, false otherwise.</returns>
        protected override bool ProcessDialogKey(Keys keyData)
        {
            // key handling is here - keys recognized by the control
            // Up&Down or Left&Right, PageUp, PageDown, Home, End
            Keys keyUp = Keys.Up;
            Keys keyDown = Keys.Down;

            if (this.orientation == ScrollBarOrientation.Horizontal)
            {
                keyUp = Keys.Left;
                keyDown = Keys.Right;
            }

            if (keyData == keyUp)
            {
                this.Value -= this.smallChange;

                return true;
            }

            if (keyData == keyDown)
            {
                this.Value += this.smallChange;

                return true;
            }

            if (keyData == Keys.PageUp)
            {
                this.Value = this.GetValue(false, true);

                return true;
            }

            if (keyData == Keys.PageDown)
            {
                if (this.value + this.largeChange > this.maximum)
                {
                    this.Value = this.maximum;
                }
                else
                {
                    this.Value += this.largeChange;
                }

                return true;
            }

            if (keyData == Keys.Home)
            {
                this.Value = this.minimum;

                return true;
            }

            if (keyData == Keys.End)
            {
                this.Value = this.maximum;

                return true;
            }

            return base.ProcessDialogKey(keyData);
        }

        /// <summary>
        /// Raises the <see cref="System.Windows.Forms.Control.EnabledChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);

            if (this.Enabled)
            {
                this.thumbState = ScrollBarState.Normal;
                this.topButtonState = ScrollBarArrowButtonState.UpNormal;
                this.bottomButtonState = ScrollBarArrowButtonState.DownNormal;
            }
            else
            {
                this.thumbState = ScrollBarState.Disabled;
                this.topButtonState = ScrollBarArrowButtonState.UpDisabled;
                this.bottomButtonState = ScrollBarArrowButtonState.DownDisabled;
            }

            this.Refresh();
        }

        #endregion

        #region misc methods

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="wnd">The handle of the control.</param>
        /// <param name="msg">The message as int.</param>
        /// <param name="param">param - true or false.</param>
        /// <param name="lparam">Additional parameter.</param>
        /// <returns>0 or error code.</returns>
        /// <remarks>Needed for sending the stop/start drawing of the control.</remarks>
        [DllImport("user32.dll")]
        private static extern int SendMessage(
           IntPtr wnd,
           int msg,
           bool param,
           int lparam);

        /// <summary>
        /// Sets up the scrollbar.
        /// </summary>
        private void SetUpScrollBar()
        {
            // if no drawing - return
            if (this.inUpdate)
            {
                return;
            }

            // set up the width's, height's and rectangles for the different
            // elements
            if (this.orientation == ScrollBarOrientation.Vertical)
            {
                this.arrowHeight = 17;
                this.arrowWidth = 15;
                this.thumbWidth = 15;
                this.thumbHeight = this.GetThumbSize();

                this.clickedBarRectangle = this.ClientRectangle;
                this.clickedBarRectangle.Inflate(-1, -1);
                this.clickedBarRectangle.Y += this.arrowHeight;
                this.clickedBarRectangle.Height -= this.arrowHeight * 2;

                this.channelRectangle = this.clickedBarRectangle;

                this.thumbRectangle = new Rectangle(
                   ClientRectangle.X + 2,
                   ClientRectangle.Y + this.arrowHeight + 1,
                   this.thumbWidth - 1,
                   this.thumbHeight
                );

                this.topArrowRectangle = new Rectangle(
                   ClientRectangle.X + 2,
                   ClientRectangle.Y + 1,
                   this.arrowWidth,
                   this.arrowHeight
                );

                this.bottomArrowRectangle = new Rectangle(
                   ClientRectangle.X + 2,
                   ClientRectangle.Bottom - this.arrowHeight - 1,
                   this.arrowWidth,
                   this.arrowHeight
                );

                // Set the default starting thumb position.
                this.thumbPosition = this.thumbRectangle.Height / 2;

                // Set the bottom limit of the thumb's bottom border.
                this.thumbBottomLimitBottom =
                   ClientRectangle.Bottom - this.arrowHeight - 2;

                // Set the bottom limit of the thumb's top border.
                this.thumbBottomLimitTop =
                   this.thumbBottomLimitBottom - this.thumbRectangle.Height;

                // Set the top limit of the thumb's top border.
                this.thumbTopLimit = ClientRectangle.Y + this.arrowHeight + 1;
            }
            else
            {
                this.arrowHeight = 15;
                this.arrowWidth = 17;
                this.thumbHeight = 15;
                this.thumbWidth = this.GetThumbSize();

                this.clickedBarRectangle = this.ClientRectangle;
                this.clickedBarRectangle.Inflate(-1, -1);
                this.clickedBarRectangle.X += this.arrowWidth;
                this.clickedBarRectangle.Width -= this.arrowWidth * 2;

                this.channelRectangle = this.clickedBarRectangle;

                this.thumbRectangle = new Rectangle(
                   ClientRectangle.X + this.arrowWidth + 1,
                   ClientRectangle.Y + 2,
                   this.thumbWidth,
                   this.thumbHeight - 1
                );

                this.topArrowRectangle = new Rectangle(
                   ClientRectangle.X + 1,
                   ClientRectangle.Y + 2,
                   this.arrowWidth,
                   this.arrowHeight
                );

                this.bottomArrowRectangle = new Rectangle(
                   ClientRectangle.Right - this.arrowWidth - 1,
                   ClientRectangle.Y + 2,
                   this.arrowWidth,
                   this.arrowHeight
                );

                // Set the default starting thumb position.
                this.thumbPosition = this.thumbRectangle.Width / 2;

                // Set the bottom limit of the thumb's bottom border.
                this.thumbBottomLimitBottom =
                   ClientRectangle.Right - this.arrowWidth - 2;

                // Set the bottom limit of the thumb's top border.
                this.thumbBottomLimitTop =
                   this.thumbBottomLimitBottom - this.thumbRectangle.Width;

                // Set the top limit of the thumb's top border.
                this.thumbTopLimit = ClientRectangle.X + this.arrowWidth + 1;
            }

            this.ChangeThumbPosition(this.GetThumbPosition());

            this.Refresh();
        }

        /// <summary>
        /// Handles the updating of the thumb.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">An object that contains the event data.</param>
        private void ProgressTimerTick(object sender, EventArgs e)
        {
            this.ProgressThumb(true);
        }

        /// <summary>
        /// Resets the scroll status of the scrollbar.
        /// </summary>
        private void ResetScrollStatus()
        {
            // get current mouse position
            Point pos = this.PointToClient(Cursor.Position);

            // set appearance of buttons in relation to where the mouse is -
            // outside or inside the control
            if (this.ClientRectangle.Contains(pos))
            {
                this.bottomButtonState = ScrollBarArrowButtonState.DownActive;
                this.topButtonState = ScrollBarArrowButtonState.UpActive;
            }
            else
            {
                this.bottomButtonState = ScrollBarArrowButtonState.DownNormal;
                this.topButtonState = ScrollBarArrowButtonState.UpNormal;
            }

            // set appearance of thumb
            this.thumbState = this.thumbRectangle.Contains(pos) ?
               ScrollBarState.Hot : ScrollBarState.Normal;

            this.bottomArrowClicked = this.bottomBarClicked =
               this.topArrowClicked = this.topBarClicked = false;

            this.StopTimer();

            this.Refresh();
        }

        /// <summary>
        /// Calculates the new value of the scrollbar.
        /// </summary>
        /// <param name="smallIncrement">true for a small change, false otherwise.</param>
        /// <param name="up">true for up movement, false otherwise.</param>
        /// <returns>The new scrollbar value.</returns>
        private int GetValue(bool smallIncrement, bool up)
        {
            int newValue;

            // calculate the new value of the scrollbar
            // with checking if new value is in bounds (min/max)
            if (up)
            {
                newValue = this.value - (smallIncrement ? this.smallChange : this.largeChange);

                if (newValue < this.minimum)
                {
                    newValue = this.minimum;
                }
            }
            else
            {
                newValue = this.value + (smallIncrement ? this.smallChange : this.largeChange);

                if (newValue > this.maximum)
                {
                    newValue = this.maximum;
                }
            }

            return newValue;
        }

        /// <summary>
        /// Calculates the new thumb position.
        /// </summary>
        /// <returns>The new thumb position.</returns>
        private int GetThumbPosition()
        {
            int pixelRange, arrowSize;

            if (this.orientation == ScrollBarOrientation.Vertical)
            {
                pixelRange = this.Height - (2 * this.arrowHeight) - this.thumbHeight;
                arrowSize = this.arrowHeight;
            }
            else
            {
                pixelRange = this.Width - (2 * this.arrowWidth) - this.thumbWidth;
                arrowSize = this.arrowWidth;
            }

            int realRange = this.maximum - this.minimum;
            float perc = 0f;

            if (realRange != 0)
            {
                perc = ((float)this.value - (float)this.minimum) / (float)realRange;
            }

            return Math.Max(this.thumbTopLimit, Math.Min(
               this.thumbBottomLimitTop,
               Convert.ToInt32((perc * pixelRange) + arrowSize)));
        }

        /// <summary>
        /// Calculates the height of the thumb.
        /// </summary>
        /// <returns>The height of the thumb.</returns>
        private int GetThumbSize()
        {
            int trackSize =
               this.orientation == ScrollBarOrientation.Vertical ?
               this.Height - (2 * this.arrowHeight) : this.Width - (2 * this.arrowWidth);

            if (this.maximum == 0 || this.largeChange == 0)
            {
                return trackSize;
            }

            float newThumbSize = ((float)this.largeChange * (float)trackSize) / (float)this.maximum;

            return Convert.ToInt32(Math.Min((float)trackSize, Math.Max(newThumbSize, 10f)));
        }

        /// <summary>
        /// Enables the timer.
        /// </summary>
        private void EnableTimer()
        {
            // if timer is not already enabled - enable it
            if (!this.progressTimer.Enabled)
            {
                this.progressTimer.Interval = 600;
                this.progressTimer.Start();
            }
            else
            {
                // if already enabled, change tick time
                this.progressTimer.Interval = 10;
            }
        }

        /// <summary>
        /// Stops the progress timer.
        /// </summary>
        private void StopTimer()
        {
            this.progressTimer.Stop();
        }

        /// <summary>
        /// Changes the position of the thumb.
        /// </summary>
        /// <param name="position">The new position.</param>
        private void ChangeThumbPosition(int position)
        {
            if (this.orientation == ScrollBarOrientation.Vertical)
            {
                this.thumbRectangle.Y = position;
            }
            else
            {
                this.thumbRectangle.X = position;
            }
        }

        /// <summary>
        /// Controls the movement of the thumb.
        /// </summary>
        /// <param name="enableTimer">true for enabling the timer, false otherwise.</param>
        private void ProgressThumb(bool enableTimer)
        {
            int scrollOldValue = this.value;
            ScrollEventType type = ScrollEventType.First;
            int thumbSize, thumbPos;

            if (this.orientation == ScrollBarOrientation.Vertical)
            {
                thumbPos = this.thumbRectangle.Y;
                thumbSize = this.thumbRectangle.Height;
            }
            else
            {
                thumbPos = this.thumbRectangle.X;
                thumbSize = this.thumbRectangle.Width;
            }

            // arrow down or shaft down clicked
            if (this.bottomArrowClicked || (this.bottomBarClicked && (thumbPos + thumbSize) < this.trackPosition))
            {
                type = this.bottomArrowClicked ? ScrollEventType.SmallIncrement : ScrollEventType.LargeIncrement;

                this.value = this.GetValue(this.bottomArrowClicked, false);

                if (this.value == this.maximum)
                {
                    this.ChangeThumbPosition(this.thumbBottomLimitTop);

                    type = ScrollEventType.Last;
                }
                else
                {
                    this.ChangeThumbPosition(Math.Min(this.thumbBottomLimitTop, this.GetThumbPosition()));
                }
            }
            else if (this.topArrowClicked || (this.topBarClicked && thumbPos > this.trackPosition))
            {
                type = this.topArrowClicked ? ScrollEventType.SmallDecrement : ScrollEventType.LargeDecrement;

                // arrow up or shaft up clicked
                this.value = this.GetValue(this.topArrowClicked, true);

                if (this.value == this.minimum)
                {
                    this.ChangeThumbPosition(this.thumbTopLimit);

                    type = ScrollEventType.First;
                }
                else
                {
                    this.ChangeThumbPosition(Math.Max(this.thumbTopLimit, this.GetThumbPosition()));
                }
            }
            else if (!((this.topArrowClicked && thumbPos == this.thumbTopLimit) || (this.bottomArrowClicked && thumbPos == this.thumbBottomLimitTop)))
            {
                this.ResetScrollStatus();

                return;
            }

            if (scrollOldValue != this.value)
            {
                this.OnScroll(new ScrollEventArgs(type, scrollOldValue, this.value, this.scrollOrientation));

                this.Invalidate(this.channelRectangle);

                if (enableTimer)
                {
                    this.EnableTimer();
                }
            }
            else
            {
                if (this.topArrowClicked)
                {
                    type = ScrollEventType.SmallDecrement;
                }
                else if (this.bottomArrowClicked)
                {
                    type = ScrollEventType.SmallIncrement;
                }

                this.OnScroll(new ScrollEventArgs(type, this.value));
            }
        }

        /// <summary>
        /// Changes the displayed text of the context menu items dependent of the current <see cref="ScrollBarOrientation"/>.
        /// </summary>
        private void ChangeContextMenuItems()
        {
            if (this.orientation == ScrollBarOrientation.Vertical)
            {
                this.tsmiTop.Text = "Top";
                this.tsmiBottom.Text = "Bottom";
                this.tsmiLargeDown.Text = "Page down";
                this.tsmiLargeUp.Text = "Page up";
                this.tsmiSmallDown.Text = "Scroll down";
                this.tsmiSmallUp.Text = "Scroll up";
                this.tsmiScrollHere.Text = "Scroll here";
            }
            else
            {
                this.tsmiTop.Text = "Left";
                this.tsmiBottom.Text = "Right";
                this.tsmiLargeDown.Text = "Page left";
                this.tsmiLargeUp.Text = "Page right";
                this.tsmiSmallDown.Text = "Scroll right";
                this.tsmiSmallUp.Text = "Scroll left";
                this.tsmiScrollHere.Text = "Scroll here";
            }
        }

        #endregion

        #region context menu methods

        /// <summary>
        /// Initializes the context menu.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiScrollHere = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiTop = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiBottom = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiLargeUp = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiLargeDown = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiSmallUp = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiSmallDown = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenu
            // 
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiScrollHere,
            this.toolStripSeparator1,
            this.tsmiTop,
            this.tsmiBottom,
            this.toolStripSeparator2,
            this.tsmiLargeUp,
            this.tsmiLargeDown,
            this.toolStripSeparator3,
            this.tsmiSmallUp,
            this.tsmiSmallDown});
            this.contextMenu.Name = "contextMenu";
            this.contextMenu.Size = new System.Drawing.Size(151, 176);
            // 
            // tsmiScrollHere
            // 
            this.tsmiScrollHere.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsmiScrollHere.Name = "tsmiScrollHere";
            this.tsmiScrollHere.Size = new System.Drawing.Size(150, 22);
            this.tsmiScrollHere.Text = "Scroll here";
            this.tsmiScrollHere.Click += ScrollHereClick;
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(147, 6);
            // 
            // tsmiTop
            // 
            this.tsmiTop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsmiTop.Name = "tsmiTop";
            this.tsmiTop.Size = new System.Drawing.Size(150, 22);
            this.tsmiTop.Text = "Top";
            this.tsmiTop.Click += this.TopClick;
            // 
            // tsmiBottom
            // 
            this.tsmiBottom.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsmiBottom.Name = "tsmiBottom";
            this.tsmiBottom.Size = new System.Drawing.Size(150, 22);
            this.tsmiBottom.Text = "Bottom";
            this.tsmiBottom.Click += this.BottomClick;
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(147, 6);
            // 
            // tsmiLargeUp
            // 
            this.tsmiLargeUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsmiLargeUp.Name = "tsmiLargeUp";
            this.tsmiLargeUp.Size = new System.Drawing.Size(150, 22);
            this.tsmiLargeUp.Text = "Page up";
            this.tsmiLargeUp.Click += this.LargeUpClick;
            // 
            // tsmiLargeDown
            // 
            this.tsmiLargeDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsmiLargeDown.Name = "tsmiLargeDown";
            this.tsmiLargeDown.Size = new System.Drawing.Size(150, 22);
            this.tsmiLargeDown.Text = "Page down";
            this.tsmiLargeDown.Click += this.LargeDownClick;
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(147, 6);
            // 
            // tsmiSmallUp
            // 
            this.tsmiSmallUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsmiSmallUp.Name = "tsmiSmallUp";
            this.tsmiSmallUp.Size = new System.Drawing.Size(150, 22);
            this.tsmiSmallUp.Text = "Scroll up";
            this.tsmiSmallUp.Click += this.SmallUpClick;
            // 
            // tsmiSmallDown
            // 
            this.tsmiSmallDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsmiSmallDown.Name = "tsmiSmallDown";
            this.tsmiSmallDown.Size = new System.Drawing.Size(150, 22);
            this.tsmiSmallDown.Text = "Scroll down";
            this.tsmiSmallDown.Click += this.SmallDownClick;
            this.contextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        /// <summary>
        /// Context menu handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ScrollHereClick(object sender, EventArgs e)
        {
            int thumbSize, thumbPos, arrowSize, size;

            if (this.orientation == ScrollBarOrientation.Vertical)
            {
                thumbSize = this.thumbHeight;
                arrowSize = this.arrowHeight;
                size = this.Height;

                this.ChangeThumbPosition(Math.Max(this.thumbTopLimit, Math.Min(this.thumbBottomLimitTop, this.trackPosition - (this.thumbRectangle.Height / 2))));

                thumbPos = this.thumbRectangle.Y;
            }
            else
            {
                thumbSize = this.thumbWidth;
                arrowSize = this.arrowWidth;
                size = this.Width;

                this.ChangeThumbPosition(Math.Max(this.thumbTopLimit, Math.Min(this.thumbBottomLimitTop, this.trackPosition - (this.thumbRectangle.Width / 2))));

                thumbPos = this.thumbRectangle.X;
            }

            int pixelRange = size - (2 * arrowSize) - thumbSize;
            float perc = 0f;

            if (pixelRange != 0)
            {
                perc = (float)(thumbPos - arrowSize) / (float)pixelRange;
            }

            int oldValue = this.value;

            this.value = Convert.ToInt32((perc * (this.maximum - this.minimum)) + this.minimum);

            this.OnScroll(new ScrollEventArgs(ScrollEventType.ThumbPosition, oldValue, this.value, this.scrollOrientation));

            this.Refresh();
        }

        /// <summary>
        /// Context menu handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void TopClick(object sender, EventArgs e)
        {
            this.Value = this.minimum;
        }

        /// <summary>
        /// Context menu handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void BottomClick(object sender, EventArgs e)
        {
            this.Value = this.maximum;
        }

        /// <summary>
        /// Context menu handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void LargeUpClick(object sender, EventArgs e)
        {
            this.Value = this.GetValue(false, true);
        }

        /// <summary>
        /// Context menu handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void LargeDownClick(object sender, EventArgs e)
        {
            this.Value = this.GetValue(false, false);
        }

        /// <summary>
        /// Context menu handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void SmallUpClick(object sender, EventArgs e)
        {
            this.Value = this.GetValue(true, true);
        }

        /// <summary>
        /// Context menu handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void SmallDownClick(object sender, EventArgs e)
        {
            this.Value = this.GetValue(true, false);
        }

        #endregion

        #endregion
    }
}