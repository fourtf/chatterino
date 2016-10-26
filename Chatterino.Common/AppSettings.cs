using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Chatterino.Common
{
    public static class AppSettings
    {
        public static string CurrentVersion { get; set; } = "0.0";

        // Theme
        public static event EventHandler ThemeChanged;

        private static string theme = "Dark";

        public static string Theme
        {
            get { return theme; }
            set
            {
                if (theme != value)
                {
                    theme = value;
                    ThemeChanged?.Invoke(null, EventArgs.Empty);
                }
            }
        }

        private static double themeHue = 0.6;

        public static double ThemeHue
        {
            get { return themeHue; }
            set { themeHue = value; ThemeChanged?.Invoke(null, null); }
        }

        // Chat
        public static double ScrollMultiplyer { get; set; } = 1;

        public static bool ChatShowTimestamps { get; set; } = true;
        public static bool ChatShowTimestampSeconds { get; set; } = false;
        public static bool ChatAllowSameMessage { get; set; } = true;
        public static bool ChatLinksDoubleClickOnly { get; set; } = false;
        public static bool ChatHideInputIfEmpty { get; set; } = false;
        public static bool ChatInputShowMessageLength { get; set; } = false;
        public static bool ChatSeperateMessages { get; set; } = false;
        public static bool ChatTabLocalizedNames { get; set; } = true;

        public static bool ChatAllowCommandsAtEnd { get; set; } = false;

        public static bool ChatMentionUsersWithAt { get; set; } = true;

        public static event EventHandler MessageLimitChanged;

        private static int chatMessageLimit = 1000;
        public static int ChatMessageLimit
        {
            get { return chatMessageLimit; }
            set
            {
                if (chatMessageLimit != value)
                {
                    chatMessageLimit = value;
                    MessageLimitChanged?.Invoke(null, EventArgs.Empty);
                }
            }
        }

        public static bool ChatEnableHighlight { get; set; } = true;
        public static bool ChatEnableHighlightSound { get; set; } = false;
        public static bool ChatEnableHighlightTaskbar { get; set; } = true;
        public static bool ChatCustomHighlightSound { get; set; } = false;

        public static ConcurrentDictionary<string, object> HighlightIgnoredUsers { get; private set; } = new ConcurrentDictionary<string, object>();
        public static ConcurrentDictionary<string, object> ChatIgnoredEmotes { get; private set; } = new ConcurrentDictionary<string, object>();

        // Custom Highlights
        private static string[] chatCustomHighlights = new string[0];
        public static string[] ChatCustomHighlights
        {
            get
            {
                return chatCustomHighlights;
            }
            set
            {
                chatCustomHighlights = value;
                UpdateCustomHighlightRegex();
            }
        }

        public static void UpdateCustomHighlightRegex()
        {
            CustomHighlightRegex = new Regex($@"\b({(IrcManager.Username)}{(IrcManager.Username == null || chatCustomHighlights.Length == 0 ? "" : "|")}{string.Join("|", chatCustomHighlights.Select(x => Regex.Escape(x)))})\b".Log(), RegexOptions.IgnoreCase);
        }
        public static Regex CustomHighlightRegex { get; private set; } = null;

        // Ignored words
        private static string[] chatIgnoredKeywords = new string[0];
        public static string[] ChatIgnoredKeywords
        {
            get
            {
                return chatIgnoredKeywords;
            }
            set
            {
                chatIgnoredKeywords = value;
                UpdateIgnoredKeywordsRegex();
            }
        }

        public static void UpdateIgnoredKeywordsRegex()
        {
            if (chatIgnoredKeywords.Length == 0)
            {
                IgnoredKeywordsRegex = null;
            }
            else
            {
                IgnoredKeywordsRegex = new Regex($@"\b({string.Join("|", chatIgnoredKeywords.Select(x => Regex.Escape(x)))})\b".Log(), RegexOptions.IgnoreCase);
            }
        }
        public static Regex IgnoredKeywordsRegex { get; private set; } = null;

        public static bool EnableTwitchUserIgnores { get; set; } = true;

        public static bool ChatEnableTwitchEmotes { get; set; } = true;
        public static bool ChatEnableBttvEmotes { get; set; } = true;
        public static bool ChatEnableFfzEmotes { get; set; } = true;
        public static bool ChatEnableEmojis { get; set; } = true;
        public static bool ChatEnableGifAnimations { get; set; } = true;

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

        public static event EventHandler<ValueEventArgs<bool>> WindowTopMostChanged;

        private static bool windowTopMost;

        public static bool WindowTopMost
        {
            get { return windowTopMost; }
            set
            {
                if (windowTopMost != value)
                {
                    windowTopMost = value;

                    WindowTopMostChanged?.Invoke(null, new ValueEventArgs<bool>(value));
                }
            }
        }


        public static event EventHandler FontChanged;

        public static string FontFamily { get; private set; } = "Helvetica Neue";
        public static double FontBaseSize { get; private set; } = 10;

        public static void SetFont(string family, float size)
        {
            FontFamily = family;
            FontBaseSize = size;

            FontChanged?.Invoke(null, EventArgs.Empty);
        }

        public static string BrowserExtensionKey { get; set; } = "pass123";


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
                else if (prop.PropertyType == typeof(string[]))
                {
                    string[] vals;
                    if (settings.TryGetStrings(prop.Name, out vals))
                        prop.SetValue(null, vals);
                }
                else if (prop.PropertyType == typeof(ConcurrentDictionary<string, object>))
                {
                    var dict = (ConcurrentDictionary<string, object>)prop.GetValue(null);

                    dict.Clear();

                    string[] vals;
                    if (settings.TryGetStrings(prop.Name, out vals))
                    {
                        foreach (string s in vals)
                            dict[s] = null;
                    }
                }
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
                else if (prop.PropertyType == typeof(string[]))
                    settings.Set(prop.Name, (string[])prop.GetValue(null));
                else if (prop.PropertyType == typeof(ConcurrentDictionary<string, object>))
                    settings.Set(prop.Name, ((ConcurrentDictionary<string, object>)prop.GetValue(null)).Keys);
            }

            settings.Save(path);
        }
    }
}
