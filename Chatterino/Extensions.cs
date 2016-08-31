using Chatterino.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatterino
{
    public static class Extensions
    {
        public static void Invoke(this System.Windows.Forms.Control c, Action action)
        {
            if (c.InvokeRequired)
            {
                c.Invoke((System.Windows.Forms.MethodInvoker)delegate { action(); });
            }
            else
            {
                action();
            }
        }

        public static Color ToColor(this HSLColor hsl)
        {
            float r, _g, b;
            hsl.ToRGB(out r, out _g, out b);
            return Color.FromArgb((int)((r < 0 ? 0 : r) * 255), (int)((_g < 0 ? 0 : _g) * 255), (int)((b < 0 ? 0 : b) * 255));
        }

        public static Brush ToBrush(this HSLColor hsl)
        {
            return new SolidBrush(ToColor(hsl));
        }

        public static Pen ToPen(this HSLColor hsl)
        {
            return new Pen(ToColor(hsl));
        }
    }
}
