using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Chatterino.Common
{
    public static class Emotes
    {
        public static event EventHandler EmotesLoaded;

        public static ConcurrentDictionary<string, string> TwitchEmotes = new ConcurrentDictionary<string, string>();
        public static ConcurrentDictionary<string, TwitchEmote> BttvGlobalEmotes = new ConcurrentDictionary<string, TwitchEmote>();
        public static ConcurrentDictionary<string, TwitchEmote> FfzGlobalEmotes = new ConcurrentDictionary<string, TwitchEmote>();
        public static ConcurrentDictionary<string, TwitchEmote> BttvChannelEmotesCache = new ConcurrentDictionary<string, TwitchEmote>();
        public static ConcurrentDictionary<int, TwitchEmote> TwitchEmotesByIDCache = new ConcurrentDictionary<int, TwitchEmote>();
        public static ConcurrentDictionary<string, TwitchEmote> MiscEmotesByUrl = new ConcurrentDictionary<string, TwitchEmote>();

        public const string TwitchEmoteTemplate = "https://static-cdn.jtvnw.net/emoticons/v1/{id}/1.0";

        private const string twitchemotesGlobalCache = "./Cache/twitchemotes_global.json";
        private const string bttvEmotesGlobalCache = "./Cache/bttv_global.json";
        private const string ffzEmotesGlobalCache = "./Cache/ffz_global.json";

        public static void LoadGlobalEmotes()
        {
            // twitchemotes
            /*
            Task.Run(() =>
            {
                try
                {
                    Directory.CreateDirectory("./Cache");

                    System.Text.Json.JsonParser parser = new System.Text.Json.JsonParser();

                    // twitchemotes api global emotes
                    if (!File.Exists(twitchemotesGlobalCache) || DateTime.Now - new FileInfo(twitchemotesGlobalCache).LastWriteTime > TimeSpan.FromHours(24))
                    {
                        try
                        {
                            if (Util.IsLinux)
                            {
                                Util.LinuxDownloadFile("https://twitchemotes.com/api_cache/v2/global.json", twitchemotesGlobalCache);
                            }
                            else
                            {
                                using (var webClient = new WebClient())
                                using (var readStream = webClient.OpenRead("https://twitchemotes.com/api_cache/v2/global.json"))
                                using (var writeStream = File.OpenWrite(twitchemotesGlobalCache))
                                {
                                    readStream.CopyTo(writeStream);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            e.Message.Log("emotes");
                        }
                    }

                    using (var stream = File.OpenRead(twitchemotesGlobalCache))
                    {
                        dynamic json = parser.Parse(stream);
                        dynamic templates = json["template"];
                        string template112 = templates["large"];

                        foreach (KeyValuePair<string, object> e in json["emotes"])
                        {
                            string code = e.Key;

                            TwitchGlobalEmotes[code.ToUpper()] = code;
                        }
                    }
                    EmotesLoaded?.Invoke(null, EventArgs.Empty);
                }
                catch { }
            });
            */

            // bttv emotes
            Task.Run(() =>
            {
                try
                {
                    Directory.CreateDirectory("./Cache");
                    System.Text.Json.JsonParser parser = new System.Text.Json.JsonParser();

                    // better twitch tv emotes
                    if (!File.Exists(bttvEmotesGlobalCache) || DateTime.Now - new FileInfo(bttvEmotesGlobalCache).LastWriteTime > TimeSpan.FromHours(1))
                    {
                        try
                        {
                            if (Util.IsLinux)
                            {
                                Util.LinuxDownloadFile("https://api.betterttv.net/2/emotes", bttvEmotesGlobalCache);
                            }
                            else
                            {
                                using (var webClient = new WebClient())
                                using (var readStream = webClient.OpenRead("https://api.betterttv.net/2/emotes"))
                                using (var writeStream = File.OpenWrite(bttvEmotesGlobalCache))
                                {
                                    readStream.CopyTo(writeStream);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            e.Message.Log("emotes");
                        }
                    }

                    using (var stream = File.OpenRead(bttvEmotesGlobalCache))
                    {
                        dynamic json = parser.Parse(stream);
                        var template = "https:" + json["urlTemplate"]; //{{id}} {{image}}

                        foreach (dynamic e in json["emotes"])
                        {
                            string id = e["id"];
                            string code = e["code"];
                            string imageType = e["imageType"];
                            string url = template.Replace("{{id}}", id).Replace("{{image}}", "1x");

                            BttvGlobalEmotes[code] = new TwitchEmote { Name = code, Url = url, Tooltip = code + "\nBetterTTV Global Emote" };
                        }
                    }
                    EmotesLoaded?.Invoke(null, EventArgs.Empty);
                }
                catch (Exception exc)
                {
                    Console.WriteLine("error loading emotes: " + exc.Message);
                }
            });

            // ffz emotes
            Task.Run(() =>
            {
                try
                {
                    Directory.CreateDirectory("./Cache");
                    System.Text.Json.JsonParser parser = new System.Text.Json.JsonParser();

                    // better twitch tv emotes
                    if (!File.Exists(ffzEmotesGlobalCache) || DateTime.Now - new FileInfo(ffzEmotesGlobalCache).LastWriteTime > TimeSpan.FromHours(1))
                    {
                        try
                        {
                            if (Util.IsLinux)
                            {
                                Util.LinuxDownloadFile("https://api.frankerfacez.com/v1/set/global", ffzEmotesGlobalCache);
                            }
                            else
                            {
                                using (var webClient = new WebClient())
                                using (var readStream = webClient.OpenRead("https://api.frankerfacez.com/v1/set/global"))
                                using (var writeStream = File.OpenWrite(ffzEmotesGlobalCache))
                                {
                                    readStream.CopyTo(writeStream);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            e.Message.Log("emotes");
                        }
                    }

                    using (var stream = File.OpenRead(ffzEmotesGlobalCache))
                    {
                        dynamic json = parser.Parse(stream);

                        foreach (var set in json["sets"])
                        {
                            var val = set.Value;
                            foreach (var emote in val["emoticons"])
                            {
                                var name = emote["name"];
                                var urlX1 = "http:" + emote["urls"]["1"];

                                FfzGlobalEmotes[name] = new TwitchEmote { Name = name, Url = urlX1, Tooltip = name + "\nFrankerFaceZ Global Emote" };
                            }
                        }
                    }
                    EmotesLoaded?.Invoke(null, EventArgs.Empty);
                }
                catch (Exception exc)
                {
                    Console.WriteLine("error loading emotes: " + exc.Message);
                }
            });
        }

        internal static void TriggerEmotesLoaded()
        {
            EmotesLoaded?.Invoke(null, EventArgs.Empty);
        }
    }
}
