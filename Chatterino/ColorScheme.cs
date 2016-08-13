using Chatterino.Common;
using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Reflection;

namespace Chatterino
{
    public class ColorScheme
    {
        public bool IsLightTheme { get; private set; } = true;

        public Brush TooltipBackground { get; set; } = Brushes.Black;
        public Brush TooltipText { get; set; } = Brushes.White;

        public Brush ChatBackground { get; set; } = Brushes.White;
        public Brush ChatBackgroundHighlighted { get; set; } = Brushes.LightBlue;
        public Brush ChatInputOuter { get; set; } = Brushes.White;
        public Brush ChatInputInner { get; set; } = Brushes.White;
        public Pen ChatInputBorder { get; set; } = Pens.White;
        
        public Pen ChatBorder { get; set; } = Pens.LightGray;
        public Pen ChatBorderFocused { get; set; } = Pens.Black;
        public Color Text { get; set; } = Color.Black;
        public Brush TextCaret { get; set; } = Brushes.Black;
        public Color TextLink { get; set; } = Color.Blue;
        public Color TextFocused { get; set; } = Color.Red;
        public Brush Menu { get; set; } = Brushes.Black;

        public Brush ScrollbarBG { get; set; } = Brushes.White;
        public Brush ScrollbarThumb { get; set; } = Brushes.Gray;
        public Brush ScrollbarThumbSelected { get; set; } = Brushes.Gray;
        public Brush ScrollbarArrow { get; set; } = Brushes.Gray;

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
                                prop.SetValue(this, Color.FromArgb(-16777216 | Convert.ToInt32(value.Substring(1), 16)), null);
                            }
                        }
                        else if (prop.PropertyType == typeof(Brush))
                        {
                            // #FFFFFF
                            if (value.Length == 7 && value[0] == '#')
                            {
                                prop.SetValue(this, new SolidBrush(Color.FromArgb(-16777216 | Convert.ToInt32(value.Substring(1), 16))), null);
                            }
                        }
                        else if (prop.PropertyType == typeof(Pen))
                        {
                            // #FFFFFF
                            if (value.Length == 7 && value[0] == '#')
                            {
                                prop.SetValue(this, new Pen(new SolidBrush(Color.FromArgb(-16777216 | Convert.ToInt32(value.Substring(1), 16)))), null);
                            }
                        }
                    }
                }
                catch { }
            }

            IsLightTheme = new HSLColor((ChatBackground as SolidBrush)?.Color ?? Color.White).Luminosity > 120;
        }

        public void Save(string path)
        {
            IniSettings settings = new IniSettings();

            foreach (var prop in properties.Values)
            {
                if (prop.PropertyType == typeof(Color))
                {
                    settings.Set(prop.Name, "#" + (((Color)prop.GetValue(this, null)).ToArgb() & 0xFFFFFF).ToString("X"));
                }
                else if (prop.PropertyType == typeof(Brush))
                {
                    var value = prop.GetValue(this, null);
                    var solidBrush = value as SolidBrush;
                    if (solidBrush != null)
                    {
                        settings.Set(prop.Name, "#" + ((solidBrush.Color).ToArgb() & 0xFFFFFF).ToString("X"));
                    }
                }
                else if (prop.PropertyType == typeof(Pen))
                {
                    var value = prop.GetValue(this, null);
                    var solidBrush = (value as Pen)?.Brush as SolidBrush;
                    if (solidBrush != null)
                    {
                        settings.Set(prop.Name, "#" + ((solidBrush.Color).ToArgb() & 0xFFFFFF).ToString("X"));
                    }
                }
            }

            settings.Save(path);
        }
    }
}
