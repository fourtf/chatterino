using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Chatterino.Common
{
    public static class Cache
    {
        public static RoomIdCache RoomIdCache = new RoomIdCache();

        // IO
        public static void Load()
        {
            try
            {
                Directory.CreateDirectory(Path.Combine(Util.GetUserDataPath(), "Cache"));
            }
            catch (Exception) { }

            RoomIdCache.Load();
        }

        public static void Save()
        {
            RoomIdCache.Save();
        }
    }

    public abstract class BaseCache
    {
        private string _cachePath;

        protected BaseCache(string cachePath)
        {
            _cachePath = cachePath;
        }

        public abstract void LoadData(string inData);
        public abstract string SaveData();

        public void Load()
        {
            if (File.Exists(_cachePath))
            {
                try
                {
                    LoadData(File.ReadAllText(_cachePath));
                }
                catch (Exception exc)
                {
                    Console.WriteLine("error loading cache: " + exc.Message);
                }
            }
        }

        public void Save()
        {
            try
            {
                File.WriteAllText(_cachePath, SaveData());
            }
            catch (Exception exc)
            {
                Console.WriteLine("error saving cache: " + exc.Message);
            }
        }
    }

    public class RoomIdCache : BaseCache
    {
        private Dictionary<string, int> _data = new Dictionary<string, int>();
        private object _dataLock = new object();


        public RoomIdCache()
            : base(Path.Combine(Util.GetUserDataPath(), "Cache", "room_ids.json"))
        {

        }

        public int Get(string name)
        {
            lock (_dataLock)
            {
                int d;

                if (_data.TryGetValue(name, out d))
                {
                    return d;
                }

                return -1;
            }
        }

        public void Set(string name, int roomID)
        {
            lock (_dataLock)
            {
                _data[name] = roomID;
            }
        }

        public override void LoadData(string inData)
        {
            lock (_dataLock)
            {
                try
                {
                    _data = JsonConvert.DeserializeObject<Dictionary<string, int>>(inData);
                }
                catch (Exception exc)
                {
                    Console.WriteLine("error loading room id cache: " + exc.Message);

                    _data.Clear();
                }
            }
        }

        public override string SaveData()
        {
            lock (_dataLock)
            {
                return JsonConvert.SerializeObject(_data);
            }
        }
    }
}
