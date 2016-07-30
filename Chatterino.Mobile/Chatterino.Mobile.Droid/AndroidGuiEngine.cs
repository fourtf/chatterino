using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Chatterino.Common;
using Android.Graphics;
using Android.Graphics.Drawables;

namespace Chatterino.Mobile.Droid
{
    public class AndroidGuiEngine : IGuiEngine
    {
        public void DisposeMessageGraphicsBuffer(Common.Message message)
        {

        }

        public void DrawGifEmotes(object graphics, Common.Message message, Selection selection, int currentLineIndex)
        {

        }

        public void DrawMessage(object graphics, Common.Message message, int xOffset, int yOffset, Selection selection, int currentLineIndex)
        {
            Canvas canvas = (Canvas)graphics;

            foreach (Word word in message.Words)
            {
                if (word.Type == SpanType.Text)
                {
                    if (word.SplitSegments == null)
                    {
                        var text = (string)word.Value;

                        canvas.DrawText(text, xOffset + word.X, yOffset + word.Y, Fonts.GetFont(word.Font));
                    }
                    else
                    {
                    }
                }
                else if (word.Type == SpanType.Emote)
                {

                }
                else if (word.Type == SpanType.Image)
                {

                }
            }
        }

        public void FlashTaskbar()
        {

        }

        public object GetImage(ImageType type)
        {
            return null;
            //return MainActivity.Current.Resources.GetDrawable(Resource.Drawable.dev_bg);
        }

        public CommonSize GetImageSize(object image)
        {
            Drawable drawable = (Drawable)image;
            return new CommonSize(drawable.Bounds.Width(), drawable.Bounds.Height());
        }

        public void HandleAnimatedTwitchEmote(TwitchEmote emote)
        {
            //throw new NotImplementedException();
        }

        public void HandleLink(string link)
        {
            try
            {
                Intent intend = new Intent(Intent.ActionView, global::Android.Net.Uri.Parse(link));
                MainActivity.Current.StartActivity(intend);
            }
            catch { }
        }

        public CommonSize MeasureStringSize(object graphics, FontType font, string text)
        {
            Paint paint = Fonts.GetFont(font);

            Rect rect = new Rect();
            paint.GetTextBounds(text, 0, text.Length, rect);

            return new CommonSize(rect.Width(), rect.Height());
        }

        public void PlaySound(NotificationSound sound)
        {
            //throw new NotImplementedException();
        }

        public object ReadImageFromStream(Stream stream)
        {
            return Drawable.CreateFromStream(stream, "");
        }
    }
}