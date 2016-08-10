using Chatterino.Common;
using Meebey.SmartIrc4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Message = Chatterino.Common.Message;

namespace Chatterino.Controls
{
    public class ChatControl : ColumnLayoutItemBase
    {
        // Properties
        public const int TopMenuBarHeight = 32;
        public const int ScrollToBottomBarHeight = 24;

        public Padding TextPadding { get; private set; } = new Padding(12, 8 + TopMenuBarHeight, 16 + SystemInformation.VerticalScrollBarWidth, 8);

        ChatControlHeader _header = null;
        public ChatInputControl Input { get; private set; }

        // vars
        private bool scrollAtBottom = true;

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

                    this.Invoke(() =>
                    {
                        _header?.Invalidate();

                        Invalidate();
                    });
                }
            }
        }

        CustomScrollBar _scroll = new CustomScrollBar
        {
            Enabled = false,
            SmallChange = 4,
        };

        // ctor
        public ChatControl()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);

            Input = new ChatInputControl(this);
            Input.Width = 600 - 2 - SystemInformation.VerticalScrollBarWidth;
            Input.Location = new Point(1, Height - 33);

            Input.VisibleChanged += (s, e) =>
            {
                updateMessageBounds();
                Invalidate();
            };

            Input.SizeChanged += (s, e) =>
            {
                Input.Location = new Point(1, Height - Input.Height);

                updateMessageBounds();
                Invalidate();
            };

            Width = 600;
            Height = 500;

            Input.Anchor = AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Left;

            Controls.Add(Input);

            App.GifEmoteFramesUpdated += App_GifEmoteFramesUpdated;
            App.EmoteLoaded += App_EmoteLoaded;

            Disposed += (s, e) =>
            {
                App.GifEmoteFramesUpdated -= App_GifEmoteFramesUpdated;
                App.EmoteLoaded -= App_EmoteLoaded;

                TwitchChannel.RemoveChannel(ChannelName);
            };

            Font = Fonts.Medium;

            ChatControlHeader header = _header = new ChatControlHeader(this);
            Controls.Add(header);

            GotFocus += (s, e) => { Input.Logic.ClearSelection(); header.Invalidate(); };
            LostFocus += (s, e) => { header.Invalidate(); };

            _scroll.Location = new Point(Width - SystemInformation.VerticalScrollBarWidth - 1, TopMenuBarHeight);
            _scroll.Size = new Size(SystemInformation.VerticalScrollBarWidth, Height - TopMenuBarHeight - 1);
            _scroll.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;

            _scroll.Scroll += (s, e) =>
            {
                checkScrollBarPosition();
                updateMessageBounds();
                Invalidate();
            };

            Controls.Add(_scroll);
        }

        // public functions
        private void App_GifEmoteFramesUpdated(object s, EventArgs e)
        {
            channel.Process(c =>
            {
                var g = CreateGraphics();

                lock (c.MessageLock)
                {
                    for (int i = 0; i < c.Messages.Length; i++)
                    {
                        var msg = c.Messages[i];
                        if (msg.IsVisible)
                        {
                            msg.UpdateGifEmotes(g, selection, i);
                        }
                    }
                }
            });
        }

        private void App_EmoteLoaded(object s, EventArgs e)
        {
            updateMessageBounds(true);
            Invalidate();
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

            if (e.Message.Highlighted)
                _scroll.AddHighlight((channel?.MessageCount ?? 1) - 1, Color.Red);

            updateMessageBounds();
            Invalidate();
        }

        private void Channel_MessagesAddedAtStart(object sender, ValueEventArgs<Message[]> e)
        {
            _scroll.UpdateHighlights(h => h.Position += e.Value.Length);

            for (int i = 0; i < e.Value.Length; i++)
            {
                if (e.Value[i].Highlighted)
                    _scroll.AddHighlight(i, Color.Red);
            }

            updateMessageBounds();
            Invalidate();
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
            Invalidate();
        }

        private void Channel_ChatCleared(object sender, ChatClearedEventArgs e)
        {
            this.Invoke(() => Invalidate());
        }

        private void Channel_RoomStateChanged(object sender, EventArgs e)
        {
            _header.Invoke(() =>
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

                    _header.RoomstateButton.Text = text == "" ? "-" : text.TrimEnd(' ', ',', '\n');
                    _header.Invalidate();
                }
            });
        }

        string mouseDownLink = null;
        Word mouseDownWord = null;
        Selection selection = null;
        bool mouseDown = false;

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (_scroll.Enabled)
            {
                _scroll.Value -= ((double)e.Delta / 20);

                if (e.Delta > 0)
                    scrollAtBottom = false;
                else
                    checkScrollBarPosition();

                updateMessageBounds();

                Invalidate();
            }

            base.OnMouseWheel(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!scrollAtBottom && e.Y > Height - (Input.Visible ? Input.Height : 0) - ScrollToBottomBarHeight)
            {
                App.ShowToolTip(PointToScreen(new Point(e.Location.X + 16, e.Location.Y)), "jump to bottom");
                Cursor = Cursors.Hand;
            }
            else
            {
                int index;

                var graphics = CreateGraphics();

                var msg = MessageAtPoint(e.Location, out index);
                if (msg != null)
                {
                    var word = msg.WordAtPoint(new CommonPoint(e.X - TextPadding.Left, e.Y - msg.Y));

                    var pos = msg.MessagePositionAtPoint(graphics, new CommonPoint(e.X - TextPadding.Left, e.Y - msg.Y), index);
                    //Console.WriteLine($"pos: {pos.MessageIndex} : {pos.WordIndex} : {pos.SplitIndex} : {pos.CharIndex}");

                    if (selection != null && mouseDown)
                    {
                        var newSelection = new Selection(selection.Start, pos);
                        if (!newSelection.Equals(selection))
                        {
                            selection = newSelection;
                            Input.Logic.ClearSelection();
                            Invalidate();
                        }
                    }

                    if (word != null)
                    {
                        if (word.Link != null)
                        {
                            Cursor = Cursors.Hand;
                        }
                        else if (word.Type == SpanType.Text)
                        {
                            Cursor = Cursors.IBeam;
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
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            Cursor = Cursors.Default;

            base.OnMouseLeave(e);
        }

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
                    var position = msg.MessagePositionAtPoint(graphics, new CommonPoint(e.X - TextPadding.Left, e.Y - msg.Y), index);
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
                if (!scrollAtBottom && e.Y > Height - (Input.Visible ? Input.Height : 0) - ScrollToBottomBarHeight)
                {
                    App.ShowToolTip(PointToScreen(new Point(e.Location.X, e.Location.Y + 16)), "jump to bottom");

                    mouseDown = false;
                    mouseDownLink = null;

                    scrollAtBottom = true;
                    updateMessageBounds();
                    Invalidate();
                }
                else
                {
                    mouseDown = false;

                    int index;

                    var msg = MessageAtPoint(e.Location, out index);
                    if (msg != null)
                    {
                        var word = msg.WordAtPoint(new CommonPoint(e.X - TextPadding.Left, e.Y - msg.Y));
                        if (word != null)
                        {
                            if (mouseDownLink != null && mouseDownWord == word && !AppSettings.ChatLinksDoubleClickOnly)
                            {
                                GuiEngine.Current.HandleLink(mouseDownLink);
                            }
                        }
                    }

                    mouseDownLink = null;
                }
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            //if ((ModifierKeys & ~Keys.Shift) == Keys.None)
            {
                if (e.KeyChar == '\u007f')
                    return;

                if (e.KeyChar == '\b')
                {
                    resetCompletion();
                }
                else if (e.KeyChar == '\r')
                {
                    var text = Input.Logic.Text;

                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        channel.SendMessage(text);
                        Input.Logic.Clear();
                    }

                    resetCompletion();
                }
                else if (e.KeyChar >= ' ')
                {
                    Input.Logic.InsertText(e.KeyChar.ToString());

                    resetCompletion();
                }

                updateMessageBounds();
                Invalidate();
            }
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            base.OnDoubleClick(e);

            if (AppSettings.ChatLinksDoubleClickOnly)
            {
                if (mouseDownLink != null)
                    GuiEngine.Current.HandleLink(mouseDownLink);
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

            // DRAW MESSAGES
            Message[] M = channel?.CloneMessages();

            if (M != null && M.Length > 0)
            {
                int startIndex = Math.Max(0, (int)_scroll.Value);

                int y = TextPadding.Top - (int)(M[startIndex].Height * (_scroll.Value % 1));
                int h = Height - TextPadding.Top - TextPadding.Bottom;

                for (int i = 0; i < startIndex; i++)
                {
                    M[i].IsVisible = false;
                }

                for (int i = startIndex; i < M.Length; i++)
                {
                    var msg = M[i];
                    msg.IsVisible = true;

                    msg.Draw(e.Graphics, TextPadding.Left, y, selection, i);

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

            e.Graphics.DrawRectangle(borderPen, 0, 0, Width - 1/* - SystemInformation.VerticalScrollBarWidth*/, Height - 1);

            if (!scrollAtBottom)
            {
                int start = Height - (Input.Visible ? Input.Height : 0) - ScrollToBottomBarHeight;

                Brush scrollToBottomBg = new LinearGradientBrush(new Point(0, start), new Point(0, start + ScrollToBottomBarHeight), Color.Transparent, Color.FromArgb(127, 0, 0, 0));

                e.Graphics.FillRectangle(scrollToBottomBg, 1, start, Width - 2, ScrollToBottomBarHeight);
            }
        }

        string[] tabCompleteItems = null;
        int currentTabIndex = -1;

        public void HandleTabCompletion(bool forward)
        {
            string[] items = tabCompleteItems;
            string word = null;

            string text = Input.Logic.Text;
            int caretPositon = Input.Logic.CaretPosition;

            int wordStart = caretPositon - 1;
            int wordEnd = caretPositon;

            for (; wordStart >= 0; wordStart--)
            {
                if (text[wordStart] == ' ')
                {
                    wordStart++;
                    break;
                }
            }
            wordStart = wordStart == -1 ? 0 : wordStart;

            for (; wordEnd < text.Length; wordEnd++)
            {
                if (text[wordEnd] == ' ')
                    break;
            }

            if (wordStart == wordEnd || wordStart == caretPositon)
                return;

            word = text.Substring(wordStart, wordEnd - wordStart).ToUpperInvariant();

            if (items == null)
            {
                items = tabCompleteItems = channel.GetCompletionItems().Where(s => s.Key.StartsWith(word)).Select(x => x.Value).ToArray();
            }

            currentTabIndex += forward ? 1 : -1;

            currentTabIndex = currentTabIndex < 0 ? 0 : (currentTabIndex >= items.Length ? items.Length - 1 : currentTabIndex);

            Input.Logic.SelectionStart = wordStart;
            Input.Logic.SelectionLength = word.Length;

            Input.Logic.InsertText(items[currentTabIndex]);
        }

        private void resetCompletion()
        {
            tabCompleteItems = null;
            currentTabIndex = -1;
        }

        public void HandleArrowKey(Keys keyData)
        {
            switch (keyData)
            {
                // left
                case Keys.Left:
                    Input.Logic.MoveCursorLeft(false, false);
                    break;
                case Keys.Left | Keys.Control:
                    Input.Logic.MoveCursorLeft(true, false);
                    break;
                case Keys.Left | Keys.Shift:
                    Input.Logic.MoveCursorLeft(false, true);
                    break;
                case Keys.Left | Keys.Shift | Keys.Control:
                    Input.Logic.MoveCursorLeft(true, true);
                    break;

                // right
                case Keys.Right:
                    Input.Logic.MoveCursorRight(false, false);
                    break;
                case Keys.Right | Keys.Control:
                    Input.Logic.MoveCursorRight(true, false);
                    break;
                case Keys.Right | Keys.Shift:
                    Input.Logic.MoveCursorRight(false, true);
                    break;
                case Keys.Right | Keys.Shift | Keys.Control:
                    Input.Logic.MoveCursorRight(true, true);
                    break;

                // up + down
                case Keys.Up:
                    break;
                case Keys.Down:
                    break;
            }
        }

        void updateMessageBounds(bool emoteChanged = false)
        {
            var g = CreateGraphics();

            TextPadding = new Padding(TextPadding.Left, TextPadding.Top, TextPadding.Right, 8 + (Input.Visible ? Input.Height : 0));

            // determine if
            double scrollbarThumbHeight = 0;
            int totalHeight = Height - TextPadding.Top - TextPadding.Bottom;
            int currentHeight = 0;
            int tmpHeight = Height - TextPadding.Top - TextPadding.Bottom;
            bool enableScrollbar = false;
            int messageCount = 0;

            var c = channel;

            if (c != null)
            {
                lock (channel.MessageLock)
                {
                    var messages = channel.Messages;
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

                        msg.CalculateBounds(g, Width - TextPadding.Left - TextPadding.Right);
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
                        msg.CalculateBounds(g, Width - TextPadding.Left - TextPadding.Right);
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
            g.Dispose();

            this.Invoke(() =>
            {
                if (enableScrollbar)
                {
                    _scroll.Enabled = true;
                    _scroll.LargeChange = scrollbarThumbHeight;
                    _scroll.Maximum = messageCount - 1;

                    if (scrollAtBottom)
                        _scroll.Value = messageCount - scrollbarThumbHeight;
                }
                else
                {
                    _scroll.Enabled = false;
                    _scroll.Value = 0;
                }
            });
        }

        void checkScrollBarPosition()
        {
            scrollAtBottom = !_scroll.Enabled || _scroll.Maximum < _scroll.Value + _scroll.LargeChange + 0.0001;
        }

        public Message MessageAtPoint(Point p, out int index)
        {
            var c = channel;

            if (c != null)
            {
                lock (c.MessageLock)
                {
                    for (int i = Math.Max(0, (int)_scroll.Value); i < c.Messages.Length; i++)
                    {
                        var m = c.Messages[i];
                        if (m.Y > p.Y - m.Height)
                        {
                            index = i;
                            return m;
                        }
                    }
                }
            }
            index = -1;
            return null;
        }

        public void CopySelection()
        {
            string text = null;

            if (selection?.IsEmpty ?? true)
            {
                text = Input.Logic.SelectedText;
            }
            else
            {
                text = GetSelectedText();
            }

            if (!string.IsNullOrEmpty(text))
                Clipboard.SetText(text);
        }

        public string GetSelectedText()
        {
            if (selection == null || selection.IsEmpty)
                return null;

            StringBuilder b = new StringBuilder();

            var c = channel;

            if (c != null)
            {
                lock (c.MessageLock)
                {
                    bool isFirstLine = true;

                    for (int currentLine = selection.First.MessageIndex; currentLine <= selection.Last.MessageIndex; currentLine++)
                    {
                        if (isFirstLine)
                        {
                            isFirstLine = false;
                        }
                        else
                        {
                            b.Append('\n');
                        }

                        var message = c.Messages[currentLine];

                        var first = selection.First;
                        var last = selection.Last;

                        bool appendNewline = false;

                        for (int i = 0; i < message.Words.Count; i++)
                        {
                            if ((currentLine != first.MessageIndex || i >= first.WordIndex) && (currentLine != last.MessageIndex || i <= last.WordIndex))
                            {
                                var word = message.Words[i];

                                if (appendNewline)
                                {
                                    appendNewline = false;
                                    b.Append(' ');
                                }

                                if (word.Type == SpanType.Text)
                                {
                                    for (int j = 0; j < (word.SplitSegments?.Length ?? 1); j++)
                                    {
                                        if ((first.MessageIndex == currentLine && first.WordIndex == i && first.SplitIndex > j) || (last.MessageIndex == currentLine && last.WordIndex == i && last.SplitIndex < j))
                                            continue;

                                        var split = word.SplitSegments?[j];
                                        string text = split?.Item1 ?? (string)word.Value;
                                        CommonRectangle rect = split?.Item2 ?? new CommonRectangle(word.X, word.Y, word.Width, word.Height);

                                        int textLength = text.Length;

                                        int offset = (first.MessageIndex == currentLine && first.SplitIndex == j && first.WordIndex == i) ? first.CharIndex : 0;
                                        int length = ((last.MessageIndex == currentLine && last.SplitIndex == j && last.WordIndex == i) ? last.CharIndex : textLength) - offset;

                                        b.Append(text.Substring(offset, length));

                                        if (j + 1 == (word.SplitSegments?.Length ?? 1) && ((last.MessageIndex > currentLine) || last.WordIndex > i))
                                            appendNewline = true;
                                        //b.Append(' ');
                                    }
                                }
                                else if (word.Type == SpanType.Image)
                                {
                                    int textLength = word.Type == SpanType.Text ? ((string)word.Value).Length : 2;

                                    int offset = (first.MessageIndex == currentLine && first.WordIndex == i) ? first.CharIndex : 0;
                                    int length = ((last.MessageIndex == currentLine && last.WordIndex == i) ? last.CharIndex : textLength) - offset;

                                    if (word.CopyText != null)
                                    {
                                        if (offset == 0)
                                            b.Append(word.CopyText);
                                        if (offset + length == 2)
                                            appendNewline = true;
                                        //b.Append(' ');
                                    }
                                }
                                else if (word.Type == SpanType.Emote)
                                {
                                    int textLength = word.Type == SpanType.Text ? ((string)word.Value).Length : 2;

                                    int offset = (first.MessageIndex == currentLine && first.WordIndex == i) ? first.CharIndex : 0;
                                    int length = ((last.MessageIndex == currentLine && last.WordIndex == i) ? last.CharIndex : textLength) - offset;

                                    if (word.CopyText != null)
                                    {
                                        if (offset == 0)
                                            b.Append(word.CopyText);
                                        if (offset + length == 2)
                                            appendNewline = true;
                                        //b.Append(' ');
                                    }
                                }
                            }
                        }
                    }
                    //for (int i = selection.First.MessageIndex; i <= selection.Last.MessageIndex; i++)
                    //{
                    //    if (i != selection.First.MessageIndex)
                    //        b.AppendLine();

                    //    for (int j = (i == selection.First.MessageIndex ? selection.First.WordIndex : 0); j < (i == selection.Last.MessageIndex ? selection.Last.WordIndex : c.Messages[i].Words.Count); j++)
                    //    {
                    //        if (c.Messages[i].Words[j].CopyText != null)
                    //        {
                    //            b.Append(c.Messages[i].Words[j].CopyText);
                    //            b.Append(' ');
                    //        }
                    //    }
                    //}
                }
            }

            return b.ToString();
        }

        public void ClearSelection()
        {
            if (!(selection?.IsEmpty ?? true))
            {
                selection = null;

                Invalidate();
            }
        }

        public void PasteText(string text)
        {
            text = Regex.Replace(text, @"\r?\n", " ");

            Input.Logic.InsertText(text);

            Invalidate();
        }

        // header
        class ChatControlHeader : Control
        {
            // static Menu Dropdown
            static ContextMenu contextMenu;
            static ContextMenu roomstateContextMenu;
            static ChatControl selected = null;

            static MenuItem roomstateSlow;
            static MenuItem roomstateSub;
            static MenuItem roomstateEmoteonly;
            static MenuItem roomstateR9K;

            public static MenuItem LoginMenuItem { get; set; }

            static ChatControlHeader()
            {
                contextMenu = new ContextMenu();
                contextMenu.MenuItems.Add(new MenuItem("Add new Split", (s, e) => { App.MainForm?.AddNewSplit(); }, Shortcut.CtrlT));
                contextMenu.MenuItems.Add(new MenuItem("Close Split", (s, e) => { App.MainForm?.RemoveSelectedSplit(); }, Shortcut.CtrlW));
                contextMenu.MenuItems.Add(new MenuItem("Change Channel", (s, e) => { App.MainForm?.RenameSelectedSplit(); }, Shortcut.CtrlR));
                contextMenu.MenuItems.Add(new MenuItem("Clear Chat", (s, e) => { selected.Channel.ClearChat(); }));
                contextMenu.MenuItems.Add("-");
                contextMenu.MenuItems.Add(new MenuItem("Open Channel", (s, e) => { GuiEngine.Current.HandleLink(selected.Channel.ChannelLink); }));
                contextMenu.MenuItems.Add(new MenuItem("Open Pop-out Player", (s, e) => { GuiEngine.Current.HandleLink(selected.Channel.PopoutPlayerLink); }));
                contextMenu.MenuItems.Add("-");
                contextMenu.MenuItems.Add(LoginMenuItem = new MenuItem("Login", (s, e) => new LoginForm().ShowDialog(), Shortcut.CtrlL));
                contextMenu.MenuItems.Add(new MenuItem("Preferences", (s, e) => App.ShowSettings(), Shortcut.CtrlP));
                //contextMenu.MenuItems.Add("-");
                //contextMenu.MenuItems.Add(messageCountItem = new MenuItem("MessageCount: 0", (s, e) => { }) { Enabled = false });

                roomstateContextMenu = new ContextMenu();
                roomstateContextMenu.Popup += (s, e) =>
                {
                    roomstateR9K.Checked = (selected.Channel?.RoomState ?? RoomState.None).HasFlag(RoomState.R9k);
                    roomstateSlow.Checked = (selected.Channel?.RoomState ?? RoomState.None).HasFlag(RoomState.SlowMode);
                    roomstateSub.Checked = (selected.Channel?.RoomState ?? RoomState.None).HasFlag(RoomState.SubOnly);
                    roomstateEmoteonly.Checked = (selected.Channel?.RoomState ?? RoomState.None).HasFlag(RoomState.EmoteOnly);
                };

                roomstateContextMenu.MenuItems.Add(roomstateSlow = new MenuItem("Slowmode", (s, e) =>
                {
                    if (selected.Channel != null)
                    {
                        if (selected.Channel.RoomState.HasFlag(RoomState.SlowMode))
                            selected.Channel.SendMessage("/Slowoff");
                        else
                            selected.Channel.SendMessage("/Slow");
                    }
                }));
                roomstateContextMenu.MenuItems.Add(roomstateSub = new MenuItem("Subscribers Only", (s, e) =>
                {
                    if (selected.Channel != null)
                    {
                        if (selected.Channel.RoomState.HasFlag(RoomState.SubOnly))
                            selected.Channel.SendMessage("/Subscribersoff");
                        else
                            selected.Channel.SendMessage("/Subscribers");
                    }
                }));
                roomstateContextMenu.MenuItems.Add(roomstateR9K = new MenuItem("R9K", (s, e) =>
                {
                    if (selected.Channel != null)
                    {
                        if (selected.Channel.RoomState.HasFlag(RoomState.R9k))
                            selected.Channel.SendMessage("/R9KBetaOff");
                        else
                            selected.Channel.SendMessage("/R9KBeta");
                    }
                }));
                roomstateContextMenu.MenuItems.Add(roomstateEmoteonly = new MenuItem("Emote Only", (s, e) =>
                {
                    if (selected.Channel != null)
                    {
                        if (selected.Channel.RoomState.HasFlag(RoomState.EmoteOnly))
                            selected.Channel.SendMessage("/Emoteonlyoff");
                        else
                            selected.Channel.SendMessage("/emoteonly ");
                    }
                }));

                if (IrcManager.Username != null)
                    LoginMenuItem.Text = "Change User";
                else
                    IrcManager.LoggedIn += (s, e) => LoginMenuItem.Text = "Change User";
            }

            // local controls
            private ChatControl chatControl;

            public ChatControlHeaderButton RoomstateButton { get; private set; }
            public ChatControlHeaderButton DropDownButton { get; private set; }

            // Constructor
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
                ChatControlHeaderButton button = DropDownButton = new ChatControlHeaderButton
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
                    selected = chatControl;
                    contextMenu.Show(this, new Point(Location.X, Location.Y + Height));
                };

                Controls.Add(button);

                RoomstateButton = button = new ChatControlHeaderButton
                {
                    Height = Height - 2,
                    Width = Height - 2,
                    Location = new Point(Width - Height, 1),
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };
                button.Font = new Font(button.Font.FontFamily, 8f);
                button.Text = "-";
                button.MouseDown += (s, e) =>
                {
                    if (e.Button == MouseButtons.Left)
                        chatControl.Select();
                };
                button.Click += (s, e) =>
                {
                    selected = chatControl;
                    roomstateContextMenu.Show(this, new Point(Location.X + Width, Location.Y + Height), LeftRightAlignment.Left);
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
                TextRenderer.DrawText(e.Graphics, title, chatControl.Font, new Rectangle(DropDownButton.Width, 0, Width - DropDownButton.Width - RoomstateButton.Width, Height), chatControl.Focused ? App.ColorScheme.TextFocused : App.ColorScheme.Text, App.DefaultTextFormatFlags | TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
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
                    int width = Width;
                    Width = 16 + TextRenderer.MeasureText(Text, Font).Width;
                    if ((Anchor & AnchorStyles.Right) == AnchorStyles.Right)
                        Location = new Point(Location.X - (Width - width), Location.Y);
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
                    TextRenderer.DrawText(g, Text, Font, ClientRectangle, App.ColorScheme.Text, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            }
        }
    }
}
