using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;

namespace Chatterino.Controls
{
#if DEBUG
	[Designer(typeof(SettingsTabPageDesigner))]
#endif
    public class SettingsTabPage : Control
    {
        public event EventHandler PanelChanged;

        private Panel panel;

        public Panel Panel
        {
            get { return panel; }
            set { panel = value; if (PanelChanged != null && selected) PanelChanged(this, EventArgs.Empty); }
        }

        private Image image = null;

        public Image Image
        {
            get { return image; }
            set { image = value; Invalidate(); }
        }

        private Boolean selected;

        [DefaultValue(true)]
        public Boolean Selected
        {
            get { return selected; }
            set { selected = value; Invalidate(); }
        }

        protected override void OnClick(EventArgs e)
        {

        }

        public SettingsTabPage()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            Visible = true;
            this.Location = new Point(0, 0);
            this.Size = new Size(200, 30);
            this.BackColor = Color.FromArgb(64, 64, 64);
            selected = false;
            Invalidate();
        }

        public SettingsTabPage(string text, Image image, bool selected)
        {
            Text = text;
            this.image = image;
            Visible = true;
            this.Location = new Point(0, 0);
            this.Size = new Size(200, 30);
            this.BackColor = Color.FromArgb(64, 64, 64);
            this.selected = selected;
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            Panel.Dispose();
            base.Dispose(disposing);
        }

        public Rectangle GetTabRect()
        {
            return new Rectangle(/*Parent.PointToClient(*/PointToScreen(new Point(0, 0))/*)*/, Size);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
            //if (mOver)
            //    e.Graphics.FillRectangle(Styles.SettingsTabHighlight1, ClientRectangle);
            if (selected)
            {
                using (var gradientBrush = new LinearGradientBrush(new Point(0, 0), new Point(Width, 0)
                , BackColor, Color.FromArgb(80, 80, 80)))
                {
                    e.Graphics.FillRectangle(gradientBrush, 0, 0, Width, Height);
                }
                //using (var gradientBrush = new LinearGradientBrush(new Point(Width-64, 0), new Point(Width, 0)
                //, Color.Transparent, Color.FromArgb(80,80,80)))
                //{
                //    e.Graphics.FillRectangle(gradientBrush, Width-64, 0, 64, Height);
                //}
            }
            else
            {
                using (var gradientBrush = new LinearGradientBrush(new Point(Width - 16, 0), new Point(Width, 0)
                , Color.Transparent, Color.FromArgb(31, 0, 0, 0)))
                {
                    e.Graphics.FillRectangle(gradientBrush, Width - 16, 0, 16, Height);
                }
            }

            //using (var gradientBrush = new LinearGradientBrush(new Point(0, 0), new Point(16, 0)
            //    , Color.FromArgb(31, 0, 0, 0), BackColor))
            //{
            //    e.Graphics.FillRectangle(gradientBrush, 0, 0, 16, Height);
            //}

            //e.Graphics.DrawLine(Pens.DimGray, 0, Height-1, Width, Height-1);
            //e.Graphics.DrawRectangle(Pens.LightGray, 1, 1, Width - 2, Height - 2);
            //e.Graphics.DrawRectangle(Pens.DimGray, 0, 0, Width - 1, Height - 1);
            //e.Graphics.FillRectangle(Brushes.Gray, 0, 0, 1, 1);
            //e.Graphics.FillRectangle(Brushes.Gray, Width-1, 0, 1, 1);
            //e.Graphics.FillRectangle(Brushes.Gray, Width-1, Height - 1, 1, 1);
            //e.Graphics.FillRectangle(Brushes.Gray, 0,       Height - 1, 1, 1);
            if (Image != null)
                e.Graphics.DrawImage(image, (Height - (Math.Min(image.Width, Height - 4))) / 2, (Height - (Math.Min(image.Height, Height - 4))) / 2, Math.Min(image.Width, Height - 4), Math.Min(image.Height, Height - 4));
            e.Graphics.DrawString(Text, Font, Brushes.White, Height, Height / 2 - 7);
        }

#pragma warning disable CS0414
        bool mOver = false;
#pragma warning restore CS0414

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            mOver = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            mOver = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                base.OnClick(EventArgs.Empty);

            Invalidate();
        }
    }
}
