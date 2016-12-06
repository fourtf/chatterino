using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chatterino.Controls
{
    public class MoreButton : Control
    {
        private MoreButtonIcon icon;

        public MoreButtonIcon Icon
        {
            get { return icon; }
            set
            {
                icon = value;
                Invalidate();
            }
        }

        bool mover, mdown;

        public MoreButton()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            MouseEnter += (s, e) =>
            {
                mover = true;

                Invalidate();
            };

            MouseLeave += (s, e) =>
            {
                mover = false;

                Invalidate();
            };

            MouseDown += (s, e) =>
            {
                mdown = true;

                Invalidate();
            };
            MouseUp += (s, e) =>
            {
                mdown = false;

                Invalidate();
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Brush plusBrush;
            Brush backgroundBrush;
            var disposePlusBrush = false;
            var disposeBackgroundBrush = false;


            if (mdown)
            {
                backgroundBrush = App.ColorScheme.TabSelectedBG;

                plusBrush = new SolidBrush(App.ColorScheme.TabSelectedText);

                disposePlusBrush = true;
            }
            else if (mover)
            {
                backgroundBrush = App.ColorScheme.TabHoverBG;

                plusBrush = App.ColorScheme.TabSelectedBG;
            }
            else
            {
                backgroundBrush = new SolidBrush(App.ColorScheme.TabPanelBG);
                disposeBackgroundBrush = true;

                plusBrush = App.ColorScheme.TabSelectedBG;
            }

            e.Graphics.FillRectangle(backgroundBrush, ClientRectangle);

            if (Icon == MoreButtonIcon.Plus)
            {
                var a = Width / 8f;

                //e.Graphics.FillRectangle(plusBrush, a, 3.5f * a, 6 * a, a);
                //e.Graphics.FillRectangle(plusBrush, 3.5f * a, a, a, 6 * a);

                e.Graphics.FillRectangle(plusBrush, (Height / 12) * 2 + 1, (Height / 12) * 5 + 1, Width - ((Height / 12) * 5), (Height / 12) * 1);
                e.Graphics.FillRectangle(plusBrush, (Height / 12) * 5 + 1, (Height / 12) * 2 + 1, (Height / 12) * 1, Width - ((Height / 12) * 5));
            }
            else if (Icon == MoreButtonIcon.User)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                var a = Width / 8f;

                e.Graphics.SetClip(new RectangleF(a, a, a * 6, a * 6));

                e.Graphics.FillEllipse(plusBrush, a, Height / 2f, 6 * a, 6 * a);

                e.Graphics.FillEllipse(backgroundBrush, 2f * a, 1f * a, 4 * a, 4 * a);
                e.Graphics.FillEllipse(plusBrush, 2.5f * a, 1.5f * a, 3 * a, 3 * a);
            }
            else if (Icon == MoreButtonIcon.Settings)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                var a = Width / 8f;

                var r = new Rectangle((int)a, (int)a, (int)(6 * a), (int)(6 * a));

                for (var i = 0; i < 8; i++)
                {
                    e.Graphics.FillPie(plusBrush, r, i * (360 / 8f) - (360 / 32f), 360 / 16f);
                }

                e.Graphics.FillEllipse(plusBrush, 2 * a, 2 * a, 4 * a, 4 * a);
                e.Graphics.FillEllipse(backgroundBrush, 3 * a, 3 * a, 2 * a, 2 * a);
            }

            //using (var pen = new Pen(App.ColorScheme.TabSelectedBG))
            //{
            //    //e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            //}

            if (disposePlusBrush)
            {
                plusBrush.Dispose();
            }

            if (disposeBackgroundBrush)
            {
                backgroundBrush.Dispose();
            }
        }
    }

    public enum MoreButtonIcon
    {
        Plus,
        User,
        Settings
    }
}
