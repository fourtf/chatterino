using Chatterino.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Message = Chatterino.Common.Message;

namespace Chatterino.Controls
{
    public class ChatControl : MessageContainerControl
    {
        const int LastMessagesLimit = 25;

        // Properties
        public bool IsNetCurrent { get; private set; } = false;

        public const int TopMenuBarHeight = 32;
        public const int ScrollToBottomBarHeight = 24;

        ChatControlHeader _header = null;
        public ChatInputControl Input { get; private set; }

        protected override object MessageLock
        {
            get { return channel?.MessageLock; }
        }

        protected override Message[] Messages
        {
            get { return channel?.Messages; }
        }

        // used to break out of a search loop
        protected long CurrentSearchId = 0;

        // channel
        TwitchChannel channel = null;

        public TwitchChannel Channel
        {
            get { return channel; }
        }

        private string channelName;

        // the channelname, can be /current which will set ActualChannelName to Net.CurrentChannel
        public string ChannelName
        {
            get { return channelName; }
            set
            {
                value = value.Trim();

                if (value != channelName)
                {
                    channelName = value;
                    IsNetCurrent = value == "/current";

                    if (IsNetCurrent)
                    {
                        ActualChannelName = Net.CurrentChannel;
                    }
                    else
                    {
                        ActualChannelName = value;
                    }
                }
            }
        }

        private string actualChannelName;

        private string ActualChannelName
        {
            get { return actualChannelName; }
            set
            {
                _scroll.RemoveHighlightsWhere(x => true);

                if (channel != null)
                {
                    channel.MessageAdded -= Channel_MessageAdded;
                    channel.MessagesAddedAtStart -= Channel_MessagesAddedAtStart;
                    channel.MessagesRemovedAtStart -= Channel_MessagesRemovedAtStart;
                    channel.ChatCleared -= Channel_ChatCleared;
                    channel.RoomStateChanged -= Channel_RoomStateChanged;
                    channel.LiveStatusUpdated -= Channel_LiveStatusUpdated;
                    channel = null;
                    TwitchChannel.RemoveChannel(actualChannelName);
                }

                actualChannelName = value;

                if (!string.IsNullOrWhiteSpace(value))
                {
                    channel = TwitchChannel.AddChannel(value);
                    Input.Logic.Channel = channel;
                    channel.MessageAdded += Channel_MessageAdded;
                    channel.MessagesAddedAtStart += Channel_MessagesAddedAtStart;
                    channel.MessagesRemovedAtStart += Channel_MessagesRemovedAtStart;
                    channel.ChatCleared += Channel_ChatCleared;
                    channel.RoomStateChanged += Channel_RoomStateChanged;
                    channel.LiveStatusUpdated += Channel_LiveStatusUpdated;
                }
                else
                {
                    Input.Logic.Channel = null;
                }

                this.Invoke(() =>
                {
                    _header?.Invalidate();

                    updateMessageBounds();

                    ProposeInvalidation();
                });

                (Parent as ColumnTabPage)?.UpdateDefaultTitle();
            }
        }

        private void Channel_LiveStatusUpdated(object sender, EventArgs e)
        {
            this.Invoke(() => _header.Invalidate());
        }

        // last message
        int _currentLastMessageIndex = 0;
        List<string> _lastMessages = new List<string> { "" };

        // ctor
        public ChatControl()
        {
            MessagePadding = new Padding(12, 8 + TopMenuBarHeight, 16 + SystemInformation.VerticalScrollBarWidth, 8);

            _scroll.Location = new Point(Width - SystemInformation.VerticalScrollBarWidth, TopMenuBarHeight);
            _scroll.Size = new Size(SystemInformation.VerticalScrollBarWidth, Height - TopMenuBarHeight - 2);
            _scroll.Anchor = AnchorStyles.None; // AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Top;

            Input = new ChatInputControl(this)
            {
                Width = 600 - 2,
                Location = new Point(0, Height - 32)
            };

            Input.VisibleChanged += (s, e) =>
            {
                updateMessageBounds();
                Invalidate();
            };

            Input.SizeChanged += (s, e) =>
            {
                Input.Location = new Point(1, Height - Input.Height - 1);

                updateMessageBounds();
                Invalidate();
            };

            Input.Anchor = AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Left;

            Controls.Add(Input);

            Fonts.FontChanged += Fonts_FontChanged;
            Net.CurrentChannelChanged += Net_CurrentChannelChanged;

            Disposed += (s, e) =>
            {
                Fonts.FontChanged -= Fonts_FontChanged;
                Net.CurrentChannelChanged -= Net_CurrentChannelChanged;

                TwitchChannel.RemoveChannel(ActualChannelName);
            };

            //Font = Fonts.GdiMedium;
            Font = new Font("Segoe UI", 9.5f);

            var header = _header = new ChatControlHeader(this);
            header.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            header.Width = Width - 2;
            header.Location = new Point(1, 0);
            Controls.Add(header);

            GotFocus += (s, e) =>
            {
                Input.Logic.ClearSelection();
                Input.Invalidate();
                header.Invalidate();
            };
            LostFocus += (s, e) =>
            {
                header.Invalidate();
                Input.Invalidate();
            };
        }

        private void Net_CurrentChannelChanged(object sender, ValueEventArgs<string> e)
        {
            if (IsNetCurrent)
            {
                ActualChannelName = e.Value;
            }
        }

        private void Fonts_FontChanged(object sender, EventArgs e)
        {
            Task.Delay(150).ContinueWith(task =>
            {
                this.Invoke(() =>
                {
                    OnSizeChanged(EventArgs.Empty);
                    updateMessageBounds();
                    Invalidate();
                });
            });
        }

        // public functions
        private void Channel_MessageAdded(object sender, MessageAddedEventArgs e)
        {
            if (e.RemovedMessage != null)
            {
                if (selection != null)
                {
                    if (selection.Start.MessageIndex == 0)
                        selection = null;
                    else
                        selection = new Selection(selection.Start.WithMessageIndex(selection.Start.MessageIndex - 1),
                            selection.Start.WithMessageIndex(selection.End.MessageIndex - 1));
                }

                _scroll.Value--;

                _scroll.UpdateHighlights(h => h.Position--);
                _scroll.RemoveHighlightsWhere(h => h.Position < 0);
            }

            if ((e.Message.HighlightType & (HighlightType.Highlighted | HighlightType.Resub)) != HighlightType.None)
            {
                _scroll.AddHighlight((channel?.MessageCount ?? 1) - 1,
                    e.Message.HasAnyHighlightType(HighlightType.Highlighted)
                        ? Color.Red
                        : Color.FromArgb(-16777216 | 0x3F6ABF));
            }

            var parent = Parent as ColumnTabPage;

            if (e.Message.HighlightTab && parent != null)
            {
                if (e.Message.HasAnyHighlightType(HighlightType.Highlighted | HighlightType.Whisper))
                {
                    parent.HighlightType = TabPageHighlightType.Highlighted;
                }
                else if (parent.EnableNewMessageHighlights && parent.HighlightType == TabPageHighlightType.None)
                {
                    parent.HighlightType = TabPageHighlightType.NewMessage;
                }
            }

            updateMessageBounds();
            ProposeInvalidation();
        }

        private void Channel_MessagesAddedAtStart(object sender, ValueEventArgs<Message[]> e)
        {
            _scroll.UpdateHighlights(h => h.Position += e.Value.Length);

            for (var i = 0; i < e.Value.Length; i++)
            {
                if (e.Value[i].HasAnyHighlightType(HighlightType.Highlighted | HighlightType.Resub))
                {
                    _scroll.AddHighlight(i,
                        e.Value[i].HasAnyHighlightType(HighlightType.Highlighted)
                            ? Color.Red
                            : Color.FromArgb(-16777216 | 0x3F6ABF));
                }
            }

            updateMessageBounds();
            ProposeInvalidation();
        }

        private void Channel_MessagesRemovedAtStart(object sender, ValueEventArgs<Message[]> e)
        {
            if (selection != null)
            {
                if (selection.Start.MessageIndex < e.Value.Length)
                    selection = null;
                else
                    selection =
                        new Selection(selection.Start.WithMessageIndex(selection.Start.MessageIndex - e.Value.Length),
                            selection.Start.WithMessageIndex(selection.End.MessageIndex - e.Value.Length));
            }

            _scroll.Value -= e.Value.Length;

            _scroll.UpdateHighlights(h => h.Position -= e.Value.Length);
            _scroll.RemoveHighlightsWhere(h => h.Position < 0);

            updateMessageBounds();
            ProposeInvalidation();
        }

        private void Channel_ChatCleared(object sender, ChatClearedEventArgs e)
        {
            this.Invoke(() =>
            {
                updateMessageBounds();
                ProposeInvalidation();
            });
        }

        private void Channel_RoomStateChanged(object sender, EventArgs e)
        {
            _header.Invoke(() =>
            {
                var c = channel;
                if (c != null)
                {
                    var text = "";

                    var state = c.RoomState;
                    var count = 0;
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

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!scrollAtBottom && e.Y > Height - (Input.Visible ? Input.Height : 0) - ScrollToBottomBarHeight)
            {
                //App.ShowToolTip(PointToScreen(new Point(e.Location.X + 16, e.Location.Y)), "jump to bottom");
                Cursor = Cursors.Hand;
            }
            else
            {
                base.OnMouseMove(e);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            //if (e.Button == MouseButtons.Left)
            {
                if (!scrollAtBottom && e.Y > Height - (Input.Visible ? Input.Height : 0) - ScrollToBottomBarHeight)
                {
                    mouseDown = false;
                    mouseDownLink = null;

                    scrollAtBottom = true;
                    updateMessageBounds();
                    ProposeInvalidation();
                }
                else
                {
                    base.OnMouseUp(e);
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

                if (e.KeyChar == ' ' && ModifierKeys == Keys.Control)
                {
                    // show autocomplete
                }
                else if (e.KeyChar == '\x7F')
                {
                    Input.Logic.Delete(true, false);
                }
                else if (e.KeyChar == '\b')
                {
                    resetCompletion();
                }
                else if (e.KeyChar == '\r' || e.KeyChar == '\n')
                {
                    SendMessage((ModifierKeys & Keys.Control) != Keys.Control);
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

        protected override void OnPaintOnBuffer(Graphics g)
        {
            if (!scrollAtBottom)
            {
                var start = Height - (Input.Visible ? Input.Height : 0) - ScrollToBottomBarHeight;

                Brush scrollToBottomBg = new SolidBrush(
                    Color.FromArgb(230, ((SolidBrush)App.ColorScheme.ChatBackground).Color));

                g.FillRectangle(scrollToBottomBg, 1, start, Width - 2, ScrollToBottomBarHeight);

                g.DrawString("Jump to bottom", Font, App.ColorScheme.IsLightTheme ? Brushes.Black : Brushes.White,
                    new Rectangle(
                    1, start, Width - 2, ScrollToBottomBarHeight), new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    });

                scrollToBottomBg.Dispose();
            }
        }

        public override void HandleKeys(Keys keys)
        {
            switch (keys)
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
                    if (_lastMessages.Count != 0)
                    {
                        if (_currentLastMessageIndex > 0)
                        {
                            _lastMessages[_currentLastMessageIndex] = Input.Logic.Text;
                            _currentLastMessageIndex--;
                            Input.Logic.SetText(_lastMessages[_currentLastMessageIndex]);
                        }
                    }
                    break;
                case Keys.Down:
                    if (_lastMessages.Count != 0)
                    {
                        if (_currentLastMessageIndex < _lastMessages.Count - 1)
                        {
                            _lastMessages[_currentLastMessageIndex] = Input.Logic.Text;
                            _currentLastMessageIndex++;
                            Input.Logic.SetText(_lastMessages[_currentLastMessageIndex]);
                        }
                    }
                    break;

                // tabbing
                case Keys.Tab:
                    HandleTabCompletion(true);
                    break;
                case Keys.Shift | Keys.Tab:
                    HandleTabCompletion(false);
                    break;

                // select all
                case (Keys.Control | Keys.A):
                    Input.Logic.SelectAll();
                    break;

                // delete
                case Keys.Back:
                case (Keys.Back | Keys.Control):
                case (Keys.Back | Keys.Shift):
                case Keys.Delete:
                case (Keys.Delete | Keys.Control):
                case (Keys.Delete | Keys.Shift):
                    Input.Logic.Delete((keys & Keys.Control) == Keys.Control, (keys & ~Keys.Control) == Keys.Delete);
                    break;

                // paste
                case Keys.Control | Keys.V:
                    PasteText(Clipboard.GetText());
                    break;

                // home / end
                case Keys.Home:
                    Input.Logic.SetCaretPosition(0);
                    break;
                case Keys.Home | Keys.Shift:
                    Input.Logic.SetSelectionEnd(0);
                    break;
                case Keys.End:
                    Input.Logic.SetCaretPosition(Input.Logic.Text.Length);
                    break;
                case Keys.End | Keys.Shift:
                    Input.Logic.SetSelectionEnd(Input.Logic.Text.Length);
                    break;

                // rename split
                case Keys.Control | Keys.R:
                    using (var dialog = new InputDialogForm("channel name") { Value = ChannelName })
                    {
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            ChannelName = dialog.Value;
                        }
                    }
                    break;

                // default
                default:
                    base.HandleKeys(keys);
                    break;
            }

            base.HandleKeys(keys);
        }

        string[] tabCompleteItems = null;
        int currentTabIndex = -1;

        public void HandleTabCompletion(bool forward)
        {
            var items = tabCompleteItems;
            string word = null;

            var text = Input.Logic.Text;
            var caretPositon = Input.Logic.CaretPosition;

            var wordStart = caretPositon - 1;
            var wordEnd = caretPositon;

            for (; wordStart >= 0; wordStart--)
            {
                if (text[wordStart] != ' ' && text[wordStart] != ',')
                {
                    break;
                }
            }
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
                items =
                    tabCompleteItems =
                        channel.GetCompletionItems(wordStart == 0 || (wordStart == 1 && text[0] == '@'),
                            (!text.Trim().StartsWith("!") && !text.Trim().StartsWith("/") &&
                             !text.Trim().StartsWith(".")))
                            .Where(s => s.Key.StartsWith(word))
                            .Select(x => x.Value)
                            .ToArray();
            }

            currentTabIndex += forward ? 1 : -1;

            currentTabIndex = currentTabIndex < 0
                ? 0
                : (currentTabIndex >= items.Length ? items.Length - 1 : currentTabIndex);

            if (currentTabIndex != -1 && items.Length != 0)
            {
                Input.Logic.SelectionStart = wordStart;
                Input.Logic.SelectionLength = word.Length;

                Input.Logic.InsertText(items[currentTabIndex] + " ");
            }
        }

        private void resetCompletion()
        {
            tabCompleteItems = null;
            currentTabIndex = -1;
        }

        protected override void clearOtherSelections()
        {
            base.clearOtherSelections();

            Input.Logic.ClearSelection();
        }

        protected override void updateMessageBounds(bool emoteChanged = false)
        {
            if (Parent?.Parent != null)
            {
                if (Input != null)
                {
                    this.Invoke(() =>
                    {
                        MessagePadding = new Padding(MessagePadding.Left, MessagePadding.Top, MessagePadding.Right,
                            10 + (Input.Visible ? Input.Height : 0));
                        _scroll.Location = new Point(Width - SystemInformation.VerticalScrollBarWidth,
                            TopMenuBarHeight + 3);
                        _scroll.Size = new Size(SystemInformation.VerticalScrollBarWidth,
                            Height - TopMenuBarHeight - (Input.Visible ? Input.Height : 0) - 4);
                    });
                }

                base.updateMessageBounds(emoteChanged);
            }
        }

        public void PasteText(string text)
        {
            text = Regex.Replace(text, @"\r?\n", " ");

            Input.Logic.InsertText(text);

            Invalidate();
        }

        public override string GetSelectedText(bool clear)
        {
            if (selection?.IsEmpty ?? true)
            {
                var text = Input.Logic.SelectedText;

                if (clear && text.Length > 0)
                    Input.Logic.InsertText("");

                return text;
            }
            else
            {
                return base.GetSelectedText(clear);
            }
        }

        protected override Message[] GetMessagesClone()
        {
            return channel?.CloneMessages();
        }

        protected override void OnSplitDragStart()
        {
            base.OnSplitDragStart();

            mouseDown = false;
            mouseDownLink = null;
        }

        public void SendMessage(bool clear)
        {
            var text = Input.Logic.Text;

            if (!string.IsNullOrWhiteSpace(text))
            {
                channel.SendMessage(text);

                _lastMessages.RemoveAt(_lastMessages.Count - 1);
                if (_lastMessages.Count > LastMessagesLimit)
                {
                    _lastMessages.RemoveAt(0);
                }

                if (_lastMessages.Count == 0 || _lastMessages[_lastMessages.Count - 1] != text)
                {
                    _lastMessages.Add(text);
                }
                _lastMessages.Add("");

                _currentLastMessageIndex = _lastMessages.Count - 1;

                if (clear)
                    Input.Logic.Clear();
            }

            resetCompletion();
        }

        public ConcurrentDictionary<long, object> SearchResults { get; } = new ConcurrentDictionary<long, object>();

        public void SearchFor(string term)
        {
            //    var searchId = ++CurrentSearchId;

            //    var messages = Channel.CloneMessages();

            //    var results = new Queue<Message>();

            //    for (var i = messages.Length - 1; i >= 0; i--)
            //    {
            //        if (searchId != CurrentSearchId)
            //            return;

            //        var message = messages[i];

            //        if ((message.Username != null &&
            //             message.Username.IndexOf(term, StringComparison.OrdinalIgnoreCase) != -1) ||
            //            message.RawMessage.IndexOf(term, StringComparison.OrdinalIgnoreCase) != -1)
            //        {
            //            message.HighlightType |= HighlightType.SearchResult;

            //            results.Enqueue(message);
            //        }
            //    }

            //    lock (Channel.MessageLock)
            //    {
            //        messages = Channel.Messages;

            //        if (results.Count != 0)
            //        {
            //            for (var i = messages.Length - 1; i >= 0; i--)
            //            {
            //                var message = messages[i];
            //                var peek = results.Peek();

            //                if (message.Id == peek.Id)
            //                {
            //                    message.HighlightType |= HighlightType.SearchResult;
            //                    _scroll.AddHighlight(i, Color.GreenYellow, ScrollBarHighlightStyle.Right);
            //                    SearchResults[results.Dequeue().Id] = null;

            //                    if (results.Count == 0)
            //                        break;
            //                }
            //            }
            //        }
            //    }

            //    this.Invoke(Invalidate);
        }

        public void ClearSearchHighlights()
        {

        }

        // header
        private class ChatControlHeader : Control
        {
            // static Menu Dropdown
            static ContextMenu _contextMenu;
            static ContextMenu _roomstateContextMenu;
            static ChatControl _selected = null;

            static MenuItem _roomstateSlow;
            static MenuItem _roomstateSub;
            static MenuItem _roomstateEmoteonly;
            static MenuItem _roomstateR9K;

            public static MenuItem LoginMenuItem { get; set; }

            static ChatControlHeader()
            {
                _contextMenu = new ContextMenu();
                _contextMenu.MenuItems.Add(new MenuItem("Add new Split", (s, e) => { App.MainForm?.AddNewSplit(); },
                    Shortcut.CtrlT));
                _contextMenu.MenuItems.Add(new MenuItem("Close Split",
                    (s, e) => { App.MainForm?.RemoveSelectedSplit(); }, Shortcut.CtrlW));
                _contextMenu.MenuItems.Add(new MenuItem("Move Split", (s, e) =>
                {
                    Cursor.Position =
                        _selected._header.PointToScreen(new Point(_selected._header.Width / 2, _selected._header.Height / 2));
                    _selected.Cursor = Cursors.SizeAll;
                }));
                _contextMenu.MenuItems.Add(new MenuItem("Change Channel",
                    (s, e) => { App.MainForm?.RenameSelectedSplit(); }, Shortcut.CtrlR));
                _contextMenu.MenuItems.Add(new MenuItem("Clear Chat", (s, e) => { _selected.Channel.ClearChat(true); }));
                _contextMenu.MenuItems.Add("-");
                _contextMenu.MenuItems.Add(new MenuItem("Open Channel",
                    (s, e) => { GuiEngine.Current.HandleLink(new Link(LinkType.Url, _selected.Channel.ChannelLink)); }));
                _contextMenu.MenuItems.Add(new MenuItem("Open Pop-out Player",
                    (s, e) =>
                    {
                        GuiEngine.Current.HandleLink(new Link(LinkType.Url, _selected.Channel.PopoutPlayerLink));
                    }));
                _contextMenu.MenuItems.Add(new MenuItem("Open Streamlink",
                    (s, e) =>
                    {
                        if (AppSettings.EnableStreamlinkPath)
                        {
                            var strCmdText = _selected.Channel.ChannelLink + " " + AppSettings.Quality;
                            Process.Start(AppSettings.StreamlinkPath, strCmdText);
                        }
                        else
                        {
                            var strCmdText = _selected.Channel.ChannelLink + " " + AppSettings.Quality;
                            Process.Start("streamlink", strCmdText);
                        }
                    }));
                _contextMenu.MenuItems.Add("-");
                _contextMenu.MenuItems.Add(new MenuItem("Reload Channel Emotes", (s, e) =>
                {
                    _selected.Channel.ReloadEmotes();
                }));
                _contextMenu.MenuItems.Add(new MenuItem("Manual Reconnect", (s, e) =>
                {
                    IrcManager.Client.Reconnect();
                }));
                _contextMenu.MenuItems.Add(new MenuItem("Show Changelog", (s, e) => App.MainForm.ShowChangelog()));
                //contextMenu.MenuItems.Add(LoginMenuItem = new MenuItem("Login", (s, e) => new LoginForm().ShowDialog(), Shortcut.CtrlL));
                //contextMenu.MenuItems.Add(new MenuItem("Preferences", (s, e) => App.ShowSettings(), Shortcut.CtrlP));
#if DEBUG
                _contextMenu.MenuItems.Add(new MenuItem("Copy Version Number",
                    (s, e) => { Clipboard.SetText(App.CurrentVersion.ToString()); }));
#endif

                _roomstateContextMenu = new ContextMenu();
                _roomstateContextMenu.Popup += (s, e) =>
                {
                    _roomstateR9K.Checked = (_selected.Channel?.RoomState ?? RoomState.None).HasFlag(RoomState.R9k);
                    _roomstateSlow.Checked = (_selected.Channel?.RoomState ?? RoomState.None).HasFlag(RoomState.SlowMode);
                    _roomstateSub.Checked = (_selected.Channel?.RoomState ?? RoomState.None).HasFlag(RoomState.SubOnly);
                    _roomstateEmoteonly.Checked =
                        (_selected.Channel?.RoomState ?? RoomState.None).HasFlag(RoomState.EmoteOnly);
                };

                _roomstateContextMenu.MenuItems.Add(_roomstateSlow = new MenuItem("Slowmode", (s, e) =>
                {
                    if (_selected.Channel != null)
                    {
                        if (_selected.Channel.RoomState.HasFlag(RoomState.SlowMode))
                            _selected.Channel.SendMessage("/Slowoff");
                        else
                            _selected.Channel.SendMessage("/Slow");
                    }
                }));
                _roomstateContextMenu.MenuItems.Add(_roomstateSub = new MenuItem("Subscribers Only", (s, e) =>
                {
                    if (_selected.Channel != null)
                    {
                        if (_selected.Channel.RoomState.HasFlag(RoomState.SubOnly))
                            _selected.Channel.SendMessage("/Subscribersoff");
                        else
                            _selected.Channel.SendMessage("/Subscribers");
                    }
                }));
                _roomstateContextMenu.MenuItems.Add(_roomstateR9K = new MenuItem("R9K", (s, e) =>
                {
                    if (_selected.Channel != null)
                    {
                        if (_selected.Channel.RoomState.HasFlag(RoomState.R9k))
                            _selected.Channel.SendMessage("/R9KBetaOff");
                        else
                            _selected.Channel.SendMessage("/R9KBeta");
                    }
                }));
                _roomstateContextMenu.MenuItems.Add(_roomstateEmoteonly = new MenuItem("Emote Only", (s, e) =>
                {
                    if (_selected.Channel != null)
                    {
                        if (_selected.Channel.RoomState.HasFlag(RoomState.EmoteOnly))
                            _selected.Channel.SendMessage("/Emoteonlyoff");
                        else
                            _selected.Channel.SendMessage("/emoteonly ");
                    }
                }));

                //if (IrcManager.Account.Username != null)
                //    LoginMenuItem.Text = "Change User";
                //else
                //    IrcManager.LoggedIn += (s, e) => LoginMenuItem.Text = "Change User";
            }

            // local controls
            private ChatControl _chatControl;

            public FlatButton RoomstateButton { get; private set; }
            public FlatButton DropDownButton { get; private set; }

            TooltipValue tooltipValue = new TooltipValue();

            // Constructor
            public ChatControlHeader(ChatControl chatControl)
            {
                _chatControl = chatControl;

                this.SetTooltip(tooltipValue);

                SetStyle(ControlStyles.ResizeRedraw, true);
                SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

                Height = TopMenuBarHeight + 1;

                // Mousedown
                var mouseDown = false;

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
                            var layout = chatControl.Parent as ColumnTabPage;
                            if (layout != null)
                            {
                                var position = layout.RemoveWidget(chatControl);
                                if (
                                    DoDragDrop(new ColumnLayoutDragDropContainer { Control = chatControl },
                                        DragDropEffects.Move) == DragDropEffects.None)
                                {
                                    layout.AddWidget(chatControl, position.Item1, position.Item2);
                                }
                            }
                        }
                    }
                };

                // Buttons
                var button = DropDownButton = new FlatButton
                {
                    Height = Height - 2,
                    Width = Height - 2,
                    Location = new Point(1, 1),
                    Image = Properties.Resources.tool_moreCollapser_off16
                };
                button.MouseDown += (s, e) =>
                {
                    if (e.Button == MouseButtons.Left)
                        chatControl.Select();

                };
                button.Click += (s, e) =>
                {
                    _selected = chatControl;
                    _contextMenu.Show(this, new Point(Location.X, Location.Y + Height));
                };

                Controls.Add(button);

                RoomstateButton = button = new FlatButton
                {
                    Height = Height - 2,
                    Width = Height - 2,
                    MinimumSize = new Size(Height - 2, Height - 2),
                    Location = new Point(Width - Height, 1),
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };
                button.Text = "-";
                button.MouseDown += (s, e) =>
                {
                    if (e.Button == MouseButtons.Left)
                        chatControl.Select();
                };
                button.Click += (s, e) =>
                {
                    _selected = chatControl;
                    _roomstateContextMenu.Show(this, new Point(Location.X + Width, Location.Y + Height),
                        LeftRightAlignment.Left);
                };

                Controls.Add(button);
            }

            protected override void OnDoubleClick(EventArgs e)
            {
                base.OnDoubleClick(e);

                using (var dialog = new InputDialogForm("channel name") { Value = _chatControl.ChannelName })
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        _chatControl.ChannelName = dialog.Value;
                    }
                }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                e.Graphics.SmoothingMode = SmoothingMode.None;

                // CHANNEL NAME
                e.Graphics.FillRectangle(App.ColorScheme.Menu, 0, 0, Width, TopMenuBarHeight);
                //e.Graphics.DrawRectangle(Focused ? App.ColorScheme.ChatBorderFocused : App.ColorScheme.ChatBorder, 0, 1, Width - 1, Height - 1);
                e.Graphics.DrawRectangle(App.ColorScheme.MenuBorder, 0, 0, Width - 1, Height - 1);

                var title1 = string.IsNullOrWhiteSpace(_chatControl.ActualChannelName)
                    ? "<no channel>"
                    : _chatControl.ActualChannelName;
                string title2 = null;

                if (_chatControl.IsNetCurrent)
                {
                    title1 += " (current)";
                }

                var channel = _chatControl.Channel;

                if (_chatControl?.Channel?.IsLive ?? false)
                {
                    title1 += " (live)";

                    //title2 = _chatControl.Channel.StreamStatus + "";

                    var text =
                        _chatControl.Channel.StreamStatus + "\n\n" +
                        _chatControl.Channel.StreamGame + "\n" +
                        "Live for ";

                    var uptime = DateTime.Now - channel.StreamStart;

                    if (uptime.TotalDays > 1)
                    {
                        text += (int)uptime.TotalDays + " days, " + uptime.ToString("h\\h\\ m\\m");
                    }
                    else
                    {
                        text += uptime.ToString("h\\h\\ m\\m");
                    }

                    text += " with " + channel.StreamViewerCount + " viewers";

                    tooltipValue.Value = text;
                }
                else
                {
                    tooltipValue.Value = null;
                }

                var flags = App.DefaultTextFormatFlags | TextFormatFlags.VerticalCenter;

                if (TextRenderer.MeasureText(e.Graphics, title1, _chatControl.Font).Width <=
                    Width - DropDownButton.Width - RoomstateButton.Width)
                {
                    flags |= TextFormatFlags.HorizontalCenter;
                }

                TextRenderer.DrawText(e.Graphics, title1, _chatControl.Font,
                    new Rectangle(DropDownButton.Width, 0, Width - DropDownButton.Width - RoomstateButton.Width,
                        (title2 == null ? Height : Height / 2)),
                    _chatControl.Selected ? App.ColorScheme.TextFocused : App.ColorScheme.Text,
                    flags);

                if (title2 != null)
                {
                    flags = App.DefaultTextFormatFlags | TextFormatFlags.VerticalCenter;

                    if (TextRenderer.MeasureText(e.Graphics, title2, _chatControl.Font).Width <=
                        Width - DropDownButton.Width - RoomstateButton.Width)
                    {
                        flags |= TextFormatFlags.HorizontalCenter;
                    }

                    TextRenderer.DrawText(e.Graphics, title2, _chatControl.Font,
                        new Rectangle(DropDownButton.Width, Height / 2,
                            Width - DropDownButton.Width - RoomstateButton.Width,
                            Height / 2),
                        App.ColorScheme.Text,
                        flags);
                }
            }
        }
    }
}
