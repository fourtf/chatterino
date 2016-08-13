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

        Padding messagePadding = new Padding(12, 4, 12, 8);
        int minHeight;

        public ChatInputControl(ChatControl chatControl)
        {
            SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);

            this.chatControl = chatControl;

            Height = minHeight = TextRenderer.MeasureText("X", Fonts.GdiMedium).Height + 8 + messagePadding.Top + messagePadding.Bottom;

            if (AppSettings.ChatHideInputIfEmpty && Logic.Text.Length == 0)
                Visible = false;

            Logic.Changed += (s, e) =>
            {
                if (AppSettings.ChatHideInputIfEmpty && Logic.Text.Length == 0)
                    Visible = false;
                else
                    Visible = true;

                if (Logic.SelectionLength != 0)
                    chatControl.ClearSelection();

                calculateBounds();
                Invalidate();
                Update();
            };
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            chatControl.Focus();

            base.OnMouseClick(e);
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

                msg.CalculateBounds(g, Width - messagePadding.Left - messagePadding.Right);

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
                g.FillRectangle(new SolidBrush(App.ColorScheme.TextFocused), 8, Height - messagePadding.Bottom, Width - 17, 1);

            var sendMessage = Logic.Message;

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
