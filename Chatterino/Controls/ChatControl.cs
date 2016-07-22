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
        public int MaxMessages { get; set; } = 200;

        public const int TopMenuBarHeight = 32;

        public Padding TextPadding { get; set; } = new Padding(12, 12 + TopMenuBarHeight, 16 + SystemInformation.VerticalScrollBarWidth, 4);

        public Message SendMessage { get; set; } = null;

        ChatControlHeader _header = null;

        // vars
        private bool scrollAtBottom = true;

        public int totalMessageHeight = 0;

        Message[] Messages = new Message[0];

        Timer gifEmoteTimer = new Timer { Interval = 33 };

        CustomScrollBar vscroll = new CustomScrollBar
        {
            Enabled = false,
            SmallChange = 32,
        };

        bool isSelecting = false;

        // ctor
        public ChatControl()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);

            IrcManager.IrcMessageReceived += onRawMessage;
            App.EmoteLoaded += onEmoteLoaded;
            //App.GifEmoteFramesUpdated += onEmoteUpdated;

            Disposed += (s, e) =>
            {
                IrcManager.IrcMessageReceived -= onRawMessage;
                App.EmoteLoaded -= onEmoteLoaded;
                //App.GifEmoteFramesUpdated -= onEmoteUpdated;
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

            App.GifEmoteFramesUpdated += onRedrawGifEmotes;

            //gifEmoteTimer.Tick += onEmoteUpdated;
            //gifEmoteTimer.Start();

            //vscroll.Visible = false;
            //MouseEnter += (s, e) => { vscroll.Visible = true; };
            //MouseLeave += (s, e) => { vscroll.Visible = false; };
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            vscroll.Value -= e.Delta;

            Invalidate();

            base.OnMouseWheel(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            int index;

            var msg = MessageAtPoint(e.Location, out index);
            if (msg != null)
            {
                var word = msg.WordAtPoint(new CommonPoint(e.X - TextPadding.Left, e.Y - msg.Y));

                var pos = msg.MessagePositionAtPoint(new CommonPoint(e.X, e.Y - msg.Y), index);
                //Console.WriteLine($"pos: {pos.MessageIndex} : {pos.WordIndex} : {pos.CharIndex}");

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
                    var position = msg.MessagePositionAtPoint(new CommonPoint(e.X, e.Y - msg.Y), index);
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
                foreach (Message msg in Messages)
                {
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
            if (e.Data.RawMessageArray.Length > 2 && e.Data.RawMessageArray[2] == "CLEARCHAT")
            {
                var channel = e.Data.RawMessageArray[3].TrimStart('#');

                if (channel == ChannelName)
                {
                    var reason = e.Data.Tags["ban-reason"];
                    var duration = e.Data.Tags["ban-duration"];
                    var user = e.Data.Message;

                    //lock (Messages)
                    {
                        addMessage(new Message($"{user} was timed out for {duration} second{(duration == "1" ? "" : "s")}{(string.IsNullOrEmpty(reason) ? "." : ": " + reason)}", -8355712));
                        foreach (var msg in Messages)
                        {
                            updateMessageBounds();

                            if (msg.Username == user)
                            {
                                msg.Disabled = true;
                                this.Invoke(() =>
                                {
                                    Invalidate();
                                });
                            }
                        }
                    }
                }
            }

            if ((e.Data.Channel?.Length ?? 0) > 1 && (e.Data.Channel?.Substring(1) ?? "") == ChannelName)
            {
                if (e.Data.RawMessageArray.Length > 4 && e.Data.RawMessageArray[2] == "PRIVMSG")
                {
                    TwitchChannel c;

                    if (IrcManager.Channels.TryGetValue((e.Data.Channel ?? "").TrimStart('#'), out c))
                    {
                        Message firstMessage = null;

                        Message msg = new Message(e.Data, c);
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
            }
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
                }
                else if (e.KeyChar == '\r')
                {
                    if (SendMessage != null)
                    {
                        IrcManager.SendMessage(ircChannelName, SendMessage.RawMessage);
                        SendMessage = null;
                    }
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
                }
                updateMessageBounds();
                Invalidate();
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

            var borderPen = Focused ? App.ColorScheme.ChatBorderFocused : App.ColorScheme.ChatBorder;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int y = (int)(-vscroll.Value + TextPadding.Top);

            // DRAW MESSAGES
            lock (Messages)
            {
                for (int i = 0; i < Messages.Length; i++)
                {
                    var msg = Messages[i];
                    if (y + msg.Height > 0)
                    {
                        if (y > Height)
                        {
                            for (; i < Messages.Length; i++)
                            {
#warning move out of drawing function
                                Messages[i].IsVisible = false;
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

                    for (int j =(i == selection.First.MessageIndex ? selection.First.WordIndex : 0); j < (i == selection.Last.MessageIndex ? selection.Last.WordIndex : Messages[i].Words.Count); j++)
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
