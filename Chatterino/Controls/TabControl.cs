using Chatterino.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chatterino.Controls
{
    public partial class TabControl : Control
    {
        public event EventHandler<ValueEventArgs<TabPage>> TabPageSelected;

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
        MoreButton newTabButton = new MoreButton();
        static Image dropDownImage = null;

        static TabControl()
        {
            try
            {
                var S = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames();

                dropDownImage = Properties.Resources.ExpandChevronDown_16x;
            }
            catch (Exception exc)
            {
                ;
            }
        }

        public TabControl()
        {
            SizeChanged += (s, e) =>
            {
                layout();
            };

            // drop down button
            dropDownButton.FlatStyle = FlatStyle.Flat;

            dropDownButton.Width = 24;
            dropDownButton.Height = 23;

            dropDownButton.Image = dropDownImage;

            Controls.Add(dropDownButton);

            dropDownButton.Click += (s, e) =>
            {
                ContextMenu menu = new ContextMenu();

                foreach (var t in _tabPages)
                {
                    if (!t.Item1.Visible)
                    {
                        menu.MenuItems.Add(new MenuItem(t.Item2.Title));
                    }
                }

                menu.Show(dropDownButton, new Point(dropDownButton.Width, dropDownButton.Height), LeftRightAlignment.Left);
            };

            // add tab button
            Controls.Add(newTabButton);
            newTabButton.Size = new Size(24, 24);

            newTabButton.Click += (s, e) =>
            {
                AddTab(new ColumnTabPage(), true);
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
            BackColor = App.ColorScheme.TabPanelBG;
        }

        private void layout()
        {
            if (Bounds.Height > 0 && Bounds.Width > 0)
            {
                // calc tabs
                int maxLines = int.MaxValue, currentLine = 0;
                var lineHeight = Tab.GetHeight();

                int x = 0, y = 0, w = Bounds.Width - dropDownButton.Width;
                bool firstInline = true;

                bool allTabsVisible = true;

                // go through all the tabs
                for (int i = 0; i < _tabPages.Count; i++)
                {
                    var t = _tabPages[i];

                    // tab doesn't fit in line
                    if (!firstInline && x + t.Item1.Width > w)
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
                        t.Item1.SetBounds(0, y, t.Item1.Width, lineHeight);

                        x = t.Item1.Width;
                    }
                    // tab doesn't fit in line
                    else
                    {
                        t.Item1.Visible = true;
                        t.Item1.SetBounds(x, y, t.Item1.Width, lineHeight);

                        x += t.Item1.Width;
                    }
                    firstInline = false;
                }

                dropDownButton.Location = new Point(Width - dropDownButton.Width - newTabButton.Width, y);
                newTabButton.Location = new Point(Width - newTabButton.Width, y);

                dropDownButton.Visible = !allTabsVisible;

                y += Tab.GetHeight();

                if (Selected != null)
                {
                    Selected.SetBounds(0, y, Bounds.Width, Math.Max(1, Bounds.Height - y));
                }
            }
        }

        // public
        public void AddTab(TabPage page, bool select = false)
        {
            var tab = new Tab(this, page);

            _tabPages.Add(new Tuple<Tab, TabPage>(tab, page));

            Controls.Add(tab);

            if (select || _tabPages.Count == 1)
            {
                this.Select(page);
            }

            layout();
        }

        public void InsertTab(int index, TabPage page, bool select = false)
        {
            var tab = new Tab(this, page);

            _tabPages.Insert(index, new Tuple<Tab, TabPage>(tab, page));

            Controls.Add(tab);

            if (select || _tabPages.Count == 1)
            {
                this.Select(page);
            }

            layout();
        }

        public void RemoveTab(TabPage page)
        {
            var index = _tabPages.FindIndex(x => x.Item2 == page);

            if (index == -1)
                throw new ArgumentException("\"child\" is not a child of this control.");

            ColumnTabPage ctab = page as ColumnTabPage;

            if (ctab != null)
            {
                foreach (ChatControl c in ctab.Columns.SelectMany(x => x.Widgets).Where(w => w is ChatControl))
                {
                    TwitchChannel.RemoveChannel(c.ChannelName);
                }
            }

            Controls.Remove(_tabPages[index].Item1);
            _tabPages.RemoveAt(index);

            Controls.Remove(page);

            if (index < _tabPages.Count)
            {
                Select(_tabPages[index].Item2);
            }
            else if (_tabPages.Count > 0)
            {
                Select(_tabPages[index - 1].Item2);
            }
            else
            {
                var p = new ColumnTabPage();
                AddTab(p);
            }
        }

        public void Select(TabPage page)
        {
            var s = _selected;

            if (s != null)
            {
                s.Item1.Selected = false;
                s.Item2.Selected = false;
                Controls.Remove(s.Item2);
            }

            Selected = page;
            s = _selected;

            if (Selected != null)
            {
                s.Item1.Selected = true;
                s.Item2.Selected = true;
                Controls.Add(s.Item2);
                layout();
                TabPageSelected?.Invoke(this, new ValueEventArgs<TabPage>(page));
            }
        }
    }
}
