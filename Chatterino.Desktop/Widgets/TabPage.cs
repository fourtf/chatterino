using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xwt;
using Xwt.Drawing;

namespace Chatterino.Desktop.Widgets
{
    public class TabPage : Canvas
    {
        private string title;

        public string Title
        {
            get { return title; }
            set
            {
                if (title != value)
                {
                    title = value;
                    TitleChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private bool selected;

        public bool Selected
        {
            get { return selected; }
            set
            {
                if (selected != value)
                {
                    selected = value;
                    QueueDraw();
                }
            }
        }

        protected WidgetSpacing Padding = new WidgetSpacing(0, 2, 0, 0);

        public event EventHandler TitleChanged;

        public TabPage()
        {

        }

        protected override void OnDraw(Context ctx, Rectangle dirtyRect)
        {
            // colored top
            ctx.SetColor(App.ColorScheme.TabSelectedBG);
            ctx.Rectangle(0, 0, Bounds.Width, Padding.Top);
            ctx.Fill();
        }
    }
}
