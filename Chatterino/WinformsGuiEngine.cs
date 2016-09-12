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
using Chatterino.Controls;

namespace Chatterino
{
    public class WinformsGuiEngine : IGuiEngine
    {
        public WinformsGuiEngine()
        {
            AppSettings.FontChanged += (s, e) =>
            {
                gdiSizeCaches.Clear();
                dwSizeCaches.Clear();
            };
        }

        // LINKS
        public void HandleLink(string link)
        {
            if (link != null && link.Length > 1)
            {
                if (link[0] == '@')
                {
                    var S = link.Substring(1).Split('|');

                    switch (S[0])
                    {
                        case "closeCurrentSplit":
                            App.MainForm.RemoveSelectedSplit();
                            break;
                        case "insertText":
                            (App.MainForm.Selected as ChatControl)?.Input.Logic.InsertText(S[1]);
                            break;
                    }
                }
                else
                {
                    try
                    {
                        if (link.StartsWith("http://") || link.StartsWith("https://")
                            || MessageBox.Show($"The link \"{link}\" will be opened in an external application.", "open link", MessageBoxButtons.OKCancel) == DialogResult.OK)
                            Process.Start(link);
                    }
                    catch { }
                }
            }
        }


        // SOUNDS
        SoundPlayer defaultHighlightSound = new SoundPlayer(Properties.Resources.ping2);
        public SoundPlayer HighlightSound { get; private set; } = null;

        public bool IsDarkTheme
        {
            get
            {
                return !App.ColorScheme.IsLightTheme;
            }
        }

        DateTime highlightTimeStamp = DateTime.MinValue;

        public void PlaySound(NotificationSound sound, bool forceCustom = false)
        {
            try
            {
                bool focused = false;

                App.MainForm.Invoke(() => focused = App.MainForm.ContainsFocus);

                if (!focused)
                {
                    SoundPlayer player = null;

                    if (forceCustom || AppSettings.ChatCustomHighlightSound)
                    {
                        try
                        {
                            var fileInfo = new FileInfo("./Custom/Ping.wav");
                            if (fileInfo.Exists)
                            {
                                if (fileInfo.LastWriteTime != highlightTimeStamp)
                                {
                                    HighlightSound?.Dispose();

                                    try
                                    {
                                        using (FileStream stream = new FileStream("./Custom/Ping.wav", FileMode.Open))
                                        {
                                            HighlightSound = new SoundPlayer(stream);
                                            HighlightSound.Load();
                                        }

                                        player = HighlightSound;
                                        Console.WriteLine("loaded");
                                    }
                                    catch
                                    {
                                        HighlightSound.Dispose();
                                    }
                                }
                            }
                            else
                            {
                                player = HighlightSound;
                            }
                        }
                        catch { }
                    }

                    if (player == null)
                    {
                        player = defaultHighlightSound;
                    }

                    player.Play();
                }
            }
            catch { }
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
                    try
                    {
                        var dimension = new FrameDimension(img.FrameDimensionsList[0]);
                        var frameCount = img.GetFrameCount(dimension);
                        int[] frameDuration = new int[frameCount];
                        int currentFrame = 0;
                        int currentFrameOffset = 0;

                        byte[] times = img.GetPropertyItem(0x5100).Value;
                        int frame = 0;
                        for (int i = 0; i < frameCount; i++)
                        {
                            frameDuration[i] = BitConverter.ToInt32(times, 4 * frame);
                        }
                        emote.Animated = true;

                        App.GifEmoteFramesUpdating += (s, e) =>
                        {
                            currentFrameOffset += 3;

                            while (true)
                            {
                                if (currentFrameOffset > frameDuration[currentFrame])
                                {
                                    currentFrameOffset -= frameDuration[currentFrame];
                                    currentFrame = (currentFrame + 1) % frameCount;
                                }
                                else
                                    break;
                            }

                            lock (img)
                                img.SelectActiveFrame(dimension, currentFrame);
                        };
                    }
                    catch { }
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

            [ImageType.Cheer1] = Properties.Resources.cheer1,
            [ImageType.Cheer100] = Properties.Resources.cheer100,
            [ImageType.Cheer1000] = Properties.Resources.cheer1000,
            [ImageType.Cheer5000] = Properties.Resources.cheer5000,
            [ImageType.Cheer10000] = Properties.Resources.cheer10000,
            [ImageType.Cheer100000] = Properties.Resources.cheer100000,
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
                try
                {
                    Image img = (Image)image;
                    lock (img)
                        return new CommonSize(img.Width, img.Height);
                }
                catch { }

                return new CommonSize(16, 16);
            }
        }


        // MESSAGES
        static int sizeCacheStackLimit = 2048;

        ConcurrentDictionary<FontType, Tuple<ConcurrentDictionary<string, CommonSize>, ConcurrentStack<string>, int>> gdiSizeCaches = new ConcurrentDictionary<FontType, Tuple<ConcurrentDictionary<string, CommonSize>, ConcurrentStack<string>, int>>();
        ConcurrentDictionary<FontType, Tuple<ConcurrentDictionary<string, CommonSize>, ConcurrentStack<string>, int>> dwSizeCaches = new ConcurrentDictionary<FontType, Tuple<ConcurrentDictionary<string, CommonSize>, ConcurrentStack<string>, int>>();

        public CommonSize MeasureStringSize(object graphics, FontType font, string text)
        {
            bool isGdi = graphics is Graphics;

            var sizeCache = (isGdi ? gdiSizeCaches : dwSizeCaches).GetOrAdd(font, f =>
            {
                int lineHeight;

                if (isGdi)
                {
                    lineHeight = TextRenderer.MeasureText((Graphics)graphics, "X", Fonts.GetFont(font), Size.Empty, App.DefaultTextFormatFlags).Height;
                }
                else
                {
                    var metrics = new SharpDX.DirectWrite.TextLayout(Fonts.Factory, "X", Fonts.GetTextFormat(font), 1000000, 1000000).Metrics;
                    lineHeight = (int)metrics.Height;
                    //lineHeight = (int)Math.Ceiling(Fonts.GetTextFormat(font).FontSize);
                }

                return Tuple.Create(new ConcurrentDictionary<string, CommonSize>(), new ConcurrentStack<string>(), lineHeight);
            });

            return sizeCache.Item1.GetOrAdd(text, s =>
            {
                if (sizeCache.Item2.Count >= sizeCacheStackLimit)
                {
                    string value;
                    if (sizeCache.Item2.TryPop(out value))
                    {
                        CommonSize _s;
                        sizeCache.Item1.TryRemove(value, out _s);
                    }
                }

                sizeCache.Item2.Push(s);

                if (isGdi)
                {
                    Size size = TextRenderer.MeasureText((IDeviceContext)graphics, text, Fonts.GetFont(font), Size.Empty, App.DefaultTextFormatFlags);
                    return new CommonSize(size.Width, sizeCache.Item3);
                }
                else
                {
                    var metrics = new SharpDX.DirectWrite.TextLayout(Fonts.Factory, text, Fonts.GetTextFormat(font), 1000000, 1000000).Metrics;

                    return new CommonSize((int)metrics.WidthIncludingTrailingWhitespace, sizeCache.Item3);
                }
            });
        }

        public void DisposeMessageGraphicsBuffer(Common.Message message)
        {
            if (message.buffer != null)
            {
                ((IDisposable)message.buffer).Dispose();
                message.buffer = null;
            }
        }

        public void FlashTaskbar()
        {
            if (!Util.IsLinux && App.MainForm != null)
            {

                App.MainForm.Invoke(() => Win32.FlashWindow.Flash(App.MainForm, 1));

                //App.MainForm.Invoke(() => Win32.FlashWindowEx(App.MainForm));
            }
        }

        public object ScaleImage(object image, double scale)
        {
            Image img = (Image)image;

            int w = (int)(img.Width * scale), h = (int)(img.Height * scale);

            var newImage = new Bitmap(w, h);

            using (var graphics = Graphics.FromImage(newImage))
            {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                graphics.DrawImage(img, 0, 0, w, h);
            }

            return newImage;
        }

        public void FreezeImage(object img)
        {

        }
    }
}
