using Chatterino.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Message = Chatterino.Common.Message;

namespace Chatterino.Controls
{
    public class ChatInputControl : Control
    {
        public MessageInputLogic Logic { get; private set; } = new MessageInputLogic();

        private ChatControl chatControl;

        Padding textPadding = new Padding(8, 0, 8, 0);
        int minHeight;

        public ChatInputControl(ChatControl chatControl)
        {
            SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);

            this.chatControl = chatControl;

            Height = minHeight = TextRenderer.MeasureText("X", Fonts.Medium).Height + 8;

            Logic.Changed += (s, e) =>
            {
                if (Logic.SelectionLength != 0)
                    chatControl.ClearSelection();

                calculateBounds();
                Invalidate();
                Update();
            };
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
                using (var g = CreateGraphics())
                    msg.CalculateBounds(g, Width - textPadding.Left - textPadding.Right);

                Height = Math.Max(msg.Height, minHeight);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(App.ColorScheme.ChatBackground, ClientRectangle);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;

            g.DrawRectangle(Pens.Black, 0, 0, Width - 1, Height - 1);

            var sendMessage = Logic.Message;

            if (sendMessage != null)
            {
                Selection selection = Logic.Selection;

                sendMessage.Draw(e.Graphics, textPadding.Left, 0, selection, 0);

                int spaceWidth = GuiEngine.Current.MeasureStringSize(g, FontType.Medium, " ").Width;

                Rectangle? caretRect = new Rectangle?();
                int x = 0;
                bool isFirst = true;

                foreach (var word in sendMessage.Words)
                {
                    for (int j = 0; j < (word.SplitSegments?.Length ?? 1); j++)
                    {
                        string text = word.SplitSegments?[j].Item1 ?? (string)word.Value;

                        if (x == Logic.CaretPosition)
                        {
                            caretRect = new Rectangle(textPadding.Left + word.X - spaceWidth, word.Y, 1, word.Height);
                            goto end;
                        }

                        if (j == 0)
                            if (isFirst)
                                isFirst = false;
                            else
                                x++;

                        for (int i = 0; i < text.Length; i++)
                        {
                            if (x == Logic.CaretPosition)
                            {
                                var size = TextRenderer.MeasureText(g, text.Remove(i), Fonts.GetFont(word.Font), Size.Empty, App.DefaultTextFormatFlags);
                                caretRect = new Rectangle(textPadding.Left + (word.SplitSegments?[j].Item2.X ?? word.X) + size.Width, word.SplitSegments?[j].Item2.Y ?? word.Y, 1, word.Height);
                                goto end;
                            }
                            x++;
                        }
                    }
                }

                var _word = sendMessage.Words[sendMessage.Words.Count - 1];
                var _lastSegmentText = _word.SplitSegments?[_word.SplitSegments.Length - 1].Item1;
                var _lastSegment = _word.SplitSegments?[_word.SplitSegments.Length - 1].Item2;
                caretRect = _word.SplitSegments == null ? new Rectangle(textPadding.Left + _word.X + _word.Width, _word.Y, 1, _word.Height) : new Rectangle(textPadding.Left + _lastSegment.Value.X + GuiEngine.Current.MeasureStringSize(g, _word.Font, _lastSegmentText).Width, _lastSegment.Value.Y, 1, _lastSegment.Value.Height);

                end:

                if (caretRect != null)
                    g.FillRectangle(App.ColorScheme.TextCaret, caretRect.Value);
            }
        }
    }
}
