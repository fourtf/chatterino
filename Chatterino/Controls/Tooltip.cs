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
            Opacity = 0.9;

            Padding = new Padding(8, 4, 8, 4);
            ShowInTaskbar = false;

            StartPosition = FormStartPosition.Manual;
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

        public enum GWL
        {
            ExStyle = -20
        }

        public enum WS_EX
        {
            Transparent = 0x20,
            Layered = 0x80000
        }

        public enum LWA
        {
            ColorKey = 0x1,
            Alpha = 0x2
        }

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        public static extern int GetWindowLong(IntPtr hWnd, GWL nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        public static extern int SetWindowLong(IntPtr hWnd, GWL nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetLayeredWindowAttributes")]
        public static extern bool SetLayeredWindowAttributes(IntPtr hWnd, int crKey, byte alpha, LWA dwFlags);

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (!Common.Util.IsLinux)
            {
                int wl = GetWindowLong(Handle, GWL.ExStyle);
                wl = wl | 0x80000 | 0x20;
                SetWindowLong(Handle, GWL.ExStyle, wl);
                SetLayeredWindowAttributes(Handle, 0, 128, LWA.Alpha);
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
