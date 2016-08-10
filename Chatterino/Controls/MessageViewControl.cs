using Chatterino.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Message = Chatterino.Common.Message;

namespace Chatterino.Controls
{
    public class MessageContainerControl : ColumnLayoutItemBase
    {
        public Padding MessagePadding { get; protected set; } = new Padding(8, 8, SystemInformation.VerticalScrollBarWidth + 8, 8);

        protected CustomScrollBar _scroll = new CustomScrollBar
        {
            Enabled = false,
            SmallChange = 4,
        };

        protected bool scrollAtBottom = true;

        private object messageLock = new object();

        protected virtual object MessageLock
        {
            get { return messageLock; }
        }

        Message[] messages = new Message[0];

        protected virtual Message[] Messages
        {
            get { return messages; }
        }

        protected virtual Message[] GetMessagesClone()
        {
            Message[] M;
            lock (MessageLock)
            {
                M = new Message[Messages.Length];
                Array.Copy(Messages, M, M.Length);
            }
            return M;
        }

        // mouse
        protected string mouseDownLink = null;
        protected Word mouseDownWord = null;
        protected Selection selection = null;
        protected bool mouseDown = false;

        // Constructor
        public MessageContainerControl()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            Width = 600;
            Height = 500;

            _scroll.Location = new Point(Width - SystemInformation.VerticalScrollBarWidth - 1, 1);
            _scroll.Size = new Size(SystemInformation.VerticalScrollBarWidth, Height - 1);
            _scroll.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;

            _scroll.Scroll += (s, e) =>
            {
                checkScrollBarPosition();
                updateMessageBounds();
                Invalidate();
            };

            Controls.Add(_scroll);
        }

        // overrides
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
            int index;

            var graphics = CreateGraphics();

            var msg = MessageAtPoint(e.Location, out index);
            if (msg != null)
            {
                var word = msg.WordAtPoint(new CommonPoint(e.X - MessagePadding.Left, e.Y - msg.Y));

                var pos = msg.MessagePositionAtPoint(graphics, new CommonPoint(e.X - MessagePadding.Left, e.Y - msg.Y), index);
                //Console.WriteLine($"pos: {pos.MessageIndex} : {pos.WordIndex} : {pos.SplitIndex} : {pos.CharIndex}");

                if (selection != null && mouseDown)
                {
                    var newSelection = new Selection(selection.Start, pos);
                    if (!newSelection.Equals(selection))
                    {
                        selection = newSelection;
                        clearOtherSelections();
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

            base.OnMouseMove(e);
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
                    var position = msg.MessagePositionAtPoint(graphics, new CommonPoint(e.X - MessagePadding.Left, e.Y - msg.Y), index);
                    selection = new Selection(position, position);

                    var word = msg.WordAtPoint(new CommonPoint(e.X - MessagePadding.Left, e.Y - msg.Y));
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
            mouseDown = false;

            int index;

            var msg = MessageAtPoint(e.Location, out index);
            if (msg != null)
            {
                var word = msg.WordAtPoint(new CommonPoint(e.X - MessagePadding.Left, e.Y - msg.Y));
                if (word != null)
                {
                    if (mouseDownLink != null && mouseDownWord == word && !AppSettings.ChatLinksDoubleClickOnly)
                    {
                        GuiEngine.Current.HandleLink(mouseDownLink);
                    }
                }
            }

            mouseDownLink = null;

            base.OnMouseUp(e);
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
            Message[] M = GetMessagesClone();

            if (M != null && M.Length > 0)
            {
                int startIndex = Math.Max(0, (int)_scroll.Value);

                int y = MessagePadding.Top - (int)(M[startIndex].Height * (_scroll.Value % 1));
                int h = Height - MessagePadding.Top - MessagePadding.Bottom;

                for (int i = 0; i < startIndex; i++)
                {
                    M[i].IsVisible = false;
                }

                for (int i = startIndex; i < M.Length; i++)
                {
                    var msg = M[i];
                    msg.IsVisible = true;

                    msg.Draw(e.Graphics, MessagePadding.Left, y, selection, i);

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
        }

        // Public Functions
        public Message MessageAtPoint(Point p, out int index)
        {
            if (MessageLock != null)
            {
                lock (MessageLock)
                {
                    var messages = Messages;

                    for (int i = Math.Max(0, (int)_scroll.Value); i < messages.Length; i++)
                    {
                        var m = messages[i];
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

            text = GetSelectedText();

            if (!string.IsNullOrEmpty(text))
                Clipboard.SetText(text);
        }

        public virtual string GetSelectedText()
        {
            if (selection == null || selection.IsEmpty)
                return null;

            StringBuilder b = new StringBuilder();

            if (MessageLock != null)
            {
                lock (MessageLock)
                {
                    Message[] messages = Messages;

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

                        var message = messages[currentLine];

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

        // Private Helpers
        protected virtual void clearOtherSelections() { }

        protected virtual void updateMessageBounds(bool emoteChanged = false)
        {
            var g = CreateGraphics();

            // determine if
            double scrollbarThumbHeight = 0;
            int totalHeight = Height - MessagePadding.Top - MessagePadding.Bottom;
            int currentHeight = 0;
            int tmpHeight = Height - MessagePadding.Top - MessagePadding.Bottom;
            bool enableScrollbar = false;
            int messageCount = 0;

            if (MessageLock != null)
            {
                lock (MessageLock)
                {

                    var messages = Messages;
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

                        msg.CalculateBounds(g, Width - MessagePadding.Left - MessagePadding.Right);
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
                        msg.CalculateBounds(g, Width - MessagePadding.Left - MessagePadding.Right);
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

        protected void checkScrollBarPosition()
        {
            scrollAtBottom = !_scroll.Enabled || _scroll.Maximum < _scroll.Value + _scroll.LargeChange + 0.0001;
        }
    }
}
