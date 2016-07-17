using Meebey.SmartIrc4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chatterino
{
    public static class App
    {
        public static string Username { get; private set; } = null;

        public static AppSettings Settings { get; private set; } = new AppSettings();

        [STAThread]
        static void Main()
        {
            ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.ThreadException += (s, e) =>
            {
                e.Exception.Log("exception", "{0}\n");
            };
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                (e.ExceptionObject as Exception).Log("exception", "{0}\n");
            };

            new Timer { Interval = 20, Enabled = true }.Tick += (s, e) =>
            {
                UpdateGifEmotes?.Invoke(null, EventArgs.Empty);
            };

            Settings.Load("./settings.ini");
            ColorScheme.Load("./colors.ini");

            runIrc();
            loadGlobalEmotes();

            MainForm = new MainForm();

            Application.Run(MainForm);

            Settings.Save("./settings.ini");
        }

        public const int MaxMessageLength = 500;

        public const TextFormatFlags DefaultTextFormatFlags = TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;

        //public static IniSettings Settings { get; set; } = new IniSettings();

        public static event EventHandler UpdateGifEmotes;

        public static event EventHandler GifEmoteFramesUpdated;
        public static void TriggerGifEmoteFramesUpdated()
        {
            GifEmoteFramesUpdated?.Invoke(null, EventArgs.Empty);
        }


        // COLOR SCHEME
        public static ColorScheme ColorScheme { get; set; } = new ColorScheme();
        public static event EventHandler ColorSchemeChanged;
        public static void TriggerColorSchemeChanged()
        {
            ColorSchemeChanged?.Invoke(null, EventArgs.Empty);
        }


        // WINDOW
        public static MainForm MainForm { get; set; }


        // IRC
        public static event EventHandler<IrcEventArgs> IrcMessageReceived;

        public static IrcClient IrcReadClient { get; set; }
        public static IrcClient IrcWriteClient { get; set; }

        public static void SendMessage(string channel, string message)
        {
            IrcWriteClient?.SendMessage(SendType.Message, "#" + channel.TrimStart('#'), message);
        }

        static void runIrc()
        {
            string username, oauth;

            var settings = new IniSettings();
            settings.Load("./login.ini");

            if (settings.TryGetString("username", out username)
                && settings.TryGetString("oauth", out oauth))
            {
                Username = username;
                MainForm?.SetTitle();

                IrcReadClient = new IrcClient
                {
                    Encoding = new UTF8Encoding(),
                    EnableUTF8Recode = true
                };

                Action<IrcClient> setProxy = client =>
                {
                    if (Settings.ProxyEnable)
                    {
                        ProxyType type;
                        Enum.TryParse(Settings.ProxyType, out type);
                        if (type != ProxyType.None)
                        {
                            IrcWriteClient.ProxyType = type;
                            IrcWriteClient.ProxyPort = Settings.ProxyPort;
                            IrcWriteClient.ProxyHost = Settings.ProxyHost;
                            IrcWriteClient.ProxyUsername = Settings.ProxyUsername;
                            IrcWriteClient.ProxyPassword = Settings.ProxyPassword;
                        }
                    }
                };

                setProxy(IrcReadClient);

                Task.Run(() =>
                {
                    IrcWriteClient = new IrcClient
                    {
                        Encoding = new UTF8Encoding(),
                        EnableUTF8Recode = true
                    };

                    setProxy(IrcWriteClient);

                    IrcWriteClient.OnRawMessage += (s, e) => { e.Data.RawMessage.Log(); };

                    "Connecting writeclient to irc.twitch.tv:6667".Log();
                    IrcWriteClient.Connect("irc.twitch.tv", 6667);
                    "Connected writeclient to irc.twitch.tv:6667".Log();

                    IrcWriteClient.Login(username, username, 0, username, (oauth.StartsWith("oauth:") ? oauth : "oauth:" + oauth));

                    new Task(() =>
                    {
                        IrcWriteClient.Listen();
                    }).Start();
                });

                try
                {
                    "Connecting readclient to irc.twitch.tv:6667".Log();
                    IrcReadClient.Connect("irc.twitch.tv", 6667);
                    "Connected readclient to irc.twitch.tv:6667".Log();
                }
                catch (ConnectionException e)
                {
                    $"ConnectionException: {e.Message}".Log();
                }

                "logging in".Log("irc");

                IrcReadClient.Login(username, username, 0, username, (oauth.StartsWith("oauth:") ? oauth : "oauth:" + oauth));
                //IrcReadClient.WriteLine("NICK justinfan987123");

                IrcReadClient.WriteLine("CAP REQ :twitch.tv/commands");
                IrcReadClient.WriteLine("CAP REQ :twitch.tv/tags");

                IrcReadClient.OnRawMessage += onRawMessage;

                new Task(() =>
                {
                    IrcReadClient.Listen();
                }).Start();
            }
        }

        static void onRawMessage(object sender, IrcEventArgs e)
        {
            IrcMessageReceived?.Invoke(sender, e);

            e.Data.RawMessage.Log();

            //if (e.Data.RawMessageArray.Length > 0 && e.Data.RawMessageArray[0] == "PONG")
            //{
            //    receivedTwitchPong = true;
            //}
        }

        public static ConcurrentDictionary<string, TwitchChannel> Channels { get; private set; } = new ConcurrentDictionary<string, TwitchChannel>();

        public static TwitchChannel AddChannel(string channel) => Channels.AddOrUpdate((channel ?? "").ToLower(), cname => new TwitchChannel(cname), (cname, c) => { c.Uses++; return c; });
        public static void RemoveChannel(string channel)
        {
            channel = channel.ToLower();

            TwitchChannel data;
            if (Channels.TryGetValue(channel ?? "", out data))
            {
                data.Uses--;
                if (data.Uses <= 0)
                {
                    data.Disconnect();
                    Channels.TryRemove(channel ?? "", out data);
                }
            }
        }


        // EMOTES
        public static event EventHandler EmoteLoaded;

        public static void TriggerEmoteLoaded()
        {
            EmoteLoaded?.Invoke(null, EventArgs.Empty);
        }

        private const string bttvEmotesGlobalCache = "./cache/bttv_global.json";
        public static ConcurrentDictionary<string, TwitchEmote> BttvGlobalEmotes = new ConcurrentDictionary<string, TwitchEmote>();
        public static ConcurrentDictionary<string, TwitchEmote> BttvChannelEmotesCache = new ConcurrentDictionary<string, TwitchEmote>();
        public static ConcurrentDictionary<int, TwitchEmote> TwitchEmotes = new ConcurrentDictionary<int, TwitchEmote>();

        static void loadGlobalEmotes()
        {
            Task.Run(() =>
            {
                try
                {
                    Directory.CreateDirectory("./cache");
                    System.Text.Json.JsonParser parser = new System.Text.Json.JsonParser();

                    // better twitch tv emotes
                    if (!File.Exists(bttvEmotesGlobalCache) || DateTime.Now - new FileInfo(bttvEmotesGlobalCache).LastWriteTime > TimeSpan.FromHours(24))
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

                            BttvGlobalEmotes[code] = new TwitchEmote { Name = code, Url = url };
                        }
                    }
                }
                catch (Exception exc)
                {
                    Console.WriteLine("error loading emotes: " + exc.Message);
                }
            });
        }


        // MISC
        public const string TwitchEmoteTemplate = "https://static-cdn.jtvnw.net/emoticons/v1/{id}/1.0";
    }
}
