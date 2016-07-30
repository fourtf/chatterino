using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Chatterino.Common;
using Android.Graphics;

namespace Chatterino.Mobile.Droid
{
    public static class Fonts
    {
        public static Paint Small;
        public static Paint Medium;
        public static Paint MediumBold;

        static Fonts()
        {
            Small = new Paint { TextSize = 32 };
            Medium = new Paint { TextSize = 48 };
            MediumBold = new Paint { TextSize = 48, FakeBoldText = true };
        }

        public static Paint GetFont(FontType type)
        {
            if (type == FontType.Medium)
                return Medium;
            else if (type == FontType.Small)
                return Small;
            else if (type == FontType.MediumBold)
                return MediumBold;

            throw new ArgumentException($"Font {type} doesn't exists.");
        }
    }
}