using Meebey.SmartIrc4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace Chatterino.Controls
{
    public class ChatControl : ColumnLayoutItemBase
    {
        // Properties
        public int MaxMessages { get; set; } = 200;

        public const int TopMenuBarHeight = 32;

        public Padding TextPadding { get; set; } = new Padding(12, 12 + TopMenuBarHeight, 16 + SystemInformation.VerticalScrollBarWidth, 4);

        public Message SendMessage { get; set; } = null; // new Message("xD xD xD xD xD xD xD xD xD xD xD xD xD xD xD xD xD xD xD xD xD xD xD xD xD xD xD xD ");

        ChatControlHeader _header = null;


        // vars
        private bool scrollAtBottom = true;

        public int totalMessageHeight = 0;

        List<Message> Messages = new List<Message>();

        Timer gifEmoteTimer = new Timer { Interval = 33 };

        CustomScrollBar.ScrollBarEx vscroll = new CustomScrollBar.ScrollBarEx
        {
            Enabled = false
        };


        // ctor
        public ChatControl()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);

            App.IrcMessageReceived += onRawMessage;
            App.EmoteLoaded += onEmoteLoaded;
            //App.GifEmoteFramesUpdated += onEmoteUpdated;

            Disposed += (s, e) =>
            {
                App.IrcMessageReceived -= onRawMessage;
                App.EmoteLoaded -= onEmoteLoaded;
                //App.GifEmoteFramesUpdated -= onEmoteUpdated;
                App.RemoveChannel(ChannelName);
            };

            Font = new Font("Helvetica Neue", 9.5f);

            ChatControlHeader header = _header = new ChatControlHeader(this);
            Controls.Add(header);

            GotFocus += (s, e) => { header.Invalidate(); };
            LostFocus += (s, e) => { header.Invalidate(); };

            Controls.Add(vscroll);

            vscroll.SmallChange = 16;

            vscroll.Height = Height - TopMenuBarHeight;
            vscroll.Location = new Point(Width - SystemInformation.VerticalScrollBarWidth - 1, TopMenuBarHeight);
            vscroll.Size = new Size(SystemInformation.VerticalScrollBarWidth, Height - TopMenuBarHeight - 1);
            vscroll.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;

            vscroll.Scroll += (s, e) =>
            {
                Invalidate();
                checkScrollBarPosition();
            };

            gifEmoteTimer.Tick += onEmoteUpdated;
            gifEmoteTimer.Start();

            //vscroll.Visible = false;
            //MouseEnter += (s, e) => { vscroll.Visible = true; };
            //MouseLeave += (s, e) => { vscroll.Visible = false; };
        }


        // event handlers
        void onEmoteUpdated(object s, EventArgs e)
        {
            var g = CreateGraphics();
            lock (Messages)
                foreach (Message msg in Messages)
                {
                    //if (msg.CurrentYOffset > -msg.Height && msg.CurrentYOffset < Height)
                    if (msg.IsVisible)
                    {
                        msg.UpdateGifEmotes(g);
                    }
                }
        }

        void onEmoteLoaded(object s, EventArgs e)
        {
            updateMessageBounds(true);
            Invalidate();
        }

        void onRawMessage(object s, IrcEventArgs e)
        {
            if ((e.Data.Channel?.Length ?? 0) > 1 && (e.Data.Channel?.Substring(1) ?? "") == ChannelName)
            {
                if (e.Data.RawMessageArray.Length > 4 && e.Data.RawMessageArray[2] == "PRIVMSG")
                {
                    TwitchChannel c;

                    if (App.Channels.TryGetValue((e.Data.Channel ?? "").TrimStart('#'), out c))
                    {
                        Message firstMessage = null;

                        Message msg = new Message(e.Data, c);
                        lock (Messages)
                        {
                            if (Messages.Count == MaxMessages)
                            {
                                firstMessage = Messages[0];
                                Messages = new List<Message>(Messages.Skip(1));
                            }
                            Messages.Add(msg);
                        }

                        bool bottom = scrollAtBottom;
                        updateMessageBounds();
                        vscroll.Invoke(() =>
                        {
                            if (vscroll.Enabled)
                            {
                                if (bottom)
                                    vscroll.Value = vscroll.Maximum;
                                else if (firstMessage != null)
                                    vscroll.Value = Math.Max(0, vscroll.Value - firstMessage.Height);
                            }
                        });
                        this.Invoke(() =>
                        {
                            Invalidate();
                        });
                    }
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            updateMessageBounds();

        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int y = -vscroll.Value + TextPadding.Top;

            // DRAW MESSAGES
            lock (Messages)
            {
                for (int i = 0; i < Messages.Count; i++)
                {
                    var msg = Messages[i];
                    if (y + msg.Height > 0)
                    {
                        if (y > Height)
                        {
                            for (; i < Messages.Count; i++)
                            {
                                Messages[i].IsVisible = false;
                            }
                            break;
                        }
                        msg.Draw(e.Graphics, Font, TextPadding.Left, y);
                        msg.IsVisible = true;
                    }
                    else
                    {
                        msg.IsVisible = false;
                    }
                    msg.CurrentYOffset = y;
                    y += msg.Height;
                }
            }

            e.Graphics.DrawRectangle(Focused ? App.ColorScheme.ChatBorderFocused : App.ColorScheme.ChatBorder, 0, 0, Width - 1/* - SystemInformation.VerticalScrollBarWidth*/, Height - 1);

            if (SendMessage != null)
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(222, Color.White)), 1, Height - SendMessage.Height - TextPadding.Bottom, Width - 3 - SystemInformation.VerticalScrollBarWidth, SendMessage.Height + TextPadding.Bottom - 1);
                SendMessage.Draw(e.Graphics, Font, TextPadding.Left, Height - SendMessage.Height - TextPadding.Bottom);
            }
        }


        // controls
        void updateMessageBounds(bool emoteChanged = false)
        {
            int totalHeight = 0;
            using (var g = CreateGraphics())
            {
                lock (Messages)
                {
                    foreach (var msg in Messages)
                    {
                        msg.CalculateBounds(g, Font, Width - TextPadding.Left - TextPadding.Right, emoteChanged);
                        totalHeight += msg.Height;
                    }
                }

                if (SendMessage != null)
                {
                    SendMessage.CalculateBounds(g, Font, Width - TextPadding.Left - TextPadding.Right);
                }

                totalMessageHeight = totalHeight;

                updateScrollBar();
            }
        }

        void updateScrollBar()
        {
            if (Height > 8)
            {
                vscroll.Invoke(() =>
                {
                    if (totalMessageHeight > Height)
                    {
                        vscroll.Enabled = true;
                        vscroll.Maximum = totalMessageHeight - Height + TextPadding.Top + TextPadding.Bottom;
                        vscroll.LargeChange = Height - TextPadding.Top - TextPadding.Bottom;

                        if (scrollAtBottom)
                        {
                            vscroll.Value = vscroll.Maximum;
                        }
                    }
                    else
                    {
                        vscroll.Enabled = false;
                    }
                });
            }
        }

        void checkScrollBarPosition()
        {
            scrollAtBottom = !vscroll.Enabled || vscroll.Maximum < vscroll.Value + vscroll.LargeChange;
        }

        private string ircChannelName;

        public string ChannelName
        {
            get { return ircChannelName; }
            set
            {
                value = value.Trim();
                if (value != ircChannelName)
                {
                    if (!string.IsNullOrWhiteSpace(ircChannelName))
                        App.RemoveChannel(ChannelName);

                    ircChannelName = value;

                    lock (Messages)
                        Messages.Clear();

                    if (!string.IsNullOrWhiteSpace(ircChannelName))
                        App.AddChannel(ircChannelName);

                    _header?.Invalidate();

                    Invalidate();
                }
            }
        }


        // header
        class ChatControlHeader : Control
        {
            private ChatControl chatControl;

            public ChatControlHeader(ChatControl chatControl)
            {
                this.chatControl = chatControl;

                SetStyle(ControlStyles.ResizeRedraw, true);
                SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

                Dock = DockStyle.Top;
                Height = TopMenuBarHeight + 1;

                // Mousedown
                bool mouseDown = false;

                MouseDown += (s, e) =>
                {
                    if (e.Button == MouseButtons.Left)
                        mouseDown = true;
                };
                MouseUp += (s, e) =>
                {
                    if (e.Button == MouseButtons.Left)
                        mouseDown = false;
                };

                // Drag + Drop
                MouseMove += (s, e) =>
                {
                    if (mouseDown)
                    {
                        if (e.X < 0 || e.Y < 0 || e.X > Width || e.Y > Height)
                        {
                            ColumnLayoutControl layout = chatControl.Parent as ColumnLayoutControl;
                            if (layout != null)
                            {
                                var position = layout.RemoveFromGrid(chatControl);
                                if (DoDragDrop(new ColumnLayoutDragDropContainer { Control = chatControl }, DragDropEffects.Move) == DragDropEffects.None)
                                {
                                    layout.AddToGrid(this, position.Item1, position.Item2);
                                }
                            }
                        }
                    }
                };
            }

            protected override void OnDoubleClick(EventArgs e)
            {
                base.OnDoubleClick(e);

                using (InputDialogForm dialog = new InputDialogForm("channel name") { Value = chatControl.ChannelName })
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        chatControl.ChannelName = dialog.Value;
                    }
                }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                // CHANNEL NAME
                e.Graphics.FillRectangle(App.ColorScheme.Menu, 0, 0, Width, ChatControl.TopMenuBarHeight);
                e.Graphics.DrawRectangle(Focused ? App.ColorScheme.ChatBorderFocused : App.ColorScheme.ChatBorder, 0, 0, Width - 1, Height - 1);
                //e.Graphics.DrawLine(Focused ? App.ColorScheme.ChatBorderFocused : App.ColorScheme.ChatBorder, 0, ChatControl.TopMenuBarHeight, Width, ChatControl.TopMenuBarHeight);
                //e.Graphics.DrawLine(Focused ? App.ColorScheme.ChatBorderFocused : App.ColorScheme.ChatBorder, 0, 0, Width, 0);

                string title = string.IsNullOrWhiteSpace(chatControl.ChannelName) ? "<no channel>" : chatControl.ChannelName;

                var size = TextRenderer.MeasureText(e.Graphics, title, chatControl.Font, Size.Empty, App.DefaultTextFormatFlags);
                TextRenderer.DrawText(e.Graphics, title, chatControl.Font, new Point((Width / 2) - (size.Width / 2), ChatControl.TopMenuBarHeight / 2 - (size.Height / 2)), chatControl.Focused ? App.ColorScheme.TextFocused : App.ColorScheme.Text, App.DefaultTextFormatFlags);
            }
        }
    }
}
