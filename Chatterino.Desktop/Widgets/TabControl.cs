using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xwt;
using Xwt.Drawing;

namespace Chatterino.Desktop.Widgets
{
    public partial class TabControl : Canvas
    {
        public IEnumerable<TabPage> TabPages
        {
            get
            {
                return _tabPages.Select(x => x.Item2);
            }
        }

        List<Tuple<Tab, TabPage>> _tabPages = new List<Tuple<Tab, TabPage>>();
        public TabPage Selected { get; private set; }

        Tuple<Tab, TabPage> _selected
        {
            get
            {
                if (Selected == null)
                {
                    return null;
                }

                var index = _tabPages.FindIndex(x => x.Item2 == Selected);

                if (index == -1)
                {
                    return null;
                }

                return _tabPages[index];
            }
        }

        Button dropDownButton = new Button();
        static Image dropDownImage = null;

        static TabControl()
        {
            try
            {
                var S = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames();

                dropDownImage = Image.FromResource("Chatterino.Desktop.Assets.tool_moreCollapser_off16.png");
            }
            catch (Exception exc)
            {
                ;
            }
        }

        public TabControl()
        {
            BoundsChanged += (s, e) =>
            {
                layout();
            };

            // drop down button
            dropDownButton.Style = ButtonStyle.Flat;

            dropDownButton.WidthRequest = 24;
            dropDownButton.HeightRequest = 23;

            dropDownButton.Image = dropDownImage;

            AddChild(dropDownButton, 0, 0);

            dropDownButton.Clicked += (s, e) =>
            {
                Menu menu = new Menu();

                var b = GetChildBounds(dropDownButton);
                menu.Popup(this, b.X + b.Width, b.Y + b.Height);

                foreach (var t in _tabPages)
                {
                    if (!t.Item1.Visible)
                    {
                        menu.Items.Add(new MenuItem(t.Item2.Title));
                    }
                }
            };

            // colors
            App_ColorSchemeChanged(null, null);
            App.ColorSchemeChanged += App_ColorSchemeChanged;
        }

        protected override void Dispose(bool disposing)
        {
            App.ColorSchemeChanged -= App_ColorSchemeChanged;

            base.Dispose(disposing);
        }

        private void App_ColorSchemeChanged(object sender, EventArgs e)
        {
            BackgroundColor = App.ColorScheme.TabPanelBG;
        }

        private void layout()
        {
            if (Bounds.Height > 0 && Bounds.Width > 0)
            {
                // calc tabs
                int maxLines = 1, currentLine = 0;
                var lineHeight = Tab.GetHeight();

                double x = 0, y = 0, w = Bounds.Width - dropDownButton.WidthRequest;
                bool firstInline = true;

                bool allTabsVisible = true;

                // go through all the tabs
                for (int i = 0; i < _tabPages.Count; i++)
                {
                    var t = _tabPages[i];

                    // tab doesn't fit in line
                    if (!firstInline && x + t.Item1.WidthRequest > w)
                    {
                        // if can't add new line
                        if (currentLine + 1 >= maxLines)
                        {
                            allTabsVisible = false;

                            for (; i < _tabPages.Count; i++)
                            {
                                // do something with the tabs that are not on screen
                                _tabPages[i].Item1.Visible = false;
                            }
                            break;
                        }

                        currentLine++;

                        y += lineHeight;
                        t.Item1.Visible = true;
                        SetChildBounds(t.Item1, new Rectangle(0, y, t.Item1.WidthRequest, lineHeight));

                        x = t.Item1.WidthRequest;
                    }
                    // tab doesn't fit in line
                    else
                    {
                        t.Item1.Visible = true;
                        SetChildBounds(t.Item1, new Rectangle(x, y, t.Item1.WidthRequest, lineHeight));

                        x += t.Item1.WidthRequest;
                    }
                    firstInline = false;
                }

                SetChildBounds(dropDownButton, new Rectangle(Bounds.Width - dropDownButton.WidthRequest, y, dropDownButton.WidthRequest, dropDownButton.HeightRequest));

                dropDownButton.Visible = !allTabsVisible;

                y += Tab.GetHeight();

                if (Selected != null)
                {
                    SetChildBounds(Selected, new Rectangle(0, y, Bounds.Width, Math.Max(1, Bounds.Height - y)));
                }
            }
        }

        // public
        public void AddTab(TabPage page)
        {
            var tab = new Tab(this, page);

            _tabPages.Add(new Tuple<Tab, TabPage>(tab, page));

            AddChild(tab, 0, 0);

            if (_tabPages.Count == 1)
            {
                select(page);
            }

            layout();
        }

        public void RemoveTab(TabPage page)
        {
            var index = _tabPages.FindIndex(x => x.Item2 == page);

            if (index == -1)
                throw new ArgumentException("\"child\" is not a child of this control.");

            if (page is TabPage)
            {
                RemoveChild(_tabPages[index].Item1);
                _tabPages.RemoveAt(index);
            }

            RemoveChild(page);
        }

        // private
        private void select(TabPage page)
        {
            var s = _selected;

            if (s != null)
            {
                s.Item1.Selected = false;
                s.Item2.Selected = false;
                base.RemoveChild(s.Item2);
            }

            Selected = page;
            s = _selected;

            if (Selected != null)
            {
                s.Item1.Selected = true;
                s.Item2.Selected = true;
                AddChild(s.Item2, 0, 0);
                layout();
            }
        }
    }
}
