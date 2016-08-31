using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chatterino.Controls
{
    public partial class TabControl
    {
        private class Tab : Control
        {
            private bool selected;

            public bool Selected
            {
                get { return selected; }
                set
                {
                    if (selected != value)
                    {
                        selected = value;
                        Invalidate();
                    }
                }
            }

            private bool mouseOver = false, mouseDown = false;

            TabControl tabControl;
            TabPage tabPage;

            double titleWidth = -1;

            // Context menu
            ContextMenu menu = new ContextMenu();

            Point lastP = Point.Empty;

            // Constructor
            public Tab(TabControl tabControl, TabPage tabPage)
            {
                AllowDrop = true;

                Text = "tab";

                Padding = new Padding(8, 4, 8, 4);

                this.tabControl = tabControl;
                this.tabPage = tabPage;

                tabPage.TitleChanged += (s, e) =>
                {
                    titleWidth = -1;
                    calcSize();
                    tabControl.layout();
                    Invalidate();
                };

                MouseDown += (s, e) =>
                {
                    tabControl.select(tabPage);
                    mouseDown = true;
                };

                MouseUp += (s, e) =>
                {
                    mouseDown = false;
                    if (e.Button == MouseButtons.Right)
                    {
                        menu.Show(this, e.Location);
                    }
                };

                MouseEnter += (s, e) =>
                {
                    mouseOver = true;

                    Invalidate();
                };

                MouseLeave += (s, e) =>
                {
                    mouseOver = false;

                    Invalidate();
                };

                MouseMove += (s, e) =>
                {
                    if (mouseDown)
                    {
                        var t = ((TabControl)Parent);
                        Point p = t.PointToClient(PointToScreen(e.Location));

                        if (p != lastP)
                        {
                            int originalIndex = t._tabPages.FindIndex(x => x.Item1 == this);
                            Tuple<Tab, TabPage> original = t._tabPages[originalIndex];

                            for (int i = 0; i < t._tabPages.Count; i++)
                            {
                                var tab = t._tabPages[i];

                                if (tab.Item1 != this)
                                {
                                    if (tab.Item1.Bounds.Contains(p) && (tab.Item1.Location.Y != original.Item1.Location.Y || p.X < tab.Item1.Location.X + original.Item1.Width))
                                    {
                                        t._tabPages.RemoveAt(originalIndex);

                                        t._tabPages.Insert(i, original);
                                        t.layout();
                                        break;
                                    }
                                }
                            }
                            lastP = p;
                        }
                    }
                };

                bool dragOver = false;

                DragEnter += (s, e) =>
                {
                    dragOver = true;

                    Timer timer = new Timer() { Interval = 200 };

                    timer.Tick += (s2, e2) =>
                    {
                        if (dragOver)
                        {
                            (Parent as TabControl)?.select(tabPage);
                        }
                        timer.Dispose();
                    };

                    timer.Start();
                };

                DragLeave += (s, e) =>
                {
                    dragOver = false;
                };

                calcSize();

                // Context
                menu.MenuItems.Add(new MenuItem("Rename", (s, e) => rename()));
                menu.MenuItems.Add(new MenuItem("Close", (s, e) => (Parent as TabControl)?.RemoveTab(tabPage)));
            }

            void rename()
            {
                var page = tabPage as ColumnTabPage;

                if (page != null)
                {
                    using (InputDialogForm dialog = new InputDialogForm("change tab title (leave empty for default)"))
                    {
                        dialog.Value = page.CustomTitle ?? "";
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            string title = dialog.Value.Trim();

                            if (title == "")
                            {
                                page.CustomTitle = null;
                            }
                            else
                            {
                                page.CustomTitle = title;
                            }
                        }
                    }
                }
            }

            protected override void OnDoubleClick(EventArgs e)
            {
                base.OnDoubleClick(e);

                rename();
            }

            protected override void OnMouseUp(MouseEventArgs e)
            {
                base.OnMouseUp(e);

                if (e.Button == MouseButtons.Middle)
                {
                    tabControl.RemoveTab(tabPage);
                }
            }

            private void calcSize()
            {
                if (titleWidth == -1)
                {
                    var size = TextRenderer.MeasureText(tabPage.Title, Font);
                    Width = (int)(Padding.Left + size.Width + Padding.Right);
                    Height = GetHeight();
                }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                e.Graphics.FillRectangle(Selected ? App.ColorScheme.TabSelectedBG : (mouseOver ? App.ColorScheme.TabHoverBG : App.ColorScheme.TabBG), 0, 0, Width, Height);

                // text
                TextRenderer.DrawText(e.Graphics, tabPage.Title ?? "<no name>", Font, new Rectangle(0, 0, Width, Height), Selected ? App.ColorScheme.TabSelectedText : (mouseOver ? App.ColorScheme.TabHoverText : App.ColorScheme.TabText), App.DefaultTextFormatFlags | TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
            }

            internal static int GetHeight()
            {
                return 24;
            }
        }
    }
}
