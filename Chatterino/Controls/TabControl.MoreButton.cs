using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chatterino.Controls
{
    public partial class TabControl
    {
        private class MoreButton : Control
        {
            bool mover = false, mdown = false;

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
                bool disposePlusBrush = false;

                if (mdown)
                {
                    e.Graphics.FillRectangle(App.ColorScheme.TabSelectedBG, ClientRectangle);

                    plusBrush = new SolidBrush(App.ColorScheme.TabSelectedText);

                    disposePlusBrush = true;
                }
                else if (mover)
                {
                    e.Graphics.FillRectangle(App.ColorScheme.TabHoverBG, ClientRectangle);

                    plusBrush = App.ColorScheme.TabSelectedBG;

                    e.Graphics.FillRectangle(App.ColorScheme.TabHoverBG, ClientRectangle);
                }
                else
                {
                    e.Graphics.Clear(App.ColorScheme.TabPanelBG);

                    plusBrush = App.ColorScheme.TabSelectedBG;
                }

                e.Graphics.FillRectangle(plusBrush, (Height / 12) * 2 + 1, (Height / 12) * 5 + 1, Width - ((Height / 12) * 5), (Height / 12) * 1);
                e.Graphics.FillRectangle(plusBrush, (Height / 12) * 5 + 1, (Height / 12) * 2 + 1, (Height / 12) * 1, Width - ((Height / 12) * 5));

                //using (var pen = new Pen(App.ColorScheme.TabSelectedBG))
                //{
                //    //e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
                //}

                if (disposePlusBrush)
                {
                    plusBrush.Dispose();
                }
            }
        }
    }
}
