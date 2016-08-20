using Chatterino.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xwt.Drawing;

namespace Chatterino.Desktop
{
    public class ColorScheme
    {
        public bool IsLightTheme { get; private set; } = true;

        // Tabs
        public Color TabBG { get; set; } = rgb(0xFFFFFF);
        public Color TabHoverBG { get; set; } = rgb(0xCCCCCC);
        public Color TabSelectedBG { get; set; } = rgb(0x8E24AA);

        public Color TabText { get; set; } = Colors.Black;
        public Color TabHoverText { get; set; } = Colors.Black;
        public Color TabSelectedText { get; set; } = Colors.White;

        // Tooltip
        public Color TooltipBackground { get; set; } = Colors.Black;
        public Color TooltipText { get; set; } = Colors.White;

        // Chat
        public Color ChatBackground { get; set; } = Colors.White;
        public Color ChatBackgroundHighlighted { get; set; } = Colors.LightBlue;
        public Color ChatInputOuter { get; set; } = Colors.White;
        public Color ChatInputInner { get; set; } = Colors.White;
        public Color ChatInputBorder { get; set; } = Colors.White;
        public Color ChatBorder { get; set; } = Colors.LightGray;
        public Color ChatBorderFocused { get; set; } = Colors.Black;
        public Color Text { get; set; } = Colors.Black;
        public Color TextCaret { get; set; } = Colors.Black;
        public Color TextLink { get; set; } = Colors.Blue;
        public Color TextFocused { get; set; } = Colors.Red;
        public Color Menu { get; set; } = Colors.Black;

        public Color ScrollbarBG { get; set; } = Colors.White;
        public Color ScrollbarThumb { get; set; } = Colors.Gray;
        public Color ScrollbarThumbSelected { get; set; } = Colors.Gray;
        public Color ScrollbarArrow { get; set; } = Colors.Gray;

        static ConcurrentDictionary<string, PropertyInfo> properties = new ConcurrentDictionary<string, PropertyInfo>();

        static ColorScheme()
        {
            Type T = typeof(ColorScheme);

            foreach (var property in T.GetProperties())
            {
                if (property.CanRead && property.CanWrite)
                    properties[property.Name] = property;
            }
        }

        // IO
        public void Load(string path)
        {
            IniSettings settings = new IniSettings();
            settings.Load(path);

            foreach (var prop in properties.Values)
            {
                try
                {
                    string value;
                    if (settings.TryGetString(prop.Name, out value))
                    {
                        if (prop.PropertyType == typeof(Color))
                        {
                            // #FFFFFF
                            if (value.Length == 7 && value[0] == '#')
                            {
                                prop.SetValue(this, Color.FromBytes(Convert.ToByte(value.Substring(1, 2)), Convert.ToByte(value.Substring(3, 2)), Convert.ToByte(value.Substring(5, 2), 16)), null);
                            }
                        }
                    }
                }
                catch { }
            }

            IsLightTheme = ChatBackground.ToHSL().Luminosity > 0.5f;
        }

        //public void Save(string path)
        //{
        //    IniSettings settings = new IniSettings();

        //    foreach (var prop in properties.Values)
        //    {
        //        if (prop.PropertyType == typeof(Color))
        //        {
        //            settings.Set(prop.Name, "#" + (((Color)prop.GetValue(this, null)).ToArgb() & 0xFFFFFF).ToString("X"));
        //        }
        //        else if (prop.PropertyType == typeof(Brush))
        //        {
        //            var value = prop.GetValue(this, null);
        //            var solidBrush = value as SolidBrush;
        //            if (solidBrush != null)
        //            {
        //                settings.Set(prop.Name, "#" + ((solidBrush.Color).ToArgb() & 0xFFFFFF).ToString("X"));
        //            }
        //        }
        //        else if (prop.PropertyType == typeof(Pen))
        //        {
        //            var value = prop.GetValue(this, null);
        //            var solidBrush = (value as Pen)?.Brush as SolidBrush;
        //            if (solidBrush != null)
        //            {
        //                settings.Set(prop.Name, "#" + ((solidBrush.Color).ToArgb() & 0xFFFFFF).ToString("X"));
        //            }
        //        }
        //    }

        //    settings.Save(path);
        //}

        // Util
        private static Color rgb(int color)
        {
            return unchecked(Color.FromBytes((byte)(color >> 16), (byte)((color >> 8) & 255), (byte)(color & 255)));
        }
    }
}
