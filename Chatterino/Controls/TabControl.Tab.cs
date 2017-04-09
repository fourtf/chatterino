using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Chatterino.Common;

namespace Chatterino.Controls
{
    public partial class TabControl
    {
        private class Tab : Control
        {
            private bool _selected;

            public bool Selected
            {
                get { return _selected; }
                set
                {
                    if (_selected != value)
                    {
                        _selected = value;
                        Invalidate();
                        _tabPage.HighlightType = TabPageHighlightType.None;
                    }
                }
            }

            private bool _mouseOver = false, _mouseDown = false;

            private TabPage _tabPage;

            private double _titleWidth = -1;

            // Context menu
            private ContextMenu _menu = new ContextMenu();
            private MenuItem _allowNewMessageHighlightsMenuItem;
            private Point _lastP = Point.Empty;

            private Rectangle _xRectangle;

            private bool _mouseOverX;
            private bool _mouseDownX;

            // Constructor
            public Tab(TabControl tabControl, TabPage tabPage)
            {
                SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

                AllowDrop = true;

                Text = "tab";

                Padding = new Padding(8, 4, 8, 4);

                _tabPage = tabPage;

                tabPage.TitleChanged += (s, e) =>
                {
                    _titleWidth = -1;
                    CalcSize();
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

                    if (_xRectangle.Contains(e.Location))
                    {
                        _mouseDownX = true;
                    }

                    _mouseDown = true;
                };

                MouseUp += (s, e) =>
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        if (_mouseDownX && _xRectangle.Contains(e.Location))
                        {
                            tabControl.RemoveTab(tabPage);
                        }
                    }

                    if (e.Button == MouseButtons.Right)
                    {
                        _menu.Show(this, e.Location);
                    }

                    if (e.Button == MouseButtons.Middle && ClientRectangle.Contains(e.Location))
                    {
                        tabControl.RemoveTab(tabPage);
                    }

                    _mouseDownX = false;
                    _mouseDown = false;
                };

                MouseEnter += (s, e) =>
                {
                    _mouseOver = true;

                    Invalidate();
                };

                MouseLeave += (s, e) =>
                {
                    _mouseOver = false;
                    _mouseOverX = false;

                    Invalidate();
                };

                MouseMove += (s, e) =>
                {
                    if (_mouseDown)
                    {
                        var t = ((TabControl)Parent);
                        var p = t.PointToClient(PointToScreen(e.Location));

                        if (p != _lastP)
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
                            _lastP = p;
                        }
                    }

                    _mouseOverX = _xRectangle.Contains(e.Location);

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

                CalcSize();

                // Context
                _menu.MenuItems.Add(new MenuItem("Rename", (s, e) => Rename()));
                _menu.MenuItems.Add(new MenuItem("Close", (s, e) => (Parent as TabControl)?.RemoveTab(tabPage)));
                _menu.MenuItems.Add(_allowNewMessageHighlightsMenuItem = new MenuItem("Enable highlights on new message", (s, e) => _tabPage.EnableNewMessageHighlights = !_tabPage.EnableNewMessageHighlights));

                _menu.Popup += (s, e) =>
                {
                    _allowNewMessageHighlightsMenuItem.Checked = _tabPage.EnableNewMessageHighlights;
                };
            }

            protected override void OnResize(EventArgs e)
            {
                _xRectangle = new Rectangle(Width - 20, Height / 2 - 8, 16, 16);

                base.OnResize(e);
            }

            private void Rename()
            {
                var page = _tabPage as ColumnTabPage;

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

                Rename();
            }

            private void CalcSize()
            {
                if (_titleWidth == -1)
                {
                    var size = TextRenderer.MeasureText(_tabPage.Title, Font);
                    Width = (int)(Padding.Left + size.Width + Padding.Right) + 12;
                    Height = GetHeight();
                }
            }

            private Brush _mouseOverXBrush = new SolidBrush(Color.FromArgb(64, 0, 0, 0));

            protected override void OnPaint(PaintEventArgs e)
            {
                Brush bg;
                Color text;

                if (Selected)
                {
                    bg = App.ColorScheme.TabSelectedBG;
                    text = App.ColorScheme.TabSelectedText;
                }
                else if (_mouseOver)
                {
                    bg = App.ColorScheme.TabHoverBG;
                    text = App.ColorScheme.TabHoverText;
                }
                else if (_tabPage.HighlightType == TabPageHighlightType.Highlighted)
                {
                    bg = App.ColorScheme.TabHighlightedBG;
                    text = App.ColorScheme.TabHighlightedText;
                }
                else if (_tabPage.HighlightType == TabPageHighlightType.NewMessage)
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
                TextRenderer.DrawText(e.Graphics, _tabPage.Title ?? "<no name>", Font, new Rectangle(0, 0, _xRectangle.Left + 4, Height), text, App.DefaultTextFormatFlags | TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);

                // x
                if (_mouseDownX || !_mouseDown)
                {
                    if (_mouseOver && _mouseOverX)
                    {
                        e.Graphics.FillRectangle(_mouseOverXBrush, _xRectangle);

                        if (_mouseDown)
                        {
                            e.Graphics.FillRectangle(_mouseOverXBrush, _xRectangle);
                        }
                    }
                }

                if (Selected || _mouseOver)
                {
                    using (var pen = new Pen(text))
                    {
                        e.Graphics.DrawLine(pen, _xRectangle.Left + 4, _xRectangle.Top + 4, _xRectangle.Right - 5, _xRectangle.Bottom - 5);
                        e.Graphics.DrawLine(pen, _xRectangle.Right - 5, _xRectangle.Top + 4, _xRectangle.Left + 4, _xRectangle.Bottom - 5);
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
