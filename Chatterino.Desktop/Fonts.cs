using Chatterino.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xwt.Drawing;

namespace Chatterino.Desktop
{
    public static class Fonts
    {
        public static Font TabControlTitle = Font.SystemFont;

        public static event EventHandler FontChanged;

        public static void SetFont(string family, float baseSize)
        {
            FontChanged?.Invoke(null, EventArgs.Empty);
        }

        static bool Initialized = false;

        public static Font Small, Medium, MediumBold, MediumItalic, Large, VeryLarge;

        public static Font GetFont(FontType type)
        {
            if (!Initialized)
            {
                string family = AppSettings.FontFamily;
                float size = (float)(AppSettings.FontBaseSize * Xwt.Desktop.Screens.FirstOrDefault().ScaleFactor);

                try
                {
                    Medium = Font.FromName("Arial").WithSize(size);
                }
                catch
                {
                    family = "Arial";
                    Medium = Font.SystemSansSerifFont.WithSize(size);
                }

                MediumBold = Medium.WithWeight(FontWeight.Bold);
                MediumItalic = Medium.WithStyle(FontStyle.Italic);
                Small = Medium.WithSize(size * 0.7);
                Large = Medium.WithSize(size * 1.3);
                VeryLarge = Medium.WithSize(size * 1.6);
            }

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

            return Medium;
        }

    }
}
