#if DEBUG
using System;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.ComponentModel.Design;
using System.Collections.Generic;


namespace Chatterino.Controls
{
    public class SettingsTabControlDesigner : ControlDesigner
    {
        DesignerVerbCollection verbs;
        public override DesignerVerbCollection Verbs
        {
            get
            {
                if (verbs == null)
                    verbs = new DesignerVerbCollection(new DesignerVerb[]
                    {
                        new DesignerVerb("Add Tab", OnAdd),
                        new DesignerVerb("Move Up", OnMoveUp),
                        new DesignerVerb("Move Down", OnMoveDown),
                        new DesignerVerb("Dock", OnDock),
                    });
                return verbs;
            }
        }

        //public override IList SnapLines
        //{
        //    get
        //    {
        //        return null;
        //    }
        //}

        void OnDock(object sender, EventArgs e)
        {
            ((SettingsTabControl)Component).Dock = DockStyle.Fill;
        }

        void OnAdd(object sender, EventArgs e)
        {
            var newPage = (SettingsTabPage)((IDesignerHost)GetService(typeof(IDesignerHost))).CreateComponent(typeof(SettingsTabPage));

            newPage.Panel = (Panel)((IDesignerHost)GetService(typeof(IDesignerHost))).CreateComponent(typeof(Panel));
            newPage.Text = newPage.Name;

            ((SettingsTabControl)Component).Controls.Add(newPage);
        }

        void OnMoveDown(object sender, EventArgs e)
        {
            var index = ((SettingsTabControl)Component).SelectedIndex + 1;
            var p = ((SettingsTabControl)Component).SelectedTab;
            ((SettingsTabControl)Component).Controls.Remove(p);

            var C = new List<Control>();
            for (var i = ((SettingsTabControl)Component).Controls.Count - 1; i >= index; i--)
            {
                C.Add(((SettingsTabControl)Component).Controls[i]);
                ((SettingsTabControl)Component).Controls.RemoveAt(i);
            }

            if (C.Count != 0)
            {
                ((SettingsTabControl)Component).Controls.Add(C[C.Count - 1]);
                C.RemoveAt(C.Count - 1);
            }
            ((SettingsTabControl)Component).Controls.Add(p);
            for (var i = C.Count - 1; i >= 0; i--)
            {
                ((SettingsTabControl)Component).Controls.Add(C[i]);
            }

            ((SettingsTabControl)Component).SelectedTab = p;
        }

        void OnMoveUp(object sender, EventArgs e)
        {
            if (((SettingsTabControl)Component).Controls.Count > 1)
                if (((SettingsTabControl)Component).Controls.Count > ((SettingsTabControl)Component).SelectedIndex + 2)
                {
                    var index = ((SettingsTabControl)Component).SelectedIndex + 1;
                    var p = ((SettingsTabControl)Component).SelectedTab;
                    ((SettingsTabControl)Component).Controls.Remove(p);

                    var C = new List<Control>();
                    for (var i = ((SettingsTabControl)Component).Controls.Count - 1; i >= index - 1; i--)
                    {
                        C.Add(((SettingsTabControl)Component).Controls[i]);
                        ((SettingsTabControl)Component).Controls.RemoveAt(i);
                    }

                    ((SettingsTabControl)Component).Controls.Add(p);
                    for (var i = C.Count - 1; i >= 0; i--)
                    {
                        ((SettingsTabControl)Component).Controls.Add(C[i]);
                    }

                    ((SettingsTabControl)Component).SelectedTab = p;
                }
        }

        public override void InitializeNewComponent(IDictionary defaultValues)
        {
            base.InitializeNewComponent(defaultValues);

            try
            {
                //MessageBox.Show("1");
                ((SettingsTabControl)Component).Panel = (Panel)((IDesignerHost)GetService(typeof(IDesignerHost))).CreateComponent(typeof(Panel));
                //MessageBox.Show("2");
                ((SettingsTabControl)Component).Panel.Anchor = (AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom);
                //MessageBox.Show("3");
                ((SettingsTabControl)Component).Panel.Padding = new Padding(0, 0, 0, 40);
                //MessageBox.Show("4");
                ((SettingsTabControl)Component).Controls.Add(((SettingsTabControl)Component).Panel);
                //MessageBox.Show("5");
                ((SettingsTabControl)Component).SetPanelSize();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        protected override void Dispose(bool disposing)
        {
            for (var i = 0; i < ((SettingsTabControl)Component).Controls.Count; )
            {
                ((SettingsTabControl)Component).Controls[0].Dispose();
            }
            base.Dispose(disposing);
        }

        /*protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            //if (m.Msg == 0x200) //WM_MOUSEMOVE
            //{
            //    int lParam = m.LParam.ToInt32();
            //    Point hitPoint = new Point(lParam & 0xffff, lParam >> 0x10);
            //    mDragY = hitPoint.Y;
            //}
            //if (m.Msg == 0x201) //WM_LBUTTONDOWN
            //{
            //    SettingsTabControl tabs = (SettingsTabControl)Component;
            //    int lParam = m.LParam.ToInt32();
            //    Point hitPoint = new Point(lParam & 0xffff, lParam >> 0x10);
            //
            //    if (hitPoint.Y > ((SettingsTabControl)Component).Height-22)
            //        if (hitPoint.X < ((SettingsTabControl)Component).TabsWidth)
            //        {
            //            if (hitPoint.X < ((SettingsTabControl)Component).TabsWidth / 2)
            //                tabs.SelectedIndex--;
            //            else
            //                tabs.SelectedIndex++;
            //        }
            //    //if (Control.FromHandle(m.HWnd) == null)
            //    //{
            //    //    if (hitPoint.X < 18 && tabs.SelectedIndex > 0)
            //    //        tabs.SelectedIndex--;
            //    //    else
            //    //        tabs.SelectedIndex++;
            //    //}
            //    //else
            //    //{
            //    //    // Header click
            //    //    for (int i = 1; i < tabs.Controls.Count; i++)
            //    //        if (((SettingsTabPage)tabs.Controls[i]).GetTabRect().Contains(hitPoint))
            //    //        {
            //    //            tabs.SelectedIndex = i;
            //    //            return;
            //    //        }
            //    //}
            //}
        }*/

        /*int mDragY = 0;
        int mDragToIndex = -1;

        protected override void OnDragDrop(DragEventArgs de)
        {
            //((IDropTarget)((SettingsTabControl)Component).SelectedTab).OnDragDrop(de);

            for (int i = 1; i < ((SettingsTabControl)Component).Controls.Count; i++)
            {
                if (((SettingsTabControl)Component).Controls[i].Location.Y < de.Y
                    && (((SettingsTabControl)Component).Controls[i].Location.Y + ((SettingsTabControl)Component).Controls[i].Height > de.Y))
                {
                    mDragToIndex = i;
                    MessageBox.Show("Test");
                }
            }

            base.OnDragDrop(de);
        }

        protected override void OnDragComplete(DragEventArgs de)
        {
            base.OnDragComplete(de);
            if (mDragToIndex != -1)
            {
                ((SettingsTabControl)Component).Controls.SetChildIndex(((SettingsTabControl)Component).Controls[((SettingsTabControl)Component).Controls.Count - 1], mDragToIndex);
                mDragToIndex = -1;
            }
            ((SettingsTabControl)Component).PerformLayout();
        }
        
        protected override void OnDragEnter(DragEventArgs de)
        {
            //((IDropTarget)((SettingsTabControl)Component).SelectedTab).OnDragEnter(de);
        }
        
        protected override void OnDragLeave(EventArgs e)
        {
            //((IDropTarget)((SettingsTabControl)Component).SelectedTab).OnDragLeave(e);
        }
        
        protected override void OnDragOver(DragEventArgs de)
        {
            base.OnDragOver(de);
            //((IDropTarget)((SettingsTabControl)Component).SelectedTab).OnDragOver(de);
            mDragY = de.Y;
        }*/
    }

    public class SettingsTabPageDesigner : ControlDesigner
    {
        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == 0x201) //WM_LBUTTONDOWN
            {
                var lParam = m.LParam.ToInt32();
                var hitPoint = new Point(lParam & 0xffff, lParam >> 0x10);
                ((SettingsTabControl)((SettingsTabPage)Component).Parent).SelectedTab = ((SettingsTabPage)Component);
            }
        }
    }
}
#endif