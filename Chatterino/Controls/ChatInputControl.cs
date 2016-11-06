using Chatterino.Common;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Message = Chatterino.Common.Message;
using SCB = SharpDX.Direct2D1.SolidColorBrush;

namespace Chatterino.Controls
{
    public class ChatInputControl : Control
    {
        public MessageInputLogic Logic { get; private set; } = new MessageInputLogic();

        private ChatControl chatControl;
        private Rectangle? caretRect = null;

        Timer caretBlinkTimer;
        bool caretBlinkState = true;

        Padding messagePadding = new Padding(12, 4, 12, 8);
        int minHeight;

        FlatButton emoteListButton;

        public ChatInputControl(ChatControl chatControl)
        {
            Size = new Size(100, 100);

            int caretBlinkInterval = SystemInformation.CaretBlinkTime;

            if (caretBlinkInterval > 0) {
                caretBlinkTimer = new Timer { Interval = SystemInformation.CaretBlinkTime };

                caretBlinkTimer.Tick += (s, e) =>
                {
                    if (caretRect != null)
                    {
                        using (var g = CreateGraphics())
                        {
                            caretBlinkState = !caretBlinkState;
                        }
                    }
                };
            }

            Cursor = Cursors.IBeam;

            SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);

            this.chatControl = chatControl;

            {
                Graphics g = App.UseDirectX ? null : CreateGraphics();

                Height = minHeight = GuiEngine.Current.MeasureStringSize(g, FontType.Medium, "X").Height + 8 + messagePadding.Top + messagePadding.Bottom;

                g?.Dispose();
            }

            if (AppSettings.ChatHideInputIfEmpty && Logic.Text.Length == 0)
                Visible = false;

            Logic.Changed += (s, e) =>
            {
                if (AppSettings.ChatHideInputIfEmpty && Logic.Text.Length == 0) {
                    Visible = false;
                } else {
                    Visible = true;
                }

                if (Logic.SelectionLength != 0)
                    chatControl.ClearSelection();

                if (caretBlinkTimer != null) {
                    caretBlinkTimer.Stop();
                    caretBlinkTimer.Start();
                }

                caretBlinkState = true;

                calculateBounds();
                Invalidate();
                Update();
            };

            // emote button
            emoteListButton = new FlatButton();
            emoteListButton.Image = (Image)GuiEngine.Current.ScaleImage(Properties.Resources.Emoji_Color_1F607_19, 0.85);
            emoteListButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            emoteListButton.Size = new Size(16, 16);
            emoteListButton.Cursor = Cursors.Default;
            emoteListButton.Location = new Point(Width - emoteListButton.Width - 1, Height - emoteListButton.Height - 1);

            emoteListButton.Click += (s, e) =>
            {
                chatControl.Focus();
                App.ShowEmoteList(chatControl.Channel);
            };

            Controls.Add(emoteListButton);
        }

        bool mdown = false;

        protected override void OnMouseMove(MouseEventArgs e)
        {
            Graphics g = App.UseDirectX ? null : CreateGraphics();

            if (mdown)
            {
                if (Logic.Message != null)
                {
                    var pos = getIndexFromMessagePosition(Logic.Message.MessagePositionAtPoint(g, new CommonPoint(e.X - messagePadding.Left, e.Y - messagePadding.Top), 0));

                    Logic.SetSelectionEnd(pos);
                }
            }

            g?.Dispose();

            base.OnMouseMove(e);
        }

        private int getIndexFromMessagePosition(MessagePosition pos)
        {
            var msg = Logic.Message;

            int position = 0;

            for (int wordIndex = 0; wordIndex <= pos.WordIndex; wordIndex++)
            {
                var word = msg.Words[wordIndex];

                for (int splitIndex = 0; splitIndex <= (wordIndex == pos.WordIndex ? pos.SplitIndex : (word.SplitSegments?.Length ?? 0)); splitIndex++)
                {
                    string split = word.SplitSegments?[splitIndex].Item1 ?? word.Value as string;

                    if (pos.WordIndex == wordIndex && pos.SplitIndex == splitIndex)
                    {
                        for (int i = 0; i < pos.CharIndex; i++)
                        {
                            position++;
                        }
                        goto end;
                    }
                    else
                    {
                        position += split.Length + 1;
                    }
                }
            }

            end:
            return position;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            Graphics g = App.UseDirectX ? null : CreateGraphics();

            if (e.Button == MouseButtons.Left)
            {
                mdown = true;

                if (Logic.Message != null)
                {
                    Logic.SetSelectionEnd(Logic.SelectionStart = getIndexFromMessagePosition(Logic.Message.MessagePositionAtPoint(g, new CommonPoint(e.X - messagePadding.Left, e.Y - messagePadding.Top), 0)));
                }
            }

            g?.Dispose();

            chatControl.Focus();

            base.OnMouseClick(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mdown = false;
            }

            base.OnMouseUp(e);
        }

        protected override void OnResize(EventArgs e)
        {
            calculateBounds();

            base.OnResize(e);
        }

        void calculateBounds()
        {
            var msg = Logic.Message;
            if (msg != null)
            {
                Graphics g = App.UseDirectX ? null : CreateGraphics();

                msg.CalculateBounds(g, Width - messagePadding.Left - messagePadding.Right - 20);

                g?.Dispose();

                Height = Math.Max(msg.Height + messagePadding.Top + messagePadding.Bottom, minHeight);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(App.ColorScheme.ChatBackground, ClientRectangle);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;

            g.FillRectangle(App.ColorScheme.ChatInputOuter, 0, 0, Width - 1, Height - 1);
            //g.FillRectangle(App.ColorScheme.ChatInputInner, 8, 4, Width - 17, Height - 9);
            //g.DrawRectangle(chatControl.Focused ? new Pen(App.ColorScheme.TextFocused) : App.ColorScheme.ChatInputBorder, 0, 0, Width - 1, Height - 1);
            g.DrawRectangle(App.ColorScheme.ChatInputBorder, 0, 0, Width - 1, Height - 1);

            if (chatControl.Focused)
                g.FillRectangle(new SolidBrush(App.ColorScheme.TextFocused), 8, Height - messagePadding.Bottom, Width - 17 - 16, 1);

            var sendMessage = Logic.Message;

            if (AppSettings.ChatInputShowMessageLength)
            {
                if (Logic.MessageLength > 1)
                {
                    string messageLength = Logic.MessageLength.ToString();
                    var size = TextRenderer.MeasureText(e.Graphics, messageLength, Font, Size.Empty, App.DefaultTextFormatFlags);
                    TextRenderer.DrawText(e.Graphics, messageLength, Font, new Point(Width - size.Width - 4, 0), Logic.MessageLength > 500 ? Color.Red : App.ColorScheme.Text, App.DefaultTextFormatFlags);
                }
            }

            if (sendMessage != null)
            {
                Selection selection = Logic.Selection;

                MessageRenderer.DrawMessage(e.Graphics, sendMessage, messagePadding.Left, messagePadding.Top, selection, 0, !App.UseDirectX);

                int spaceWidth = GuiEngine.Current.MeasureStringSize(g, FontType.Medium, " ").Width;

                Rectangle? caretRect = new Rectangle?();

                int x = 0;
                bool isFirst = true;

                if (sendMessage.RawMessage.Length > 0)
                {
                    foreach (var word in sendMessage.Words)
                    {
                        for (int j = 0; j < (word.SplitSegments?.Length ?? 1); j++)
                        {
                            string text = word.SplitSegments?[j].Item1 ?? (string)word.Value;

                            if (j == 0)
                                if (isFirst)
                                    isFirst = false;
                                else
                                {
                                    if (x == Logic.CaretPosition)
                                    {
                                        caretRect = new Rectangle(messagePadding.Left + word.X - spaceWidth, word.Y + messagePadding.Top, 1, word.Height);
                                        goto end;
                                    }
                                    x++;
                                }

                            for (int i = 0; i < text.Length; i++)
                            {
                                if (x == Logic.CaretPosition)
                                {
                                    var size = GuiEngine.Current.MeasureStringSize(App.UseDirectX ? null : g, word.Font, text.Remove(i));
                                    caretRect = new Rectangle(messagePadding.Left + (word.SplitSegments?[j].Item2.X ?? word.X) + size.Width,
                                        (word.SplitSegments?[j].Item2.Y ?? word.Y) + messagePadding.Top,
                                        1,
                                        word.Height);
                                    goto end;
                                }
                                x++;
                            }
                        }
                    }

                    var _word = sendMessage.Words[sendMessage.Words.Count - 1];
                    var _lastSegmentText = _word.SplitSegments?[_word.SplitSegments.Length - 1].Item1;
                    var _lastSegment = _word.SplitSegments?[_word.SplitSegments.Length - 1].Item2;
                    caretRect = _word.SplitSegments == null ? new Rectangle(messagePadding.Left + _word.X + _word.Width, _word.Y + messagePadding.Top, 1, _word.Height) : new Rectangle(messagePadding.Left + _lastSegment.Value.X + GuiEngine.Current.MeasureStringSize(App.UseDirectX ? null : g, _word.Font, _lastSegmentText).Width, _lastSegment.Value.Y + messagePadding.Top, 1, _lastSegment.Value.Height);

                    end:

                    if (caretRect != null)
                        g.FillRectangle(App.ColorScheme.TextCaret, caretRect.Value);
                }

                if (App.UseDirectX)
                {
                    SharpDX.Direct2D1.DeviceContextRenderTarget renderTarget = null;
                    IntPtr dc = IntPtr.Zero;

                    dc = g.GetHdc();

                    renderTarget = new SharpDX.Direct2D1.DeviceContextRenderTarget(MessageRenderer.D2D1Factory, MessageRenderer.RenderTargetProperties);

                    renderTarget.BindDeviceContext(dc, new RawRectangle(0, 0, Width, Height));

                    renderTarget.BeginDraw();

                    Dictionary<RawColor4, SCB> brushes = new Dictionary<RawColor4, SCB>();

                    var textColor = App.ColorScheme.Text;
                    var textBrush = new SCB(renderTarget, new RawColor4(textColor.R / 255f, textColor.G / 255f, textColor.B / 255f, 1));

                    foreach (Word word in sendMessage.Words)
                    {
                        if (word.Type == SpanType.Text)
                        {
                            if (word.SplitSegments == null)
                            {
                                renderTarget.DrawText((string)word.Value, Fonts.GetTextFormat(word.Font), new RawRectangleF(messagePadding.Left + word.X, messagePadding.Top + word.Y, 10000, 1000), textBrush);
                            }
                            else
                            {
                                foreach (var split in word.SplitSegments)
                                    renderTarget.DrawText(split.Item1, Fonts.GetTextFormat(word.Font), new RawRectangleF(messagePadding.Left + split.Item2.X, messagePadding.Top + split.Item2.Y, 10000, 1000), textBrush);
                            }
                        }
                    }

                    renderTarget.EndDraw();

                    textBrush.Dispose();
                    g.ReleaseHdc(dc);
                    renderTarget.Dispose();
                }
            }
        }
    }
}
