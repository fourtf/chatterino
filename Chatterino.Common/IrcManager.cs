using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TwitchIrc;

namespace Chatterino.Common
{
    public static class IrcManager
    {
        // Constants
        public const int MaxMessageLength = 500;

        // Properties
        public static string Username { get; private set; } = null;

        public static IrcClient Client { get; set; }

        public static ConcurrentDictionary<string, Func<string, string>> ChatCommands = new ConcurrentDictionary<string, Func<string, string>>();

        static ConcurrentDictionary<string, object> twitchBlockedUsers = new ConcurrentDictionary<string, object>();

        // Static Ctor
        static IrcManager()
        {
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
            {
                settings.Load("./login.ini");
                settings.Load("./Login.ini");
            }
            else
            {
                settings.Load(loginReader);
            }

            if (settings.TryGetString("username", out username)
                && settings.TryGetString("oauth", out oauth))
            {
                Username = username;

                IrcManager.oauth = oauth;

                LoggedIn?.Invoke(null, EventArgs.Empty);

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

                // fetch available twitch emotes
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
                    if (Client == null)
                    {
                        Client = new IrcClient();

                        Client.ReadConnection.Connected += (s, e) =>
                        {
                            foreach (var channel in TwitchChannel.Channels)
                            {
                                Client.ReadConnection.WriteLine("JOIN #" + channel.Name);
                            }

                            Connected?.Invoke(null, EventArgs.Empty);
                        };

                        Client.ReadConnection.Disconnected += (s, e) =>
                        {
                            Disconnected?.Invoke(null, EventArgs.Empty);
                        };

                        Client.WriteConnection.Connected += (s, e) =>
                        {
                            foreach (var channel in TwitchChannel.Channels)
                            {
                                Client.WriteConnection.WriteLine("JOIN #" + channel.Name);
                            }
                        };
                    }

                    Client.Connect(username, (oauth.StartsWith("oauth:") ? oauth : "oauth:" + oauth));

                    Client.ReadConnection.MessageReceived += ReadConnection_MessageReceived;
                    Client.WriteConnection.MessageReceived += WriteConnection_MessageReceived;
                });

                // secret telemetry, please ignore :)
                // anonymously count user on fourtf.com with the first 32 characters of a sha 256 hash of the username
                Task.Run(() =>
                {
                    string hash;
                    using (SHA256 sha = SHA256.Create())
                    {
                        hash = string.Join("", sha
                          .ComputeHash(Encoding.UTF8.GetBytes(username))
                          .Select(item => item.ToString("x2")));
                    }
                    hash = hash.Remove(32);

                    var request = WebRequest.Create($"https://fourtf.com/chatterino/countuser.php?hash={hash}");
                    using (var response = request.GetResponse())
                    using (var stream = response.GetResponseStream()) { }
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

            oauth = null;
            twitchBlockedUsers.Clear();

            if (Client != null)
            {
                disconnected = true;

                Client.Disconnect();
                Client.ReadConnection.MessageReceived -= ReadConnection_MessageReceived;
                Client.WriteConnection.MessageReceived -= WriteConnection_MessageReceived;
            }

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

                    Client.Say(Emojis.ReplaceShortCodes(message), channel.TrimStart('#'));
                }
            }
        }

        public static bool IsIgnoredUser(string username)
        {
            return twitchBlockedUsers.ContainsKey(username.ToLower());
        }

        public static void AddIgnoredUser(string username)
        {
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
        public static event EventHandler LoggedIn;
        public static event EventHandler Disconnected;
        public static event EventHandler Connected;
        public static event EventHandler<ValueEventArgs<Exception>> ConnectionError;

        public static event EventHandler<ValueEventArgs<string>> NoticeAdded;

        static ConcurrentDictionary<Tuple<string, string>, object> recentChatClears = new ConcurrentDictionary<Tuple<string, string>, object>();

        private static void ReadConnection_MessageReceived(object sender, MessageEventArgs e)
        {
            var msg = e.Message;

            if (msg.Command == "PRIVMSG")
            {
                TwitchChannel.GetChannel(msg.Middle.TrimStart('#')).Process(c =>
                {
                    Message message = new Message(msg, c);

                    if (!AppSettings.IgnoreTwitchBlocks || !IsIgnoredUser(message.Username))
                    {
                        c.Users[message.Username.ToUpper()] = message.DisplayName;

                        c.AddMessage(message);
                    }
                });
            }
            else if (msg.Command == "CLEARCHAT")
            {
                var channel = msg.Middle;
                var user = msg.Params;

                var key = Tuple.Create(user, channel);

                if (!recentChatClears.ContainsKey(key))
                {
                    recentChatClears[key] = null;

                    object o;

                    new System.Threading.Timer(x => { recentChatClears.TryRemove(key, out o); }, null, 3000, System.Threading.Timeout.Infinite);

                    string reason;
                    msg.Tags.TryGetValue("ban-reason", out reason);
                    string _duration;
                    int duration = 0;

                    if (msg.Tags.TryGetValue("ban-duration", out _duration))
                    {
                        int.TryParse(_duration, out duration);
                    }

                    TwitchChannel.GetChannel((msg.Middle ?? "").TrimStart('#')).Process(c => c.ClearChat(user, reason, duration));
                }
            }
            else if (msg.Command == "ROOMSTATE")
            {
                TwitchChannel.GetChannel((msg.Middle ?? "").TrimStart('#')).Process(c =>
                {
                    RoomState state = c.RoomState;

                    string value;
                    if (msg.Tags.TryGetValue("emote-only", out value))
                    {
                        if (value == "1")
                            state |= RoomState.EmoteOnly;
                        else
                            state &= ~RoomState.EmoteOnly;
                    }
                    if (msg.Tags.TryGetValue("subs-only", out value))
                    {
                        if (value == "1")
                            state |= RoomState.SubOnly;
                        else
                            state &= ~RoomState.SubOnly;
                    }
                    if (msg.Tags.TryGetValue("slow", out value))
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
                    if (msg.Tags.TryGetValue("r9k", out value))
                    {
                        if (value == "1")
                            state |= RoomState.R9k;
                        else
                            state &= ~RoomState.R9k;
                    }
                    //if (e.Data.Tags.TryGetValue("broadcaster-lang", out value))

                    c.RoomState = state;
                    Console.WriteLine(c.RoomState);
                });
            }
            else if (msg.Command == "USERSTATE")
            {
                string value;

                if (msg.Tags.TryGetValue("mod", out value))
                {
                    TwitchChannel.GetChannel((msg.Middle ?? "").TrimStart('#')).Process(c => c.IsMod = value == "1");
                }
            }
            else if (msg.Command == "WHISPER")
            {
                var user = msg.PrefixNickname;

                TwitchChannel.WhisperChannel.AddMessage(new Message(msg, TwitchChannel.WhisperChannel, true, false));
            }
            else if (msg.Command == "USERNOTICE")
            {
                string sysMsg;
                msg.Tags.TryGetValue("system-msg", out sysMsg);

                TwitchChannel.GetChannel((msg.Middle ?? "").TrimStart('#')).Process(c => c.AddMessage(new Message(msg.Params == null ? (sysMsg ?? null) : $"{sysMsg}: {msg.Params}", HSLColor.Gray, true)));
            }
        }

        private static void WriteConnection_MessageReceived(object sender, MessageEventArgs e)
        {
            var msg = e.Message;

            if (msg.Command == "NOTICE")
            {
                TwitchChannel.GetChannel((msg.Middle ?? "").TrimStart('#')).Process(c =>
                {
                    Message message = new Message(msg.Params, null, true);

                    c.AddMessage(message);
                });
            }
        }
    }
}
