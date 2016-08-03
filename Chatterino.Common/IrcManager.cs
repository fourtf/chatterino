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

        static ConcurrentDictionary<string, object> twitchBlockedUsers = new ConcurrentDictionary<string, object>();

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

            // Chat Commands
            ChatCommands.TryAdd("shrug", s => ". " + s + " ¯\\_(ツ)_/¯");
            ChatCommands.TryAdd("brainpower", s => ". " + s + " O-oooooooooo AAAAE-A-A-I-A-U- JO-oooooooooooo AAE-O-A-A-U-U-A- E-eee-ee-eee AAAAE-A-E-I-E-A- JO-ooo-oo-oo-oo EEEEO-A-AAA-AAAA " + s);

            ChatCommands.TryAdd("ignore", s =>
            {
                var S = s.SplitWords();
                if (S.Length > 0)
                {
                    AddIgnoredUser(S[0]);
                }
                return null;
            });
            ChatCommands.TryAdd("unignore", s =>
            {
                var S = s.SplitWords();
                if (S.Length > 0)
                {
                    RemoveIgnoredUser(S[0]);
                }
                return null;
            });
        }

        static string oauth = null;

        // Connection
        public static void Connect(TextReader loginReader = null)
        {
            Disconnect();

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

                IrcManager.oauth = oauth;

                AppSettings.UpdateCustomHighlightRegex();

                // fetch ignored users
                Task.Run(() =>
                {
                    try
                    {
                        int limit = 100;
                        int count = 0;
                        string nextLink = $"https://api.twitch.tv/kraken/users/{username}/blocks?limit={limit}";

                        var request = WebRequest.Create(nextLink + $"&oauth_token={oauth}");
                        using (var response = request.GetResponse())
                        using (var stream = response.GetResponseStream())
                        {
                            dynamic json = new JsonParser().Parse(stream);
                            dynamic _links = json["_links"];
                            nextLink = _links["next"];
                            dynamic blocks = json["blocks"];
                            count = blocks.Count;
                            foreach (dynamic block in blocks)
                            {
                                dynamic user = block["user"];
                                string name = user["name"];
                                string display_name = user["display_name"];
                            }
                        }
                    }
                    catch { }
                });


                // fetch availlable twitch emotes
                Task.Run(() =>
                {
                    try
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
                    catch { }
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

                            foreach (var channel in TwitchChannel.Channels)
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

                            foreach (var channel in TwitchChannel.Channels)
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

        public static void Disconnect()
        {
            bool disconnected = false;

            try
            {
                oauth = null;
                twitchBlockedUsers.Clear();
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

        // Send Messages
        public static void SendMessage(string channel, string _message)
        {
            if (channel != null)
            {
                var message = _message;

                if (_message.Length > 1 && _message[0] == '/')
                {
                    int index = _message.IndexOf(' ');
                    string _command = index == -1 ? _message.Substring(1) : _message.Substring(1, index - 1);
                    _message = index == -1 ? "" : _message.Substring(index + 1);

                    Func<string, string> command;
                    if (ChatCommands.TryGetValue(_command, out command))
                    {
                        message = command(_message) ?? message;
                    }
                }

                if (message != null)
                {
                    if (AppSettings.ChatAllowSameMessage)
                    {
                        message = message + " ";
                    }

                    IrcWriteClient?.SendMessage(SendType.Message, "#" + channel.TrimStart('#'), Emojis.ReplaceShortCodes(message));
                }
            }
        }

        public static bool IsIgnoredUser(string username)
        {
            return twitchBlockedUsers.ContainsKey(username.ToLower());
        }

        public static void AddIgnoredUser(string username)
        {
            object value;
            var _username = username.ToLower();

            bool success = false;
            HttpStatusCode statusCode;

            try
            {
                WebRequest request = WebRequest.Create($"https://api.twitch.tv/kraken/users/{Username}/blocks/{_username}?oauth_token={oauth}");
                request.Method = "PUT";
                using (var response = (HttpWebResponse)request.GetResponse())
                using (var stream = response.GetResponseStream())
                {
                    statusCode = response.StatusCode;
                    success = true;
                }
            }
            catch (WebException exc)
            {
                statusCode = ((HttpWebResponse)exc.Response).StatusCode;
            }
            catch (Exception) { statusCode = HttpStatusCode.BadRequest; }

            if (success)
            {
                NoticeAdded?.Invoke(null, new ValueEventArgs<string>($"Successfully ignored user \"{username}\"."));
                twitchBlockedUsers[_username] = null;
            }
            else
                NoticeAdded?.Invoke(null, new ValueEventArgs<string>($"Error \"{(int)statusCode}\" while trying to ignore user \"{username}\"."));
        }

        public static void RemoveIgnoredUser(string username)
        {
            object value;
            username = username.ToLower();

            bool success = false;
            HttpStatusCode statusCode;

            try
            {
                WebRequest request = WebRequest.Create($"https://api.twitch.tv/kraken/users/{Username}/blocks/{username}?oauth_token={oauth}");
                request.Method = "DELETE";
                using (var response = (HttpWebResponse)request.GetResponse())
                using (var stream = response.GetResponseStream())
                {
                    statusCode = response.StatusCode;
                    success = statusCode == HttpStatusCode.NoContent;
                }
            }
            catch (WebException exc)
            {
                statusCode = ((HttpWebResponse)exc.Response).StatusCode;
                success = statusCode == HttpStatusCode.NoContent;
            }
            catch (Exception) { statusCode = HttpStatusCode.BadRequest; }

            if (success)
            {
                twitchBlockedUsers.TryRemove(username.ToLower(), out value);

                NoticeAdded?.Invoke(null, new ValueEventArgs<string>($"Successfully unignored user \"{username}\"."));
            }
            else
                NoticeAdded?.Invoke(null, new ValueEventArgs<string>($"Error \"{(int)statusCode}\" while trying to unignore user \"{username}\"."));
        }

        // Messages
        public static event EventHandler Disconnected;
        public static event EventHandler Connected;
        public static event EventHandler<ValueEventArgs<Exception>> ConnectionError;

        public static event EventHandler<ValueEventArgs<string>> NoticeAdded;

        static ConcurrentDictionary<Tuple<string, string>, object> recentChatClears = new ConcurrentDictionary<Tuple<string, string>, object>();

        static void IrcClient_OnRawMessage(object sender, IrcEventArgs e)
        {
            if (e.Data.Type == ReceiveType.QueryNotice)
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

                    new System.Threading.Timer(x => { recentChatClears.TryRemove(key, out o); }, null, 3000, System.Threading.Timeout.Infinite);

                    string reason;
                    e.Data.Tags.TryGetValue("ban-reason", out reason);
                    string _duration;
                    int duration = 0;
                    if (e.Data.Tags.TryGetValue("ban-duration", out _duration))
                    {
                        int.TryParse(_duration, out duration);
                    }

                    if (e.Data.RawMessageArray.Length > 3)
                        TwitchChannel.GetChannel(e.Data.RawMessageArray[3].TrimStart('#')).Process(c => c.ClearChat(user, reason, duration));
                }
            }
            else if (e.Data.RawMessageArray.Length > 2 && e.Data.RawMessageArray[2] == "NOTICE")
            {
                TwitchChannel.GetChannel((e.Data.Channel ?? "").TrimStart('#')).Process(c =>
                {
                    Message msg = new Message(e.Data.Message, null, true);

                    c.AddMessage(msg);
                });
            }
            else if (e.Data.RawMessageArray.Length > 3 && e.Data.RawMessageArray[2] == "ROOMSTATE")
            {
                TwitchChannel.GetChannel(e.Data.RawMessageArray[3].TrimStart('#')).Process(c =>
                {
                    // @broadcaster-lang=;emote-only=0;r9k=0;slow=0;subs-only=0 :tmi.twitch.tv ROOMSTATE #fourtf

                    RoomState state = c.RoomState;

                    string value;
                    if (e.Data.Tags.TryGetValue("emote-only", out value))
                    {
                        if (value == "1")
                            state |= RoomState.EmoteOnly;
                        else
                            state &= ~RoomState.EmoteOnly;
                    }
                    if (e.Data.Tags.TryGetValue("subs-only", out value))
                    {
                        if (value == "1")
                            state |= RoomState.SubOnly;
                        else
                            state &= ~RoomState.SubOnly;
                    }
                    if (e.Data.Tags.TryGetValue("slow", out value))
                    {
                        if (value == "0")
                            state &= ~RoomState.SlowMode;
                        else
                        {
                            int time;
                            if (!int.TryParse(value, out time))
                                time = -1;
                            c.SlowModeTime = time;
                            state |= RoomState.SlowMode;
                        }
                    }
                    if (e.Data.Tags.TryGetValue("r9k", out value))
                    {
                        if (value == "1")
                            state |= RoomState.R9k;
                        else
                            state &= ~RoomState.R9k;
                    }
                    //if (e.Data.Tags.TryGetValue("broadcaster-lang", out value))
                    //{

                    //}

                    c.RoomState = state;
                });
            }
            else
            {
                if ((e.Data.Channel?.Length ?? 0) > 1 && e.Data.Channel?.Substring(1) != null
                    && e.Data.RawMessageArray.Length > 4 && e.Data.RawMessageArray[2] == "PRIVMSG")
                {
                    TwitchChannel.GetChannel((e.Data.Channel ?? "").TrimStart('#')).Process(c =>
                    {
                        Message msg = new Message(e.Data, c);

                        if (!AppSettings.IgnoreTwitchBlocks || !IsIgnoredUser(msg.Username))
                        {
                            c.Users[msg.Username.ToUpper()] = msg.DisplayName;

                            c.AddMessage(msg);
                        }
                    });
                }
            }

            e.Data.RawMessage.Log();
        }
    }
}
