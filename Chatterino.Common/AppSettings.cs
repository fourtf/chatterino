using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chatterino.Common
{
    public static class AppSettings
    {
        public static bool ChatShowTimestamps { get; set; } = true;
        public static bool ChatShowTimestampSeconds { get; set; } = false;
        public static bool ChatAllowSameMessage { get; set; } = false;

        public static bool ChatEnableHighlight { get; set; } = true;
        public static bool ChatEnableHighlightSound { get; set; } = true;
        public static bool ChatEnableHighlightTaskbar { get; set; } = true;
        //public static bool PingIgnoreBots { get; set; } = false;

        public static bool ChatEnableBttvEmotes { get; set; } = true;
        public static bool ChatEnableFfzEmotes { get; set; } = true;
        public static bool ChatEnableEmojis { get; set; } = true;
        public static bool ChatEnableGifEmotes { get; set; } = true;

        public static bool ProxyEnable { get; set; } = false;
        public static string ProxyType { get; set; } = "http";
        public static string ProxyHost { get; set; } = "";
        public static string ProxyUsername { get; set; } = "";
        public static string ProxyPassword { get; set; } = "";
        public static int ProxyPort { get; set; } = 80;

        public static int WindowX { get; set; } = 200;
        public static int WindowY { get; set; } = 200;
        public static int WindowWidth { get; set; } = 600;
        public static int WindowHeight { get; set; } = 400;


        // static stuff
        public static ConcurrentDictionary<string, PropertyInfo> Properties = new ConcurrentDictionary<string, PropertyInfo>();

        static AppSettings()
        {
            Type T = typeof(AppSettings);

            foreach (var property in T.GetProperties())
            {
                if (property.CanRead && property.CanWrite)
                    Properties[property.Name] = property;
            }
        }


        // IO
        public static void Load(string path)
        {
            IniSettings settings = new IniSettings();
            settings.Load(path);

            foreach (var prop in Properties.Values)
            {
                if (prop.PropertyType == typeof(string))
                    prop.SetValue(null, settings.GetString(prop.Name, (string)prop.GetValue(null)));
                else if (prop.PropertyType == typeof(int))
                    prop.SetValue(null, settings.GetInt(prop.Name, (int)prop.GetValue(null)));
                else if (prop.PropertyType == typeof(double))
                    prop.SetValue(null, settings.GetDouble(prop.Name, (double)prop.GetValue(null)));
                else if (prop.PropertyType == typeof(bool))
                    prop.SetValue(null, settings.GetBool(prop.Name, (bool)prop.GetValue(null)));
            }
        }

        public static void Save(string path)
        {
            IniSettings settings = new IniSettings();

            foreach (var prop in Properties.Values)
            {
                if (prop.PropertyType == typeof(string))
                    settings.Set(prop.Name, (string)prop.GetValue(null));
                else if (prop.PropertyType == typeof(int))
                    settings.Set(prop.Name, (int)prop.GetValue(null));
                else if (prop.PropertyType == typeof(double))
                    settings.Set(prop.Name, (double)prop.GetValue(null));
                else if (prop.PropertyType == typeof(bool))
                    settings.Set(prop.Name, (bool)prop.GetValue(null));
            }

            settings.Save(path);
        }
    }
}
