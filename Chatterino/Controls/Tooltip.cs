using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chatterino.Controls
{
    public class ToolTip : Form
    {
        private string tooltip = null;

        public string TooltipText
        {
            get { return tooltip; }
            set
            {
                tooltip = value;

                if (tooltip != null)
                {
                    var size = CreateGraphics().MeasureString(tooltip, Font, 1000, format);
                    Size = new Size(Padding.Left + (int)size.Width + Padding.Right,
                        Padding.Top + (int)size.Height + Padding.Bottom);
                }
            }
        }

        public ToolTip()
        {
            FormBorderStyle = FormBorderStyle.None;
            Opacity = 0.8;

            Padding = new Padding(8, 4, 8, 4);
            ShowInTaskbar = false;

            StartPosition = FormStartPosition.Manual;

            //SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            //Win32.EnableWindowBlur(Handle);
        }

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        private const int WS_EX_TOPMOST = 0x00000008;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;
                createParams.ExStyle |= WS_EX_TOPMOST;
                return createParams;
            }
        }

        protected override void OnFontChanged(EventArgs e)
        {
            TooltipText = TooltipText;

            base.OnFontChanged(e);
        }

        static StringFormat format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        //Color bgcolor = Color.FromArgb(127,64,64,64);

        //protected override void OnPaintBackground(PaintEventArgs e)
        //{
        //    e.Graphics.Clear(bgcolor);

        //    //base.OnPaintBackground(e);
        //}

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.FillRectangle(App.ColorScheme.TooltipBackground, e.ClipRectangle);

            if (tooltip != null)
            {
                e.Graphics.DrawString(tooltip, Font, App.ColorScheme.TooltipText, ClientRectangle, format);
            }
        }
    }
}
