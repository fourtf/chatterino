using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TwitchIrc;

namespace Chatterino.Common
{
    public class Message
    {
        public static bool EnablePings { get; set; } = true;

        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;
        public int TotalY { get; set; }
        public int Height { get; private set; }
        public int Width { get; set; } = 0;

        public bool Disabled { get; set; } = false;
        public bool Highlighted { get; set; } = false;
        public bool EmoteBoundsChanged { get; set; } = true;

        public string Username { get; set; }
        public string DisplayName { get; set; }
        public HSLColor UsernameColor { get; set; }

        public MessageBadges Badges { get; set; }

        private bool isVisible = false;

        public bool IsVisible
        {
            get { return isVisible; }
            set
            {
                isVisible = value;
                if (!value && buffer != null)
                {
                    var b = buffer;
                    buffer = null;
                    GuiEngine.Current.DisposeMessageGraphicsBuffer(this);
                }
            }
        }

        public object buffer = null;

        public string RawMessage { get; private set; }
        public List<Word> Words { get; set; }
        public TwitchChannel Channel { get; set; }

        Regex linkRegex = new Regex(@"^((?<Protocol>\w+):\/\/)?(?<Domain>[\w%@-][\w.%-:@]+\w)\/?[\w\.?=#%&=\+\-@/$,]*$");
        static char[] linkIdentifiers = new char[] { '.', ':' };

        private Message()
        {

        }

        public Message(IrcMessage data, TwitchChannel channel, bool enableTimestamp = true, bool enablePingSound = true)
        {
            //var w = Stopwatch.StartNew();

            Channel = channel;

            List<Word> words = new List<Word>();

            string text = data.Params ?? "";

            Username = data.PrefixNickname;

            bool slashMe = false;

            // Handle /me messages
            if (text.Length > 8 && text.StartsWith("\u0001ACTION "))
            {
                text = text.Substring("\u0001ACTION ".Length, text.Length - "\u0001ACTION ".Length - 1);
                slashMe = true;
            }

            // Highlights
            if ((AppSettings.ChatEnableHighlight || AppSettings.ChatEnableHighlightSound || AppSettings.ChatEnableHighlightTaskbar) && Username != IrcManager.Username.ToLower())
            {
                if (!AppSettings.HighlightIgnoredUsers.ContainsKey(Username))
                {
                    if (AppSettings.CustomHighlightRegex != null && AppSettings.CustomHighlightRegex.IsMatch(text))
                    {
                        if (AppSettings.ChatEnableHighlight)
                            Highlighted = true;
                        if (EnablePings && enablePingSound)
                        {
                            if (AppSettings.ChatEnableHighlightSound)
                                GuiEngine.Current.PlaySound(NotificationSound.Ping);
                            if (AppSettings.ChatEnableHighlightTaskbar)
                                GuiEngine.Current.FlashTaskbar();
                        }
                    }
                }
            }

            // Tags
            string value;
            if (data.Tags.TryGetValue("color", out value))
            {
                try
                {
                    if (value.Length == 7 && value[0] == '#')
                    {
                        UsernameColor = HSLColor.FromRGB(Convert.ToInt32(value.Substring(1), 16));
                    }
                }
                catch { }
            }
            if (data.Tags.TryGetValue("display-name", out value))
            {
                DisplayName = value;
            }

            // Bits
            string bits = null;
            string bitsLink = null;
            HSLColor? bitsColor = null;

            if (data.Tags.TryGetValue("bits", out bits))
            {
                int cheer;

                if (int.TryParse(bits, out cheer))
                {
                    string color = null;

                    if (cheer >= 10000)
                    {
                        color = "red";
                        bitsColor = new HSLColor(0, 1f, 0.5f);
                    }
                    else if (cheer >= 5000)
                    {
                        color = "blue";
                        bitsColor = new HSLColor(0.61f, 1f, 0.4f);
                    }
                    else if (cheer >= 1000)
                    {
                        color = "green";
                        bitsColor = new HSLColor(0.5f, 1f, 0.5f);
                    }
                    else if (cheer >= 100)
                    {
                        color = "purple";
                        bitsColor = new HSLColor(0.8f, 1f, 0.5f);
                    }
                    else
                    {
                        color = "gray";
                        bitsColor = HSLColor.FromRGB(0.5f, 0.5f, 0.5f);
                    }

                    bits = "cheer" + bits;
                    bitsLink = $"http://static-cdn.jtvnw.net/bits/{(GuiEngine.Current.IsDarkTheme ? "dark" : "light")}/animated/{color}/1";
                }
                else
                {
                    bits = null;
                }
            }

            // Add timestamp
            string timestampTag;
            string timestamp = null;

            if (data.Tags.TryGetValue("timestamp-utc", out timestampTag))
            {
                DateTime time;
                if (DateTime.TryParseExact(timestampTag, "yyyyMMdd-HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out time))
                {
                    timestamp = time.ToString(AppSettings.ChatShowTimestampSeconds ? "HH:mm:ss" : "HH:mm");
                    enableTimestamp = true;
                }
            }

            if (enableTimestamp && AppSettings.ChatShowTimestamps)
            {
                timestamp = timestamp ?? DateTime.Now.ToString(AppSettings.ChatShowTimestampSeconds ? "HH:mm:ss" : "HH:mm");

                words.Add(new Word
                {
                    Type = SpanType.Text,
                    Value = timestamp,
                    Color = HSLColor.FromRGB(-8355712),
                    Font = FontType.Small,
                    CopyText = timestamp
                });
            }

            if (data.Tags.TryGetValue("badges", out value))
            {
                var badges = value.Split(',');

                foreach (var badge in badges)
                {
                    if (badge.StartsWith("bits/"))
                    {
                        int cheer;
                        if (int.TryParse(badge.Substring("bits/".Length), out cheer))
                        {
                            object image;

                            if (cheer >= 100000)
                            {
                                image = GuiEngine.Current.GetImage(ImageType.Cheer100000);
                            }
                            else if (cheer >= 10000)
                            {
                                image = GuiEngine.Current.GetImage(ImageType.Cheer10000);
                            }
                            else if (cheer >= 5000)
                            {
                                image = GuiEngine.Current.GetImage(ImageType.Cheer5000);
                            }
                            else if (cheer >= 1000)
                            {
                                image = GuiEngine.Current.GetImage(ImageType.Cheer1000);
                            }
                            else if (cheer >= 100)
                            {
                                image = GuiEngine.Current.GetImage(ImageType.Cheer100);
                            }
                            else
                            {
                                image = GuiEngine.Current.GetImage(ImageType.Cheer1);
                            }

                            words.Add(new Word { Type = SpanType.Image, Value = image, Tooltip = "Twitch Cheer " + cheer });
                        }
                    }
                    else
                    {
                        switch (badge)
                        {
                            case "staff/1":
                                Badges |= MessageBadges.Staff;
                                words.Add(new Word { Type = SpanType.Image, Value = GuiEngine.Current.GetImage(ImageType.BadgeStaff), Tooltip = "Twitch Staff" });
                                break;
                            case "admin/1":
                                Badges |= MessageBadges.Admin;
                                words.Add(new Word { Type = SpanType.Image, Value = GuiEngine.Current.GetImage(ImageType.BadgeAdmin), Tooltip = "Twitch Admin" });
                                break;
                            case "global_mod/1":
                                Badges |= MessageBadges.GlobalMod;
                                words.Add(new Word { Type = SpanType.Image, Value = GuiEngine.Current.GetImage(ImageType.BadgeGlobalmod), Tooltip = "Global Moderator" });
                                break;
                            case "moderator/1":
                                Badges |= MessageBadges.Mod;
                                words.Add(new Word { Type = SpanType.Image, Value = GuiEngine.Current.GetImage(ImageType.BadgeModerator), Tooltip = "Channel Moderator" });
                                break;
                            case "subscriber/1":
                                Badges |= MessageBadges.Sub;
                                words.Add(new Word { Type = SpanType.Emote, Value = channel.SubscriberBadge, Link = Channel.SubLink, Tooltip = "Channel Subscriber" });
                                break;
                            case "turbo/1":
                                Badges |= MessageBadges.Turbo;
                                words.Add(new Word { Type = SpanType.Image, Value = GuiEngine.Current.GetImage(ImageType.BadgeTurbo), Tooltip = "Turbo Subscriber" });
                                break;
                            case "broadcaster/1":
                                Badges |= MessageBadges.Broadcaster;
                                words.Add(new Word { Type = SpanType.Image, Value = GuiEngine.Current.GetImage(ImageType.BadgeBroadcaster), Tooltip = "Channel Broadcaster" });
                                break;
                        }
                    }
                }
            }

            TwitchEmote fourtfBadge;
            if (Common.Badges.FourtfGlobalBadges.TryGetValue(Username, out fourtfBadge))
                words.Add(new Word { Type = SpanType.Emote, Value = fourtfBadge, Tooltip = fourtfBadge.Tooltip });


            // Username
            if (string.IsNullOrWhiteSpace(DisplayName))
            {
                DisplayName = Username;
            }
            else
            {
                if (DisplayName.ToLower() != Username)
                {
                    DisplayName = DisplayName + " (" + Username + ")";
                }
            }

            var messageUser = DisplayName + (slashMe ? "" : ":");
            words.Add(new Word
            {
                Type = SpanType.Text,
                Value = messageUser,
                Color = UsernameColor,
                Font = FontType.MediumBold,
                Link = "https://twitch.tv/" + Username,
                //Link = "@twitchuser:" + Username,
                CopyText = messageUser
            });

            List<Tuple<int, TwitchEmote>> twitchEmotes = new List<Tuple<int, TwitchEmote>>();

            // Twitch Emotes
            if (data.Tags.TryGetValue("emotes", out value))
            {
                //  93064:0-6,8-14/80481:16-20,22-26
                value.Split('/').Do(emote =>
                {
                    if (emote != "")
                    {
                        var x = emote.Split(':');
                        var id = int.Parse(x[0]);
                        foreach (var y in x[1].Split(','))
                        {
                            var coords = y.Split('-');
                            int index = int.Parse(coords[0]);
                            string name = text.Substring(index, int.Parse(coords[1]) - index + 1);

                            // ignore ignored emotes
                            if (!AppSettings.ChatIgnoredEmotes.ContainsKey(name))
                            {
                                TwitchEmote e;
                                if (!Emotes.TwitchEmotesByIDCache.TryGetValue(id, out e))
                                {
                                    e = new TwitchEmote
                                    {
                                        Name = name,
                                        Url = Emotes.TwitchEmoteTemplate.Replace("{id}", id.ToString()),
                                        Tooltip = name + "\nTwitch Emote"
                                    };
                                    Emotes.TwitchEmotesByIDCache[id] = e;
                                }
                                twitchEmotes.Add(Tuple.Create(index, e));
                            }
                        };
                    }
                });
                twitchEmotes.Sort((e1, e2) => e1.Item1.CompareTo(e2.Item1));
            }

            //if (data.Tags.TryGetValue("id", out value))
            //{

            //}
            //if (data.Tags.TryGetValue("mod", out value))
            //{

            //}
            //if (data.Tags.TryGetValue("subscriber", out value))
            //{
            //    if (value == "1")
            //        Badges |= MessageBadges.Sub;
            //}
            //if (data.Tags.TryGetValue("turbo", out value))
            //{
            //    if (value == "1")
            //        Badges |= MessageBadges.Turbo;
            //}

            int i = 0;
            int currentTwitchEmoteIndex = 0;
            Tuple<int, TwitchEmote> currentTwitchEmote = twitchEmotes.FirstOrDefault();

            foreach (var split in text.Split(' '))
            {
                if (currentTwitchEmote != null)
                {
                    if (currentTwitchEmote.Item1 == i)
                    {
                        words.Add(new Word
                        {
                            Type = SpanType.Emote,
                            Value = currentTwitchEmote.Item2,
                            Link = currentTwitchEmote.Item2.Url,
                            Tooltip = currentTwitchEmote.Item2.Tooltip,
                            CopyText = currentTwitchEmote.Item2.Name
                        });
                        i += split.Length + 1;
                        currentTwitchEmoteIndex++;
                        currentTwitchEmote = currentTwitchEmoteIndex == twitchEmotes.Count ? null : twitchEmotes[currentTwitchEmoteIndex];
                        continue;
                    }
                }

                foreach (object o in Emojis.ParseEmojis(split))
                {
                    string s = o as string;

                    if (s != null)
                    {
                        //foreach (var match in Regex.Matches(@"\b\w+\b", s))
                        //{
                        //    TwitchEmote bttvEmote;
                        //    if (AppSettings.ChatEnableBttvEmotes && (Emotes.BttvGlobalEmotes.TryGetValue(s, out bttvEmote) || channel.BttvChannelEmotes.TryGetValue(s, out bttvEmote))
                        //        || (AppSettings.ChatEnableFfzEmotes && Emotes.FfzGlobalEmotes.TryGetValue(s, out bttvEmote)))
                        //    {
                        //        words.Add(new Word
                        //        {
                        //            Type = SpanType.Emote,
                        //            Value = bttvEmote,
                        //            Color = slashMe ? UsernameColor : new int?(),
                        //            Tooltip = bttvEmote.Tooltip,
                        //            Link = bttvEmote.Url,
                        //            CopyText = bttvEmote.Name
                        //        });
                        //    }
                        //}

                        if (bits != null && s == bits)
                        {
                            words.Add(new Word { Type = SpanType.Emote, Value = Emotes.MiscEmotesByUrl.GetOrAdd(bitsLink, url => new TwitchEmote { Name = "cheer", Url = url, Tooltip = "Twitch Bits Badge" }), Tooltip = "Twitch Bits Donation", CopyText = bits, Link = "https://blog.twitch.tv/introducing-cheering-celebrate-together-da62af41fac6" });
                            words.Add(new Word { Type = SpanType.Text, Value = "x" + bits.Substring(5), Font = FontType.Small, Color = bitsColor });

                            continue;
                        }

                        TwitchEmote bttvEmote;
                        if (!AppSettings.ChatIgnoredEmotes.ContainsKey(s) && (AppSettings.ChatEnableBttvEmotes && (Emotes.BttvGlobalEmotes.TryGetValue(s, out bttvEmote) || channel.BttvChannelEmotes.TryGetValue(s, out bttvEmote))
                            || (AppSettings.ChatEnableFfzEmotes && Emotes.FfzGlobalEmotes.TryGetValue(s, out bttvEmote))))
                        {
                            words.Add(new Word
                            {
                                Type = SpanType.Emote,
                                Value = bttvEmote,
                                Color = slashMe ? UsernameColor : new HSLColor?(),
                                Tooltip = bttvEmote.Tooltip,
                                Link = bttvEmote.Url,
                                CopyText = bttvEmote.Name
                            });
                        }
                        else
                        {
                            string link = null;

                            if (split.IndexOfAny(linkIdentifiers) != -1)
                            {
                                Match m = linkRegex.Match(split);

                                if (m.Success)
                                {
                                    link = m.Value;

                                    if (!m.Groups["Protocol"].Success)
                                        link = "http://" + link;

                                    if (!m.Groups["Protocol"].Success || m.Groups["Protocol"].Value.ToUpper() == "HTTP" || m.Groups["Protocol"].Value.ToUpper() == "HTTPS")
                                    {
                                        if (m.Groups["Domain"].Value.IndexOf('.') == -1)
                                            link = null;
                                    }
                                }
                            }

                            words.Add(new Word
                            {
                                Type = SpanType.Text,
                                Value = s,
                                Color = slashMe ? UsernameColor : (link == null ? new HSLColor?() : HSLColor.FromRGB(-8355712)),
                                Link = link,
                                CopyText = s
                            });
                        }
                    }
                    else
                    {
                        TwitchEmote e = o as TwitchEmote;

                        if (e != null)
                        {
                            words.Add(new Word
                            {
                                Type = SpanType.Emote,
                                Value = e,
                                Link = e.Url,
                                Tooltip = e.Tooltip,
                                CopyText = e.Name
                            });
                        }
                    }
                }

                int splitLength = 0;
                for (int j = 0; j < split.Length; j++)
                {
                    splitLength++;

                    if (char.IsHighSurrogate(split[j]))
                        j += 1;
                }

                i += splitLength + 1;
            }

            Words = words;

            RawMessage = text;

            //w.Stop();
            //Console.WriteLine("Message parsed in " + w.Elapsed.TotalSeconds.ToString("0.000000") + " seconds");
        }

        public Message(string text)
            : this(text, null, false)
        {

        }

        public Message(string text, HSLColor? color, bool addTimeStamp)
        {
            RawMessage = text;
            Words = new List<Word>();

            // Add timestamp
            if (addTimeStamp && AppSettings.ChatShowTimestamps)
            {
                var timestamp = DateTime.Now.ToString(AppSettings.ChatShowTimestampSeconds ? "HH:mm:ss" : "HH:mm");

                Words.Add(new Word
                {
                    Type = SpanType.Text,
                    Value = timestamp,
                    Color = color ?? HSLColor.FromRGB(-8355712),
                    Font = FontType.Small,
                    CopyText = timestamp
                });
            }

            Words.AddRange(text.Split(' ').Select(x => new Word { Type = SpanType.Text, Value = x, Color = color, CopyText = x }));
        }

        public static Message[] ParseMD(string md)
        {
            List<Message> list = new List<Message>();

            using (StringReader reader = new StringReader(md))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    Message msg = new Message();

                    FontType font = FontType.Medium;

                    // Heading
                    var headerMatch = Regex.Match(line, "^(#{1,6}) ");
                    if (headerMatch.Success)
                    {
                        if (headerMatch.Length == 2)
                            font = FontType.VeryLarge;
                        else
                            font = FontType.Large;

                        line = line.Substring(headerMatch.Length);
                    }

                    // Lists
                    if (line.Length > 2 && line[0] == '-' && line[1] == ' ')
                    {
                        line = " • " + line.Substring(2);
                    }

                    // Add words
                    msg.Words = line.Split(' ').Select(x => new Word { Type = SpanType.Text, Font = font, Value = x, CopyText = x }).ToList();

                    list.Add(msg);
                }
            }

            return list.ToArray();
        }

        public void InvalidateTextMeasurements()
        {
            measureText = true;
        }

        bool measureText = true;
        bool measureImages = true;

        // return true if control needs to be redrawn
        public bool CalculateBounds(object graphics, int width)
        {
            var emotesChanged = EmoteBoundsChanged;
            bool redraw = false;

            if (Width != width)
            {
                Width = width;
                redraw = true;
            }

            // check if any words need to be recalculated
            if (emotesChanged || measureText || measureImages)
            {
                foreach (Word word in Words)
                {
                    if (word.Type == SpanType.Text)
                    {
                        if (measureText)
                        {
                            CommonSize size = GuiEngine.Current.MeasureStringSize(graphics, word.Font, (string)word.Value);
                            word.Width = size.Width;
                            word.Height = size.Height;
                        }
                    }
                    else if (word.Type == SpanType.Image)
                    {
                        //if (measureImages)
                        //{
                        //    if (word.Value == null)
                        //    {
                        //        CommonSize size = GuiEngine.Current.GetImageSize(word.Value);
                        //        word.Width = size.Width;
                        //        word.Height = size.Height;
                        //    }
                        //    else
                        //    {
                        //        word.Width = word.Height = 16;
                        //    }
                        //}
                    }
                    else if (word.Type == SpanType.Emote)
                    {
                        if (emotesChanged || measureImages)
                        {
                            TwitchEmote emote = word.Value as TwitchEmote;
                            object image = emote?.Image;
                            if (image == null)
                            {
                                word.Width = word.Height = 16;
                            }
                            else
                            {
                                CommonSize size = GuiEngine.Current.GetImageSize(image);
                                word.Width = size.Width;
                                word.Height = size.Height;
                            }
                        }
                    }
                }

                measureText = false;
                measureImages = false;

                redraw = true;
            }

            if (redraw)
            {
                int x = 0;
                int y = 4;
                int lineStartIndex = 0;

                int spaceWidth = GuiEngine.Current.MeasureStringSize(graphics, FontType.Medium, " ").Width;

                int i = 0;

                Func<int> fixCurrentLineHeight = () =>
                {
                    int lineHeight = 0;

                    for (int j = lineStartIndex; j < i; j++)
                    {
                        int h = Words[j].Height;
                        lineHeight = h > lineHeight ? h : lineHeight;
                    }

                    for (int j = lineStartIndex; j < i; j++)
                    {
                        var word = Words[j];
                        if (j == lineStartIndex && word.Type == SpanType.Text && word.SplitSegments != null)
                        {
                            var segment = word.SplitSegments[word.SplitSegments.Length - 1];
                            CommonRectangle rec = segment.Item2;
                            word.SplitSegments[word.SplitSegments.Length - 1] = Tuple.Create(segment.Item1, new CommonRectangle(rec.X, rec.Y + lineHeight - word.Height, rec.Width, rec.Height));
                        }
                        else
                        {
                            word.Y += lineHeight - word.Height;
                        }
                    }

                    return lineHeight;
                };

                for (; i < Words.Count; i++)
                {
                    Word word = Words[i];

                    word.SplitSegments = null;

                    // word wrapped text
                    if (word.Width > width && word.Type == SpanType.Text && ((string)word.Value).Length > 2)
                    {
                        y += fixCurrentLineHeight();

                        lineStartIndex = i;

                        word.X = 0;
                        word.Y = y;

                        string text = (string)word.Value;
                        int startIndex = 0;
                        List<Tuple<string, CommonRectangle>> items = new List<Tuple<string, CommonRectangle>>();

                        string s;

                        int[] widths = word.CharacterWidths;

                        // calculate word widths
                        if (widths == null)
                        {
                            widths = new int[text.Length];

                            int lastW = 0;
                            for (int j = 0; j < text.Length; j++)
                            {
                                int w = GuiEngine.Current.MeasureStringSize(graphics, word.Font, text.Remove(j)).Width;
                                widths[j] = w - lastW;
                                lastW = w;
                            }

                            word.CharacterWidths = widths;
                        }

                        // create word splits
                        {
                            int w = widths[0];

                            for (int j = 1; j < text.Length; j++)
                            {
                                w += widths[j];
                                if (w > width - spaceWidth - spaceWidth - spaceWidth)
                                {
                                    items.Add(Tuple.Create(text.Substring(startIndex, j - startIndex), new CommonRectangle(0, y, w, word.Height)));
                                    startIndex = j;
                                    y += word.Height;
                                    w = 0;
                                }
                            }

                            s = text.Substring(startIndex);

                            for (int j = startIndex; j < text.Length; j++)
                            {
                                w += widths[j];
                            }

                            items.Add(Tuple.Create(s, new CommonRectangle(0, y, w, word.Height)));
                        }

                        x = GuiEngine.Current.MeasureStringSize(graphics, word.Font, s).Width + spaceWidth;

                        if (items.Count > 1)
                            word.SplitSegments = items.ToArray();
                    }
                    // word in new line
                    else if (word.Width > width - x)
                    {
                        y += fixCurrentLineHeight();

                        word.X = 0;
                        word.Y = y;

                        x = word.Width + spaceWidth;

                        lineStartIndex = i;
                    }
                    // word fits in current line
                    else
                    {
                        word.X = x;
                        word.Y = y;

                        x += word.Width + spaceWidth;
                    }
                }

                y += fixCurrentLineHeight();
                Height = y + 4;
            }

            if (redraw)
                buffer = null;

            return redraw;
        }

        public Word WordAtPoint(CommonPoint point)
        {
            for (int i = 0; i < Words.Count; i++)
            {
                var word = Words[i];
                if (word.Type == SpanType.Text && word.SplitSegments != null)
                {
                    if (word.SplitSegments.Any(x => x.Item2.Contains(point)))
                        return word;
                }
                else if (word.X < point.X && word.Y < point.Y && word.X + word.Width > point.X && word.Y + word.Height > point.Y)
                {
                    return word;
                }
            }

            return null;
        }

        public MessagePosition MessagePositionAtPoint(object graphics, CommonPoint point, int messageIndex)
        {
            int currentWord = 0;
            int currentChar = 0;
            int currentSplit = 0;

            for (int i = 0; i < Words.Count; i++)
            {
                var word = Words[i];

                if (word.Type == SpanType.Text && word.SplitSegments != null)
                {
                    var splits = word.SplitSegments;

                    for (int j = 0; j < splits.Length; j++)
                    {
                        var split = splits[j];
                        if (point.Y > split.Item2.Y)
                        {
                            if (point.X > split.Item2.X)
                            {
                                currentSplit = j;
                                currentWord = i;
                            }
                        }
                    }
                }
                else
                {
                    if (point.Y > word.Y)
                    {
                        if (point.X > word.X)
                            currentWord = i;
                    }
                }
            }

            var w = Words[currentWord];
            int _currentSplit = 0;

            if (w.Type == SpanType.Image)
            {
                var imageSize = GuiEngine.Current.GetImageSize(w.Value);

                if (point.X - w.X > imageSize.Width)
                    currentChar = 1;
            }
            else if (w.Type == SpanType.Emote)
            {
                var emote = (TwitchEmote)w.Value;

                var imageSize = emote.Image == null ? new CommonSize(16, 16) : GuiEngine.Current.GetImageSize(emote.Image);

                if (point.X - w.X > imageSize.Width)
                    currentChar = 1;
            }
            else if (w.Type == SpanType.Text)
            {
                string text;
                CommonRectangle bounds;

                if (w.SplitSegments == null)
                {
                    text = (string)w.Value;
                    bounds = new CommonRectangle(w.X, w.Y, w.Width, w.Height);
                }
                else
                {
                    text = w.SplitSegments[currentSplit].Item1;
                    _currentSplit = currentSplit;
                    bounds = w.SplitSegments[currentSplit].Item2;
                }

                if (point.X > bounds.X + bounds.Width)
                {
                    currentChar = text.Length;
                }
                else
                {
                    for (int i = text.Length - 1; i >= 1; i--)
                    {
                        string s = text.Remove(i);

                        var size = GuiEngine.Current.MeasureStringSize(graphics, w.Font, s);

                        if (point.X - bounds.X > size.Width)
                        {
                            currentChar = s.Length;
                            break;
                        }
                    }
                }
            }

            return new MessagePosition(messageIndex, currentWord, _currentSplit, currentChar);
        }

        public MessagePosition PositionFromIndex(int index)
        {
            if (index == 0)
                return new MessagePosition(0, 0, 0, 0);

            bool isFirstWord = true;

            int _index = 0;

            int i = 0;
            int j = 0;
            int k = 0;

            for (; i < Words.Count; i++)
            {
                var word = Words[i];

                j = 0;
                for (; j < (word.SplitSegments?.Length ?? 1); j++)
                {
                    var text = word.SplitSegments?[j].Item1 ?? (string)word.Value;

                    if (j == 0)
                        if (isFirstWord)
                            isFirstWord = false;
                        else
                            _index++;

                    k = 0;
                    for (; k < text.Length; k++)
                    {
                        if (_index == index)
                            return new MessagePosition(0, i, j, k);
                        _index++;
                    }
                }

                if (_index == index)
                    return new MessagePosition(0, i, j, k);
            }

            return new MessagePosition(0, i, j, k);
        }

        public int IndexFromPosition(MessagePosition pos)
        {
            bool isFirstWord = true;

            int index = 0;

            for (int i = 0; i < pos.WordIndex; i++)
            {
                var word = Words[i];

                for (int j = 0; j < (word.SplitSegments?.Length ?? 1); j++)
                {
                    var text = word.SplitSegments?[j].Item1 ?? (string)word.Value;

                    if (j == 0)
                        if (isFirstWord)
                            isFirstWord = false;
                        else
                            index++;

                    for (int k = 0; k < text.Length; k++)
                    {
                        if (pos.WordIndex == i && pos.SplitIndex == j && pos.CharIndex == k)
                            return index;
                    }
                }
            }
            return 0;
        }
    }
}
