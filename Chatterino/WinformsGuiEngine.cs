using Chatterino.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Media;
using System.Drawing.Imaging;
using System.Collections.Concurrent;

namespace Chatterino
{
    public class WinformsGuiEngine : IGuiEngine
    {
        // LINKS
        public void HandleLink(string link)
        {
            try
            {
                if (link.StartsWith("http://") || link.StartsWith("https://")
                    || MessageBox.Show($"The link \"{link}\" will be opened in an external application.", "open link", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    Process.Start(link);
            }
            catch { }
        }


        // SOUNDS
        SoundPlayer snd = new SoundPlayer(Properties.Resources.ping2);
        public void PlaySound(NotificationSound sound)
        {
            snd.Play();
        }


        // IMAGES
        public object ReadImageFromStream(Stream stream)
        {
            try
            {
                return Image.FromStream(stream);
            }
            catch { }

            return null;
        }

        public void HandleAnimatedTwitchEmote(TwitchEmote emote)
        {
            if (emote.Image != null)
            {
                Image img = (Image)emote.Image;

                bool animated = ImageAnimator.CanAnimate(img);

                if (animated)
                {
                    emote.Animated = true;

                    var dimension = new FrameDimension(img.FrameDimensionsList[0]);
                    var frameCount = img.GetFrameCount(dimension);
                    int currentFrame = 0;

                    App.GifEmoteFramesUpdating += (s, e) =>
                    {
                        currentFrame += 1;

                        if (currentFrame >= frameCount)
                        {
                            currentFrame = 0;
                        }

                        lock (img)
                            img.SelectActiveFrame(dimension, currentFrame);
                    };
                }
                App.TriggerEmoteLoaded();
            }
        }

        Dictionary<ImageType, Image> images = new Dictionary<ImageType, Image>
        {
            [ImageType.BadgeAdmin] = Properties.Resources.admin_bg,
            [ImageType.BadgeBroadcaster] = Properties.Resources.broadcaster_bg,
            [ImageType.BadgeDev] = Properties.Resources.dev_bg,
            [ImageType.BadgeGlobalmod] = Properties.Resources.globalmod_bg,
            [ImageType.BadgeModerator] = Properties.Resources.moderator_bg,
            [ImageType.BadgeStaff] = Properties.Resources.staff_bg,
            [ImageType.BadgeTurbo] = Properties.Resources.turbo_bg,
        };

        public object GetImage(ImageType type)
        {
            lock (images)
            {
                Image img;
                return images.TryGetValue(type, out img) ? img : null;
            }
        }

        public CommonSize GetImageSize(object image)
        {
            if (image == null)
            {
                return new CommonSize();
            }
            else
            {
                Image img = (Image)image;
                return new CommonSize(img.Width, img.Height);
            }
        }


        // MESSAGES
        bool enableBitmapDoubleBuffering = true;

        public CommonSize MeasureStringSize(object graphics, FontType font, string text)
        {
            Size size = TextRenderer.MeasureText((IDeviceContext)graphics, text, Fonts.GetFont(font), Size.Empty, App.DefaultTextFormatFlags);
            return new CommonSize(size.Width, size.Height);
        }

        public void DrawMessage(object graphics, Common.Message message, int xOffset2, int yOffset2)
        {
            message.CurrentXOffset = xOffset2;
            message.CurrentYOffset = yOffset2;

            Graphics g2 = (Graphics)graphics;

            int xOffset = 0, yOffset = 0;
            Graphics g = null;
            Bitmap bitmap = null;

            if (enableBitmapDoubleBuffering)
            {
                if (message.buffer == null)
                {
                    bitmap = new Bitmap(message.Width == 0 ? 10 : message.Width, message.Height == 0 ? 10 : message.Height);
                    g = Graphics.FromImage(bitmap);
                }
            }
            else
            {
                g = g2;
                xOffset = xOffset2;
                yOffset = yOffset2;
            }

            if (!enableBitmapDoubleBuffering || message.buffer == null)
            {
                message.CurrentXOffset = xOffset2;
                var textColor = App.ColorScheme.Text;


                for (int i = 0; i < message.Words.Count; i++)
                {
                    var span = message.Words[i];

                    if (span.Type == SpanType.Text)
                    {
                        Font font = Fonts.GetFont(span.Font);

                        Color color = span.Color == null ? textColor : Color.FromArgb(span.Color.Value);
                        if (span.Color != null && color.GetBrightness() < 0.5f)
                        {
                            color = ControlPaint.Light(color, 1f);
                        }

                        if (span.SplitSegments == null)
                        {
                            TextRenderer.DrawText(g, (string)span.Value, font, new Point(xOffset + span.X, span.Y + yOffset), color, App.DefaultTextFormatFlags);
                        }
                        else
                        {
                            var segments = span.SplitSegments;
                            for (int x = 0; x < segments.Length; x++)
                            {
                                TextRenderer.DrawText(g, segments[x].Item1, font, new Point(xOffset + segments[x].Item2.X, yOffset + segments[x].Item2.Y), color, App.DefaultTextFormatFlags);
                            }
                        }
                    }
                    else if (span.Type == SpanType.Emote)
                    {
                        var emote = (TwitchEmote)span.Value;
                        var img = (Image)emote.Image;
                        if (img != null)
                        {
                            lock (img)
                            {
                                g.DrawImage(img, span.X + xOffset, span.Y + yOffset, span.Width, span.Height);
                            }
                        }
                        else
                        {
                            g.DrawRectangle(Pens.Red, xOffset + span.X, span.Y + yOffset, span.Width, span.Height);
                        }
                    }
                    else if (span.Type == SpanType.Image)
                    {
                        var img = (Image)span.Value;
                        if (img != null)
                            g.DrawImage(img, span.X + xOffset, span.Y + yOffset, span.Width, span.Height);
                    }
                }

                if (message.Disabled)
                {
                    Brush disabledBrush = new SolidBrush(Color.FromArgb(172, (App.ColorScheme.ChatBackground as SolidBrush)?.Color ?? Color.Black));
                    g.FillRectangle(disabledBrush, xOffset, yOffset, 1000, message.Height);
                }

                if (enableBitmapDoubleBuffering)
                    message.buffer = bitmap;
            }

            if (enableBitmapDoubleBuffering)
            {
                g2.DrawImageUnscaled((Image)message.buffer, xOffset2, yOffset2);
                DrawGifEmotes(graphics, message);
            }
        }

        public void DrawGifEmotes(object graphics, Common.Message message)
        {
            var Words = message.Words;
            Graphics g = (Graphics)graphics;

            for (int i = 0; i < Words.Count; i++)
            {
                var span = Words[i];

                TwitchEmote emote;
                if (span.Type == SpanType.Emote && (emote = (TwitchEmote)span.Value).Animated)
                {
                    if (emote.Image != null)
                    {
                        lock (emote.Image)
                        {
                            BufferedGraphicsContext context = BufferedGraphicsManager.Current;

                            var CurrentXOffset = message.CurrentXOffset;
                            var CurrentYOffset = message.CurrentYOffset;

                            var buffer = context.Allocate(g, new Rectangle(span.X + CurrentXOffset, span.Y + CurrentYOffset, span.Width, span.Height));

                            buffer.Graphics.FillRectangle(App.ColorScheme.ChatBackground, span.X + CurrentXOffset, span.Y + CurrentYOffset, span.Width, span.Height);
                            buffer.Graphics.DrawImage((Image)emote.Image, span.X + CurrentXOffset, span.Y + CurrentYOffset, span.Width, span.Height);
                            if (message.Disabled)
                            {
                                buffer.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(172, (App.ColorScheme.ChatBackground as SolidBrush)?.Color ?? Color.Black)),
                                    span.X + CurrentXOffset, span.Y + CurrentYOffset, span.Width, span.Height);
                            }

                            buffer.Render(g);

                            //g.FillRectangle(App.ColorScheme.ChatBackground, span.X + CurrentXOffset, span.Y + CurrentYOffset - 4, span.Width, span.Height);
                            //g.DrawImage(emote.Image, span.X + CurrentXOffset, span.Y + CurrentYOffset - 4, span.Width, span.Height);
                        }
                    }
                }
            }
        }

        public void DisposeMessageGraphicsBuffer(Common.Message message)
        {
            if (message.buffer != null)
                ((IDisposable)message.buffer).Dispose();
        }
    }
}
