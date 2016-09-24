using Chatterino.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xwt.Drawing;

namespace Chatterino.Desktop
{
    public static class MessageRenderer
    {
        static Color selectionBrush = Colors.Orange.WithAlpha(0.5);

        public static void DrawMessage(object graphics, Message message, int xOffset, int yOffset, Selection selection, int currentLine, bool drawText)
        {
            message.X = xOffset;
            message.Y = yOffset;

            Context ctx = (Context)graphics;

            int spaceWidth = GuiEngine.Current.MeasureStringSize(graphics, FontType.Medium, " ").Width;

            message.X = xOffset;
            var textColor = App.ColorScheme.Text;

            if (message.HighlightType)
            {
                ctx.SetColor(App.ColorScheme.ChatBackgroundHighlighted);
                ctx.Rectangle(0, yOffset, 1000, message.Height);
                ctx.Fill();
            }

            for (int i = 0; i < message.Words.Count; i++)
            {
                var word = message.Words[i];

                if (word.Type == SpanType.Text)
                {
                    if (drawText)
                    {
                        Font font = Fonts.GetFont(word.Font);

                        Color color;

                        if (word.Color == null)
                        {
                            color = textColor;
                        }
                        else
                        {
                            HSLColor hsl = word.Color.Value;

                            if (App.ColorScheme.IsLightTheme)
                            {
                                if (hsl.Luminosity > 0.5f)
                                {
                                    color = hsl.WithLuminosity(0.5f).ToColor();
                                }
                                else
                                {
                                    color = hsl.ToColor();
                                }
                            }
                            else
                            {
                                if (hsl.Luminosity < 0.5f)
                                {
                                    color = hsl.WithLuminosity(0.5f).ToColor();
                                }
                                else
                                {
                                    color = hsl.ToColor();
                                }
                            }
                        }

                        ctx.SetColor(color);
                        if (word.SplitSegments == null)
                        {
                            ctx.DrawTextLayout(new TextLayout() { Text = (string)word.Value, Font = font }, xOffset + word.X, yOffset + word.Y);
                        }
                        else
                        {
                            var segments = word.SplitSegments;
                            for (int x = 0; x < segments.Length; x++)
                            {
                                ctx.DrawTextLayout(new TextLayout() { Text = segments[x].Item1, Font = font }, xOffset + segments[x].Item2.X, yOffset + segments[x].Item2.Y);
                            }
                        }
                    }
                }
                else if (word.Type == SpanType.Emote)
                {
                    var emote = (TwitchEmote)word.Value;
                    var img = (Image)emote.Image;
                    if (img != null)
                    {
                        lock (img)
                        {
                            ctx.DrawImage(img, word.X + xOffset, word.Y + yOffset, word.Width, word.Height);
                        }
                    }
                    else
                    {
                        //g.DrawRectangle(Pens.Red, xOffset + word.X, word.Y + yOffset, word.Width, word.Height);
                    }
                }
                else if (word.Type == SpanType.Image)
                {
                    var img = (Image)word.Value;
                    if (img != null)
                    {
                        ctx.DrawImage(img, word.X + xOffset, word.Y + yOffset, word.Width, word.Height);
                    }
                }
            }

            if (selection != null && !selection.IsEmpty && selection.First.MessageIndex <= currentLine && selection.Last.MessageIndex >= currentLine)
            {
                ctx.SetColor(selectionBrush);

                var first = selection.First;
                var last = selection.Last;

                for (int i = 0; i < message.Words.Count; i++)
                {
                    if ((currentLine != first.MessageIndex || i >= first.WordIndex) && (currentLine != last.MessageIndex || i <= last.WordIndex))
                    {
                        var word = message.Words[i];

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

                                if (offset == 0 && length == text.Length)
                                    ctx.Rectangle(rect.X + xOffset, rect.Y + yOffset, GuiEngine.Current.MeasureStringSize(graphics, word.Font, text).Width + spaceWidth - 1, rect.Height);
                                else if (offset == text.Length)
                                    ctx.Rectangle(rect.X + xOffset + rect.Width, rect.Y + yOffset, spaceWidth, rect.Height);
                                else
                                    ctx.Rectangle(rect.X + xOffset + (offset == 0 ? 0 : GuiEngine.Current.MeasureStringSize(graphics, word.Font, text.Remove(offset)).Width),
                                        rect.Y + yOffset,
                                        GuiEngine.Current.MeasureStringSize(graphics, word.Font, text.Substring(offset, length)).Width +
                                        ((last.MessageIndex > currentLine || last.SplitIndex > j || last.WordIndex > i) ? spaceWidth : 0),
                                        rect.Height);

                                ctx.Fill();
                            }
                        }
                        else if (word.Type == SpanType.Image)
                        {
                            int textLength = 2;

                            int offset = (first.MessageIndex == currentLine && first.WordIndex == i) ? first.CharIndex : 0;
                            int length = ((last.MessageIndex == currentLine && last.WordIndex == i) ? last.CharIndex : textLength) - offset;

                            ctx.Rectangle(word.X + xOffset + (offset == 0 ? 0 : word.Width), word.Y + yOffset, (offset == 0 ? word.Width : 0) + (offset + length == 2 ? spaceWidth : 0) - 1, word.Height);
                            ctx.Fill();
                        }
                        else if (word.Type == SpanType.Emote)
                        {
                            int textLength = 2;

                            int offset = (first.MessageIndex == currentLine && first.WordIndex == i) ? first.CharIndex : 0;
                            int length = ((last.MessageIndex == currentLine && last.WordIndex == i) ? last.CharIndex : textLength) - offset;

                            if (!((TwitchEmote)word.Value).Animated)
                            {
                                ctx.Rectangle(word.X + xOffset, word.Y + yOffset, word.Width + spaceWidth - 1, word.Height);
                                ctx.Fill();
                            }
                        }
                    }
                }
            }
        }

        public static void DrawGifEmotes(object graphics, Common.Message message, Selection selection, int currentLine)
        {
            //var Words = message.Words;
            //Graphics g = (Graphics)graphics;

            //int spaceWidth = TextRenderer.MeasureText(g, " ", Fonts.GdiMedium, Size.Empty, App.DefaultTextFormatFlags).Width;

            //for (int i = 0; i < Words.Count; i++)
            //{
            //    var word = Words[i];

            //    TwitchEmote emote;
            //    if (word.Type == SpanType.Emote && (emote = (TwitchEmote)word.Value).Animated)
            //    {
            //        if (emote.Image != null)
            //        {
            //            lock (emote.Image)
            //            {
            //                var CurrentXOffset = message.X;
            //                var CurrentYOffset = message.Y;

            //                g.FillRectangle(message.Highlighted ? App.ColorScheme.ChatBackgroundHighlighted : App.ColorScheme.ChatBackground, word.X + CurrentXOffset, word.Y + CurrentYOffset, word.Width, word.Height);
            //                g.DrawImage((Image)emote.Image, word.X + CurrentXOffset, word.Y + CurrentYOffset, word.Width, word.Height);

            //                //if (message.Highlighted)
            //                //    g.FillRectangle(, word.X + CurrentXOffset, word.Y + CurrentYOffset, word.Width, word.Height);

            //                if (selection != null && !selection.IsEmpty && (currentLine > selection.First.MessageIndex || (currentLine == selection.First.MessageIndex && i >= selection.First.WordIndex)) && (currentLine < selection.Last.MessageIndex || (selection.Last.MessageIndex == currentLine && i < selection.Last.WordIndex)))
            //                    g.FillRectangle(selectionBrush, word.X + CurrentXOffset, word.Y + CurrentYOffset, word.Width, word.Height);

            //                if (message.Disabled)
            //                {
            //                    g.FillRectangle(new SolidBrush(Color.FromArgb(172, (App.ColorScheme.ChatBackground as SolidBrush)?.Color ?? Color.Black)),
            //                        word.X + CurrentXOffset, word.Y + CurrentYOffset, word.Width + spaceWidth, word.Height);
            //                }
            //            }
            //        }
            //    }
            //}
        }
    }
}
