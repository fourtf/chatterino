using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;

namespace Chatterino.Controls
{
#if DEBUG
    [Designer(typeof(SettingsTabControlDesigner))]
#endif
    public class SettingsTabControl : Control
    {
        public bool ProcessNewControls = true;

        public event EventHandler PageSelected;
        private SettingsTabPage selectedTab = null;

        public SettingsTabPage SelectedTab
        {
            get
            {
                return selectedTab;
            }
            set
            {
                var tmp = selectedIndex;
                if (selectedTab != null)
                {
                    Controls.Remove(selectedTab.Panel);
                    selectedTab.Selected = false;
                }
                selectedTab = value;
                selectedIndex = Controls.IndexOf((Control)value);
                if (selectedTab != null)
                    selectedTab.Selected = true;
                if (tmp != selectedIndex)
                    pageSelected();
            }
        }

        private int selectedIndex = 0;

        public int SelectedIndex
        {
            get
            {
                return selectedIndex - 1;
            }
            set
            {
                value = value + 1;
                if (value > 0 && value < Controls.Count)
                {
                    var tmp = selectedIndex;
                    if (selectedTab != null)
                        selectedTab.Selected = false;
                    if (Controls.Count > value)
                    {
                        selectedTab = (SettingsTabPage)Controls[value];
                        selectedIndex = value;
                    }
                    else
                    {
                        if (Controls.Count == 0)
                        {
                            selectedTab = null;
                            selectedIndex = 0;
                        }
                        else
                        {
                            selectedIndex = Controls.Count - 1;
                            selectedTab = (SettingsTabPage)Controls[selectedIndex];
                        }
                    }
                    if (selectedTab != null)
                        selectedTab.Selected = true;
                    if (tmp != selectedIndex)
                        pageSelected();
                }
            }
        }

        private int tabsWidth = 150;

        public int TabsWidth
        {
            get
            {
                return tabsWidth;
            }
            set
            {
                tabsWidth = value;
                OnLayout(new LayoutEventArgs(this, ""));
                SetPanelSize();
            }
        }

        void pageSelected()
        {
            if (Panel != null)
            if (SelectedTab != null)
            {
                if (SelectedTab.Panel != null)
                {
                    Panel.Controls.Add(SelectedTab.Panel);
                    Panel.Controls.SetChildIndex(Panel.Controls[Panel.Controls.Count - 1], 0);
                    Panel.Controls[0].Dock = DockStyle.Fill;
                }
            }

            if (PageSelected != null)
                PageSelected(this, EventArgs.Empty);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            SetPanelSize();
        }
        
        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            if (ProcessNewControls)
                if (e.Control is SettingsTabPage)
                {
                    ((SettingsTabPage)e.Control).Click += new EventHandler(s_Click);
                    ((SettingsTabPage)e.Control).PanelChanged += new EventHandler(s_PanelChanged);
                    if (Controls.Count == 2)
                    {
                        selectedTab = (SettingsTabPage)e.Control;
                        SelectedIndex = 0;
                        pageSelected();
                    }
                }
        }

        void s_PanelChanged(object sender, EventArgs e)
        {
            pageSelected();
        }

        public Panel Panel { get; set; }

        public int Count
        {
            get
            {
                return Controls.Count - 1;
            }
        }

        public SettingsTabPage this[int index]
        {
            get
            {
                return (SettingsTabPage)Controls[index + 1];
            }
        }

        //CTOR
        public SettingsTabControl()
        {
            //SetPanelSize();
            this.AllowDrop = true;
        }

        //SET SIZE
        public void SetPanelSize()
        {
            if (Panel != null)
            {
                Panel.Size = new Size(Width - tabsWidth, Height);
                Panel.Location = new Point(tabsWidth, 0);
            }
        }

        void s_Click(object sender, EventArgs e)
        {
            if (sender != (object)SelectedTab)
            {
                SelectedTab.Selected = false;
                ((SettingsTabPage)sender).Selected = true;
                SelectedTab = (SettingsTabPage)sender;
            }
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            var y = Padding.Top;
            for (var i = 1; i < Controls.Count; i++)
            {
                var c = Controls[i];
                if (c.Visible)
                {
                    c.Location = new Point(Padding.Left, y);
                    c.Width = tabsWidth;
                    y += c.Height;
                }
            }
        }

        Brush TabsBg = new SolidBrush(Color.FromArgb(64, 64, 64));

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            e.Graphics.FillRectangle(TabsBg, new Rectangle(0, 0, tabsWidth, Height));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //base.OnPaint(e);

            using (var gradientBrush = new LinearGradientBrush(new Point(tabsWidth - 16, 0), new Point(tabsWidth, 0)
            , Color.Transparent, Color.FromArgb(31, 0, 0, 0)))
            {
                e.Graphics.FillRectangle(gradientBrush, tabsWidth - 16, 0, 16, Height);
            }
        }
    }
}
