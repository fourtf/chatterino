using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Chatterino.Common
{
    public static class Cache
    {
        public static RoomIDCache roomIDCache = new RoomIDCache();

        // IO
        public static void Load()
        {
            try
            {
                Directory.CreateDirectory(Path.Combine(Util.GetUserDataPath(), "Cache"));
            }
            catch (Exception) { }

            roomIDCache.Load();
        }

        public static void Save()
        {
            roomIDCache.Save();
        }
    }

    public abstract class BaseCache
    {
        private string cachePath;
        protected dynamic data;

        public BaseCache(string _cachePath)
        {
            cachePath = _cachePath;
        }

        public abstract void InitData();
        public abstract void LoadData(string inData);

        public void Load()
        {
            if (File.Exists(cachePath))
            {
                try
                {
                    LoadData(File.ReadAllText(cachePath));
                }
                catch (Exception exc)
                {
                    Console.WriteLine("error loading cache: " + exc.Message);
                }
            }
            else
            {
                InitData();
            }
        }

        public void Save()
        {
            try
            {
                using (var stream = File.OpenWrite(cachePath))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        serializer.Serialize(writer, data);
                    }
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("error saving cache: " + exc.Message);
            }
        }
    }

    public class RoomIDCache : BaseCache
    {
        public RoomIDCache()
            : base(Path.Combine(Util.GetUserDataPath(), "Cache", "room_ids.json"))
        {

        }

        public int Get(string name)
        {
            try
            {
                return data[name];
            }
            catch (KeyNotFoundException)
            {
                // do nothing
                return -1;
            }
            catch (Exception exc)
            {
                Console.WriteLine("error getting room id cache: " + exc.Message);
            }

            return -1;
        }

        public void Set(string name, int roomID)
        {
            // XXX(pajlada): We really shouldn't need to do a try-catch here
            data[name] = roomID;
        }

        public override void InitData()
        {
            data = new Dictionary<string, int>();
        }

        public override void LoadData(string inData)
        {
            try
            {
                data = JsonConvert.DeserializeObject<Dictionary<string, int>>(inData);
            }
            catch (Exception exc)
            {
                Console.WriteLine("error loading room id cache: " + exc.Message);
            }
        }
    }
}
