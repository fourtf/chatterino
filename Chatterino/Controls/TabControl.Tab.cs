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
                        tabPage.HighlightType = TabPageHighlightType.None;
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

            Rectangle xRectangle;

            bool mouseOverX;
            bool mouseDownX;

            // Constructor
            public Tab(TabControl tabControl, TabPage tabPage)
            {
                SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

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

                tabPage.HighlightTypeChanged += (s, e) =>
                {
                    Invalidate();
                };

                MouseDown += (s, e) =>
                {
                    tabControl.Select(tabPage);

                    if (xRectangle.Contains(e.Location))
                    {
                        mouseDownX = true;
                    }

                    mouseDown = true;
                };

                MouseUp += (s, e) =>
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        if (mouseDownX && xRectangle.Contains(e.Location))
                        {
                            tabControl.RemoveTab(tabPage);
                        }
                    }

                    if (e.Button == MouseButtons.Right)
                    {
                        menu.Show(this, e.Location);
                    }

                    if (e.Button == MouseButtons.Middle && ClientRectangle.Contains(e.Location))
                    {
                        tabControl.RemoveTab(tabPage);
                    }

                    mouseDownX = false;
                    mouseDown = false;
                };

                MouseEnter += (s, e) =>
                {
                    mouseOver = true;

                    Invalidate();
                };

                MouseLeave += (s, e) =>
                {
                    mouseOver = false;
                    mouseOverX = false;

                    Invalidate();
                };

                MouseMove += (s, e) =>
                {
                    if (mouseDown)
                    {
                        var t = ((TabControl)Parent);
                        var p = t.PointToClient(PointToScreen(e.Location));

                        if (p != lastP)
                        {
                            var originalIndex = t._tabPages.FindIndex(x => x.Item1 == this);
                            var original = t._tabPages[originalIndex];

                            for (var i = 0; i < t._tabPages.Count; i++)
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

                    mouseOverX = xRectangle.Contains(e.Location);

                    Invalidate();
                };

                var dragOver = false;

                DragEnter += (s, e) =>
                {
                    dragOver = true;

                    var timer = new Timer() { Interval = 200 };

                    timer.Tick += (s2, e2) =>
                    {
                        if (dragOver)
                        {
                            (Parent as TabControl)?.Select(tabPage);
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

            protected override void OnResize(EventArgs e)
            {
                xRectangle = new Rectangle(Width - 20, Height / 2 - 8, 16, 16);

                base.OnResize(e);
            }

            void rename()
            {
                var page = tabPage as ColumnTabPage;

                if (page != null)
                {
                    using (var dialog = new InputDialogForm("change tab title (leave empty for default)"))
                    {
                        dialog.Value = page.CustomTitle ?? "";
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            var title = dialog.Value.Trim();

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

            private void calcSize()
            {
                if (titleWidth == -1)
                {
                    var size = TextRenderer.MeasureText(tabPage.Title, Font);
                    Width = (int)(Padding.Left + size.Width + Padding.Right) + 12;
                    Height = GetHeight();
                }
            }

            Brush mouseOverXBrush = new SolidBrush(Color.FromArgb(64, 0, 0, 0));

            protected override void OnPaint(PaintEventArgs e)
            {
                Brush bg;
                Color text;

                if (Selected)
                {
                    bg = App.ColorScheme.TabSelectedBG;
                    text = App.ColorScheme.TabSelectedText;
                }
                else if (mouseOver)
                {
                    bg = App.ColorScheme.TabHoverBG;
                    text = App.ColorScheme.TabHoverText;
                }
                else if (tabPage.HighlightType == TabPageHighlightType.Highlighted)
                {
                    bg = App.ColorScheme.TabHighlightedBG;
                    text = App.ColorScheme.TabHighlightedText;
                }
                else if (tabPage.HighlightType == TabPageHighlightType.NewMessage)
                {
                    bg = App.ColorScheme.TabNewMessageBG;
                    text = App.ColorScheme.TabHighlightedText;
                }
                else
                {
                    bg = App.ColorScheme.TabBG;
                    text = App.ColorScheme.TabText;
                }

                e.Graphics.FillRectangle(bg, 0, 0, Width, Height);

                // text
                TextRenderer.DrawText(e.Graphics, tabPage.Title ?? "<no name>", Font, new Rectangle(0, 0, xRectangle.Left + 4, Height), text, App.DefaultTextFormatFlags | TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);

                // x
                if (mouseDownX || !mouseDown)
                {
                    if (mouseOver && mouseOverX)
                    {
                        e.Graphics.FillRectangle(mouseOverXBrush, xRectangle);

                        if (mouseDown)
                        {
                            e.Graphics.FillRectangle(mouseOverXBrush, xRectangle);
                        }
                    }
                }

                if (Selected || mouseOver)
                {
                    using (var pen = new Pen(text))
                    {
                        e.Graphics.DrawLine(pen, xRectangle.Left + 4, xRectangle.Top + 4, xRectangle.Right - 5, xRectangle.Bottom - 5);
                        e.Graphics.DrawLine(pen, xRectangle.Right - 5, xRectangle.Top + 4, xRectangle.Left + 4, xRectangle.Bottom - 5);
                    }
                }
            }

            internal static int GetHeight()
            {
                return 24;
            }
        }
    }
}
