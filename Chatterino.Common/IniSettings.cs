using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Map = System.Collections.Concurrent.ConcurrentDictionary<string, string>;

namespace Chatterino.Common
{
    public class IniSettings
    {
        public IniSettings()
        {

        }

        public const string DateTimeFormat = "yyyy-MM-dd HH:mm";

        Map map = new Map();

        string settingsPath = null;

        // IO
        public void Load(string path)
        {
            try
            {
                settingsPath = path;

                if (File.Exists(path))
                {
                    using (StreamReader reader = new StreamReader(path))
                    {
                        Load(reader);
                    }
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("Error while loading settings: \"" + exc.Message + "\"");
            }
        }

        public void Load(TextReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                int index;
                if ((index = line.IndexOf('=')) != -1)
                {
                    string key = line.Remove(index).Trim();
                    if (key != "")
                    {
                        map[key] = line.Substring(index + 1).Trim();
                    }
                }
            }
        }

        public void Save()
        {
            if (settingsPath == null)
                throw new InvalidOperationException("The settings were never loaded.");

            Save(settingsPath);
        }

        public void Save(string path)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    foreach (var k in map)
                    {
                        writer.Write(k.Key);
                        writer.Write('=');
                        writer.WriteLine(k.Value);
                    }
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("Error while loading settings: \"" + exc.Message + "\"");
            }
        }

        // Try get
        public bool TryGetString(string key, out string value) => map.TryGetValue(key, out value);

        public bool TryGetBool(string key, out bool value)
        {
            string v;
            if (map.TryGetValue(key, out v))
            {
                return value = (v.ToUpper() == "TRUE");
            }
            value = false;
            return false;
        }

        public bool TryGetInt(string key, out int value)
        {
            string v;
            if (map.TryGetValue(key, out v))
            {
                return int.TryParse(v, out value);
            }
            value = 0;
            return false;
        }

        public bool TryGetInt(string key, out double value)
        {
            string v;
            if (map.TryGetValue(key, out v))
            {
                return double.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
            }
            value = 0;
            return false;
        }

        public bool TryGetLong(string key, out long value)
        {
            string v;
            if (map.TryGetValue(key, out v))
            {
                return long.TryParse(v, out value);
            }
            value = 0;
            return false;
        }

        public bool TryGetULong(string key, out ulong value)
        {
            string v;
            if (map.TryGetValue(key, out v))
            {
                return ulong.TryParse(v, out value);
            }
            value = 0;
            return false;
        }

        public bool TryGetDateTime(string key, out DateTime value)
        {
            string v;
            if (map.TryGetValue(key, out v))
            {
                return DateTime.TryParseExact(v, DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out value);
            }
            value = DateTime.MinValue;
            return false;
        }

        public bool TryGetStrings(string key, out string[] values)
        {
            string v;
            if (map.TryGetValue(key, out v))
            {
                values = v.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                return true;
            }
            values = new string[0];
            return false;
        }

        // Get or default
        public string GetString(string key, string defaultValue)
        {
            string value;

            return (map.TryGetValue(key, out value) ? value : map[key] = defaultValue);
        }

        public bool GetBool(string key, bool defaultValue)
        {
            string value;

            return (map.TryGetValue(key, out value) ? (value.ToUpper() == "TRUE") : defaultValue);
        }

        public long GetLong(string key, long defaultValue)
        {
            long value;

            return (long.TryParse(GetString(key, defaultValue.ToString()), out value) ? value : defaultValue);
        }

        public ulong GetULong(string key, ulong defaultValue)
        {
            ulong value;

            return (ulong.TryParse(GetString(key, defaultValue.ToString()), out value) ? value : defaultValue);
        }

        public int GetInt(string key, int defaultValue)
        {
            int value;

            return (int.TryParse(GetString(key, defaultValue.ToString()), out value) ? value : defaultValue);
        }

        public double GetDouble(string key, double defaultValue)
        {
            double value;

            return (double.TryParse(GetString(key, defaultValue.ToString()), NumberStyles.Float, CultureInfo.InvariantCulture, out value) ? value : defaultValue);
        }

        public DateTime GetTime(string key, DateTime defaultValue)
        {
            DateTime value;

            return (DateTime.TryParseExact(GetString(key, defaultValue.ToString(DateTimeFormat)), DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out value) ? value : defaultValue);
        }

        // Set values
        public void Set(string key, string value) => map[key] = value;
        public void Set(string key, bool value) => map[key] = value.ToString();
        public void Set(string key, int value) => map[key] = value.ToString();
        public void Set(string key, double value) => map[key] = value.ToString();
        public void Set(string key, long value) => map[key] = value.ToString();
        public void Set(string key, ulong value) => map[key] = value.ToString();
        public void Set(string key, DateTime value) => map[key] = value.ToString(DateTimeFormat);
        public void Set(string key, IEnumerable<string> values) => map[key] = string.Join(",", values);
    }
}
