using Meebey.SmartIrc4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Chatterino
{
    public class Message
    {
        public int Height { get; private set; }

        public string Username { get; set; }
        public Color UsernameColor { get; set; }

        public MessageBadges Badges { get; set; }

        public int CurrentXOffset { get; set; } = 0;
        public int CurrentYOffset { get; set; } = 0;

        public bool IsVisible { get; set; } = false;

        public string RawMessage { get; private set; }
        public List<Span> Words { get; set; }
        public Tuple<string, Point>[][] SplitWordSegments { get; set; }
        public TwitchChannel Channel { get; set; }

        public Message(IrcMessageData data, TwitchChannel channel)
        {
            Channel = channel;

            List<Span> words = new List<Span>();

            string text = data.Message;
            Username = data.From;

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
                        UsernameColor = Color.FromArgb(-16777216 | Convert.ToInt32(value.Substring(1), 16));
                    }
                }
                catch { }
            }
            if (data.Tags.TryGetValue("display-name", out value))
            {
                Username = value;
            }


            // Add timestamp
            if (true /* App.Settings.ShowTimestamp */) {
                words.Add(new Span {
                    Type = SpanType.Text,
                    Value = DateTime.Now.ToString(App.Settings.ChatShowSeconds ? "HH:mm:ss" : "HH:mm"),
                    Color = Color.Gray,
                    Font = Fonts.Small,
                });
            }

            if (Username.ToUpper() == "FOURTF")
                words.Add(new Span { Type = SpanType.Image, Value = Properties.Resources.dev_bg });

            if (data.Tags.TryGetValue("badges", out value))
            {
                var badges = value.Split(',');

                foreach (var badge in badges)
                {
                    switch (badge)
                    {
                        case "staff/1":
                            Badges |= MessageBadges.Staff;
                            //words.Add(new Span { Type = SpanType.Image, Value = Properties.Resources.staff_1x });
                            break;
                        case "admin/1":
                            Badges |= MessageBadges.Admin;
                            words.Add(new Span { Type = SpanType.Image, Value = Properties.Resources.admin_bg });
                            break;
                        case "global_mod/1":
                            Badges |= MessageBadges.GlobalMod;
                            break;
                        case "moderator/1":
                            Badges |= MessageBadges.Mod;
                            words.Add(new Span { Type = SpanType.Image, Value = Properties.Resources.moderator_bg });
                            break;
                        case "subscriber/1":
                            Badges |= MessageBadges.Sub;
                            words.Add(new Span { Type = SpanType.Emote, Value = channel.SubscriberBadge });
                            break;
                        case "turbo/1":
                            Badges |= MessageBadges.Turbo;
                            words.Add(new Span { Type = SpanType.Image, Value = Properties.Resources.turbo_bg });
                            break;
                        case "broadcaster/1":
                            Badges |= MessageBadges.Broadcaster;
                            words.Add(new Span { Type = SpanType.Image, Value = Properties.Resources.broadcaster_bg });
                            break;
                    }
                }
            }

            //  93064:0-6,8-14/80481:16-20,22-26

            words.Add(new Span { Type = SpanType.Text, Value = Username + (slashMe ? "" : ":"), Color = UsernameColor });

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
                            if (!App.TwitchEmotes.TryGetValue(id, out e))
                            {
                                e = new TwitchEmote { Name = name, Url = App.TwitchEmoteTemplate.Replace("{id}", id.ToString()) };
                                App.TwitchEmotes[id] = e;
                            }
                            twitchEmotes.Add(Tuple.Create(index, e));
                        });
                    }
                });
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
                if (App.BttvGlobalEmotes.TryGetValue(s, out bttvEmote) || channel.BttvChannelEmotes.TryGetValue(s, out bttvEmote))
                {
                    words.Add(new Span { Type = SpanType.Emote, Value = bttvEmote, Color = slashMe ? UsernameColor : new Color?() });
                }
                else
                {
                    words.Add(new Span { Type = SpanType.Text, Value = s, Color = slashMe ? UsernameColor : new Color?() });
                }

                i += s.Length + 1;
            }

            Words = words;

            RawMessage = text;

            SplitWordSegments = new Tuple<string, Point>[words.Count][];
        }

        public Message(string text)
        {
            RawMessage = text;

            Words = text.Split(' ').Select(x => new Span { Type = SpanType.Text, Value = x }).ToList();
            SplitWordSegments = new Tuple<string, Point>[Words.Count][];
        }

        Font lastFont = null;
        int lastWidth = -1;
        int lineHeight = 10;

        public void CalculateBounds(IDeviceContext g, Font font, int width, bool emoteChanged = false)
        {
            var xHeight = TextRenderer.MeasureText(g, "X", font, Size.Empty, App.DefaultTextFormatFlags).Height;

            int spaceWidth = TextRenderer.MeasureText(g, " ", font, Size.Empty, App.DefaultTextFormatFlags).Width;

            if (emoteChanged || lastFont != font)
            {
                lineHeight = TextRenderer.MeasureText(g, "X", font, Size.Empty, App.DefaultTextFormatFlags).Height;

                for (int i = 0; i < Words.Count; i++)
                {
                    var span = Words[i];
                    SplitWordSegments[i] = null;

                    if (span.Type == SpanType.Text)
                    {
                        string s = (string)span.Value;
                        var _font = span.Font ?? Fonts.Medium;

                        var size = TextRenderer.MeasureText(g, s, _font, Size.Empty, App.DefaultTextFormatFlags);
                        span.Width = size.Width;
                        span.Height = xHeight;// size.Height;
                    }
                    else if (span.Type == SpanType.Image)
                    {
                        var obj = (Image)span.Value;

                        span.Width = obj.Width;
                        span.Height = obj.Height;
                    }
                    else
                    {
                        var obj = (TwitchEmote)span.Value;

                        var image = obj.Image;
                        if (image != null)
                        {
                            lock (image)
                            {
                                span.Width = image.Width;
                                span.Height = image.Height;
                            }
                        }
                        else
                        {
                            span.Width = 16;
                            span.Height = 16;
                        }
                    }
                }
            }

            if (lastFont != font || lastWidth != width)
            {
                lastWidth = width;

                Height = TextRenderer.MeasureText(g, RawMessage, font).Height;
            }

            int x = 0, y = 0;
            int currentLineHeight = 0;

            int linestart = 0;

            for (int wordIndex = 0; wordIndex < Words.Count; wordIndex++)
            {
                SplitWordSegments[wordIndex] = null;
                var span = Words[wordIndex];
                if (x + span.Width > width)
                {
                    y += currentLineHeight;

                    lineHeight = 0;

                    for (int i = linestart; i < wordIndex; i++)
                        lineHeight = Math.Max(lineHeight, Words[i].Height);
                    for (int i = linestart; i < wordIndex; i++)
                        Words[i].Y += lineHeight - Words[i].Height;

                    if (linestart > 0 && SplitWordSegments[linestart - 1] != null)
                    {
                        var items = SplitWordSegments[linestart - 1];
                        var item = items[items.Length - 1];
                        items[items.Length - 1] = Tuple.Create(item.Item1, new Point(item.Item2.X, item.Item2.Y + lineHeight - Words[linestart - 1].Height));
                    }

                    if (span.Type == SpanType.Text && span.Width > width)
                    {
                        x = 0;

                        span.X = 0;
                        span.Y = y;

                        currentLineHeight = span.Height;

                        string text = (string)span.Value;
                        int startIndex = 0;
                        List<Tuple<string, Point>> items = new List<Tuple<string, Point>>();

                        string s;
                        for (int i = 1; i < text.Length; i++)
                        {
                            s = text.Substring(startIndex, i - startIndex);
                            if (TextRenderer.MeasureText(s, font, Size.Empty, App.DefaultTextFormatFlags).Width + x > width)
                            {
                                items.Add(Tuple.Create(s, new Point(x, y)));
                                startIndex = i;
                                x = 0;
                                //y += xHeight;
                                y += span.Height;
                                i++;
                            }
                        }

                        s = text.Substring(startIndex);
                        items.Add(Tuple.Create(s, new Point(x, y)));
                        x += TextRenderer.MeasureText(s, font, Size.Empty, App.DefaultTextFormatFlags).Width;
                        SplitWordSegments[wordIndex] = items.ToArray();

                        linestart = wordIndex + 1;
                    }
                    else
                    {
                        span.X = 0;
                        span.Y = y;
                        x = span.Width;

                        linestart = wordIndex;

                        currentLineHeight = span.Height;
                    }
                }
                else
                {
                    span.X = x;
                    span.Y = y;
                    x += span.Width;
                    currentLineHeight = Math.Max(currentLineHeight, span.Height);
                }
                x += spaceWidth;
            }

            lineHeight = 0;
            for (int i = linestart; i < Words.Count; i++)
                lineHeight = Math.Max(lineHeight, Words[i].Height);
            for (int i = linestart; i < Words.Count; i++)
                Words[i].Y += lineHeight - Words[i].Height;

            if (linestart > 0 && SplitWordSegments[linestart - 1] != null)
            {
                var items = SplitWordSegments[linestart - 1];
                var item = items[items.Length - 1];
                items[items.Length - 1] = Tuple.Create(item.Item1, new Point(item.Item2.X, item.Item2.Y + lineHeight - Words[linestart - 1].Height));
            }

            Height = y + currentLineHeight + 8;

            lastFont = font;
        }

        public void Draw(Graphics g, int xOffset, int yOffset)
        {
            CurrentXOffset = xOffset;
            var textColor = App.ColorScheme.Text;

            Font font;

            for (int i = 0; i < Words.Count; i++)
            {
                var span = Words[i];

                font = span.Font ?? Fonts.Medium;

                if (span.Type == SpanType.Text)
                {
                    var segments = SplitWordSegments[i];
                    Color color = span.Color ?? textColor;
                    if (span.Color != null && span.Color.Value.GetBrightness() < 0.5f)
                    {
                        color = ControlPaint.Light(color, 1f);
                        //color = Color.FromArgb(color.R < 127 ? 127 : color.R, color.G < 127 ? 127 : color.G, color.B < 127 ? 127 : color.B);
                    }

                    if (segments == null)
                    {
                        //g.DrawString((string)span.Value, font, Brushes.White, new Point(xOffset + span.X, span.Y + yOffset));
                        TextRenderer.DrawText(g, (string)span.Value, font, new Point(xOffset + span.X, span.Y + yOffset), color, App.DefaultTextFormatFlags);
                    }
                    else
                    {
                        for (int x = 0; x < segments.Length; x++)
                        {
                            TextRenderer.DrawText(g, segments[x].Item1, font, new Point(xOffset + segments[x].Item2.X, yOffset + segments[x].Item2.Y), color, App.DefaultTextFormatFlags);
                        }
                    }
                }
                else if (span.Type == SpanType.Emote)
                {
                    var emote = (TwitchEmote)span.Value;
                    var img = emote.Image;
                    if (img != null)
                    {
                        lock (img)
                        {
                            g.DrawImage(img, span.X + xOffset, span.Y + yOffset, span.Width, span.Height);
                        }
                    }
                    else
                    {
                        g.DrawRectangle(Pens.Red, xOffset + span.X, span.Y + yOffset, span.Width, span.Height);
                    }
                }
                else if (span.Type == SpanType.Image)
                {
                    var img = (Image)span.Value;
                    if (img != null)
                        g.DrawImage(img, span.X + xOffset, span.Y + yOffset, span.Width, span.Height);
                }
            }
        }

        public void UpdateGifEmotes(Graphics g)
        {
            for (int i = 0; i < Words.Count; i++)
            {
                var span = Words[i];

                TwitchEmote emote;
                if (span.Type == SpanType.Emote && (emote = (TwitchEmote)span.Value).Animated)
                {
                    lock (emote.Image)
                    {
                        BufferedGraphicsContext context = BufferedGraphicsManager.Current;

                        var buffer = context.Allocate(g, new Rectangle(span.X + CurrentXOffset, span.Y + CurrentYOffset, span.Width, span.Height));

                        buffer.Graphics.FillRectangle(App.ColorScheme.ChatBackground, span.X + CurrentXOffset, span.Y + CurrentYOffset, span.Width, span.Height);
                        buffer.Graphics.DrawImage(emote.Image, span.X + CurrentXOffset, span.Y + CurrentYOffset, span.Width, span.Height);

                        buffer.Render(g);

                        //g.FillRectangle(App.ColorScheme.ChatBackground, span.X + CurrentXOffset, span.Y + CurrentYOffset - 4, span.Width, span.Height);
                        //g.DrawImage(emote.Image, span.X + CurrentXOffset, span.Y + CurrentYOffset - 4, span.Width, span.Height);
                    }
                }
            }
        }
    }
}
