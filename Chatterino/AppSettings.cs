using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chatterino
{
    public class AppSettings
    {
        public bool ChatShowTimestamp { get; set; } = true;
        public bool ChatShowSeconds { get; set; } = false;

        public bool ChatEnableBttvEmotes { get; set; } = true;
        public bool ChatEnableBttvGifEmotes { get; set; } = true;
        public bool ChatEnableFfzEmotes { get; set; } = true;

        public bool ProxyEnable { get; set; } = false;
        public string ProxyHost { get; set; } = "";
        public string ProxyUsername { get; set; } = "";
        public string ProxyPassword { get; set; } = "";
        public string ProxyType { get; set; } = "http";
        public int ProxyPort { get; set; } = 6667;


        // static stuff
        static ConcurrentDictionary<string, PropertyInfo> properties = new ConcurrentDictionary<string, PropertyInfo>();

        static AppSettings()
        {
            Type T = typeof(AppSettings);

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
                if (prop.PropertyType == typeof(string))
                    prop.SetValue(this, settings.GetString(prop.Name, (string)prop.GetValue(this)));
                else if (prop.PropertyType == typeof(int))
                    prop.SetValue(this, settings.GetInt(prop.Name, (int)prop.GetValue(this)));
                else if (prop.PropertyType == typeof(double))
                    prop.SetValue(this, settings.GetDouble(prop.Name, (double)prop.GetValue(this)));
                else if (prop.PropertyType == typeof(bool))
                    prop.SetValue(this, settings.GetBool(prop.Name, (bool)prop.GetValue(this)));
            }
        }

        public void Save(string path)
        {
            IniSettings settings = new IniSettings();

            foreach (var prop in properties.Values)
            {
                if (prop.PropertyType == typeof(string))
                    settings.Set(prop.Name, (string)prop.GetValue(this));
                else if (prop.PropertyType == typeof(int))
                    settings.Set(prop.Name, (int)prop.GetValue(this));
                else if (prop.PropertyType == typeof(double))
                    settings.Set(prop.Name, (double)prop.GetValue(this));
                else if (prop.PropertyType == typeof(bool))
                    settings.Set(prop.Name, (bool)prop.GetValue(this));
            }

            settings.Save(path);
        }
    }
}
