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
                    return Image.FromResource(res);
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

        Font stdFont = Font.SystemSansSerifFont;
        public CommonSize MeasureStringSize(object graphics, FontType font, string text)
        {
            var layout = new TextLayout() { Font = stdFont, Text = text };

            return new CommonSize((int)layout.Width, (int)layout.Height);
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
