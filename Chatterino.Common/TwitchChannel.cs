using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using TwitchIrc;

namespace Chatterino.Common
{
    public class TwitchChannel
    {
        const int maxMessages = 1000;

        static readonly System.Timers.Timer refreshChatterListTimer = new System.Timers.Timer(30 * 1000 * 60);

        // properties
        public string Name { get; private set; }

        public int RoomID { get; private set; } = -1;

        public string SubLink { get; private set; }
        public string ChannelLink { get; private set; }
        public string PopoutPlayerLink { get; private set; }

        protected int Uses { get; set; } = 0;

        public bool IsLive { get; private set; }

        public int StreamViewerCount { get; private set; }
        public string StreamStatus { get; private set; }
        public string StreamGame { get; private set; }
        public DateTime StreamStart { get; set; }

        public event EventHandler LiveStatusUpdated;

        // Channel Emotes
        public ConcurrentDictionary<string, LazyLoadedImage> BttvChannelEmotes { get; private set; }
        = new ConcurrentDictionary<string, LazyLoadedImage>();

        public ConcurrentDictionary<string, LazyLoadedImage> FfzChannelEmotes { get; private set; }
        = new ConcurrentDictionary<string, LazyLoadedImage>();


        // Sub Badge
        private LazyLoadedImage subBadge;

        public LazyLoadedImage SubscriberBadge
        {
            get
            {
                return subBadge ?? (subBadge = new LazyLoadedImage
                {
                    LoadAction = () =>
                    {
                        try
                        {
                            string imageUrl = null;

                            var request =
                                WebRequest.Create(
                                    $"https://api.twitch.tv/kraken/chat/{Name}/badges?client_id={IrcManager.DefaultClientID}");
                            if (AppSettings.IgnoreSystemProxy)
                            {
                                request.Proxy = null;
                            }
                            using (var response = request.GetResponse())
                            using (var stream = response.GetResponseStream())
                            {
                                var json = new JsonParser().Parse(stream);

                                imageUrl =
                                    (string)
                                    (((Dictionary<string, object>)
                                        ((Dictionary<string, object>)json)["subscriber"])["image"]);
                            }

                            request = WebRequest.Create(imageUrl);
                            if (AppSettings.IgnoreSystemProxy)
                            {
                                request.Proxy = null;
                            }
                            using (var response = request.GetResponse())
                            using (var stream = response.GetResponseStream())
                            {
                                var img = GuiEngine.Current.ReadImageFromStream(stream);
                                GuiEngine.Current.FreezeImage(img);
                                return img;
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

        public ConcurrentDictionary<int, LazyLoadedImage> SubscriberBadges = new ConcurrentDictionary<int, LazyLoadedImage>();

        public LazyLoadedImage GetSubscriberBadge(int months)
        {
            LazyLoadedImage emote;

            if (SubscriberBadges.TryGetValue(months, out emote))
            {
                return emote;
            }

            return SubscriberBadge;
        }


        // Moderator Badge
        public LazyLoadedImage ModeratorBadge { get; private set; } = null;


        // Roomstate
        public event EventHandler RoomStateChanged;

        private RoomState roomState;

        public RoomState RoomState
        {
            get { return roomState; }
            set
            {
                if (roomState != value)
                {
                    roomState = value;
                    RoomStateChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private int slowModeTime;

        public int SlowModeTime
        {
            get { return slowModeTime; }
            set
            {
                if (slowModeTime != value)
                {
                    slowModeTime = value;
                    RoomStateChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }


        // Userstate
        public event EventHandler UserStateChanged;

        private bool isMod;

        public bool IsMod
        {
            get { return isMod; }
            set
            {
                if (isMod != value)
                {
                    isMod = value;

                    UserStateChanged?.Invoke(null, EventArgs.Empty);
                }
            }
        }

        public bool IsModOrBroadcaster
        {
            get
            {
                if (IrcManager.Account.IsAnon)
                    return false;

                return IsMod ||
                       string.Equals(Name, IrcManager.Account.Username, StringComparison.InvariantCultureIgnoreCase);
            }
        }

        public bool IsBroadcaster
        {
            get
            {
                if (IrcManager.Account.IsAnon)
                    return false;

                return string.Equals(Name, IrcManager.Account.Username, StringComparison.InvariantCultureIgnoreCase);
            }
        }

        protected void loadData()
        {
            loadRoomID();
        }

        protected void loadRoomID()
        {
            // Try to load from cache
            RoomID = Cache.RoomIdCache.Get(Name);

            if (RoomID == -1)
            {
                // No room ID was saved in the cache

                if (loadRoomIDFromTwitch())
                {
                    // Successfully got a room ID from twitch
                    Cache.RoomIdCache.Set(Name, RoomID);
                }
            }
        }

        protected bool loadRoomIDFromTwitch()
        {
            // call twitch kraken api
            try
            {
                var request =
                    WebRequest.Create(
                        $"https://api.twitch.tv/kraken/channels/{Name}?client_id={IrcManager.DefaultClientID}");
                if (AppSettings.IgnoreSystemProxy)
                {
                    request.Proxy = null;
                }
                using (var response = request.GetResponse())
                using (var stream = response.GetResponseStream())
                {
                    var parser = new JsonParser();

                    dynamic json = parser.Parse(stream);

                    int roomID;

                    if (int.TryParse(json["_id"], out roomID))
                    {
                        RoomID = roomID;
                        return true;
                    }
                }
            }
            catch
            {
            }

            return false;
        }


        // ctor
        private TwitchChannel(string channelName)
        {
            if (!channelName.StartsWith("/"))
            {
                Name = channelName.Trim('#');
                SubLink = $"https://www.twitch.tv/{Name}/subscribe?ref=in_chat_subscriber_link";
                ChannelLink = $"https://twitch.tv/{Name}";
                PopoutPlayerLink = $"https://player.twitch.tv/?channel={Name}";

                Join();

                ReloadEmotes();

                // recent chat
                Task.Run(() =>
                {
                    loadData();

                    if (RoomID != -1)
                    {
                        try
                        {
                            var request =
                                WebRequest.Create($"https://badges.twitch.tv/v1/badges/channels/{RoomID}/display");
                            if (AppSettings.IgnoreSystemProxy)
                            {
                                request.Proxy = null;
                            }
                            using (var response = request.GetResponse())
                            using (var stream = response.GetResponseStream())
                            {
                                var parser = new JsonParser();

                                dynamic json = parser.Parse(stream);

                                dynamic badgeSets = json["badge_sets"];
                                dynamic subscriber = badgeSets["subscriber"];
                                dynamic versions = subscriber["versions"];

                                foreach (var version in versions)
                                {
                                    int months = int.Parse(version.Key);

                                    dynamic value = version.Value;

                                    string imageUrl = value["image_url_1x"];
                                    string title = value["title"];
                                    string description = value["description"];
                                    string clickUrl = value["click_url"];

                                    SubscriberBadges[months] = new LazyLoadedImage
                                    {
                                        Name = title,
                                        Url = imageUrl,
                                        Tooltip = "Subscriber Badge" + (months == 0 ? "" : $" ({months} months)")
                                    };
                                }
                            }
                        }
                        catch
                        {
                        }

                        try
                        {
                            var messages = new List<Message>();

                            var request =
                                WebRequest.Create(
                                    $"https://tmi.twitch.tv/api/rooms/{RoomID}/recent_messages?client_id={IrcManager.DefaultClientID}");
                            if (AppSettings.IgnoreSystemProxy)
                            {
                                request.Proxy = null;
                            }
                            using (var response = request.GetResponse())
                            using (var stream = response.GetResponseStream())
                            {
                                var parser = new JsonParser();

                                dynamic json = parser.Parse(stream);

                                dynamic _messages = json["messages"];

                                foreach (string s in _messages)
                                {
                                    IrcMessage msg;

                                    if (IrcMessage.TryParse(s, out msg))
                                    {
                                        messages.Add(new Message(msg, this) { HighlightTab = false });
                                    }
                                }

                                //StreamReader reader = new StreamReader(stream);
                                //string line;
                                //while ((line = reader.ReadLine()) != null)
                                //{
                                //    IrcMessage msg;

                                //    if (IrcMessage.TryParse(line, out msg))
                                //    {
                                //        if (msg.Params != null)
                                //            messages.Add(new Message(msg, this, false, false));
                                //    }
                                //}
                            }

                            AddMessagesAtStart(messages.ToArray());
                        }
                        catch
                        {
                        }
                    }
                });

                // get chatters
                Task.Run(() =>
                {
                    fetchUsernames();
                });

                checkIfIsLive();
            }

            Emotes.EmotesLoaded += Emotes_EmotesLoaded;
            IrcManager.Connected += IrcManager_Connected;
            IrcManager.Disconnected += IrcManager_Disconnected;
            IrcManager.NoticeAdded += IrcManager_NoticeAdded;
            AppSettings.MessageLimitChanged += AppSettings_MessageLimitChanged;
            AppSettings.FontChanged += AppSettings_FontChanged;
        }

        private void AppSettings_FontChanged(object sender, EventArgs e)
        {
            lock (MessageLock)
            {
                foreach (var msg in Messages)
                {
                    msg.InvalidateTextMeasurements();
                }
            }
        }

        private void IrcManager_Connected(object sender, EventArgs e)
        {
            lock (MessageLock)
            {
                if (Messages.Length != 0 && Messages[Messages.Length - 1].HighlightType.HasFlag(HighlightType.Disconnected))
                {
                    Messages[Messages.Length - 1] = new Message("reconnected to chat",
                        HSLColor.Gray, true)
                    {
                        HighlightTab = false,
                        HighlightType = HighlightType.Connected,
                    };

                    Task.Run(() => ChatCleared?.Invoke(this, new ChatClearedEventArgs("", "", 0)));

                    return;
                }
            }

            AddMessage(new Message("connected to chat", HSLColor.Gray, true)
            {
                HighlightTab = false,
                HighlightType = HighlightType.Disconnected,
            });
        }

        private void IrcManager_Disconnected(object sender, EventArgs e)
        {
            AddMessage(new Message("disconnected from chat", HSLColor.Gray, true)
            {
                HighlightTab = false,
                HighlightType = HighlightType.Disconnected
            });
        }

        private void IrcManager_NoticeAdded(object sender, ValueEventArgs<string> e)
        {
            AddMessage(new Message(e.Value, HSLColor.Gray, true) { HighlightTab = false });
        }

        private void AppSettings_MessageLimitChanged(object sender, EventArgs e)
        {
            Message[] _messages = null;

            lock (MessageLock)
            {
                if (Messages.Length > AppSettings.ChatMessageLimit)
                {
                    _messages = new Message[Messages.Length - AppSettings.ChatMessageLimit];
                    Array.Copy(Messages, _messages, _messages.Length);

                    var M = new Message[AppSettings.ChatMessageLimit];
                    Array.Copy(Messages, Messages.Length - AppSettings.ChatMessageLimit, M, 0,
                        AppSettings.ChatMessageLimit);

                    Messages = M;
                }
            }

            if (_messages != null)
                MessagesRemovedAtStart?.Invoke(this, new ValueEventArgs<Message[]>(_messages));
        }

        private void Emotes_EmotesLoaded(object sender, EventArgs e)
        {
            updateEmoteNameList();
        }


        // Channels
        static TwitchChannel()
        {
            WhisperChannel?.AddMessage(
                new Message("Please note that chatterino can only read whispers while it is running!", null, true));
            WhisperChannel?.AddMessage(new Message("You can send whispers using the \"/w user message\" command!", null,
                true));
            MentionsChannel?.AddMessage(
                new Message("Please note that chatterino can only read mentions while it is running!", null, true));

            refreshChatterListTimer.Elapsed += (s, e) =>
            {
                foreach (var channel in Channels)
                {
                    channel.fetchUsernames();
                }
            };
            refreshChatterListTimer.Start();

            UpdateIsLiveTimer.Elapsed += UpdateIsLiveTimerElapsed;
            UpdateIsLiveTimer.Start();
            UpdateIsLiveTimerElapsed(null, null);
        }

        private void checkIfIsLive()
        {
            Task.Run(() =>
            {
                try
                {
                    var req =
                        WebRequest.Create(
                            $"https://api.twitch.tv/kraken/streams/{Name}?client_id={IrcManager.DefaultClientID}");
                    if (AppSettings.IgnoreSystemProxy)
                    {
                        req.Proxy = null;
                    }
                    using (var res = req.GetResponse())
                    using (var resStream = res.GetResponseStream())
                    {
                        var parser = new JsonParser();

                        dynamic json = parser.Parse(resStream);

                        var tmpIsLive = IsLive;

                        IsLive = json["stream"] != null;

                        if (!IsLive)
                        {
                            StreamViewerCount = 0;
                            StreamStatus = null;
                            StreamGame = null;

                            if (tmpIsLive)
                            {
                                LiveStatusUpdated?.Invoke(this, EventArgs.Empty);
                            }
                        }
                        else
                        {
                            dynamic stream = json["stream"];
                            dynamic channel = stream["channel"];

                            StreamViewerCount = int.Parse(stream["viewers"]);
                            StreamStatus = channel["status"];
                            StreamGame = channel["game"];
                            StreamStart = DateTime.Parse(stream["created_at"]);

                            LiveStatusUpdated?.Invoke(this, EventArgs.Empty);
                        }
                    }
                }
                catch
                {

                }
            });
        }

        private static void UpdateIsLiveTimerElapsed(object sender, ElapsedEventArgs args)
        {
            foreach (var channel in Channels)
            {
                channel.checkIfIsLive();
            }
        }


        private static readonly System.Timers.Timer UpdateIsLiveTimer = new System.Timers.Timer(1000 * 60);

        private static readonly ConcurrentDictionary<string, TwitchChannel> channels = new ConcurrentDictionary<string, TwitchChannel>();
        public static IEnumerable<TwitchChannel> Channels { get { return channels.Values; } }

        public static TwitchChannel WhisperChannel { get; private set; } = new TwitchChannel("/whispers");
        public static TwitchChannel MentionsChannel { get; private set; } = new TwitchChannel("/mentions");

        public static TwitchChannel AddChannel(string channelName)
        {
            if (channelName.StartsWith("/"))
            {
                switch (channelName)
                {
                    case "/whispers":
                        return WhisperChannel;

                    case "/mentions":
                        return MentionsChannel;
                }
            }

            return channels.AddOrUpdate((channelName ?? "").ToLower(), cname => new TwitchChannel(cname) { Uses = 1 }, (cname, c) => { c.Uses++; return c; });
        }

        public static void RemoveChannel(string channelName)
        {
            if (channelName == null)
                return;

            channelName = channelName.ToLower();

            TwitchChannel data;
            if (channels.TryGetValue(channelName ?? "", out data))
            {
                data.Uses--;
                if (data.Uses <= 0)
                {
                    data.Disconnect();
                    data.Dispose();
                    channels.TryRemove(channelName ?? "", out data);
                }
            }
        }

        public static TwitchChannel GetChannel(string channelName)
        {
            channelName = channelName.ToLower();

            TwitchChannel data;
            if (channels.TryGetValue(channelName ?? "", out data))
                return data;

            return null;
        }


        // LazyLoadedImage + Name Autocompletion
        public ConcurrentDictionary<string, string> Users = new ConcurrentDictionary<string, string>();

        List<KeyValuePair<string, string>> emoteNames = new List<KeyValuePair<string, string>>();

        void updateEmoteNameList()
        {
            var names = new List<KeyValuePair<string, string>>();

            names.AddRange(Emotes.TwitchEmotes.Keys.Select(x => new KeyValuePair<string, string>(x.ToUpper(), x)));
            names.AddRange(Emotes.BttvGlobalEmotes.Keys.Select(x => new KeyValuePair<string, string>(x.ToUpper(), x)));
            names.AddRange(Emotes.FfzGlobalEmotes.Keys.Select(x => new KeyValuePair<string, string>(x.ToUpper(), x)));
            names.AddRange(BttvChannelEmotes.Keys.Select(x => new KeyValuePair<string, string>(x.ToUpper(), x)));
            names.AddRange(FfzChannelEmotes.Keys.Select(x => new KeyValuePair<string, string>(x.ToUpper(), x)));
            names.AddRange(Emojis.ShortCodeToEmoji.Keys.Select(x => new KeyValuePair<string, string>(":" + x.ToUpper() + ":", ":" + x + ":")));

            emoteNames = names;
        }

        public IEnumerable<KeyValuePair<string, string>> GetCompletionItems(bool firstWord, bool allowAt)
        {
            var usernames = new List<KeyValuePair<string, string>>();

            var commaAtEnd = firstWord ? "," : "";

            if (AppSettings.ChatMentionUsersWithAt)
            {
                usernames.AddRange(Users.Select(x => new KeyValuePair<string, string>(x.Key, (allowAt ? "@" : "") + (!AppSettings.ChatTabLocalizedNames && !string.Equals(x.Value, x.Key, StringComparison.OrdinalIgnoreCase) ? x.Key.ToLower() : x.Value) + commaAtEnd)));
                usernames.AddRange(Users.Select(x => new KeyValuePair<string, string>((allowAt ? "@" : "") + x.Key, (allowAt ? "@" : "") + (!AppSettings.ChatTabLocalizedNames && !string.Equals(x.Value, x.Key, StringComparison.OrdinalIgnoreCase) ? x.Key.ToLower() : x.Value) + commaAtEnd)));
            }
            else
            {
                usernames.AddRange(Users.Select(x => new KeyValuePair<string, string>(x.Key, (!AppSettings.ChatTabLocalizedNames && !string.Equals(x.Value, x.Key, StringComparison.OrdinalIgnoreCase) ? x.Key.ToLower() : x.Value) + commaAtEnd)));
                if (allowAt)
                    usernames.AddRange(Users.Select(x => new KeyValuePair<string, string>("@" + x.Key, "@" + (!AppSettings.ChatTabLocalizedNames && !string.Equals(x.Value, x.Key, StringComparison.OrdinalIgnoreCase) ? x.Key.ToLower() : x.Value) + commaAtEnd)));
            }

            lock (Commands.CustomCommandsLock)
            {
                usernames.AddRange(Commands.CustomCommands.Select(x => new KeyValuePair<string, string>("/" + x.Name.ToUpper(), "/" + x.Name)));
            }

            lock (Util.TwitchChatCommandNames)
            {
                usernames.AddRange(Util.TwitchChatCommandNames.Select(x => new KeyValuePair<string, string>(x.ToUpper(), x)));
            }

            if (AppSettings.PrefereEmotesOverUsernames)
            {
                usernames.Sort((x1, x2) => string.Compare(x1.Value, x2.Value));

                var emotes = new List<KeyValuePair<string, string>>(emoteNames);

                emotes.Sort((x1, x2) => string.Compare(x1.Value, x2.Value));

                emotes.AddRange(usernames);

                return emotes;
            }
            else
            {
                usernames.AddRange(emoteNames);

                usernames.Sort((x1, x2) => string.Compare(x1.Value, x2.Value));

                return usernames;
            }
        }

        public void Join()
        {
            IrcManager.Client?.Join("#" + Name);
        }

        public void Disconnect()
        {
            IrcManager.Client?.Part("#" + Name);
        }

        //static readonly Random ColorRandom = new Random();
        private static float usernameHue = 0;
        public void SendMessage(string text)
        {
            if (Name == null)
            {
                if (text.StartsWith("/w "))
                {
                    var channel = AllChannels.FirstOrDefault();

                    if (channel != null)
                    {
                        //IrcManager.Client.Say(text, "fivetf", true);
                        IrcManager.SendMessage(channel, text, IsModOrBroadcaster);
                    }
                }

                return;
            }

            IrcManager.SendMessage(this, text, IsModOrBroadcaster);

            if (AppSettings.Rainbow)
            {
                usernameHue += 0.1f;

                float r, g, b;
                (new HSLColor(usernameHue % 1, 0.5f, 0.5f)).ToRGB(out r, out g, out b);

                IrcManager.SendMessage(this, $".color #{(int)(r * 255):X}{(int)(g * 255):X}{(int)(b * 255):X}", IsModOrBroadcaster);
            }
        }

        void fetchUsernames()
        {
            try
            {
                var request = WebRequest.Create($"http://tmi.twitch.tv/group/user/{Name}/chatters");
                if (AppSettings.IgnoreSystemProxy)
                {
                    request.Proxy = null;
                }
                using (var response = request.GetResponse())
                using (var stream = response.GetResponseStream())
                {
                    var parser = new JsonParser();
                    dynamic json = parser.Parse(stream);
                    dynamic chatters = json["chatters"];

                    Users.Clear();

                    foreach (var group in chatters)
                    {
                        foreach (string user in group.Value)
                        {
                            Users[user.ToUpper()] = user;
                        }
                    }
                }
            }
            catch { }
        }

        // Messages
        public event EventHandler<ChatClearedEventArgs> ChatCleared;
        public event EventHandler<MessageAddedEventArgs> MessageAdded;
        public event EventHandler<ValueEventArgs<Message[]>> MessagesAddedAtStart;
        public event EventHandler<ValueEventArgs<Message[]>> MessagesRemovedAtStart;

        public int MessageCount { get; private set; } = 0;

        private Message[] _messages = new Message[0];

        public Message[] Messages
        {
            get { return _messages; }
            set { _messages = value; }
        }

        public Message[] CloneMessages()
        {
            Message[] M;
            lock (MessageLock)
            {
                M = new Message[_messages.Length];
                Array.Copy(_messages, M, M.Length);
            }
            return M;
        }

        public object MessageLock { get; private set; } = new object();

        public static IEnumerable<TwitchChannel> AllChannels => Channels.Concat(new[] { WhisperChannel, MentionsChannel });

        public void ClearChat(bool removeMessages)
        {
            if (removeMessages)
            {
                var _messages = Messages;

                lock (MessageLock)
                {
                    Messages = new Message[0];
                }

                MessagesRemovedAtStart?.Invoke(this, new ValueEventArgs<Message[]>(_messages));
            }
            else
            {
                lock (MessageLock)
                {
                    foreach (var message in Messages)
                    {
                        message.Disabled = true;
                    }
                }

                AddMessage(new Message("Chat has been cleared by a moderator.", HSLColor.Gray, true));
            }
        }

        public void ClearChat(string user, string reason, int duration)
        {
            if (string.IsNullOrWhiteSpace(user))
            {
                ClearChat(false);
                return;
            }

            Monitor.Enter(MessageLock);

            foreach (var msg in Messages)
            {
                if (!msg.HasAnyHighlightType(HighlightType.Whisper) && msg.Username == user)
                {
                    msg.Disabled = true;
                }
            }

            for (var i = Messages.Length - 1; i >= 0; i--)
            {
                var m = Messages[i];

                if (m.ParseTime > DateTime.Now - TimeSpan.FromSeconds(30))
                {
                    if (m.TimeoutUser != user) continue;

                    Messages[i] =
                        new Message(
                            $"{user} was timed out for {duration} second{(duration != 1 ? "s" : "")}: \"{reason}\" (multiple times)",
                            HSLColor.Gray, true)
                        { TimeoutUser = user, Id = Messages[i].Id };
                    Monitor.Exit(MessageLock);

                    ChatCleared?.Invoke(this, new ChatClearedEventArgs(user, reason, duration));
                    return;
                }

                break;
            }

            Monitor.Exit(MessageLock);

            AddMessage(
                new Message($"{user} was timed out for {duration} second{(duration != 1 ? "s" : "")}: \"{reason}\"",
                    HSLColor.Gray, true)
                { TimeoutUser = user });

            ChatCleared?.Invoke(this, new ChatClearedEventArgs(user, reason, duration));
        }

        public void AddMessage(Message message)
        {
            Message[] M;
            Message removedMessage = null;

            lock (MessageLock)
            {
                if (Messages.Length == maxMessages)
                {
                    removedMessage = Messages[0];
                    M = new Message[maxMessages];
                    Array.Copy(Messages, 1, M, 0, Messages.Length - 1);
                }
                else
                {
                    M = new Message[Messages.Length + 1];
                    Array.Copy(Messages, M, Messages.Length);
                }

                M[M.Length - 1] = message;
                Messages = M;
                MessageCount = M.Length;
            }

            MessageAdded?.Invoke(this, new MessageAddedEventArgs(message, removedMessage));
        }

        public void AddMessagesAtStart(Message[] messages)
        {
            Message[] M;

            lock (MessageLock)
            {
                if (Messages.Length == maxMessages)
                    return;

                if (messages.Length + Messages.Length <= maxMessages)
                {
                    M = new Message[messages.Length + Messages.Length];

                    Array.Copy(Messages, 0, M, messages.Length, Messages.Length);
                    Array.Copy(messages, 0, M, 0, messages.Length);
                }
                else
                {
                    M = new Message[maxMessages];

                    Array.Copy(Messages, 0, M, maxMessages - Messages.Length, Messages.Length);

                    var _messages = new Message[maxMessages - Messages.Length];

                    Array.Copy(messages, messages.Length - maxMessages + Messages.Length, M, 0, maxMessages - Messages.Length);
                    Array.Copy(messages, messages.Length - maxMessages + Messages.Length, _messages, 0, maxMessages - Messages.Length);

                    messages = _messages;
                }
                Messages = M;
                MessageCount = M.Length;
            }

            MessagesAddedAtStart?.Invoke(this, new ValueEventArgs<Message[]>(messages));
        }

        public void Dispose()
        {
            Emotes.EmotesLoaded -= Emotes_EmotesLoaded;
            IrcManager.Connected -= IrcManager_Connected;
            IrcManager.Disconnected -= IrcManager_Disconnected;
            IrcManager.NoticeAdded -= IrcManager_NoticeAdded;
            AppSettings.MessageLimitChanged -= AppSettings_MessageLimitChanged;
        }

        public void ReloadEmotes()
        {
            var channelName = Name;

            var bttvChannelEmotesCache = Path.Combine(Util.GetUserDataPath(), "Cache", $"bttv_channel_{channelName}");
            var ffzChannelEmotesCache = Path.Combine(Util.GetUserDataPath(), "Cache", $"ffz_channel_{channelName}");

            // bttv channel emotes
            Task.Run(() =>
            {
                try
                {
                    var parser = new JsonParser();

                    //if (!File.Exists(bttvChannelEmotesCache))
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

                        BttvChannelEmotes.Clear();

                        foreach (var e in json["emotes"])
                        {
                            string id = e["id"];
                            string code = e["code"];
                            string channel = e["channel"];

                            LazyLoadedImage emote;
                            if (Emotes.BttvChannelEmotesCache.TryGetValue(id, out emote))
                            {
                                BttvChannelEmotes[code] = emote;
                            }
                            else
                            {
                                string imageType = e["imageType"];
                                string url = template.Replace("{{id}}", id);

                                double scale;
                                url = Emotes.GetBttvEmoteLink(url, out scale);

                                Emotes.BttvChannelEmotesCache[id] =
                                    BttvChannelEmotes[code] =
                                        new LazyLoadedImage
                                        {
                                            Name = code,
                                            Url = url,
                                            Tooltip = code + "\nBetterTTV Channel Emote\nChannel: " + channel,
                                            Scale = scale,
                                            IsEmote = true
                                        };
                            }
                        }
                    }
                    updateEmoteNameList();
                }
                catch { }
            });

            // ffz channel emotes
            Task.Run(() =>
            {
                try
                {
                    var parser = new JsonParser();

                    //if (!File.Exists(ffzChannelEmotesCache))
                    {
                        try
                        {
                            if (Util.IsLinux)
                            {
                                Util.LinuxDownloadFile("http://api.frankerfacez.com/v1/room/" + channelName, ffzChannelEmotesCache);
                            }
                            else
                            {
                                using (var webClient = new WebClient())
                                using (var readStream = webClient.OpenRead("http://api.frankerfacez.com/v1/room/" + channelName))
                                using (var writeStream = File.OpenWrite(ffzChannelEmotesCache))
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

                    using (var stream = File.OpenRead(ffzChannelEmotesCache))
                    {
                        dynamic json = parser.Parse(stream);

                        dynamic room = json["room"];

                        try
                        {
                            object moderator;

                            if (room.TryGetValue("moderator_badge", out moderator))
                            {
                                if (moderator != null && !string.IsNullOrWhiteSpace((string)moderator))
                                {
                                    var url = "https:" + (moderator as string);
                                    ModeratorBadge = new LazyLoadedImage
                                    {
                                        Url = url,
                                        Tooltip = "custom moderator badge\nFFZ",
                                        LoadAction = () =>
                                        {
                                            try
                                            {
                                                object img;

                                                var request = WebRequest.Create(url);
                                                if (AppSettings.IgnoreSystemProxy)
                                                {
                                                    request.Proxy = null;
                                                }
                                                using (var response = request.GetResponse())
                                                using (var s = response.GetResponseStream())
                                                {
                                                    img = GuiEngine.Current.ReadImageFromStream(s);
                                                }

                                                GuiEngine.Current.FreezeImage(img);

                                                return GuiEngine.Current.DrawImageBackground(img, HSLColor.FromRGB(0x45A41E));
                                            }
                                            catch
                                            {
                                                return null;
                                            }
                                        }
                                    };
                                }
                            }
                        }
                        catch { }

                        dynamic sets = json["sets"];

                        FfzChannelEmotes.Clear();

                        foreach (var set in sets.Values)
                        {
                            string title = set["title"];

                            dynamic emoticons = set["emoticons"];

                            foreach (LazyLoadedImage emote in Emotes.GetFfzEmoteFromDynamic(emoticons, false))
                            {
                                FfzChannelEmotes[emote.Name] = emote;
                            }
                        }
                    }
                    updateEmoteNameList();
                }
                catch { }
            });
        }
    }
}
