using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chatterino.Common;
using Gdk;
using FontType = Chatterino.Common.FontType;
using ImageType = Chatterino.Common.ImageType;

namespace Chatterino.Gtk
{
    public class GtkGuiEngine : IGuiEngine
    {
        public bool IsDarkTheme => false;

        public void DisposeMessageGraphicsBuffer(Message message)
        {

        }

        public object DrawImageBackground(object image, HSLColor color)
        {
            return image;
        }

        public void ExecuteHotkeyAction(HotkeyAction action)
        {
        }

        public void FlashTaskbar()
        {
        }

        public void FreezeImage(object img)
        {
        }

        public object GetImage(ImageType type)
        {
            return null;
        }

        public CommonSize GetImageSize(object image)
        {
            return new CommonSize(16, 16);
        }

        public void HandleAnimatedTwitchEmote(LazyLoadedImage emote)
        {
        }

        public void HandleLink(Link link)
        {
        }

        public CommonSize MeasureStringSize(object graphics, FontType font, string text)
        {
            return new CommonSize(64, 16);
        }

        public void PlaySound(NotificationSound sound, bool forceCustom = false)
        {
        }

        public object ReadImageFromStream(Stream stream)
        {
            return new Pixbuf(stream);
        }

        public object ScaleImage(object image, double scale)
        {
            return image;
        }
    }
}
