using Chatterino.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatterino
{
    public static class Fonts
    {
        public static Font Small = new Font("Helvetica Neue", 6.5f);
        public static Font Medium = new Font("Helvetica Neue", 9.5f);
        public static Font MediumBold = new Font("Helvetica Neue", 9.5f, FontStyle.Bold);
        public static Font MediumItalic = new Font("Helvetica Neue", 9.5f, FontStyle.Italic);

        public static Font GetFont(FontType type)
        {
            if (type == FontType.Medium)
                return Medium;
            else if (type == FontType.Small)
                return Small;
            else if (type == FontType.MediumBold)
                return MediumBold;
            else if (type == FontType.MediumItalic)
                return MediumItalic;

            throw new ArgumentException($"Font {type} doesn't exists.");
        }
    }
}
