using Chatterino.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Xwt.Drawing;
using System.Collections.Concurrent;

namespace Chatterino.Desktop
{
    public class XwtGuiEngine : IGuiEngine
    {
        public virtual void FreezeImage(object obj)
        {

        }

        public void DisposeMessageGraphicsBuffer(Message message)
        {
        }

        public void FlashTaskbar()
        {
        }

        ConcurrentDictionary<ImageType, object> images = new ConcurrentDictionary<ImageType, object>();

        public object GetImage(ImageType type)
        {
            return images.GetOrAdd(type, t =>
            {
                string res = "Chatterino.Desktop.Assets.";
                switch (t)
                {
                    case ImageType.
                        BadgeAdmin:
                        res += "admin_bg.png";
                        break;
                    case ImageType.BadgeBroadcaster:
                        res += "broadcaster_bg.png";
                        break;
                    case ImageType.BadgeDev:
                        res += "dev_bg.png";
                        break;
                    case ImageType.BadgeGlobalmod:
                        res += "globalmod_bg.png";
                        break;
                    case ImageType.BadgeModerator:
                        res += "admin_bg.png";
                        break;
                    case ImageType.BadgeStaff:
                        res += "admin_bg.png";
                        break;
                    case ImageType.BadgeTurbo:
                        res += "admin_bg.png";
                        break;
                    case ImageType.Cheer1:
                        res += "cheer1.png";
                        break;
                    case ImageType.Cheer100:
                        res += "cheer100.png";
                        break;
                    case ImageType.Cheer1000:
                        res += "cheer1000.png";
                        break;
                    case ImageType.Cheer5000:
                        res += "cheer5000.png";
                        break;
                    case ImageType.Cheer10000:
                        res += "cheer10000.png";
                        break;
                }

                try
                {
                    var image = Image.FromResource(res);

                    GuiEngine.Current.FreezeImage(image);

                    return image;
                }
                catch
                {
                    return null;
                }
            });
        }

        public CommonSize GetImageSize(object image)
        {
            Image img = image as Image;

            return img == null ? new CommonSize() : new CommonSize((int)img.Width, (int)img.Height);
        }

        public void HandleAnimatedTwitchEmote(TwitchEmote emote)
        {

        }

        public void HandleLink(string link)
        {

        }

        const int sizeCacheStackLimit = 1024;
        ConcurrentDictionary<FontType, Tuple<ConcurrentDictionary<string, CommonSize>, ConcurrentStack<string>, int>> sizeCaches = new ConcurrentDictionary<FontType, Tuple<ConcurrentDictionary<string, CommonSize>, ConcurrentStack<string>, int>>();

        Font stdFont = Font.SystemSansSerifFont;
        public CommonSize MeasureStringSize(object graphics, FontType font, string text)
        {
            var sizeCache = sizeCaches.GetOrAdd(font, f =>
            {
                int lineHeight = (int)new TextLayout() { Text = "X", Font = Fonts.GetFont(font) }.GetSize().Height;

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

                //if (text == " ")
                //{
                //    float w1 = new SharpDX.DirectWrite.TextLayout(Fonts.Factory, "a a", Fonts.GetTextFormat(font), 1000000, 1000000).Metrics.Width;
                //    float w2 = new SharpDX.DirectWrite.TextLayout(Fonts.Factory, "a", Fonts.GetTextFormat(font), 1000000, 1000000).Metrics.Width;
                //    return new CommonSize((int)(w1 - (w2 * 2f)), sizeCache.Item3);
                //}

                return new CommonSize((int)new TextLayout() { Text = text, Font = Fonts.GetFont(font) }.GetSize().Width, sizeCache.Item3);
            });
        }

        public void PlaySound(NotificationSound sound, bool forceCustom = false)
        {

        }

        public object ReadImageFromStream(Stream stream)
        {
            try
            {
                return Image.FromStream(stream);
            }
            catch
            {
                return null;
            }
        }

        public object ScaleImage(object image, double scale)
        {
            return image;
        }
    }
}
