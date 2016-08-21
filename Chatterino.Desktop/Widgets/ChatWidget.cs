using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xwt;
using Xwt.Drawing;

namespace Chatterino.Desktop.Widgets
{
    public class ChatWidget : Canvas
    {
        private string channelChannel;

        public string ChannelName
        {
            get { return channelChannel; }
            set { channelChannel = value; }
        }


        protected override void OnDraw(Context ctx, Rectangle dirtyRect)
        {
            if (Bounds.Width > 0 && Bounds.Height > 0)
            {
                ctx.SetLineWidth(1);
                ctx.Translate(0.5, 0.5);

                ctx.SetColor(App.ColorScheme.TabSelectedBG);
                ctx.Rectangle(0, 0, Bounds.Width - 1, Bounds.Height - 1);
                ctx.Stroke();

                //ctx.DrawTextLayout(new TextLayout(this) { Text = ChannelName, Font = Fonts.TabControlTitle }, 8, 8);
            }
        }
    }
}
