using Chatterino.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xwt.Drawing;

namespace Chatterino.Desktop
{
    public static class ColorExtensions
    {
        public static Color ToColor(this HSLColor hsl)
        {
            return Color.FromHsl(hsl.Hue, hsl.Saturation, hsl.Luminosity);
        }

        public static HSLColor ToHSL(this Color c)
        {
            return new HSLColor((float)c.Hue, (float)c.Saturation, (float)c.Light);
        }
    }
}
