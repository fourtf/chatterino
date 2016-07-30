using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Chatterino.Common
{
    public class TwitchChannel
    {
        // properties
        private string name;

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                SubLink = "https://www.twitch.tv/" + value + "/subscribe?ref=in_chat_subscriber_link";
            }
        }

        public string SubLink { get; private set; }

        public int Uses { get; set; } = 0;

        public ConcurrentDictionary<string, TwitchEmote> BttvChannelEmotes { get; private set; }
            = new ConcurrentDictionary<string, TwitchEmote>();

        private TwitchEmote subBadge;

        public TwitchEmote SubscriberBadge
        {
            get
            {
                return subBadge ?? (subBadge = new TwitchEmote
                {
                    LoadAction = () =>
                    {
                        try
                        {
                            string imageUrl = null;

                            var request = WebRequest.Create($"https://api.twitch.tv/kraken/chat/{Name}/badges");
                            using (var response = request.GetResponse())
                            using (var stream = response.GetResponseStream())
                            {
                                var json = new JsonParser().Parse(stream);

                                imageUrl = (string)(((Dictionary<string, object>)((Dictionary<string, object>)json)["subscriber"])["image"]);
                            }

                            request = WebRequest.Create(imageUrl);
                            using (var response = request.GetResponse())
                            using (var stream = response.GetResponseStream())
                            {
                                return GuiEngine.Current.ReadImageFromStream(stream);
                            }
                        }
                        catch
                        {
                            return null;
                        }
                    }
                });
            }
        }

        public ConcurrentDictionary<string, string> Users = new ConcurrentDictionary<string, string>();

        List<KeyValuePair<string, string>> emoteNames = new List<KeyValuePair<string, string>>();


        // ctor
        public TwitchChannel(string channelName)
        {
            Name = channelName.Trim('#');
            JoinWrite();
            JoinRead();

            string bttvChannelEmotesCache = $"./cache/bttv_channel_{channelName}";

            // bttv channel emotes
            Task.Run(() =>
            {
                try
                {
                    JsonParser parser = new JsonParser();

                    if (!File.Exists(bttvChannelEmotesCache) || DateTime.Now - new FileInfo(bttvChannelEmotesCache).LastWriteTime > TimeSpan.FromHours(1))
                    {
                        try
                        {
                            if (Util.IsLinux)
                            {
                                Util.LinuxDownloadFile("https://api.betterttv.net/2/channels/" + channelName, bttvChannelEmotesCache);
                            }
                            else
                            {
                                using (var webClient = new WebClient())
                                using (var readStream = webClient.OpenRead("https://api.betterttv.net/2/channels/" + channelName))
                                using (var writeStream = File.OpenWrite(bttvChannelEmotesCache))
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

                    using (var stream = File.OpenRead(bttvChannelEmotesCache))
                    {
                        dynamic json = parser.Parse(stream);
                        var template = "https:" + json["urlTemplate"]; //{{id}} {{image}}

                        foreach (dynamic e in json["emotes"])
                        {
                            string id = e["id"];
                            string code = e["code"];

                            TwitchEmote emote;
                            if (Emotes.BttvChannelEmotesCache.TryGetValue(id, out emote))
                            {
                                BttvChannelEmotes[code] = emote;
                            }
                            else
                            {
                                string imageType = e["imageType"];
                                string url = template.Replace("{{id}}", id).Replace("{{image}}", "1x");
                                Emotes.BttvChannelEmotesCache[id] = BttvChannelEmotes[code] = new TwitchEmote { Name = code, Url = url, Tooltip = code + "\nBetterTTV Channel Emote" };
                            }
                        }
                    }
                    updateEmoteNameList();
                }
                catch { }
            });

            Task.Run(() =>
            {
                try
                {
                    var request = WebRequest.Create($"http://tmi.twitch.tv/group/user/{channelName}/chatters");
                    using (var response = request.GetResponse())
                    using (var stream = response.GetResponseStream())
                    {
                        JsonParser parser = new JsonParser();
                        dynamic json = parser.Parse(stream);
                        dynamic chatters = json["chatters"];
                        foreach (dynamic group in chatters)
                        {
                            foreach (string user in group.Value)
                            {
                                Users[user.ToUpper()] = user;
                            }
                        }
                    }
                }
                catch { }
            });

            Emotes.EmotesLoaded += (s, e) =>
            {
                updateEmoteNameList();
            };
        }

        void updateEmoteNameList()
        {
            List<KeyValuePair<string, string>> names = new List<KeyValuePair<string, string>>();

            names.AddRange(Emotes.TwitchEmotes.Select(x => new KeyValuePair<string, string>(x.Key.ToUpper(), x.Key)));
            names.AddRange(Emotes.BttvGlobalEmotes.Keys.Select(x => new KeyValuePair<string, string>(x.ToUpper(), x)));
            names.AddRange(Emotes.FfzGlobalEmotes.Keys.Select(x => new KeyValuePair<string, string>(x.ToUpper(), x)));
            names.AddRange(BttvChannelEmotes.Keys.Select(x => new KeyValuePair<string, string>(x.ToUpper(), x)));

            emoteNames = names;
        }

        public string GetEmoteCompletion(string name, ref int index, bool forward)
        {
            name = name.ToUpper();

            var names = new List<KeyValuePair<string, string>>(emoteNames);

            names.AddRange(Users);
            names.Sort((x1, x2) => x1.Key.CompareTo(x2.Key));

            KeyValuePair<string, string> firstItem = new KeyValuePair<string, string>();
            KeyValuePair<string, string> lastItem = new KeyValuePair<string, string>();

            bool first = true;

            index += forward ? 1 : (index == 0 ? 4523453 : -1);

            int currentIndex = 0;
            for (int i = 0; i < names.Count; i++)
            {
                if (names[i].Key.StartsWith(name))
                {
                    if (first)
                    {
                        first = false;
                        firstItem = names[i];
                    }
                    if (currentIndex == index)
                    {
                        return names[i].Value;
                    }
                    currentIndex++;
                    lastItem = names[i];
                }
                else if (!first)
                {
                    break;
                }
            }

            if (!first)
            {
                index = currentIndex - 1;
                return firstItem.Value;
            }

            return null;
        }

        //public string GetNameCompletion(string name, ref int index)
        //{
        //    name = name.ToUpper();

        //    return null;
        //}

        public void JoinWrite()
        {
            IrcManager.IrcWriteClient?.RfcJoin("#" + Name);
        }

        public void JoinRead()
        {
            IrcManager.IrcReadClient?.RfcJoin("#" + Name);
        }

        public void Disconnect()
        {
            IrcManager.IrcReadClient?.RfcPart("#" + Name);
            IrcManager.IrcWriteClient?.RfcPart("#" + Name);
        }
    }
}
