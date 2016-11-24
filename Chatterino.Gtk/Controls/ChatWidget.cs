using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cairo;
using Chatterino.Common;
using Gdk;
using Gtk;

namespace Chatterino.Gtk.Controls
{
    public class ChatWidget : DrawingArea
    {
        public TwitchChannel TwitchChannel { get; set; }

        FontOptions crFontOptions = new FontOptions();

        protected override bool OnExposeEvent(EventExpose evnt)
        {
            var cr = CairoHelper.Create(GdkWindow);

            cr.LineWidth = 1;
            cr.SetSourceRGB(0.7, 0.2, 0.0);

            cr.FontOptions = crFontOptions;

            var width = Allocation.Width;
            var height = Allocation.Height;

            lock (TwitchChannel.MessageLock)
            {
                if (TwitchChannel.Messages.Length != 0)
                {
                    cr.Translate(8, 8);

                    for (var i = 0; i < Math.Min(10, TwitchChannel.MessageCount); i++)
                    {
                        cr.TextPath(TwitchChannel.Messages[i].RawMessage);

                        cr.SetSourceRGB(0.3, 0.4, 0.6);
                        cr.Fill();

                        cr.Translate(0, 8);
                    }
                }
            }

            cr.GetTarget().Dispose();
            ((IDisposable)cr).Dispose();

            return true;
        }
    }
}
