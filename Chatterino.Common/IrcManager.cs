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
        public static Account Account { get; set; } = Account.AnonAccount;
        //public static string Username { get; private set; } = null;

        public static string DefaultClientID { get; set; } = "7ue61iz46fz11y3cugd0l3tawb4taal";
        //public static string ClientID { get; set; } = null;

        public static IrcClient Client { get; set; }

        static ConcurrentDictionary<string, object> twitchBlockedUsers = new ConcurrentDictionary<string, object>();

        public static IEnumerable<string> IgnoredUsers
        {
            get { return twitchBlockedUsers.Keys; }
        }

        public static string LastReceivedWhisperUser { get; set; }

        // Static Ctor
        public struct TwitchEmoteValue
        {
            public int Set { get; set; }
            public int ID { get; set; }
            public string ChannelName { get; set; }
        }

        // Connection
        public static void Connect()
        {
            Disconnect();

            // Login
            string username = Account.Username, oauth = Account.OauthToken;

            try
            {
                if (Account.IsAnon)
                {
                    if (AppSettings.SelectedUser != "")
                    {
                        AppSettings.SelectedUser = "";
                        AppSettings.Save();
                    }
                }
                else
                {
                    if (!string.Equals(username, AppSettings.SelectedUser))
                    {
                        AppSettings.SelectedUser = username;
                        AppSettings.Save();
                    }
                }
            }
            catch
            {
            }

            LoggedIn?.Invoke(null, EventArgs.Empty);

            AppSettings.UpdateCustomHighlightRegex();

            // fetch ignored users
            if (!Account.IsAnon)
            {
                Task.Run(() =>
                {
                    try
                    {
                        var limit = 100;
                        var count = 0;
                        string nextLink =
                            $"https://api.twitch.tv/kraken/users/{username}/blocks?limit={limit}&client_id={Account.ClientId}";

                        var request = WebRequest.Create(nextLink + $"&oauth_token={oauth}");
                        using (var response = request.GetResponse())
                        using (var stream = response.GetResponseStream())
                        {
                            dynamic json = new JsonParser().Parse(stream);
                            dynamic _links = json["_links"];
                            nextLink = _links["next"];
                            dynamic blocks = json["blocks"];
                            count = blocks.Count;
                            foreach (var block in blocks)
                            {
                                dynamic user = block["user"];
                                string name = user["name"];
                                string display_name = user["display_name"];
                                twitchBlockedUsers[name] = null;
                            }
                        }
                    }
                    catch
                    {
                    }
                });
            }

            // fetch available twitch emotes
            if (!Account.IsAnon)
            {
                Task.Run(() =>
                {
                    try
                    {
                        var request =
                            WebRequest.Create(
                                $"https://api.twitch.tv/kraken/users/{username}/emotes?oauth_token={oauth}&client_id={Account.ClientId}");
                        using (var response = request.GetResponse())
                        using (var stream = response.GetResponseStream())
                        {
                            dynamic json = new JsonParser().Parse(stream);
                            Emotes.TwitchEmotes.Clear();

                            foreach (var set in json["emoticon_sets"])
                            {
                                int setID;

                                int.TryParse(set.Key, out setID);

                                foreach (var emote in set.Value)
                                {
                                    int id;

                                    int.TryParse(emote["id"], out id);

                                    string code = Emotes.GetTwitchEmoteCodeReplacement(emote["code"]);

                                    Emotes.TwitchEmotes[code] = new TwitchEmoteValue
                                    {
                                        ID = id,
                                        Set = setID,
                                        ChannelName = "<unknown>"
                                    };
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                    Emotes.TriggerEmotesLoaded();
                });
            }

            // connect read
            Task.Run(() =>
            {
                Client = new IrcClient(Account.IsAnon);

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

                if (!Account.IsAnon)
                {
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
            if (!Account.IsAnon)
            {
                Task.Run(() =>
                {
                    try
                    {
                        string hash;
                        using (var sha = SHA256.Create())
                        {
                            hash = string.Join("", sha
                                .ComputeHash(Encoding.UTF8.GetBytes(username))
                                .Select(item => item.ToString("x2")));
                        }
                        hash = hash.Remove(32);

                        var request = WebRequest.Create($"https://fourtf.com/chatterino/countuser.php?hash={hash}");
                        using (var response = request.GetResponse())
                        using (response.GetResponseStream())
                        {
                        }
                    }
                    catch { }
                });
            }
        }

        public static void Disconnect()
        {
            var disconnected = false;

            twitchBlockedUsers.Clear();

            if (Client != null)
            {
                disconnected = true;

                Client.Disconnect();
                Client.ReadConnection.MessageReceived -= ReadConnection_MessageReceived;
                Client.WriteConnection.MessageReceived -= WriteConnection_MessageReceived;

                Client = null;
            }

            if (disconnected)
                Disconnected?.Invoke(null, EventArgs.Empty);
        }

        // Send Messages
        public static void SendMessage(TwitchChannel channel, string _message, bool isMod)
        {
            if (channel != null)
            {
                var message = Commands.ProcessMessage(_message, channel, true);

                if (!Client.Say(message, channel.Name.TrimStart('#'), isMod))
                {
                    channel.AddMessage(new Message($"Your message was not sent to protect you from a global ban. (try again in {Client.GetTimeUntilNextMessage(isMod).Seconds} seconds)", HSLColor.Gray, false));
                }
            }
        }

        public static bool IsIgnoredUser(string username)
        {
            return twitchBlockedUsers.ContainsKey(username.ToLower());
        }

        public static void AddIgnoredUser(string username)
        {
            string message;

            TryAddIgnoredUser(username, out message);

            NoticeAdded?.Invoke(null, new ValueEventArgs<string>(message));
        }

        public static bool TryAddIgnoredUser(string username, out string message)
        {
            var _username = username.ToLower();

            var success = false;
            HttpStatusCode statusCode;

            try
            {
                var request = WebRequest.Create($"https://api.twitch.tv/kraken/users/{Account.Username}/blocks/{_username}?oauth_token={Account.OauthToken}&client_id={Account.ClientId}");
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
                twitchBlockedUsers[_username] = null;
                message = $"Successfully ignored user \"{username}\".";
                return true;
            }
            else
            {
                message = $"Error \"{(int)statusCode}\" while trying to ignore user \"{username}\".";
                return false;
            }
        }

        public static void RemoveIgnoredUser(string username)
        {
            string message;

            TryRemoveIgnoredUser(username, out message);

            NoticeAdded?.Invoke(null, new ValueEventArgs<string>(message));
        }

        public static bool TryRemoveIgnoredUser(string username, out string message)
        {
            object value;
            username = username.ToLower();

            var success = false;
            HttpStatusCode statusCode;

            try
            {
                var request = WebRequest.Create($"https://api.twitch.tv/kraken/users/{Account.Username}/blocks/{username}?oauth_token={Account.OauthToken}&client_id={Account.ClientId}");
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

                message = $"Successfully unignored user \"{username}\".";
                return true;
            }
            else
            {
                message = $"Error \"{(int)statusCode}\" while trying to unignore user \"{username}\".";
                return false;
            }
        }

        // Check followed users
        public static bool TryCheckIfFollowing(string username, out bool result, out string message)
        {
            try
            {
                var request = WebRequest.Create($"https://api.twitch.tv/kraken/users/{Account.Username}/follows/channels/{username}?client_id={Account.ClientId}&oauth_token={Account.OauthToken}");
                using (var response = request.GetResponse())
                using (var stream = response.GetResponseStream())
                {
                    result = true;
                    message = null;
                    return true;
                }
            }
            catch (Exception exc)
            {
                var webExc = exc as HttpListenerException;

                if (webExc != null)
                {
                    if (webExc.ErrorCode == 404)
                    {
                        result = false;
                        message = null;
                        return true;
                    }
                }

                result = false;
                message = exc.Message;
                return false;
            }
        }

        public static bool TryFollowUser(string username, out string message)
        {
            try
            {
                var request = WebRequest.Create($"https://api.twitch.tv/kraken/users/{Account.Username}/follows/channels/{username}?client_id={Account.ClientId}&oauth_token={Account.OauthToken}");
                request.Method = "PUT";

                using (var response = request.GetResponse())
                using (var stream = response.GetResponseStream())
                {
                    message = null;
                    return true;
                }
            }
            catch (Exception exc)
            {
                message = exc.Message;
                return false;
            }
        }

        public static bool TryUnfollowUser(string username, out string message)
        {
            try
            {
                var request = WebRequest.Create($"https://api.twitch.tv/kraken/users/{Account.Username}/follows/channels/{username}?client_id={Account.ClientId}&oauth_token={Account.OauthToken}");
                request.Method = "DELETE";

                using (var response = request.GetResponse())
                using (var stream = response.GetResponseStream())
                {
                    message = null;
                    return true;
                }
            }
            catch (Exception exc)
            {
                message = exc.Message;
                return false;
            }
        }

        // Messages
        public static event EventHandler LoggedIn;
        public static event EventHandler Disconnected;
        public static event EventHandler Connected;

        public static event EventHandler<ValueEventArgs<string>> NoticeAdded;

        private static void ReadConnection_MessageReceived(object sender, MessageEventArgs e)
        {
            var msg = e.Message;

            if (msg.Command == "PRIVMSG")
            {
                TwitchChannel.GetChannel(msg.Middle.TrimStart('#')).Process(c =>
                {
                    if (msg.PrefixUser == "twitchnotify")
                    {
                        c.AddMessage(new Message(msg.Params ?? "", HSLColor.Gray, true) { HighlightType = HighlightType.Resub });
                    }
                    else
                    {
                        // check if ignore keyword is triggered
                        if (AppSettings.IgnoredKeywordsRegex == null || !AppSettings.IgnoredKeywordsRegex.IsMatch(e.Message.Params))
                        {
                            var message = new Message(msg, c);

                            // check if user is on the ignore list
                            if (AppSettings.EnableTwitchUserIgnores && IsIgnoredUser(message.Username))
                            {
                                switch (AppSettings.ChatShowIgnoredUsersMessages)
                                {
                                    case 1:
                                        if (!c.IsModOrBroadcaster)
                                            return;
                                        break;
                                    case 2:
                                        if (!c.IsBroadcaster)
                                            return;
                                        break;
                                    default:
                                        return;
                                }
                            }

                            {
                                c.Users[message.Username.ToUpper()] = message.DisplayName;

                                if (message.HasAnyHighlightType(HighlightType.Highlighted))
                                {
                                    var mentionMessage = new Message(msg, c, enablePingSound: false, includeChannel: true)
                                    {
                                        HighlightType = HighlightType.None
                                    };

                                    TwitchChannel.MentionsChannel.AddMessage(mentionMessage);
                                }

                                c.AddMessage(message);
                            }
                        }
                    }
                });
            }
            else if (msg.Command == "CLEARCHAT")
            {
                var channel = msg.Middle;
                var user = msg.Params;

                string reason;
                msg.Tags.TryGetValue("ban-reason", out reason);
                string _duration;
                var duration = 0;

                if (msg.Tags.TryGetValue("ban-duration", out _duration))
                {
                    int.TryParse(_duration, out duration);
                }

                TwitchChannel.GetChannel((msg.Middle ?? "").TrimStart('#')).Process(c => c.ClearChat(user, reason, duration));
                //}
            }
            else if (msg.Command == "ROOMSTATE")
            {
                TwitchChannel.GetChannel((msg.Middle ?? "").TrimStart('#')).Process(c =>
                {
                    var state = c.RoomState;

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
                TwitchChannel.WhisperChannel.AddMessage(new Message(msg, TwitchChannel.WhisperChannel, true, false, isReceivedWhisper: true));

                LastReceivedWhisperUser = msg.PrefixUser;

                if (AppSettings.ChatEnableInlineWhispers)
                {
                    var inlineMessage = new Message(msg, TwitchChannel.WhisperChannel, true, false, true)
                    {
                        HighlightTab = false,
                        HighlightType = HighlightType.Whisper
                    };

                    foreach (var channel in TwitchChannel.Channels)
                    {
                        channel.AddMessage(inlineMessage);
                    }
                }
            }
            else if (msg.Command == "USERNOTICE")
            {
                string sysMsg;
                msg.Tags.TryGetValue("system-msg", out sysMsg);

                TwitchChannel.GetChannel((msg.Middle ?? "").TrimStart('#')).Process(c =>
                {
                    try
                    {
                        var sysMessage = new Message(sysMsg, HSLColor.Gray, true)
                        {
                            HighlightType = HighlightType.Resub
                        };
                        c.AddMessage(sysMessage);

                        if (!string.IsNullOrEmpty(msg.Params))
                        {
                            var message = new Message(msg, c)
                            {
                                HighlightType = HighlightType.Resub
                            };
                            c.AddMessage(message);
                        }
                    }
                    catch { }
                });
            }
        }

        private static void WriteConnection_MessageReceived(object sender, MessageEventArgs e)
        {
            var msg = e.Message;

            if (msg.Command == "NOTICE")
            {
                TwitchChannel.GetChannel((msg.Middle ?? "").TrimStart('#')).Process(c =>
                {
                    string tmp;

                    if (msg.Tags.TryGetValue("msg-id", out tmp) && tmp == "timeout_success")
                        return;

                    if (AppSettings.Rainbow && tmp == "color_changed")
                        return;

                    var message = new Message(msg.Params, null, true) { HighlightTab = false };

                    c.AddMessage(message);
                });
            }
        }
    }
}
