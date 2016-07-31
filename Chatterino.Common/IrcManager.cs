using Meebey.SmartIrc4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
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

        static bool readPongReceived = true;
        static bool writePongReceived = true;

        public static ConcurrentDictionary<string, Func<string, string>> ChatCommands = new ConcurrentDictionary<string, Func<string, string>>();

        public static void SendMessage(string channel, string message)
        {
            if (channel != null)
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

                if (AppSettings.ChatAllowSameMessage)
                {
                    message = message + " ";
                }

                IrcWriteClient?.SendMessage(SendType.Message, "#" + channel.TrimStart('#'), Common.Emojis.ReplaceShortCodes(message));
            }
        }

        static System.Threading.Timer pingTimer;

        // Static Ctor
        static IrcManager()
        {
            pingTimer = new System.Threading.Timer(t =>
            {
                bool reconnect = false;
                if (IrcWriteClient != null)
                {
                    if (writePongReceived)
                    {
                        writePongReceived = false;
                        try
                        {
                            IrcWriteClient.WriteLine("PING");
                        }
                        catch { }
                    }
                    else
                        reconnect = true;
                }
                if (IrcReadClient != null)
                {
                    if (readPongReceived)
                    {
                        readPongReceived = false;
                        try
                        {
                            IrcReadClient.WriteLine("PING");
                        }
                        catch { }
                    }
                    else
                        reconnect = true;
                }
                if (reconnect)
                {
                    Disconnected?.Invoke(null, EventArgs.Empty);
                    Connect();
                }
            }, null, 15000, 15000);
        }

        static void setProxy(IrcClient client)
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
        }

        public static void Connect(TextReader loginReader = null)
        {
            Disconnect();

            // Chat Commands
            ChatCommands.TryAdd("shrug", s => ". " + s + " ¯\\_(ツ)_/¯");
            ChatCommands.TryAdd("brainpower", s => ". " + s + " O-oooooooooo AAAAE-A-A-I-A-U- JO-oooooooooooo AAE-O-A-A-U-U-A- E-eee-ee-eee AAAAE-A-E-I-E-A- JO-ooo-oo-oo-oo EEEEO-A-AAA-AAAA " + s);

            // Login
            string username, oauth;

            var settings = new IniSettings();
            if (loginReader == null)
                settings.Load("./login.ini");
            else
                settings.Load(loginReader);

            if (settings.TryGetString("username", out username)
                && settings.TryGetString("oauth", out oauth))
            {
                Username = username;

                // fetch availlable twitch emotes
                Task.Run(() =>
                {
                    //try
                    {
                        var request = WebRequest.Create($"https://api.twitch.tv/kraken/users/{username}/emotes?oauth_token={oauth}");
                        using (var response = request.GetResponse())
                        using (var stream = response.GetResponseStream())
                        {
                            dynamic json = new JsonParser().Parse(stream);
                            Emotes.TwitchEmotes.Clear();

                            foreach (dynamic set in json["emoticon_sets"])
                            {
                                foreach (dynamic emote in set.Value)
                                {
                                    // emote["id"]

                                    Emotes.TwitchEmotes[emote["code"]] = null;
                                    Emotes.TriggerEmotesLoaded();
                                }
                            }
                        }
                    }
                    //catch { }
                });

                // connect read
                Task.Run(() =>
                {
                    readPongReceived = true;

                    var read = new IrcClient
                    {
                        Encoding = new UTF8Encoding(),
                        EnableUTF8Recode = true,
                    };

                    read.OnDisconnecting += (s, e) =>
                    {
                        Disconnected?.Invoke(null, EventArgs.Empty);
                    };

                    setProxy(read);

                    try
                    {
                        read.Connect("irc.chat.twitch.tv", 6667);

                        try
                        {
                            read.Login(username, username, 0, username, (oauth.StartsWith("oauth:") ? oauth : "oauth:" + oauth));
                            read.WriteLine("CAP REQ :twitch.tv/commands");
                            read.WriteLine("CAP REQ :twitch.tv/tags");

                            IrcReadClient = read;
                            read.OnRawMessage += IrcClient_OnRawMessage;
                            read.OnRawMessage += (s, e) =>
                            {
                                if (e.Data.RawMessageArray.Length > 0 && e.Data.RawMessageArray[0] == "PONG")
                                    readPongReceived = true;
                            };

                            foreach (var channel in Channels.Values)
                            {
                                channel.JoinRead();
                            }

                            Task.Run(() => read.Listen());

                            Connected?.Invoke(null, EventArgs.Empty);
                        }
                        catch (Exception exc)
                        {
                            ConnectionError?.Invoke(null, new ValueEventArgs<Exception>(exc));
                        }
                    }
                    catch (Exception exc)
                    {
                        ConnectionError?.Invoke(null, new ValueEventArgs<Exception>(exc));
                    }
                });

                // connect write
                Task.Run(() =>
                {
                    writePongReceived = true;

                    var write = new IrcClient
                    {
                        Encoding = new UTF8Encoding(),
                        EnableUTF8Recode = true,
                    };

                    write.OnDisconnecting += (s, e) => { Disconnected?.Invoke(null, EventArgs.Empty); };

                    setProxy(write);

                    try
                    {
                        write.Connect("irc.chat.twitch.tv", 6667);

                        try
                        {
                            write.Login(username, username, 0, username, (oauth.StartsWith("oauth:") ? oauth : "oauth:" + oauth));

                            write.OnRawMessage += (s, e) =>
                            {
                                if (e.Data.RawMessageArray.Length > 0 && e.Data.RawMessageArray[0] == "PONG")
                                    writePongReceived = true;
                            };

                            IrcWriteClient = write;

                            foreach (var channel in Channels.Values)
                            {
                                channel.JoinWrite();
                            }

                            Task.Run(() => write.Listen());
                        }
                        catch (Exception exc)
                        {
                            ConnectionError?.Invoke(null, new ValueEventArgs<Exception>(exc));
                        }
                    }
                    catch (Exception exc)
                    {
                        ConnectionError?.Invoke(null, new ValueEventArgs<Exception>(exc));
                    }
                });
            }
            else
            {
                ConnectionError?.Invoke(null, new ValueEventArgs<Exception>(new Exception("No login credentials found")));
            }
        }

        public static void Disconnect()
        {
            bool disconnected = false;

            try
            {
                if (IrcReadClient != null)
                {
                    disconnected = true;
                    IrcReadClient.OnRawMessage -= IrcClient_OnRawMessage;
                    IrcReadClient = null;
                }
                if (IrcWriteClient != null)
                {
                    disconnected = true;
                    IrcWriteClient = null;
                }
            }
            catch { }

            if (disconnected)
                Disconnected?.Invoke(null, EventArgs.Empty);
        }

        // Messages
        public static event EventHandler<MessageEventArgs> MessageReceived;
        public static event EventHandler<ChatClearedEventArgs> ChatCleared;
        public static event EventHandler Disconnected;
        public static event EventHandler Connected;
        public static event EventHandler<ValueEventArgs<Exception>> ConnectionError;

        static ConcurrentDictionary<Tuple<string, string>, object> recentChatClears = new ConcurrentDictionary<Tuple<string, string>, object>();

        static void IrcClient_OnRawMessage(object sender, IrcEventArgs e)
        {
            if (e.Data.Type == ReceiveType.QueryNotice/* && e.Data.Message == "Login authentication failed"*/)
            {
                ConnectionError?.Invoke(null, new ValueEventArgs<Exception>(new Exception(e.Data.Message)));
            }
            else if (e.Data.RawMessageArray.Length > 2 && e.Data.RawMessageArray[2] == "CLEARCHAT")
            {
                var channel = e.Data.RawMessageArray[3].TrimStart('#');
                var user = e.Data.Message;

                var key = Tuple.Create(user, channel);

                if (!recentChatClears.ContainsKey(key))
                {
                    recentChatClears[key] = null;

                    object o;

                    new System.Threading.Timer(x => { recentChatClears.TryRemove(key, out o); }, null, 1000, System.Threading.Timeout.Infinite);

                    var reason = e.Data.Tags["ban-reason"];
                    var duration = e.Data.Tags["ban-duration"];

                    ChatCleared?.Invoke(null, new ChatClearedEventArgs(channel, user, duration, reason));
                }
            }
            else
            {
                if ((e.Data.Channel?.Length ?? 0) > 1 && e.Data.Channel?.Substring(1) != null
                    && e.Data.RawMessageArray.Length > 4 && e.Data.RawMessageArray[2] == "PRIVMSG")
                {
                    TwitchChannel c;

                    if (Channels.TryGetValue((e.Data.Channel ?? "").TrimStart('#'), out c))
                    {
                        Message msg = new Message(e.Data, c);
                        c.Users[msg.Username.ToUpper()] = msg.DisplayName;

                        MessageReceived?.Invoke(sender, new MessageEventArgs(msg));
                    }
                }
            }

            e.Data.RawMessage.Log();
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
