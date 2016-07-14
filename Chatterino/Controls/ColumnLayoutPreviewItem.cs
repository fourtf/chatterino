using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Chatterino.Controls
{
    public class ColumnLayoutPreviewItem : Control
    {
        public ColumnLayoutPreviewItem()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        private bool isError;

        public bool IsError
        {
            get { return isError; }
            set
            {
                if (isError != value)
                {
                    isError = value;
                    Invalidate();
                }
            }
        }

        Brush backBrush = new SolidBrush(Color.FromArgb(100, Color.Blue));
        Brush errorBrush = new SolidBrush(Color.FromArgb(100, Color.Red));

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if ((Parent != null))
            {
                using (Bitmap behind = new Bitmap(Parent.Width, Parent.Height))
                {
                    foreach (Control c in Parent.Controls)
                    {
                        if (c != this && c.Bounds.IntersectsWith(this.Bounds))
                        {
                            c.DrawToBitmap(behind, c.Bounds);
                        }
                    }
                    e.Graphics.DrawImage(behind, -Left, -Top);
                }
            }

            //e.Graphics.FillRectangle(IsError ? errorBrush : backBrush, e.ClipRectangle);
            e.Graphics.FillRectangle(IsError ? errorBrush : backBrush, 8, 8, Width - 17, Height - 17);
        }
    }
}
