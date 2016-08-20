using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xwt;
using Xwt.Drawing;

namespace Chatterino.Desktop.Widgets
{
    public partial class TabControl
    {
        class Tab : Canvas
        {
            static WidgetSpacing Padding = new WidgetSpacing(8, 4, 8, 4);

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

            private bool mouseOver = false;

            TabControl tabControl;
            TabPage tabPage;

            double titleWidth = -1;

            public Tab(TabControl tabControl, TabPage tabPage)
            {
                this.tabControl = tabControl;
                this.tabPage = tabPage;

                tabPage.TitleChanged += (s, e) =>
                {
                    titleWidth = -1;
                    calcSize();
                    QueueDraw();
                };

                ButtonPressed += (s, e) =>
                {
                    if (e.Button == PointerButton.Left)
                    {
                        tabControl.select(tabPage);
                    }
                };

                MouseEntered += (s, e) =>
                {
                    mouseOver = true;

                    QueueDraw();
                };

                MouseExited += (s, e) =>
                {
                    mouseOver = false;

                    QueueDraw();
                };

                calcSize();
            }

            private void calcSize()
            {
                if (titleWidth == -1)
                {
                    TextLayout layout = new TextLayout { Text = tabPage.Title, Font = Fonts.TabControlTitle };
                    var size = layout.GetSize();
                    WidthRequest = (int)(Padding.Left + size.Width + Padding.Right);
                    HeightRequest = GetHeight();
                }
            }

            protected override void OnDraw(Context ctx, Rectangle dirtyRect)
            {
                // background
                ctx.SetColor(Selected ? App.ColorScheme.TabSelectedBG : (mouseOver ? App.ColorScheme.TabHoverBG : App.ColorScheme.TabBG));
                ctx.Rectangle(0, 0, Bounds.Width, Bounds.Height);
                ctx.Fill();

                // text
                ctx.SetColor(Selected ? App.ColorScheme.TabSelectedText : (mouseOver ? App.ColorScheme.TabHoverText : App.ColorScheme.TabText));

                ctx.DrawTextLayout(new TextLayout(this) { Text = tabPage.Title, Font = Fonts.TabControlTitle }, Padding.Left, Padding.Top);
            }

            internal static int GetHeight()
            {
                return 24;
            }
        }
    }
}
