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
        public static event EventHandler FontsChanged;

        public static Font Small = new Font("Helvetica Neue", 6.5f);
        public static Font Medium = new Font("Helvetica Neue", 9.5f);
        public static Font MediumBold = new Font("Helvetica Neue", 9.5f);
        public static Font MediumItalic = new Font("Helvetica Neue", 9.5f, FontStyle.Italic);
        public static Font Large = new Font("Helvetica Neue", 11.5f);
        public static Font VeryLarge = new Font("Helvetica Neue", 13.5f);

        public static void SetFont(string family, float size)
        {
            Small = new Font(family, size * 0.7f);
            Medium = new Font(family, size);
            MediumBold = new Font(family, size);
            MediumItalic = new Font(Medium, FontStyle.Italic);
            Large = new Font(family, size * 1.3f);
            VeryLarge = new Font(family, size * 1.6f);

            FontsChanged?.Invoke(null, EventArgs.Empty);
        }

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
            else if (type == FontType.Large)
                return Large;
            else if (type == FontType.VeryLarge)
                return VeryLarge;

            throw new ArgumentException($"Font {type} doesn't exists.");
        }
    }
}
