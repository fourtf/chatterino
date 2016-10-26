﻿using Chatterino.Common;
using System;
using System.Collections.Generic;
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
            get
            {
                return channel?.MessageLock;
            }
        }

        protected override Message[] Messages
        {
            get
            {
                return channel?.Messages;
            }
        }

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
                }
                else
                {
                    Input.Logic.Channel = null;
                }

                this.Invoke(() =>
                {
                    _header?.Invalidate();

                    updateMessageBounds();

                    Invalidate();
                });

                (Parent as ColumnTabPage)?.UpdateDefaultTitle();
            }
        }

        // last message
        int currentLastMessageIndex = 0;
        List<string> lastMessages = new List<string> { "" };

        // ctor
        public ChatControl()
        {
            MessagePadding = new Padding(12, 8 + TopMenuBarHeight, 16 + SystemInformation.VerticalScrollBarWidth, 8);

            _scroll.Location = new Point(Width - SystemInformation.VerticalScrollBarWidth, TopMenuBarHeight);
            _scroll.Size = new Size(SystemInformation.VerticalScrollBarWidth, Height - TopMenuBarHeight - 2);
            _scroll.Anchor = AnchorStyles.None; // AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Top;

            Input = new ChatInputControl(this);
            Input.Width = 600 - 2;
            Input.Location = new Point(0, Height - 32);

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

            ChatControlHeader header = _header = new ChatControlHeader(this);
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
                        selection = new Selection(selection.Start.WithMessageIndex(selection.Start.MessageIndex - 1), selection.Start.WithMessageIndex(selection.End.MessageIndex - 1));
                }

                _scroll.Value--;

                _scroll.UpdateHighlights(h => h.Position--);
                _scroll.RemoveHighlightsWhere(h => h.Position < 0);
            }

            if (e.Message.HighlightType == HighlightType.Highlighted || e.Message.HighlightType == HighlightType.Resub)
            {
                _scroll.AddHighlight((channel?.MessageCount ?? 1) - 1, (e.Message.HighlightType == HighlightType.Highlighted ? Color.Red : Color.FromArgb(-16777216 | 0x3F6ABF)));

                if (e.Message.HighlightType == HighlightType.Highlighted)
                {
                    ColumnTabPage parent = Parent as ColumnTabPage;

                    if (parent != null)
                    {
                        parent.Highlighted = true;
                    }
                }
            }

            updateMessageBounds();
            Invalidate();
        }

        private void Channel_MessagesAddedAtStart(object sender, ValueEventArgs<Message[]> e)
        {
            _scroll.UpdateHighlights(h => h.Position += e.Value.Length);

            for (int i = 0; i < e.Value.Length; i++)
            {
                if (e.Value[i].HighlightType == HighlightType.Highlighted || e.Value[i].HighlightType == HighlightType.Resub)
                {
                    _scroll.AddHighlight(i, (e.Value[i].HighlightType == HighlightType.Highlighted ? Color.Red : Color.FromArgb(-16777216 | 0x3F6ABF)));
                }
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

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!scrollAtBottom && e.Y > Height - (Input.Visible ? Input.Height : 0) - ScrollToBottomBarHeight)
            {
                App.ShowToolTip(PointToScreen(new Point(e.Location.X + 16, e.Location.Y)), "jump to bottom");
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
                    Invalidate();
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
                    var text = Input.Logic.Text;

                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        channel.SendMessage(text);

                        lastMessages.RemoveAt(lastMessages.Count - 1);
                        if (lastMessages.Count > LastMessagesLimit)
                        {
                            lastMessages.RemoveAt(0);
                        }

                        if (lastMessages.Count == 0 || lastMessages[lastMessages.Count - 1] != text)
                        {
                            lastMessages.Add(text);
                        }
                        lastMessages.Add("");

                        currentLastMessageIndex = lastMessages.Count - 1;

                        if ((ModifierKeys & Keys.Control) != Keys.Control)
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

        protected override void OnPaintOnBuffer(Graphics g)
        {
            if (!scrollAtBottom)
            {
                int start = Height - (Input.Visible ? Input.Height : 0) - ScrollToBottomBarHeight;

                Brush scrollToBottomBg = new LinearGradientBrush(new Point(0, start), new Point(0, start + ScrollToBottomBarHeight), Color.Transparent, Color.FromArgb(48, 0, 0, 0));

                g.FillRectangle(scrollToBottomBg, 1, start, Width - 2, ScrollToBottomBarHeight);
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
                    if (lastMessages.Count != 0)
                    {
                        if (currentLastMessageIndex > 0)
                        {
                            lastMessages[currentLastMessageIndex] = Input.Logic.Text;
                            currentLastMessageIndex--;
                            Input.Logic.SetText(lastMessages[currentLastMessageIndex]);
                        }
                    }
                    break;
                case Keys.Down:
                    if (lastMessages.Count != 0)
                    {
                        if (currentLastMessageIndex < lastMessages.Count - 1)
                        {
                            lastMessages[currentLastMessageIndex] = Input.Logic.Text;
                            currentLastMessageIndex++;
                            Input.Logic.SetText(lastMessages[currentLastMessageIndex]);
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
                    using (InputDialogForm dialog = new InputDialogForm("channel name") { Value = ChannelName })
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
            string[] items = tabCompleteItems;
            string word = null;

            string text = Input.Logic.Text;
            int caretPositon = Input.Logic.CaretPosition;

            int wordStart = caretPositon - 1;
            int wordEnd = caretPositon;

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
                items = tabCompleteItems = channel.GetCompletionItems(wordStart == 0 || (wordStart == 1 && text[0] == '@'), (!text.Trim().StartsWith("!") && !text.Trim().StartsWith("/") && !text.Trim().StartsWith("."))).Where(s => s.Key.StartsWith(word)).Select(x => x.Value).ToArray();
            }

            currentTabIndex += forward ? 1 : -1;

            currentTabIndex = currentTabIndex < 0 ? 0 : (currentTabIndex >= items.Length ? items.Length - 1 : currentTabIndex);

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
                        MessagePadding = new Padding(MessagePadding.Left, MessagePadding.Top, MessagePadding.Right, 10 + (Input.Visible ? Input.Height : 0));
                        _scroll.Location = new Point(Width - SystemInformation.VerticalScrollBarWidth, TopMenuBarHeight + 3);
                        _scroll.Size = new Size(SystemInformation.VerticalScrollBarWidth, Height - TopMenuBarHeight - (Input.Visible ? Input.Height : 0) - 4);
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
                string text = Input.Logic.SelectedText;

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
                contextMenu.MenuItems.Add(new MenuItem("Move Split", (s, e) =>
                    {
                        Cursor.Position = selected._header.PointToScreen(new Point(selected._header.Width / 2, selected._header.Height / 2));
                        selected.Cursor = Cursors.SizeAll;
                    }));
                contextMenu.MenuItems.Add(new MenuItem("Change Channel", (s, e) => { App.MainForm?.RenameSelectedSplit(); }, Shortcut.CtrlR));
                contextMenu.MenuItems.Add(new MenuItem("Clear Chat", (s, e) => { selected.Channel.ClearChat(); }));
                contextMenu.MenuItems.Add("-");
                contextMenu.MenuItems.Add(new MenuItem("Open Channel", (s, e) => { GuiEngine.Current.HandleLink(new Link(LinkType.Url, selected.Channel.ChannelLink)); }));
                contextMenu.MenuItems.Add(new MenuItem("Open Pop-out Player", (s, e) => { GuiEngine.Current.HandleLink(new Link(LinkType.Url, selected.Channel.PopoutPlayerLink)); }));
                contextMenu.MenuItems.Add("-");
                contextMenu.MenuItems.Add(new MenuItem("Manual Reconnect", (s, e) =>
                {
                    IrcManager.Client.Reconnect();
                }));
                contextMenu.MenuItems.Add(new MenuItem("Show Changelog", (s, e) => App.MainForm.ShowChangelog()));
                contextMenu.MenuItems.Add(LoginMenuItem = new MenuItem("Login", (s, e) => new LoginForm().ShowDialog(), Shortcut.CtrlL));
                contextMenu.MenuItems.Add(new MenuItem("Preferences", (s, e) => App.ShowSettings(), Shortcut.CtrlP));
#if DEBUG
                contextMenu.MenuItems.Add(new MenuItem("Copy Version Number", (s, e) => { Clipboard.SetText(App.CurrentVersion.ToString()); }));
                contextMenu.MenuItems.Add(new MenuItem("Force GC", (s, e) => { GC.Collect(); }));
                contextMenu.MenuItems.Add(new MenuItem("test", (s, e) =>
                {
                    for (int i = 0; i < 1000000; i++)
                    {
                        //var a = new SharpDX.DirectWrite.TextLayout(Fonts.Factory, "X", Fonts.GetTextFormat(FontType.Medium), 1000000, 1000000).Metrics;

                        //(App.MainForm.Selected as ChatControl).Process(x =>
                        //{
                        //    x.Channel.AddMessage(new Message("testtesttest"));
                        //});
                    }
                }));
#endif

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

            public FlatButton RoomstateButton { get; private set; }
            public FlatButton DropDownButton { get; private set; }

            // Constructor
            public ChatControlHeader(ChatControl chatControl)
            {
                this.chatControl = chatControl;

                SetStyle(ControlStyles.ResizeRedraw, true);
                SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

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
                            ColumnTabPage layout = chatControl.Parent as ColumnTabPage;
                            if (layout != null)
                            {
                                var position = layout.RemoveWidget(chatControl);
                                if (DoDragDrop(new ColumnLayoutDragDropContainer { Control = chatControl }, DragDropEffects.Move) == DragDropEffects.None)
                                {
                                    layout.AddWidget(chatControl, position.Item1, position.Item2);
                                }
                            }
                        }
                    }
                };

                // Buttons
                FlatButton button = DropDownButton = new FlatButton
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
                e.Graphics.SmoothingMode = SmoothingMode.None;

                // CHANNEL NAME
                e.Graphics.FillRectangle(App.ColorScheme.Menu, 0, 0, Width, TopMenuBarHeight);
                //e.Graphics.DrawRectangle(Focused ? App.ColorScheme.ChatBorderFocused : App.ColorScheme.ChatBorder, 0, 1, Width - 1, Height - 1);
                e.Graphics.DrawRectangle(App.ColorScheme.MenuBorder, 0, 0, Width - 1, Height - 1);

                string title = string.IsNullOrWhiteSpace(chatControl.ActualChannelName) ? "<no channel>" : chatControl.ActualChannelName;

                if (chatControl.IsNetCurrent)
                {
                    title += " (current)";
                }

                TextRenderer.DrawText(e.Graphics, title, chatControl.Font, new Rectangle(DropDownButton.Width, 0, Width - DropDownButton.Width - RoomstateButton.Width, Height), chatControl.Selected ? App.ColorScheme.TextFocused : App.ColorScheme.Text, App.DefaultTextFormatFlags | TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
            }
        }
    }
}
