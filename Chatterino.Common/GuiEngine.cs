using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatterino.Common
{
    public static class GuiEngine
    {
        public static IGuiEngine Current { get; private set; } = null;

        public static void Initialize(IGuiEngine engine)
        {
            if (Current != null)
                throw new InvalidOperationException("There already is a GuiEngine loaded.");

            Current = engine;
        }
    }

    public interface IGuiEngine
    {
        void HandleLink(string link);
        void PlaySound(NotificationSound sound);
        object GetImage(ImageType type);
        void HandleAnimatedTwitchEmote(TwitchEmote emote);

        object ReadImageFromStream(Stream stream);

        CommonSize MeasureStringSize(object graphics, FontType font, string text);
        void DrawMessage(object graphics, Message message, int xOffset, int yOffset, Selection selection, int currentLineIndex);
        void DrawGifEmotes(object graphics, Message message);

        void DisposeMessageGraphicsBuffer(Message message);

        CommonSize GetImageSize(object image);
    }
}
