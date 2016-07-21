using Meebey.SmartIrc4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Chatterino.Common
{
    public class Message
    {
        public int CurrentXOffset { get; set; } = 0;
        public int CurrentYOffset { get; set; } = 0;
        public int Height { get; private set; }
        public int Width { get; set; } = 0;

        public bool Disabled { get; set; } = false;

        public string Username { get; set; }
        public string DisplayName { get; set; }
        public int UsernameColor { get; set; }

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
        public List<Span> Words { get; set; }
        public TwitchChannel Channel { get; set; }

        Regex linkRegex = new Regex(@"^((?<Protocol>\w+):\/\/)?(?<Domain>[\w@][\w.:@]+\w)\/?[\w\.?=#%&=\-@/$,]*$", RegexOptions.Compiled);

        public Message(IrcMessageData data, TwitchChannel channel)
        {
            var w = Stopwatch.StartNew();

            Channel = channel;

            List<Span> words = new List<Span>();

            string text = data.Message;
            Username = data.Nick;

            bool slashMe = false;

            // Handle /me messages
            if (text.Length > 8 && text.StartsWith("\u0001ACTION "))
            {
                text = text.Substring("\u0001ACTION ".Length, text.Length - "\u0001ACTION ".Length - 1);
                slashMe = true;
            }

            // Split the message
            var S = text.Split(' ');

            // Read Tags
            string value;
            if (data.Tags.TryGetValue("color", out value))
            {
                try
                {
                    if (value.Length == 7 && value[0] == '#')
                    {
                        UsernameColor = (-16777216 | Convert.ToInt32(value.Substring(1), 16));
                    }
                }
                catch { }
            }
            if (data.Tags.TryGetValue("display-name", out value))
            {
                DisplayName = value;
            }

            // Add timestamp
            if (AppSettings.ChatShowTimestamps)
            {
                words.Add(new Span
                {
                    Type = SpanType.Text,
                    Value = DateTime.Now.ToString(AppSettings.ChatShowTimestampSeconds ? "HH:mm:ss" : "HH:mm"),
                    Color = -8355712,
                    Font = FontType.Small,
                });
            }

            if (Username.ToUpper() == "FOURTF")
                words.Add(new Span { Type = SpanType.Image, Value = GuiEngine.Current.GetImage(ImageType.BadgeDev) });

            if (data.Tags.TryGetValue("badges", out value))
            {
                var badges = value.Split(',');

                foreach (var badge in badges)
                {
                    switch (badge)
                    {
                        case "staff/1":
                            Badges |= MessageBadges.Staff;
                            words.Add(new Span { Type = SpanType.Image, Value = GuiEngine.Current.GetImage(ImageType.BadgeStaff) });
                            break;
                        case "admin/1":
                            Badges |= MessageBadges.Admin;
                            words.Add(new Span { Type = SpanType.Image, Value = GuiEngine.Current.GetImage(ImageType.BadgeAdmin) });
                            break;
                        case "global_mod/1":
                            Badges |= MessageBadges.GlobalMod;
                            words.Add(new Span { Type = SpanType.Image, Value = GuiEngine.Current.GetImage(ImageType.BadgeGlobalmod) });
                            break;
                        case "moderator/1":
                            Badges |= MessageBadges.Mod;
                            words.Add(new Span { Type = SpanType.Image, Value = GuiEngine.Current.GetImage(ImageType.BadgeModerator) });
                            break;
                        case "subscriber/1":
                            Badges |= MessageBadges.Sub;
                            words.Add(new Span { Type = SpanType.Emote, Value = channel.SubscriberBadge, Link = Channel.SubLink });
                            break;
                        case "turbo/1":
                            Badges |= MessageBadges.Turbo;
                            words.Add(new Span { Type = SpanType.Image, Value = GuiEngine.Current.GetImage(ImageType.BadgeTurbo) });
                            break;
                        case "broadcaster/1":
                            Badges |= MessageBadges.Broadcaster;
                            words.Add(new Span { Type = SpanType.Image, Value = GuiEngine.Current.GetImage(ImageType.BadgeBroadcaster) });
                            break;
                    }
                }
            }

            //  93064:0-6,8-14/80481:16-20,22-26

            DisplayName = DisplayName ?? Username;
            words.Add(new Span { Type = SpanType.Text, Value = DisplayName + (slashMe ? "" : ":"), Color = UsernameColor, Font = FontType.MediumBold });

            List<Tuple<int, TwitchEmote>> twitchEmotes = new List<Tuple<int, TwitchEmote>>();

            if (data.Tags.TryGetValue("emotes", out value))
            {
                value.Split('/').Do(emote =>
                {
                    if (emote != "")
                    {
                        var x = emote.Split(':');
                        var id = int.Parse(x[0]);
                        x[1].Split(',').Do(y =>
                        {
                            var coords = y.Split('-');
                            int index = int.Parse(coords[0]);
                            string name = text.Substring(index, int.Parse(coords[1]) - index);
                            TwitchEmote e;
                            if (!Emotes.TwitchEmotes.TryGetValue(id, out e))
                            {
                                e = new TwitchEmote { Name = name, Url = Emotes.TwitchEmoteTemplate.Replace("{id}", id.ToString()) };
                                Emotes.TwitchEmotes[id] = e;
                            }
                            twitchEmotes.Add(Tuple.Create(index, e));
                        });
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

            foreach (var s in S)
            {
                if (currentTwitchEmote != null)
                {
                    if (currentTwitchEmote.Item1 == i)
                    {
                        words.Add(new Span { Type = SpanType.Emote, Value = currentTwitchEmote.Item2 });
                        i += s.Length + 1;
                        currentTwitchEmoteIndex++;
                        currentTwitchEmote = currentTwitchEmoteIndex == twitchEmotes.Count ? null : twitchEmotes[currentTwitchEmoteIndex];
                        continue;
                    }
                }

                TwitchEmote bttvEmote;
                if (AppSettings.ChatEnableBttvEmotes && (Emotes.BttvGlobalEmotes.TryGetValue(s, out bttvEmote) || channel.BttvChannelEmotes.TryGetValue(s, out bttvEmote))
                    || (AppSettings.ChatEnableFfzEmotes && Emotes.FfzGlobalEmotes.TryGetValue(s, out bttvEmote)))
                {
                    words.Add(new Span { Type = SpanType.Emote, Value = bttvEmote, Color = slashMe ? UsernameColor : new int?() });
                }
                else
                {
                    string link = null;

                    Match m = linkRegex.Match(s);

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

                    words.Add(new Span { Type = SpanType.Text, Value = s, Color = slashMe ? UsernameColor : (link == null ? new int?() : -16776961), Link = link });
                }

                i += s.Length + 1;
            }

            Words = words;

            RawMessage = text;

            w.Stop();
            Console.WriteLine("Message parsed in " + w.Elapsed.TotalSeconds.ToString("0.000000") + " seconds");
        }

        public Message(string text)
        {
            RawMessage = text;

            Words = text.Split(' ').Select(x => new Span { Type = SpanType.Text, Value = x }).ToList();
        }

        bool measureText = true;
        bool measureImages = true;

        // return true if control needs to be redrawn
        public bool CalculateBounds(object graphics, int width, bool emotesChanged = false)
        {
            bool redraw = false;

            if (Width != width)
            {
                Width = width;
                redraw = true;
            }

            // check if any words need to be recalculated
            if (emotesChanged || measureText || measureImages)
            {
                foreach (Span span in Words)
                {
                    if (span.Type == SpanType.Text)
                    {
                        if (measureText)
                        {
                            CommonSize size = GuiEngine.Current.MeasureStringSize(graphics, span.Font, (string)span.Value);
                            span.Width = size.Width;
                            span.Height = size.Height;
                        }
                    }
                    else if (span.Type == SpanType.Image)
                    {
                        if (measureImages)
                        {
                            CommonSize size = GuiEngine.Current.GetImageSize(span.Value);
                            span.Width = size.Width;
                            span.Height = size.Height;
                        }
                    }
                    else if (span.Type == SpanType.Emote)
                    {
                        if (emotesChanged || measureImages)
                        {
                            TwitchEmote emote = (TwitchEmote)span.Value;
                            object image = emote.Image;
                            if (image == null)
                            {
                                span.Width = span.Height = 16;
                            }
                            else
                            {
                                CommonSize size = GuiEngine.Current.GetImageSize(image);
                                span.Width = size.Width;
                                span.Height = size.Height;
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
                int y = 0;
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
                        var span = Words[j];
                        if (j == lineStartIndex && span.Type == SpanType.Text && span.SplitSegments != null)
                        {
                            var segment = span.SplitSegments[span.SplitSegments.Length - 1];
                            CommonRectangle rec = segment.Item2;
                            span.SplitSegments[span.SplitSegments.Length - 1] = Tuple.Create(segment.Item1, new CommonRectangle(rec.X, rec.Y + lineHeight - span.Height, rec.Width, rec.Height));
                        }
                        else
                        {
                            span.Y += lineHeight - span.Height;
                        }
                    }

                    return lineHeight;
                };

                for (; i < Words.Count; i++)
                {
                    Span span = Words[i];

                    span.SplitSegments = null;

                    // word wrapped text
                    if (span.Width > width && span.Type == SpanType.Text && ((string)span.Value).Length > 2)
                    {
                        y += fixCurrentLineHeight();

                        lineStartIndex = i;

                        span.X = 0;
                        span.Y = y;

                        string text = (string)span.Value;
                        int startIndex = 0;
                        List<Tuple<string, CommonRectangle>> items = new List<Tuple<string, CommonRectangle>>();

                        string s;
                        CommonSize size = new CommonSize();
                        for (int j = 1; j < text.Length; j++)
                        {
                            s = text.Substring(startIndex, j - startIndex);
                            if ((size = GuiEngine.Current.MeasureStringSize(graphics, span.Font, s)).Width > width - spaceWidth - spaceWidth - spaceWidth)
                            {
                                items.Add(Tuple.Create(s, new CommonRectangle(0, y, size.Width, size.Height)));
                                startIndex = j;
                                y += span.Height;
                                j++;
                            }
                        }

                        s = text.Substring(startIndex);
                        items.Add(Tuple.Create(s, new CommonRectangle(0, y, size.Width, size.Height)));

                        x = size.Width + spaceWidth;

                        if (items.Count > 1)
                            span.SplitSegments = items.ToArray();
                    }
                    // word in new line
                    else if (span.Width > width - x)
                    {
                        y += fixCurrentLineHeight();

                        span.X = 0;
                        span.Y = y;

                        x = span.Width + spaceWidth;

                        lineStartIndex = i;
                    }
                    // word fits in current line
                    else
                    {
                        span.X = x;
                        span.Y = y;

                        x += span.Width + spaceWidth;
                    }
                }

                y += fixCurrentLineHeight();
                Height = y + 8;
            }

            if (redraw)
                buffer = null;

            return redraw;
        }

        public void Draw(object graphics, int xOffset, int yOffset)
        {
            GuiEngine.Current.DrawMessage(graphics, this, xOffset, yOffset);
        }

        public void UpdateGifEmotes(object graphics)
        {
            GuiEngine.Current.DrawGifEmotes(graphics, this);
        }

        public Span SpanAtPoint(CommonPoint point)
        {
            for (int i = 0; i < Words.Count; i++)
            {
                var span = Words[i];
                if (span.Type == SpanType.Text && span.SplitSegments != null)
                {
                    if (span.SplitSegments.Any(x => x.Item2.Contains(point)))
                        return span;
                }
                else if (span.X < point.X && span.Y < point.Y && span.X + span.Width > point.X && span.Y + span.Height > point.Y)
                {
                    return span;
                }
            }

            return null;
        }
    }
}
