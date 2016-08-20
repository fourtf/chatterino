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
        protected override void OnDraw(Context ctx, Rectangle dirtyRect)
        {
            if (Bounds.Width > 0 && Bounds.Height > 0)
            {
                ctx.SetColor(Colors.Red);
                ctx.Rectangle(0, 0, Bounds.Width - 1, Bounds.Height - 1);
                ctx.Stroke();
            }
        }
    }
}
