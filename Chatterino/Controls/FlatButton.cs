using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chatterino.Controls
{
    class FlatButton : Control
    {
        bool mouseOver = false;
        bool mouseDown = false;

        private Image image;

        public Image Image
        {
            get { return image; }
            set { image = value; Invalidate(); }
        }

        public FlatButton()
        {
            TextChanged += (s, e) => calcSize();
            SizeChanged += (s, e) => calcSize();
            FontChanged += (s, e) => calcSize();

            MouseEnter += (s, e) =>
            {
                mouseOver = true;
                Invalidate();
            };

            MouseLeave += (s, e) =>
            {
                mouseOver = false;
                Invalidate();
            };

            MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    mouseDown = true;
                    Invalidate();
                }
            };

            MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    mouseDown = false;
                    Invalidate();
                }
            };
        }

        void calcSize()
        {
            if (Text != "")
            {
                int width = Width;
                var size = TextRenderer.MeasureText(Text, Font);

                Width = (int)(size.Height * 0.6f) + size.Width;

                Height = (int)(size.Height * 1.4f);

                if ((Anchor & AnchorStyles.Right) == AnchorStyles.Right)
                {
                    Location = new Point(Location.X - (Width - width), Location.Y);
                }
                Invalidate();
            }
        }

        Brush mouseOverBrush = new SolidBrush(Color.FromArgb(48, 255, 255, 255));

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            try
            {
                e.Graphics.FillRectangle(App.ColorScheme?.Menu ?? Brushes.Red, e.ClipRectangle);
            }
            catch { }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;

            if (mouseDown)
                g.FillRectangle(mouseOverBrush, 0, 0, Width, Height);

            if (mouseOver)
                g.FillRectangle(mouseOverBrush, 0, 0, Width, Height);

            if (image != null)
            {
                g.DrawImage(image, Width / 2 - image.Width / 2, Height / 2 - image.Height / 2);
            }

            if (Text != null)
            {
                TextRenderer.DrawText(g, Text, Font, ClientRectangle, App.ColorScheme?.Text ?? Color.Black, App.DefaultTextFormatFlags | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }
    }
}
