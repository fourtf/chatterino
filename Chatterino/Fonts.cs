using Chatterino.Common;
using SharpDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chatterino
{
    public static class Fonts
    {
        public static event EventHandler FontsChanged;

        public static string FontFamily { get; private set; } = null;
        public static float FontBaseSize { get; private set; } = 13.333f;

        public static void SetFont(string family, float baseSize)
        {
            FontFamily = family;
            FontBaseSize = baseSize;

            gdiInitialized = false;
            dwInitialized = false;

            FontsChanged?.Invoke(null, EventArgs.Empty);
        }

        #region GDI
        static bool gdiInitialized = false;

        public static System.Drawing.Font GdiSmall, GdiMedium, GdiMediumBold, GdiMediumItalic, GdiLarge, GdiVeryLarge;

        public static System.Drawing.Font GetFont(FontType type)
        {
            if (!gdiInitialized)
            {
                GdiMedium = new System.Drawing.Font(FontFamily, FontBaseSize, System.Drawing.FontStyle.Regular);
                GdiMediumBold = new System.Drawing.Font(FontFamily, FontBaseSize, System.Drawing.FontStyle.Bold);
                GdiMediumItalic = new System.Drawing.Font(FontFamily, FontBaseSize, System.Drawing.FontStyle.Italic);
                GdiSmall = new System.Drawing.Font(FontFamily, FontBaseSize * 0.7f, System.Drawing.FontStyle.Regular);
                GdiLarge = new System.Drawing.Font(FontFamily, FontBaseSize * 1.3f, System.Drawing.FontStyle.Regular);
                GdiVeryLarge = new System.Drawing.Font(FontFamily, FontBaseSize * 1.6f, System.Drawing.FontStyle.Regular);
            }

            if (type == FontType.Medium)
                return GdiMedium;
            else if (type == FontType.Small)
                return GdiSmall;
            else if (type == FontType.MediumBold)
                return GdiMediumBold;
            else if (type == FontType.MediumItalic)
                return GdiMediumItalic;
            else if (type == FontType.Large)
                return GdiLarge;
            else if (type == FontType.VeryLarge)
                return GdiVeryLarge;

            return GdiMedium;
        }
        #endregion GDI

        #region DirectWrite
        static bool dwInitialized = false;

        static Factory fontFactory = null;

        public static Factory Factory
        {
            get
            {
                if (fontFactory == null)
                {
                    fontFactory = new Factory();
                }

                return fontFactory;
            }
        }

        public static TextFormat DwSmall, DwMedium, DwMediumBold, DwMediumItalic, DwLarge, DwVeryLarge;

        public static TextFormat GetTextFormat(FontType type)
        {
            if (!dwInitialized)
            {
                var size = FontBaseSize * MessageRenderer.D2D1Factory.DesktopDpi.Height / 72f;

                DwMedium = new TextFormat(Factory, FontFamily, size);
                DwMediumBold = new TextFormat(Factory, FontFamily, FontWeight.SemiBold, SharpDX.DirectWrite.FontStyle.Normal, size);
                DwMediumItalic = new TextFormat(Factory, FontFamily, FontWeight.SemiBold, SharpDX.DirectWrite.FontStyle.Italic, size);
                DwSmall = new TextFormat(Factory, FontFamily, size * 0.7f);
                DwLarge = new TextFormat(Factory, FontFamily, size * 1.3f);
                DwVeryLarge = new TextFormat(Factory, FontFamily, size * 1.6f);

                dwInitialized = true;
            }

            if (type == FontType.Medium)
                return DwMedium;
            else if (type == FontType.MediumBold)
                return DwMediumBold;
            else if (type == FontType.MediumItalic)
                return DwMediumItalic;
            else if (type == FontType.Small)
                return DwSmall;
            else if (type == FontType.Large)
                return DwLarge;
            else if (type == FontType.VeryLarge)
                return DwVeryLarge;

            return DwMedium;
        }
        #endregion DirectWrite
    }
}
