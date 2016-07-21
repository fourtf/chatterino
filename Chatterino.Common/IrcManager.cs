using Meebey.SmartIrc4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatterino.Common
{
    public static class IrcManager
    {
        // Constants
        public const int MaxMessageLength = 500;

        // Properties
        public static string Username { get; private set; } = null;

        public static IrcClient IrcReadClient { get; set; }
        public static IrcClient IrcWriteClient { get; set; }

        public static ConcurrentDictionary<string, Func<string, string>> ChatCommands = new ConcurrentDictionary<string, Func<string, string>>();

        public static void SendMessage(string channel, string message)
        {
            if (message.Length > 1 && message[0] == '/')
            {
                int index = message.IndexOf(' ');
                string _command = index == -1 ? message.Substring(1) : message.Substring(1, index - 1);
                message = index == -1 ? "" : message.Substring(index + 1);

                Func<string, string> command;
                if (ChatCommands.TryGetValue(_command, out command))
                {
                    message = command(message) ?? message;
                }
            }
            else if (AppSettings.ChatAllowSameMessage && (message[0] != '/' && message[0] != '.'))
            {
                message = ". " + message;
            }

            IrcWriteClient?.SendMessage(SendType.Message, "#" + channel.TrimStart('#'), Common.Emojis.ReplaceShortCodes(message));
        }

        // Static Ctor
        static bool initialized = false;

        public static void Initialize()
        {
            if (!initialized)
            {
                initialized = true;

                // Chat Commands
                ChatCommands.TryAdd("shrug", s => ". " + s + " ¯\\_(ツ)_/¯");

                // Login
                string username, oauth;

                var settings = new IniSettings();
                settings.Load("./login.ini");

                if (settings.TryGetString("username", out username)
                    && settings.TryGetString("oauth", out oauth))
                {
                    Username = username;
                    //MainForm?.SetTitle();

                    IrcReadClient = new IrcClient
                    {
                        Encoding = new UTF8Encoding(),
                        EnableUTF8Recode = true
                    };

                    Action<IrcClient> setProxy = client =>
                    {
                        if (AppSettings.ProxyEnable)
                        {
                            ProxyType type;
                            Enum.TryParse(AppSettings.ProxyType, out type);
                            if (type != ProxyType.None)
                            {
                                IrcWriteClient.ProxyType = type;
                                IrcWriteClient.ProxyPort = AppSettings.ProxyPort;
                                IrcWriteClient.ProxyHost = AppSettings.ProxyHost;
                                IrcWriteClient.ProxyUsername = AppSettings.ProxyUsername;
                                IrcWriteClient.ProxyPassword = AppSettings.ProxyPassword;
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
        }

        // Messages
        public static event EventHandler<IrcEventArgs> IrcMessageReceived;
        static void onRawMessage(object sender, IrcEventArgs e)
        {
            IrcMessageReceived?.Invoke(sender, e);

            e.Data.RawMessage.Log();

            //if (e.Data.RawMessageArray.Length > 0 && e.Data.RawMessageArray[0] == "PONG")
            //{
            //    receivedTwitchPong = true;
            //}
        }

        // Channels
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
    }
}
