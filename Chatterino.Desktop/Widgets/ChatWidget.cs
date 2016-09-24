using Chatterino.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xwt;
using Xwt.Drawing;

namespace Chatterino.Desktop.Widgets
{
    public class ChatWidget : Canvas
    {
        // Properties
        public const int TopMenuBarHeight = 32;
        public const int ScrollToBottomBarHeight = 24;

        public WidgetSpacing MessagePadding { get; protected set; } = new WidgetSpacing(8, 8, 16 + 8, 8);

        ScrollWidget _scroll = null;
        ChatHeaderWidget _header = null;
        public ChatInputWidget Input { get; private set; }

        private bool scrollAtBottom = true;

        private Selection selection = null;

        // Channel
        TwitchChannel channel = null;

        public TwitchChannel Channel
        {
            get { return channel; }
        }

        private string channelName;

        public string ChannelName
        {
            get { return channelName; }
            set
            {
                value = value.Trim();
                if (value != channelName)
                {
                    _scroll.RemoveHighlightsWhere(x => true);

                    if (channel != null)
                    {
                        channel.MessageAdded -= Channel_MessageAdded;
                        channel.MessagesAddedAtStart -= Channel_MessagesAddedAtStart;
                        channel.MessagesRemovedAtStart -= Channel_MessagesRemovedAtStart;
                        channel.ChatCleared -= Channel_ChatCleared;
                        channel.RoomStateChanged -= Channel_RoomStateChanged;
                        channel = null;
                        TwitchChannel.RemoveChannel(ChannelName);
                    }

                    channelName = value;

                    if (!string.IsNullOrWhiteSpace(channelName))
                    {
                        channel = TwitchChannel.AddChannel(channelName);
                        channel.MessageAdded += Channel_MessageAdded;
                        channel.MessagesAddedAtStart += Channel_MessagesAddedAtStart;
                        channel.MessagesRemovedAtStart += Channel_MessagesRemovedAtStart;
                        channel.ChatCleared += Channel_ChatCleared;
                        channel.RoomStateChanged += Channel_RoomStateChanged;
                    }

                    Application.Invoke(() =>
                    {
                        _header?.QueueDraw();

                        updateMessageBounds();

                        QueueDraw();
                    });
                }
            }
        }

        // Constructor
        public ChatWidget()
        {
            // Header
            _header = new ChatHeaderWidget();

            AddChild(_header);

            // Scroll
            _scroll = new ScrollWidget { HeightRequest = 100, Sensitive = false, Maximum = 100, LargeChange = 33, SmallChange = 4 };

            AddChild(_scroll);

            _scroll.Scroll += (s, e) =>
            {
                checkScrollBarPosition();
                updateMessageBounds();
                QueueDraw();
            };

            // Colors
            App_ColorSchemeChanged(null, null);
            App.ColorSchemeChanged += App_ColorSchemeChanged;
        }

        // Event handlers
        private void App_ColorSchemeChanged(object sender, EventArgs e)
        {
            BackgroundColor = App.ColorScheme.ChatBackground;
        }

        private void Channel_MessageAdded(object sender, MessageAddedEventArgs e)
        {
            if (e.RemovedMessage != null)
            {
                if (selection != null)
                {
                    if (selection.Start.MessageIndex == 0)
                        selection = null;
                    else
                        selection = new Selection(selection.Start.WithMessageIndex(selection.Start.MessageIndex - 1), selection.Start.WithMessageIndex(selection.End.MessageIndex - 1));
                }

                _scroll.Value--;

                _scroll.UpdateHighlights(h => h.Position--);
                _scroll.RemoveHighlightsWhere(h => h.Position < 0);
            }

            if (e.Message.HighlightType)
                _scroll.AddHighlight((channel?.MessageCount ?? 1) - 1, Colors.Red);

            updateMessageBounds();
            QueueDraw();
        }

        private void Channel_MessagesAddedAtStart(object sender, ValueEventArgs<Message[]> e)
        {
            _scroll.UpdateHighlights(h => h.Position += e.Value.Length);

            for (int i = 0; i < e.Value.Length; i++)
            {
                if (e.Value[i].HighlightType)
                    _scroll.AddHighlight(i, Colors.Red);
            }

            updateMessageBounds();
            QueueDraw();
        }

        private void Channel_MessagesRemovedAtStart(object sender, ValueEventArgs<Message[]> e)
        {
            if (selection != null)
            {
                if (selection.Start.MessageIndex < e.Value.Length)
                    selection = null;
                else
                    selection = new Selection(selection.Start.WithMessageIndex(selection.Start.MessageIndex - e.Value.Length), selection.Start.WithMessageIndex(selection.End.MessageIndex - e.Value.Length));
            }

            _scroll.Value -= e.Value.Length;

            _scroll.UpdateHighlights(h => h.Position -= e.Value.Length);
            _scroll.RemoveHighlightsWhere(h => h.Position < 0);

            updateMessageBounds();
            QueueDraw();
        }

        private void Channel_ChatCleared(object sender, ChatClearedEventArgs e)
        {
            Application.Invoke(() => QueueDraw());
        }

        private void Channel_RoomStateChanged(object sender, EventArgs e)
        {
            Application.Invoke(() =>
            {
                var c = channel;
                if (c != null)
                {
                    string text = "";

                    RoomState state = c.RoomState;
                    int count = 0;
                    if (state.HasFlag(RoomState.SlowMode))
                    {
                        text += "slow(" + c.SlowModeTime + "), ";
                        count++;
                    }
                    if (state.HasFlag(RoomState.SubOnly))
                    {
                        text += "sub, ";
                        count++;
                    }
                    if (count == 2)
                        text += "\n";
                    if (state.HasFlag(RoomState.R9k))
                    {
                        text += "r9k, ";
                        count++;
                    }
                    if (count == 2)
                        text += "\n";
                    if (state.HasFlag(RoomState.EmoteOnly))
                    {
                        text += "emote, ";
                        count++;
                    }

                    //_header.RoomstateButton.Text = text == "" ? "-" : text.TrimEnd(' ', ',', '\n');
                    _header.QueueDraw();
                }
            });
        }

        protected override void Dispose(bool disposing)
        {
            App.ColorSchemeChanged -= App_ColorSchemeChanged;

            base.Dispose(disposing);
        }

        protected override void OnBoundsChanged()
        {
            layout();

            updateMessageBounds();
            QueueDraw();

            base.OnBoundsChanged();
        }

        protected override void OnDraw(Context ctx, Rectangle dirtyRect)
        {
            var M = channel?.CloneMessages();

            if (M != null && M.Length > 0)
            {
                int startIndex = Math.Min(Math.Max(0, (int)_scroll.Value), M.Length - 1);
                int yStart = (int)(MessagePadding.Top - (int)(M[startIndex].Height * (_scroll.Value % 1)));
                int h = (int)(Bounds.Height - MessagePadding.Top - MessagePadding.Bottom);

                if (startIndex < M.Length)
                {
                    int y = yStart;

                    for (int i = 0; i < startIndex; i++)
                    {
                        M[i].IsVisible = false;
                    }

                    for (int i = startIndex; i < M.Length; i++)
                    {
                        var msg = M[i];
                        msg.IsVisible = true;

                        MessageRenderer.DrawMessage(ctx, msg, (int)MessagePadding.Left, y, selection, i, true);

                        if (y - msg.Height > h)
                        {
                            for (; i < M.Length; i++)
                            {
                                M[i].IsVisible = false;
                            }

                            break;
                        }

                        y += msg.Height;
                    }
                }

                //if (App.UseDirectX)
                //{
                //    SharpDX.Direct2D1.DeviceContextRenderTarget renderTarget = null;
                //    IntPtr dc = IntPtr.Zero;

                //    dc = g.GetHdc();

                //    renderTarget = new SharpDX.Direct2D1.DeviceContextRenderTarget(MessageRenderer.D2D1Factory, MessageRenderer.RenderTargetProperties);

                //    renderTarget.BindDeviceContext(dc, new RawRectangle(0, 0, Width, Height));

                //    renderTarget.BeginDraw();

                //    //renderTarget.TextRenderingParams = new SharpDX.DirectWrite.RenderingParams(Fonts.Factory, 1, 1, 1, SharpDX.DirectWrite.PixelGeometry.Flat, SharpDX.DirectWrite.RenderingMode.CleartypeGdiClassic);
                //    renderTarget.TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode.Cleartype;

                //    int y = yStart;

                //    Dictionary<RawColor4, SCB> brushes = new Dictionary<RawColor4, SCB>();

                //    var textColor = App.ColorScheme.Text;
                //    var textBrush = new SCB(renderTarget, new RawColor4(textColor.R / 255f, textColor.G / 255f, textColor.B / 255f, 1));

                //    for (int i = startIndex; i < M.Length; i++)
                //    {
                //        var msg = M[i];

                //        foreach (Word word in msg.Words)
                //        {
                //            if (word.Type == SpanType.Text)
                //            {
                //                SCB brush;

                //                if (word.Color == null)
                //                {
                //                    brush = textBrush;
                //                }
                //                else
                //                {
                //                    int value = word.Color.Value;

                //                    HSLColor hsl = new HSLColor((value & 0xFF0000) >> 16, (value & 0x00FF00) >> 8, value & 0x0000FF);

                //                    if (App.ColorScheme.IsLightTheme)
                //                    {
                //                        if (hsl.Luminosity > 120)
                //                            hsl.Luminosity = 120;
                //                    }
                //                    else
                //                    {
                //                        if (hsl.Luminosity < 170)
                //                            hsl.Luminosity = 170;
                //                    }

                //                    RawColor4 color = hsl.ToRawColor4();

                //                    if (!brushes.TryGetValue(color, out brush))
                //                    {
                //                        brushes[color] = brush = new SCB(renderTarget, color);
                //                    }
                //                }

                //                if (word.SplitSegments == null)
                //                {
                //                    renderTarget.DrawText((string)word.Value, Fonts.GetTextFormat(word.Font), new RawRectangleF(MessagePadding.Left + word.X, y + word.Y, 10000, 1000), brush);
                //                }
                //                else
                //                {
                //                    foreach (var split in word.SplitSegments)
                //                        renderTarget.DrawText(split.Item1, Fonts.GetTextFormat(word.Font), new RawRectangleF(MessagePadding.Left + split.Item2.X, y + split.Item2.Y, 10000, 1000), brush);
                //                }
                //            }
                //        }

                //        if (y - msg.Height > h)
                //        {
                //            break;
                //        }

                //        y += msg.Height;
                //    }

                //    foreach (var b in brushes.Values)
                //    {
                //        b.Dispose();
                //    }

                //    renderTarget.EndDraw();

                //    textBrush.Dispose();
                //    g.ReleaseHdc(dc);
                //    renderTarget.Dispose();
                //}

                //{
                //    int y = yStart;

                //    Brush disabledBrush = new SolidBrush(Color.FromArgb(172, (App.ColorScheme.ChatBackground as SolidBrush)?.Color ?? Color.Black));
                //    for (int i = startIndex; i < M.Length; i++)
                //    {
                //        var msg = M[i];

                //        if (msg.Disabled)
                //        {
                //            g.SmoothingMode = SmoothingMode.None;

                //            g.FillRectangle(disabledBrush, 0, y, 1000, msg.Height);
                //        }

                //        if (y - msg.Height > h)
                //        {
                //            break;
                //        }

                //        y += msg.Height;
                //    }
                //    disabledBrush.Dispose();
                //}
            }

            if (Bounds.Width > 0 && Bounds.Height > 0)
            {
                ctx.SetLineWidth(1);
                ctx.Translate(0.5, 0.5);

                ctx.SetColor(App.ColorScheme.TabSelectedBG);
                ctx.Rectangle(0, 0, Bounds.Width - 1, Bounds.Height - 1);
                ctx.Stroke();

                //ctx.DrawTextLayout(new TextLayout(this) { Text = ChannelName, Font = Fonts.TabControlTitle }, 8, 8);
            }
        }

        private void updateMessageBounds(bool emoteChanged = false)
        {
            // determine if
            double scrollbarThumbHeight = 0;
            int totalHeight = (int)(Bounds.Height - MessagePadding.Top - MessagePadding.Bottom);
            int currentHeight = 0;
            int tmpHeight = (int)(Bounds.Height - MessagePadding.Top - MessagePadding.Bottom);
            bool enableScrollbar = false;
            int messageCount = 0;

            TwitchChannel c = channel;

            if (c != null)
            {
                lock (c.MessageLock)
                {
                    var messages = c.Messages;
                    messageCount = messages.Length;

                    int visibleStart = Math.Max(0, (int)_scroll.Value);

                    // set EmotesChanged for messages
                    if (emoteChanged)
                    {
                        for (int i = 0; i < messages.Length; i++)
                        {
                            messages[i].EmoteBoundsChanged = true;
                        }
                    }

                    // calculate bounds for visible messages
                    for (int i = visibleStart; i < messages.Length; i++)
                    {
                        var msg = messages[i];

                        msg.CalculateBounds(null, (int)(Bounds.Width - MessagePadding.Left - MessagePadding.Right));
                        currentHeight += msg.Height;

                        if (currentHeight > totalHeight)
                        {
                            break;
                        }
                    }

                    // calculate bounds for messages at the bottom to determine the size of the scrollbar thumb
                    for (int i = messages.Length - 1; i >= 0; i--)
                    {
                        var msg = messages[i];
                        msg.CalculateBounds(null, (int)(Bounds.Width - MessagePadding.Left - MessagePadding.Right));
                        scrollbarThumbHeight++;

                        tmpHeight -= msg.Height;
                        if (tmpHeight < 0)
                        {
                            enableScrollbar = true;
                            scrollbarThumbHeight -= 1 - (double)tmpHeight / msg.Height;
                            break;
                        }
                    }
                }
            }

            Application.Invoke(() =>
            {
                if (enableScrollbar)
                {
                    _scroll.Sensitive = true;
                    _scroll.LargeChange = scrollbarThumbHeight;
                    _scroll.Maximum = messageCount - 1;

                    if (scrollAtBottom)
                        _scroll.Value = messageCount - scrollbarThumbHeight;
                }
                else
                {
                    _scroll.Sensitive = false;
                    _scroll.Value = 0;
                }
            });
        }

        protected void checkScrollBarPosition()
        {
            scrollAtBottom = !_scroll.Sensitive || _scroll.Maximum < _scroll.Value + _scroll.LargeChange + 0.0001;
        }

        private void layout()
        {
            if (_scroll != null && _header != null)
            {
                // Scroll
                SetChildBounds(_scroll,
                    new Rectangle(Bounds.Width - _scroll.WidthRequest - 1,
                        TopMenuBarHeight,
                        _scroll.WidthRequest,
                        Bounds.Height - TopMenuBarHeight - ((Input?.Visible ?? false) ? Input.HeightRequest : 0) - 1));
            }
        }
    }
}
