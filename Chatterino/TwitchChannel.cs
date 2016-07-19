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

namespace Chatterino
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
                                return Image.FromStream(stream);
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


        // ctor
        public TwitchChannel(string channelName)
        {
            Name = channelName.Trim('#');

            string bttvChannelEmotesCache = $"./cache/bttv_channel_{channelName}";

            App.IrcReadClient.RfcJoin("#" + channelName);
            App.IrcWriteClient.RfcJoin("#" + channelName);

            Task.Run(() =>
            {
                try
                {
                    JsonParser parser = new JsonParser();

                    if (!File.Exists(bttvChannelEmotesCache) || DateTime.Now - new FileInfo(bttvChannelEmotesCache).LastWriteTime > TimeSpan.FromHours(24))
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
                            if (App.BttvChannelEmotesCache.TryGetValue(id, out emote))
                            {
                                BttvChannelEmotes[code] = emote;
                            }
                            else
                            {
                                string imageType = e["imageType"];
                                string url = template.Replace("{{id}}", id).Replace("{{image}}", "1x");
                                App.BttvChannelEmotesCache[id] = BttvChannelEmotes[code] = new TwitchEmote { Name = code, Url = url };
                            }
                        }
                    }

                }
                catch { }
            });
        }

        public void Disconnect()
        {
            App.IrcReadClient.RfcPart("#" + Name);
            App.IrcWriteClient.RfcPart("#" + Name);
        }
    }
}
