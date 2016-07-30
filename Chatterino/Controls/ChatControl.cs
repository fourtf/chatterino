using Chatterino.Common;
using Meebey.SmartIrc4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Message = Chatterino.Common.Message;

namespace Chatterino.Controls
{
    public class ChatControl : ColumnLayoutItemBase
    {
        // Properties
        public int MaxMessages { get; set; } = 500;

        public const int TopMenuBarHeight = 32;

        public Padding TextPadding { get; set; } = new Padding(12, 12 + TopMenuBarHeight, 16 + SystemInformation.VerticalScrollBarWidth, 4);

        public Message SendMessage { get; set; } = null;

        ChatControlHeader _header = null;

        // vars
        private bool scrollAtBottom = true;

        public int totalMessageHeight = 0;

        TwitchChannel channel = null;

        Message[] Messages = new Message[0];
        //{
        //    get
        //    {
        //        return channel?.Messages;
        //    }
        //}


        Timer gifEmoteTimer = new Timer { Interval = 33 };

        CustomScrollBar vscroll = new CustomScrollBar
        {
            Enabled = false,
            SmallChange = 32,
        };

        string lastTabComplete = null;
        int currentTabIndex = 0;

        // ctor
        public ChatControl()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);

            IrcManager.MessageReceived += onRawMessage;
            IrcManager.ChatCleared += IrcManager_IrcChatCleared;
            IrcManager.Connected += IrcManager_Connected;
            IrcManager.Disconnected += IrcManager_Disconnected;
            IrcManager.ConnectionError += IrcManager_ConnectionError;
            App.GifEmoteFramesUpdated += onRedrawGifEmotes;
            App.EmoteLoaded += onEmoteLoaded;

            Disposed += (s, e) =>
            {
                IrcManager.MessageReceived -= onRawMessage;
                IrcManager.ChatCleared -= IrcManager_IrcChatCleared;
                IrcManager.Connected -= IrcManager_Connected;
                IrcManager.Disconnected -= IrcManager_Disconnected;
                IrcManager.ConnectionError -= IrcManager_ConnectionError;
                App.GifEmoteFramesUpdated -= onRedrawGifEmotes;
                App.EmoteLoaded -= onEmoteLoaded;
                IrcManager.RemoveChannel(ChannelName);
            };

            Font = Fonts.Medium;

            ChatControlHeader header = _header = new ChatControlHeader(this);
            Controls.Add(header);

            GotFocus += (s, e) => { header.Invalidate(); };
            LostFocus += (s, e) => { header.Invalidate(); };

            Controls.Add(vscroll);

            vscroll.Height = Height - TopMenuBarHeight;
            vscroll.Location = new Point(Width - SystemInformation.VerticalScrollBarWidth - 1, TopMenuBarHeight);
            vscroll.Size = new Size(SystemInformation.VerticalScrollBarWidth, Height - TopMenuBarHeight - 1);
            vscroll.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;

            vscroll.Scroll += (s, e) =>
            {
                Invalidate();
                checkScrollBarPosition();
            };
        }

        private void IrcManager_ConnectionError(object sender, ValueEventArgs<Exception> e)
        {
            addMessage(new Message(e.Value.Message, null, true));
            updateMessageBounds();
            updateScrollBar();
            Invalidate();
        }

        private void IrcManager_Disconnected(object sender, EventArgs e)
        {
            addMessage(new Message("disconnected from chat", null, true));
            updateMessageBounds();
            updateScrollBar();
            Invalidate();
        }

        private void IrcManager_Connected(object sender, EventArgs e)
        {
            addMessage(new Message("connected to chat", null, true));
            updateMessageBounds();
            updateScrollBar();
            Invalidate();
        }

        private void IrcManager_IrcChatCleared(object sender, ChatClearedEventArgs e)
        {
            if (e.Channel == ChannelName)
            {
                lock (Messages)
                {
                    addMessage(new Message(e.Message, null, true));
                }
                lock (Messages)
                {
                    foreach (var msg in Messages)
                    {
                        if (msg.Username == e.User)
                        {
                            msg.Disabled = true;
                            GuiEngine.Current.DisposeMessageGraphicsBuffer(msg);
                        }
                    }
                }
                updateMessageBounds();
                updateScrollbarHighlights();
                this.Invoke(() =>
                {
                    Invalidate();
                });
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            vscroll.Value -= e.Delta;

            checkScrollBarPosition();

            Invalidate();

            base.OnMouseWheel(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            int index;

            var graphics = CreateGraphics();

            var msg = MessageAtPoint(e.Location, out index);
            if (msg != null)
            {
                var word = msg.WordAtPoint(new CommonPoint(e.X - TextPadding.Left, e.Y - msg.Y));

                var pos = msg.MessagePositionAtPoint(graphics, new CommonPoint(e.X, e.Y - msg.Y), index);
                Console.WriteLine($"pos: {pos.MessageIndex} : {pos.WordIndex} : {pos.CharIndex}");

                if (selection != null && mouseDown)
                {
                    var newSelection = new Selection(selection.Start, pos);
                    if (!newSelection.Equals(selection))
                    {
                        selection = newSelection;
                        Invalidate();
                    }
                }

                if (word != null)
                {
                    if (word.Link != null)
                    {
                        Cursor = Cursors.Hand;
                    }
                    else
                    {
                        Cursor = Cursors.Default;
                    }

                    if (word.Tooltip != null)
                    {
                        App.ShowToolTip(PointToScreen(new Point(e.Location.X + 16, e.Location.Y + 16)), word.Tooltip);
                    }
                    else
                    {
                        App.ToolTip?.Hide();
                    }
                }
                else
                {
                    Cursor = Cursors.Default;
                    App.ToolTip?.Hide();
                }
            }
        }

        string mouseDownLink = null;
        Word mouseDownWord = null;
        Selection selection = null;
        bool mouseDown = false;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                mouseDown = true;

                int index;

                var msg = MessageAtPoint(e.Location, out index);
                if (msg != null)
                {
                    var graphics = CreateGraphics();
                    var position = msg.MessagePositionAtPoint(graphics, new CommonPoint(e.X, e.Y - msg.Y), index);
                    selection = new Selection(position, position);

                    var word = msg.WordAtPoint(new CommonPoint(e.X - TextPadding.Left, e.Y - msg.Y));
                    if (word != null)
                    {
                        if (word.Link != null)
                        {
                            mouseDownLink = word.Link;
                            mouseDownWord = word;
                        }
                    }
                }
                else
                    selection = null;
            }

            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Left)
            {
                mouseDown = false;

                int index;

                var msg = MessageAtPoint(e.Location, out index);
                if (msg != null)
                {
                    var word = msg.WordAtPoint(new CommonPoint(e.X - TextPadding.Left, e.Y - msg.Y));
                    if (word != null)
                    {
                        if (mouseDownLink != null && mouseDownWord == word)
                        {
                            GuiEngine.Current.HandleLink(mouseDownLink);
                        }
                    }
                }

                mouseDownLink = null;
            }
        }


        // event handlers
        void onRedrawGifEmotes(object s, EventArgs e)
        {
            var g = CreateGraphics();
            lock (Messages)
                for (int i = 0; i < Messages.Length; i++)
                {
                    var msg = Messages[i];
                    if (msg.IsVisible)
                    {
                        msg.UpdateGifEmotes(g, selection, i);
                    }
                }
        }

        void onEmoteLoaded(object s, EventArgs e)
        {
            updateMessageBounds(true);
            Invalidate();
        }

        void onRawMessage(object s, MessageEventArgs e)
        {
            if (e.Message.Channel.Name == ChannelName)
            {
                var msg = e.Message;
                Message firstMessage = null;

                lock (Messages)
                {
                    if (Messages.Length == MaxMessages)
                    {
                        firstMessage = Messages[0];
                    }
                    addMessage(msg);
                }

                bool bottom = scrollAtBottom;
                updateMessageBounds();
                updateScrollbarHighlights();

                vscroll.Invoke(() =>
                {
                    if (vscroll.Enabled)
                    {
                        if (bottom)
                            vscroll.Value = vscroll.Maximum - vscroll.LargeChange;
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

        void updateScrollbarHighlights()
        {
            List<ScrollBarHighlight> highlights = new List<ScrollBarHighlight>();

            lock (Messages)
                foreach (var m in Messages)
                {
                    if (m.Highlighted)
                    {
                        highlights.Add(new ScrollBarHighlight(m.TotalY, m.Height, Color.Red));
                    }
                }

            vscroll.Invoke(() => vscroll.Highlights = highlights.ToArray());
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            //if ((ModifierKeys & ~Keys.Shift) == Keys.None)
            {
                if (e.KeyChar == '\b')
                {
                    if (SendMessage != null)
                    {
                        var message = SendMessage.RawMessage;
                        if (message.Length > 1)
                        {
                            SendMessage = new Message(message.Remove(message.Length - 1));
                        }
                        else
                        {
                            SendMessage = null;
                        }
                    }
                    lastTabComplete = null;
                    currentTabIndex = 0;
                }
                else if (e.KeyChar == '\r')
                {
                    if (SendMessage != null)
                    {
                        IrcManager.SendMessage(ircChannelName, SendMessage.RawMessage);
                        SendMessage = null;
                    }
                    lastTabComplete = null;
                    currentTabIndex = 0;
                }
                else if (e.KeyChar >= ' ')
                {
                    if (SendMessage == null)
                    {
                        SendMessage = new Message(e.KeyChar.ToString());
                    }
                    else
                    {
                        SendMessage = new Message(SendMessage.RawMessage + e.KeyChar.ToString());
                    }
                    lastTabComplete = null;
                    currentTabIndex = 0;
                }

                updateMessageBounds();
                Invalidate();
            }
        }

        public void HandleTabCompletion(bool forward)
        {
            if (SendMessage != null)
            {
                string text = SendMessage.RawMessage;
                int index;
                text = (index = text.LastIndexOf(' ')) == -1 ? text : text.Substring(index + 1);
                TwitchChannel channel;
                if (IrcManager.Channels.TryGetValue(ChannelName.ToLower(), out channel))
                {
                    if (lastTabComplete == null)
                        currentTabIndex = forward ? -1 : 1;

                    var completion = channel.GetEmoteCompletion(lastTabComplete ?? text, ref currentTabIndex, forward);
                    if (completion != null)
                    {
                        lastTabComplete = lastTabComplete ?? text;

                        SendMessage = new Message((index == -1 ? "" : SendMessage.RawMessage.Remove(index + 1)) + completion);
                        updateMessageBounds();
                        this.Invoke(() => Invalidate());
                    }
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            updateMessageBounds();
            updateScrollbarHighlights();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var borderPen = Focused ? App.ColorScheme.ChatBorderFocused : App.ColorScheme.ChatBorder;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int y = (int)(-vscroll.Value + TextPadding.Top);

            // DRAW MESSAGES

            Message[] M;

            lock (Messages)
            {
                M = new Message[Messages.Length];
                Array.Copy(Messages, M, Messages.Length);
            }

            for (int i = 0; i < M.Length; i++)
            {
                var msg = M[i];
                if (y + msg.Height > 0)
                {
                    if (y > Height)
                    {
                        for (; i < M.Length; i++)
                        {
                            M[i].IsVisible = false;
                            msg.Y = y;
                            y += msg.Height;
                        }
                        break;
                    }
                    msg.Draw(e.Graphics, TextPadding.Left, y, selection, i);
                    msg.IsVisible = true;
                }
                else
                {
                    msg.IsVisible = false;
                }
                msg.Y = y;
                y += msg.Height;
            }

            e.Graphics.DrawRectangle(borderPen, 0, 0, Width - 1/* - SystemInformation.VerticalScrollBarWidth*/, Height - 1);

            if (SendMessage != null)
            {
                e.Graphics.FillRectangle(App.ColorScheme.ChatBackground, 1, Height - SendMessage.Height - 4, Width - 3 - SystemInformation.VerticalScrollBarWidth, SendMessage.Height + TextPadding.Bottom - 1);
                e.Graphics.DrawLine(borderPen, 1, Height - SendMessage.Height - 4, Width - 2 - SystemInformation.VerticalScrollBarWidth, Height - SendMessage.Height - 4);
                SendMessage.Draw(e.Graphics, TextPadding.Left, Height - SendMessage.Height, null, -1);
            }
        }

        void addMessage(Message msg)
        {
            if (Messages.Length == MaxMessages)
            {
                if (selection != null)
                {
                    if (selection.Start.MessageIndex == 0)
                        selection = null;
                    else
                        selection = new Selection(new MessagePosition(selection.Start.MessageIndex, selection.Start.WordIndex, selection.Start.CharIndex),
                            new MessagePosition(selection.End.MessageIndex, selection.End.WordIndex, selection.End.CharIndex));
                }

                Message[] M = new Message[Messages.Length];
                Array.Copy(Messages, 1, M, 0, Messages.Length - 1);
                M[M.Length - 1] = msg;
                Messages = M;
            }
            else
            {
                Message[] M = new Message[Messages.Length + 1];
                Messages.CopyTo(M, 0);
                M[M.Length - 1] = msg;
                Messages = M;
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
                        msg.TotalY = totalHeight;
                        msg.CalculateBounds(g, Width - TextPadding.Left - TextPadding.Right, emoteChanged);
                        totalHeight += msg.Height;
                    }
                }

                if (SendMessage != null)
                {
                    SendMessage.CalculateBounds(g, Width - TextPadding.Left - TextPadding.Right);
                    TextPadding = new Padding(TextPadding.Left, TextPadding.Top, TextPadding.Right, 4 + SendMessage.Height);
                }
                else
                    TextPadding = new Padding(TextPadding.Left, TextPadding.Top, TextPadding.Right, 4);

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
                    if (totalMessageHeight > Height - TextPadding.Top - TextPadding.Bottom)
                    {
                        vscroll.Enabled = true;
                        vscroll.LargeChange = Height - TextPadding.Top - TextPadding.Bottom;
                        vscroll.Maximum = totalMessageHeight - Height + TextPadding.Top + TextPadding.Bottom + vscroll.LargeChange;

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
            scrollAtBottom = !vscroll.Enabled || vscroll.Maximum < vscroll.Value + vscroll.LargeChange + 30;
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
                        IrcManager.RemoveChannel(ChannelName);

                    ircChannelName = value;

                    //lock (Messages)
                    Messages = new Message[0];

                    if (!string.IsNullOrWhiteSpace(ircChannelName))
                        IrcManager.AddChannel(ircChannelName);

                    _header?.Invalidate();

                    Invalidate();
                }
            }
        }

        public Message MessageAtPoint(Point p, out int index)
        {
            lock (Messages)
                for (int i = 0; i < Messages.Length; i++)
                {
                    var m = Messages[i];
                    if (m.Y > p.Y - m.Height)
                    {
                        index = i;
                        return m;
                    }
                }
            index = -1;
            return null;
        }

        public void CopySelection()
        {
            var text = GetSelectedText();

            if (text != null)
                Clipboard.SetText(text);
        }

        public string GetSelectedText()
        {
            if (selection == null || selection.IsEmpty)
                return null;

            StringBuilder b = new StringBuilder();

            lock (Messages)
                for (int i = selection.First.MessageIndex; i <= selection.Last.MessageIndex; i++)
                {
                    if (i != selection.First.MessageIndex)
                        b.AppendLine();

                    for (int j = (i == selection.First.MessageIndex ? selection.First.WordIndex : 0); j < (i == selection.Last.MessageIndex ? selection.Last.WordIndex : Messages[i].Words.Count); j++)
                    {
                        if (Messages[i].Words[j].CopyText != null)
                        {
                            b.Append(Messages[i].Words[j].CopyText);
                            b.Append(' ');
                        }
                    }
                }

            return b.ToString();
        }

        public void PasteText(string text)
        {
            if (SendMessage == null)
                SendMessage = new Message(text);
            else
                SendMessage = new Message(SendMessage.RawMessage + text);

            Invalidate();
        }


        // header
        class ChatControlHeader : Control
        {
            private ChatControl chatControl;

            // Menu Dropdown
            static ContextMenu contextMenu;
            static ChatControl selected = null;

            static ChatControlHeader()
            {
                contextMenu = new ContextMenu();
                //contextMenu.MenuItems.Add(new MenuItem("Close", (s, e) => { App.MainForm.Add }));
                contextMenu.MenuItems.Add(new MenuItem("Add new Split", (s, e) => { App.MainForm?.AddNewSplit(); }, Shortcut.CtrlT));
                contextMenu.MenuItems.Add(new MenuItem("Close Split", (s, e) => { App.MainForm?.RemoveSelectedSplit(); }, Shortcut.CtrlW));
                contextMenu.MenuItems.Add(new MenuItem("Change Channel", (s, e) => { App.MainForm?.RenameSelectedSplit(); }, Shortcut.CtrlR));
                contextMenu.MenuItems.Add(new MenuItem("Login", (s, e) => new LoginForm().ShowDialog(), Shortcut.CtrlL));
                contextMenu.MenuItems.Add(new MenuItem("Preferences", (s, e) => App.ShowSettings(), Shortcut.CtrlP));
            }

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
                    {
                        mouseDown = true;
                        chatControl.Select();
                    }
                };
                MouseUp += (s, e) =>
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        mouseDown = false;
                    }
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

                // Buttons
                ChatControlHeaderButton button = new ChatControlHeaderButton
                {
                    Height = Height - 2,
                    Width = Height - 2,
                    Location = new Point(1, 1),
                    Image = Properties.Resources.settings
                };
                button.MouseDown += (s, e) =>
                {
                    if (e.Button == MouseButtons.Left)
                        chatControl.Select();

                };
                button.Click += (s, e) =>
                {
                    contextMenu.Show(this, new Point(Location.X, Location.Y + Height));
                    selected = chatControl;
                };

                Controls.Add(button);
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

        class ChatControlHeaderButton : Control
        {
            bool mouseOver = false;
            bool mouseDown = false;

            private Image image;

            public Image Image
            {
                get { return image; }
                set { image = value; Invalidate(); }
            }

            void calcSize()
            {
                if (Text != "")
                {
                    Width = 16 + TextRenderer.MeasureText(Text, Font).Width;
                    Invalidate();
                }
            }

            public ChatControlHeaderButton()
            {
                TextChanged += (s, e) => calcSize();
                SizeChanged += (s, e) => calcSize();

                MouseEnter += (s, e) =>
                {
                    mouseOver = true;
                    Invalidate();
                };

                MouseLeave += (s, e) =>
                {
                    mouseOver = false;
                    Invalidate();
                };

                MouseDown += (s, e) =>
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        mouseDown = true;
                        Invalidate();
                    }
                };

                MouseUp += (s, e) =>
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        mouseDown = false;
                        Invalidate();
                    }
                };
            }

            Brush mouseOverBrush = new SolidBrush(Color.FromArgb(48, 255, 255, 255));

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                e.Graphics.FillRectangle(App.ColorScheme.Menu, e.ClipRectangle);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;

                if (mouseDown)
                    g.FillRectangle(mouseOverBrush, 0, 0, Width, Height);

                if (mouseOver)
                    g.FillRectangle(mouseOverBrush, 0, 0, Width, Height);

                if (image != null)
                {
                    g.DrawImage(image, Width / 2 - image.Width / 2, Height / 2 - image.Height / 2);
                }

                if (Text != null)
                {
                    TextRenderer.DrawText(g, Text, Font, Bounds, App.ColorScheme.Text, TextFormatFlags.HorizontalCenter);
                }
            }
        }
    }
}
