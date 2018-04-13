using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Chatterino.Common
{
    public static class Emotes
    {
        public static event EventHandler EmotesLoaded;

        public static ConcurrentDictionary<string, IrcManager.TwitchEmoteValue> TwitchEmotes =
            new ConcurrentDictionary<string, IrcManager.TwitchEmoteValue>();

        public static ConcurrentDictionary<string, LazyLoadedImage> BttvGlobalEmotes =
            new ConcurrentDictionary<string, LazyLoadedImage>();

        public static ConcurrentDictionary<string, LazyLoadedImage> FfzGlobalEmotes =
            new ConcurrentDictionary<string, LazyLoadedImage>();

        public static ConcurrentDictionary<string, LazyLoadedImage> ChatterinoEmotes =
            new ConcurrentDictionary<string, LazyLoadedImage>();

        public static ConcurrentDictionary<string, LazyLoadedImage> BttvChannelEmotesCache =
            new ConcurrentDictionary<string, LazyLoadedImage>();

        public static ConcurrentDictionary<string, LazyLoadedImage> FfzChannelEmotesCache =
            new ConcurrentDictionary<string, LazyLoadedImage>();

        public static ConcurrentDictionary<int, LazyLoadedImage> TwitchEmotesByIDCache =
            new ConcurrentDictionary<int, LazyLoadedImage>();

        public static ConcurrentDictionary<string, LazyLoadedImage> MiscEmotesByUrl =
            new ConcurrentDictionary<string, LazyLoadedImage>();

        private static ConcurrentDictionary<string, string> twitchEmotesCodeReplacements =
            new ConcurrentDictionary<string, string>();

        public const string TwitchEmoteTemplate = "https://static-cdn.jtvnw.net/emoticons/v1/{id}/{scale}.0";

        private static string twitchemotesGlobalCache = Path.Combine(Util.GetUserDataPath(), "Cache",
            "twitchemotes_global.json");

        private static string bttvEmotesGlobalCache = Path.Combine(Util.GetUserDataPath(), "Cache", "bttv_global.json");
        private static string ffzEmotesGlobalCache = Path.Combine(Util.GetUserDataPath(), "Cache", "ffz_global.json");

        static Emotes()
        {
            Func<int, string, string, LazyLoadedImage> getEmoteReplacement = (id, name, url) =>
            {
                var emote = new LazyLoadedImage()
                {
                    Url = url,
                    Name = name,
                    Tooltip = $"{name}\nTwitch Global Emote\n(chatterino dankmode friendly version)",
                    IsEmote = true
                };
                emote.LoadAction = () =>
                {
                    object img;

                    try
                    {
                        var request = WebRequest.Create(url);
                        if (AppSettings.IgnoreSystemProxy)
                        {
                            request.Proxy = null;
                        }
                        using (var response = request.GetResponse())
                        using (var stream = response.GetResponseStream())
                        {
                            img = GuiEngine.Current.ReadImageFromStream(stream);
                        }

                        GuiEngine.Current.FreezeImage(img);
                    }
                    catch
                    {
                        img = null;
                    }

                    if (img == null)
                    {
                        double scale;
                        url = GetTwitchEmoteLink(id, out scale);
                        emote.Url = url;

                        try
                        {
                            var request = WebRequest.Create(url);
                            if (AppSettings.IgnoreSystemProxy)
                            {
                                request.Proxy = null;
                            }
                            using (var response = request.GetResponse())
                            using (var stream = response.GetResponseStream())
                            {
                                img = GuiEngine.Current.ReadImageFromStream(stream);
                            }

                            GuiEngine.Current.FreezeImage(img);
                        }
                        catch
                        {
                            img = null;
                        }
                    }

                    return img;
                };
                return emote;
            };

            TwitchEmotesByIDCache.TryAdd(17,
                getEmoteReplacement(17, "StoneLightning",
                    "https://fourtf.com/chatterino/emotes/replacements/StoneLightning.png"));
            TwitchEmotesByIDCache.TryAdd(18,
                getEmoteReplacement(18, "TheRinger", "https://fourtf.com/chatterino/emotes/replacements/TheRinger.png"));
            TwitchEmotesByIDCache.TryAdd(20,
                getEmoteReplacement(20, "EagleEye", "https://fourtf.com/chatterino/emotes/replacements/EagleEye.png"));
            TwitchEmotesByIDCache.TryAdd(22,
                getEmoteReplacement(22, "RedCoat", "https://fourtf.com/chatterino/emotes/replacements/RedCoat.png"));
            TwitchEmotesByIDCache.TryAdd(33,
                getEmoteReplacement(33, "DansGame", "https://fourtf.com/chatterino/emotes/replacements/DansGame.png"));

            twitchEmotesCodeReplacements[@"[oO](_|\.)[oO]"] = "o_O";
            twitchEmotesCodeReplacements[@"\&gt\;\("] = ">(";
            twitchEmotesCodeReplacements[@"\&lt\;3"] = "<3";
            twitchEmotesCodeReplacements[@"\:-?(o|O)"] = ":O";
            twitchEmotesCodeReplacements[@"\:-?(p|P)"] = ":P";
            twitchEmotesCodeReplacements[@"\:-?[\\/]"] = ":/";
            twitchEmotesCodeReplacements[@"\:-?[z|Z|\|]"] = ":z";
            twitchEmotesCodeReplacements[@"\:-?\("] = ":(";
            twitchEmotesCodeReplacements[@"\:-?\)"] = ":)";
            twitchEmotesCodeReplacements[@"\:-?D"] = ":D";
            twitchEmotesCodeReplacements[@"\;-?(p|P)"] = ";P";
            twitchEmotesCodeReplacements[@"\;-?\)"] = ";)";
            twitchEmotesCodeReplacements[@"R-?\)"] = "R-)";

            _bttvHatEmotes["IceCold"] = null;
            //_bttvHatEmotes["SoSnowy"] = null;
            _bttvHatEmotes["TopHat"] = null;
            _bttvHatEmotes["SantaHat"] = null;

            //ChatterinoEmotes["WithAHat"] = new LazyLoadedImage { Name = "WithAHat", Tooltip = "WithAHat\nChatterino Emote", Url = "https://fourtf.com/chatterino/emotes/img/WithAHat_x1.png", IsHat = true };
        }

        private static string GetTwitchEmoteLink(int id, out double scale)
        {
            var _scale = AppSettings.EmoteScale > 2 ? 4 : (AppSettings.EmoteScale > 1 ? 2 : 1);

            scale = 1.0 / _scale;

            return TwitchEmoteTemplate.Replace("{id}", id.ToString()).Replace("{scale}", _scale.ToString());
        }

        public static string GetBttvEmoteLink(string link, out double scale)
        {
            var _scale = AppSettings.EmoteScale > 2 ? 4 : (AppSettings.EmoteScale > 1 ? 2 : 1);

            scale = 1.0 / _scale;

            return link.Replace("{{image}}", (_scale == 4 ? 3 : _scale) + "x");
        }

        public static IEnumerable<LazyLoadedImage> GetFfzEmoteFromDynamic(dynamic d, bool global)
        {
            List<LazyLoadedImage> emotes = new List<LazyLoadedImage>();

            foreach (var emote in d)
            {
                var name = emote["name"];

                dynamic urls = emote["urls"];

                int maxScale = 1;

                var urlX1 = "http:" + urls["1"];

                string urlX2 = null;
                if (urls.ContainsKey("2"))
                {
                    urlX2 = "http:" + urls["2"];
                    maxScale = 2;
                }

                string urlX4 = null;
                if (urls.ContainsKey("4"))
                {
                    urlX4 = "http:" + urls["4"];
                    maxScale = 4;
                }

                var _scale = AppSettings.EmoteScale > 2 ? 4 : (AppSettings.EmoteScale > 1 ? 2 : 1);

                string url;
                if (maxScale >= 2 && _scale == 2)
                {
                    _scale = 2;
                    url = urlX2;
                }
                else if (maxScale == 4 && _scale == 4)
                {
                    _scale = 4;
                    url = urlX4;
                }
                else
                {
                    _scale = 1;
                    url = urlX1;
                }

                var scale = 1.0 / _scale;

                string margins = emote["margins"];

                Margin margin = null;

                if (!string.IsNullOrEmpty(margins))
                {
                    var S = margins.Split(' ');

                    if (S.Length == 4)
                    {
                        try
                        {
                            var ints = S.Select(x =>
                            {
                                var index = x.Length;
                                for (var i = 0; i < x.Length; i++)
                                {
                                    if (x[i] != '-' && (x[i] < '0' || x[i] > '9'))
                                    {
                                        index = i - 1;
                                    }
                                }

                                return int.Parse(index == x.Length ? x : x.Remove(index));
                            }).ToArray();

                            margin = new Margin(ints[0], ints[1], ints[2], ints[3]);
                        }
                        catch
                        {
                        }
                    }
                }

                emotes.Add(new LazyLoadedImage { Name = name, Url = url, Scale = scale, Margin = margin, Tooltip = name + $"\nFrankerFaceZ {(global ? "Global" : "Channel")} Emote", IsEmote = true });
            }

            return emotes;
        }

        public static string GetTwitchEmoteCodeReplacement(string emoteCode)
        {
            string code;

            return twitchEmotesCodeReplacements.TryGetValue(emoteCode, out code) ? code : emoteCode;
        }

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
                    var parser = new System.Text.Json.JsonParser();

                    // better twitch tv emotes
                    //if (!File.Exists(bttvEmotesGlobalCache))
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

                        foreach (var e in json["emotes"])
                        {
                            string id = e["id"];
                            string code = e["code"];
                            string imageType = e["imageType"];
                            string url = template.Replace("{{id}}", id);

                            double scale;
                            url = GetBttvEmoteLink(url, out scale);

                            BttvGlobalEmotes[code] = new LazyLoadedImage { Name = code, Url = url, IsHat = IsBttvEmoteAHat(code), Scale = scale, Tooltip = code + "\nBetterTTV Global Emote", IsEmote = true };
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
                    var parser = new System.Text.Json.JsonParser();

                    //if (!File.Exists(ffzEmotesGlobalCache))
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

                            foreach (LazyLoadedImage emote in GetFfzEmoteFromDynamic(val["emoticons"], true))
                            {
                                FfzGlobalEmotes[emote.Name] = emote;
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

            // ffz event emotes
            Task.Run(() =>
            {
                try
                {
                    var set = 0;

                    var parser = new System.Text.Json.JsonParser();
                    using (var webClient = new WebClient())
                    using (var readStream = webClient.OpenRead("https://cdn.frankerfacez.com/script/event.json"))
                    {
                        dynamic json = parser.Parse(readStream);

                        string _set = json["set"];

                        int.TryParse(_set, out set);
                    }

                    if (set != 0)
                    {
                        using (var webClient = new WebClient())
                        using (var readStream = webClient.OpenRead("https://api.frankerfacez.com/v1/set/" + set))
                        {
                            dynamic json = parser.Parse(readStream);
                            dynamic _set = json["set"];

                            dynamic emoticons = _set["emoticons"];

                            foreach (LazyLoadedImage emote in GetFfzEmoteFromDynamic(emoticons, true))
                            {
                                FfzGlobalEmotes[emote.Name] = emote;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    e.Message.Log("emotes");
                }
            });
        }

        private static ConcurrentDictionary<string, object> _bttvHatEmotes = new ConcurrentDictionary<string, object>();

        private static bool IsBttvEmoteAHat(string code)
        {
            return code.StartsWith("Hallo") ||
                   _bttvHatEmotes.ContainsKey(code);
        }

        internal static void TriggerEmotesLoaded()
        {
            EmotesLoaded?.Invoke(null, EventArgs.Empty);
        }

        public static LazyLoadedImage GetTwitchEmoteById(int id, string name)
        {
            LazyLoadedImage e;

            if (!TwitchEmotesByIDCache.TryGetValue(id, out e))
            {
                double scale;

                e = new LazyLoadedImage
                {
                    Name = name,
                    Url = GetTwitchEmoteLink(id, out scale),
                    Scale = scale,
                    Tooltip = name + "\nTwitch Emote",
                    IsEmote = true
                };
                TwitchEmotesByIDCache[id] = e;
            }

            return e;
        }
    }
}
