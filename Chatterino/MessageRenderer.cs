using Chatterino.Common;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX.Direct2D1;
using Brush = System.Drawing.Brush;
using Image = System.Drawing.Image;
using InterpolationMode = System.Drawing.Drawing2D.InterpolationMode;

namespace Chatterino
{
    public static class MessageRenderer
    {
        public static SharpDX.Direct2D1.Factory D2D1Factory = new SharpDX.Direct2D1.Factory();

        public static SharpDX.Direct2D1.RenderTargetProperties RenderTargetProperties =
            new SharpDX.Direct2D1.RenderTargetProperties(
                new SharpDX.Direct2D1.PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                    SharpDX.Direct2D1.AlphaMode.Premultiplied))
            {
                Usage = SharpDX.Direct2D1.RenderTargetUsage.GdiCompatible
            };

        static Brush _selectionBrush = new SolidBrush(Color.FromArgb(127, Color.Orange));

        static MessageRenderer()
        {
            RenderTargetProperties.Type = RenderTargetType.Software;
        }

        public static void DrawMessage(object graphics, Common.Message message, int xOffset, int yOffset,
            Selection selection, int currentLine, bool drawText, List<GifEmoteState> gifEmotesOnScreen = null,
            bool allowMessageSeperator = true)
        {
            message.X = xOffset;
            message.Y = yOffset;

            var g = (Graphics)graphics;

            var spaceWidth =
                GuiEngine.Current.MeasureStringSize(g, FontType.Medium, " ").Width;

            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            message.X = xOffset;
            var textColor = App.ColorScheme.Text;

            Brush highlightBrush = null;

            if (message.HasAnyHighlightType(HighlightType.Highlighted))
            {
                highlightBrush = App.ColorScheme.ChatBackgroundHighlighted;
            }
            else if (message.HasAnyHighlightType(HighlightType.Resub))
            {
                highlightBrush = App.ColorScheme.ChatBackgroundResub;
            }
            else if (message.HasAnyHighlightType(HighlightType.Whisper))
            {
                highlightBrush = App.ColorScheme.ChatBackgroundWhisper;
            }

            if (highlightBrush != null)
            {
                g.FillRectangle(highlightBrush, 0, yOffset, g.ClipBounds.Width, message.Height);
            }

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;

            for (var i = 0; i < message.Words.Count; i++)
            {
                var word = message.Words[i];

                if (word.Type == SpanType.Text)
                {
                    if (drawText)
                    {
                        var font = Fonts.GetFont(word.Font);

                        var color = textColor;

                        if (word.Color.HasValue)
                        {
                            var hsl = word.Color.Value;

                            if (App.ColorScheme.IsLightTheme)
                            {
                                if (hsl.Luminosity > 0.5f)
                                {
                                    hsl = hsl.WithLuminosity(0.5f);
                                }
                            }
                            else
                            {
                                if (hsl.Luminosity < 0.5f)
                                    hsl = hsl.WithLuminosity(0.5f);
                            }

                            float r, _g, b;
                            hsl.ToRGB(out r, out _g, out b);
                            color = Color.FromArgb((int)(r * 255), (int)(_g * 255), (int)(b * 255));
                        }

                        if (word.SplitSegments == null)
                        {
                            TextRenderer.DrawText(g, (string)word.Value, font,
                                new Point(xOffset + word.X, yOffset + word.Y), color, App.DefaultTextFormatFlags);
                        }
                        else
                        {
                            var segments = word.SplitSegments;
                            for (var x = 0; x < segments.Length; x++)
                            {
                                TextRenderer.DrawText(g, segments[x].Item1, font,
                                    new Point(xOffset + segments[x].Item2.X, yOffset + segments[x].Item2.Y), color,
                                    App.DefaultTextFormatFlags);
                            }
                        }
                    }
                }
                else if (word.Type == SpanType.LazyLoadedImage)
                {
                    var emote = (LazyLoadedImage)word.Value;
                    var img = (Image)emote.Image;
                    if (img != null)
                    {
                        lock (img)
                        {
                            g.DrawImage(img, word.X + xOffset, word.Y + yOffset, word.Width, word.Height);
                        }
                    }
                    else
                    {
                        //g.DrawRectangle(Pens.Red, xOffset + word.X, word.Y + yOffset, word.Width, word.Height);
                    }
                }
                //else if (word.Type == SpanType.Image)
                //{
                //    var img = (Image)word.Value;
                //    if (img != null)
                //        g.DrawImage(img, word.X + xOffset, word.Y + yOffset, word.Width, word.Height);
                //}
            }

            Action<int, bool> addWordToGifList = (i, selected) =>
             {
                 var word = message.Words[i];
                 var words = new List<Word>();

                 for (var j = i - 1; j >= 0; j--)
                 {
                     if (message.Words[j].Intersects(word))
                     {
                         words.Insert(0, message.Words[j]);
                     }
                     else
                     {
                         break;
                     }
                 }

                 words.Add(word);

                 for (var j = i + 1; j < message.Words.Count; j++)
                 {
                     if (message.Words[j].Intersects(word))
                     {
                         words.Add(message.Words[j]);
                     }
                     else
                     {
                         break;
                     }
                 }

                 gifEmotesOnScreen?.Add(new GifEmoteState(word.X + xOffset, word.Y + yOffset, word.Width, word.Height, words, selected, message.HighlightType, message.Disabled, yOffset, xOffset));
             };

            if (selection != null && !selection.IsEmpty && selection.First.MessageIndex <= currentLine &&
                selection.Last.MessageIndex >= currentLine)
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

                var first = selection.First;
                var last = selection.Last;
                for (var i = 0; i < message.Words.Count; i++)
                {
                    var word = message.Words[i];

                    if ((currentLine != first.MessageIndex || i >= first.WordIndex) &&
                        (currentLine != last.MessageIndex || i <= last.WordIndex))
                    {

                        if (word.Type == SpanType.Text)
                        {
                            for (var j = 0; j < (word.SplitSegments?.Length ?? 1); j++)
                            {
                                if ((first.MessageIndex == currentLine && first.WordIndex == i && first.SplitIndex > j) ||
                                    (last.MessageIndex == currentLine && last.WordIndex == i && last.SplitIndex < j))
                                    continue;

                                var split = word.SplitSegments?[j];
                                var text = split?.Item1 ?? (string)word.Value;
                                var rect = split?.Item2 ??
                                                       new CommonRectangle(word.X, word.Y, word.Width, word.Height);

                                var textLength = text.Length;

                                var offset = (first.MessageIndex == currentLine && first.SplitIndex == j &&
                                              first.WordIndex == i)
                                    ? first.CharIndex
                                    : 0;
                                var length = ((last.MessageIndex == currentLine && last.SplitIndex == j &&
                                               last.WordIndex == i)
                                                 ? last.CharIndex
                                                 : textLength) - offset;

                                if (offset == 0 && length == text.Length)
                                    g.FillRectangle(_selectionBrush, rect.X + xOffset, rect.Y + yOffset,
                                        GuiEngine.Current.MeasureStringSize(App.UseDirectX ? null : g, word.Font, text)
                                            .Width + spaceWidth - 1, rect.Height);
                                else if (offset == text.Length)
                                    g.FillRectangle(_selectionBrush, rect.X + xOffset + rect.Width, rect.Y + yOffset,
                                        spaceWidth, rect.Height);
                                else
                                    g.FillRectangle(_selectionBrush,
                                        rect.X + xOffset +
                                        (offset == 0
                                            ? 0
                                            : GuiEngine.Current.MeasureStringSize(App.UseDirectX ? null : g, word.Font,
                                                text.Remove(offset)).Width),
                                        rect.Y + yOffset,
                                        GuiEngine.Current.MeasureStringSize(App.UseDirectX ? null : g, word.Font,
                                            text.Substring(offset, length)).Width +
                                        ((last.MessageIndex > currentLine || last.SplitIndex > j || last.WordIndex > i)
                                            ? spaceWidth
                                            : 0),
                                        rect.Height);
                            }
                        }
                        //else if (word.Type == SpanType.Image)
                        //{
                        //    var textLength = 2;

                        //    var offset = (first.MessageIndex == currentLine && first.WordIndex == i)
                        //        ? first.CharIndex
                        //        : 0;
                        //    var length = ((last.MessageIndex == currentLine && last.WordIndex == i)
                        //                     ? last.CharIndex
                        //                     : textLength) - offset;

                        //    g.FillRectangle(_selectionBrush, word.X + xOffset + (offset == 0 ? 0 : word.Width),
                        //        word.Y + yOffset,
                        //        (offset == 0 ? word.Width : 0) + (offset + length == 2 ? spaceWidth : 0) - 1,
                        //        word.Height);
                        //}
                        else if (word.Type == SpanType.LazyLoadedImage)
                        {
                            var textLength = 2;

                            var offset = (first.MessageIndex == currentLine && first.WordIndex == i)
                                ? first.CharIndex
                                : 0;
                            var length = ((last.MessageIndex == currentLine && last.WordIndex == i)
                                             ? last.CharIndex
                                             : textLength) - offset;

                            var emote = (LazyLoadedImage)word.Value;

                            if (emote.IsAnimated)
                            {
                                addWordToGifList(i, true);
                            }
                            else
                            {
                                g.FillRectangle(_selectionBrush, word.X + xOffset, word.Y + yOffset,
                                    word.Width + spaceWidth - 1, word.Height);
                            }
                        }
                    }
                    else
                    {
                        if (word.Type == SpanType.LazyLoadedImage)
                        {
                            var emote = (LazyLoadedImage)word.Value;
                            if (emote.IsAnimated)
                            {
                                addWordToGifList(i, true);
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < message.Words.Count; i++)
                {
                    var word = message.Words[i];
                    if (word.Type == SpanType.LazyLoadedImage)
                    {
                        var emote = (LazyLoadedImage)word.Value;
                        if (emote.IsAnimated)
                        {
                            addWordToGifList(i, false);
                        }
                    }
                }
            }

            if (allowMessageSeperator && AppSettings.ChatSeperateMessages)
            {
                g.DrawLine(App.ColorScheme.ChatMessageSeperatorBorder, 0, yOffset + 1, message.Width + 128, yOffset + 1);
                g.DrawLine(App.ColorScheme.ChatMessageSeperatorBorderInner, 0, yOffset, message.Width + 128, yOffset);
            }

            if (message.HasAnyHighlightType(HighlightType.SearchResult))
            {
                g.FillRectangle(Brushes.GreenYellow, 1, yOffset, 1, message.Height - 1);
            }
        }

        //public static void DrawGifEmotes(object graphics, Common.Message message, Selection selection, int currentLine)
        public static void DrawGifEmotes(object graphics, List<GifEmoteState> gifEmotes, Selection selection)
        {
            var g = (Graphics)graphics;

            var spaceWidth =
                GuiEngine.Current.MeasureStringSize(App.UseDirectX ? null : g, FontType.Medium, " ").Width;

            foreach (var state in gifEmotes)
            {
                Brush backgroundBrush;

                if ((state.HighlightType & HighlightType.Highlighted) == HighlightType.Highlighted)
                {
                    backgroundBrush = App.ColorScheme.ChatBackgroundHighlighted;
                }
                else if ((state.HighlightType & HighlightType.Resub) == HighlightType.Resub)
                {
                    backgroundBrush = App.ColorScheme.ChatBackgroundResub;
                }
                else
                {
                    backgroundBrush = App.ColorScheme.ChatBackground;
                }

                if (state.Words.Count > 1)
                {
                    g.Clip = new Region(new Rectangle(state.X, state.Y, state.Width, state.Height));
                }

                g.FillRectangle(backgroundBrush, state.X, state.Y,
                                state.Width, state.Height);

                foreach (var word in state.Words)
                {
                    switch (word.Type)
                    {
                        case SpanType.Text:
                            break;
                        case SpanType.LazyLoadedImage:
                            g.DrawImage((Image) ((LazyLoadedImage) word.Value).Image, word.X + state.MessageXOffset,
                                word.Y + state.MessageYOffset, word.Width, word.Height);
                            break;
                        //    case SpanType.Image:
                        //        g.DrawImage((Image)word.Value, word.X + state.MessageXOffset, word.Y + state.MessageYOffset, word.Width, word.Height);
                        //        break;
                        //}
                    }
                }

                if (state.Disabled)
                {
                    g.FillRectangle(
                        new SolidBrush(Color.FromArgb(172,
                            (App.ColorScheme.ChatBackground as SolidBrush)?.Color ?? Color.Black)),
                        state.X, state.Y, state.Width + spaceWidth,
                        state.Height);
                }

                if (state.Selected)
                {
                    g.FillRectangle(_selectionBrush, state.X, state.Y,
                        state.Width, state.Height);
                }
                if (state.Words.Count > 1)
                {
                    g.ResetClip();
                }
            }
        }
    }
}