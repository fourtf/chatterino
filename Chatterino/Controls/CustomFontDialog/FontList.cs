using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace CustomFontDialog
{
    public partial class FontList : UserControl
    {

        public event EventHandler SelectedFontFamilyChanged;
        public RecentlyUsedList<Font> RecentlyUsed = new RecentlyUsedList<Font>(5);
        private int lastSelectedIndex = -1;

        public FontList()
        {
            InitializeComponent();

            
            lstFont.Items.Add("Section"); //section entry for Recently Used
            
            lstFont.Items.Add("Section"); //section entry for All Fonts
            foreach (FontFamily f in FontFamily.Families)
            {
                try
                {
                    if (f.Name != null || f.Name != "")
                        lstFont.Items.Add(new Font(f, 12));
                }
                catch {}

            }         

          
        }

        private const int RecentlyUsedSectionIndex = 0;
        private int AllFontsSectionIndex
        {
            get
            {
                return RecentlyUsed.Count + 1;
            }
        }

        private int AllFontsStartIndex
        {
            get
            {
                return RecentlyUsed.Count + 2;
            }
        }

        

        public FontFamily SelectedFontFamily
        {
            get 
            {
                if (lstFont.SelectedItem != null)
                    return ((Font)lstFont.SelectedItem).FontFamily;
                else
                    return null;
            }
            set
            {
                if (value == null) lstFont.ClearSelected();
                else
                {
                    lstFont.SelectedIndex = IndexOf(value);
                }

            }
        }

        public int IndexOf(FontFamily ff)
        {
            for (int i = 1; i < lstFont.Items.Count; i++)
            {
#pragma warning disable CS0252
                if (lstFont.Items[i] == "Section") continue;
#pragma warning restore CS0252
                Font f = (Font)lstFont.Items[i];
                if (f.FontFamily.Name == ff.Name)
                {
                    return i;
                }
            }

            return -1;
        }

        public void AddSelectedFontToRecent()
        {
            if (lstFont.SelectedIndex < 1) return;

            lstFont.SuspendLayout();

            int tmpCount = RecentlyUsed.Count;

            RecentlyUsed.Add((Font)lstFont.SelectedItem);

            for (int i = 1; i <= tmpCount; i++)
            {
                lstFont.Items.RemoveAt(1);
            }            
                        
            for (int i = 0; i < RecentlyUsed.Count; i++)
            {
                lstFont.Items.Insert(i+1, RecentlyUsed[i]); 
            }

            lstFont.SelectedIndex = 1;

            lstFont.ResumeLayout();
            
        }

        public void AddFontToRecent(FontFamily ff)
        {
            lstFont.SuspendLayout();

            for (int i = 1; i <= RecentlyUsed.Count; i++)
            {
                lstFont.Items.RemoveAt(1);
            }

            RecentlyUsed.Add((Font)lstFont.Items[IndexOf(ff)]);

            for (int i = 0; i < RecentlyUsed.Count; i++)
            {
                lstFont.Items.Insert(i + 1, RecentlyUsed[i]);
            }

            //lstFont.SelectedIndex = 1;

            lstFont.ResumeLayout();

        }

        
        private void lstFont_DrawItem(object sender, DrawItemEventArgs e)
        {
            

            if (e.Index == 0)
            {
                e.Graphics.FillRectangle(Brushes.AliceBlue, e.Bounds);
                Font font = new Font(DefaultFont, FontStyle.Bold | FontStyle.Italic);
                e.Graphics.DrawString("Default Font", font, Brushes.Black, e.Bounds.X + 10, e.Bounds.Y + 3, StringFormat.GenericDefault);
            }
            else if (e.Index == AllFontsStartIndex - 1)
            {
                e.Graphics.FillRectangle(Brushes.AliceBlue, e.Bounds);
                Font font = new Font(DefaultFont, FontStyle.Bold | FontStyle.Italic);
                e.Graphics.DrawString("All Fonts", font, Brushes.Black, e.Bounds.X + 10, e.Bounds.Y + 3, StringFormat.GenericDefault);
            }
            else
            {
                // Draw the background of the ListBox control for each item.
                e.DrawBackground();

                Font font = (Font)lstFont.Items[e.Index];
                e.Graphics.DrawString(font.Name, font, Brushes.Black, e.Bounds, StringFormat.GenericDefault);

                // If the ListBox has focus, draw a focus rectangle around the selected item.
                e.DrawFocusRectangle();
            }          

            
            
            
        }

        private void lstFont_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            if (lstFont.SelectedIndex == RecentlyUsedSectionIndex || lstFont.SelectedIndex == AllFontsSectionIndex)
            {
                lstFont.SelectedIndex = lastSelectedIndex;
            }
            else if(lstFont.SelectedItem != null)
            {
                if (!txtFont.Focused)
                {
                    Font f = (Font)lstFont.SelectedItem;
                    txtFont.Text = f.Name;
                }

                SelectedFontFamilyChanged(lstFont, new EventArgs());
                lastSelectedIndex = lstFont.SelectedIndex;
            }
        }

        private void txtFont_TextChanged(object sender, EventArgs e)
        {
            if (!txtFont.Focused) return;

            for(int i = AllFontsStartIndex; i < lstFont.Items.Count; i++)
            {
                string str = ((Font)lstFont.Items[i]).Name;
                if (str.StartsWith(txtFont.Text, true, null))
                {
                    lstFont.SelectedIndex = i;

                    const uint WM_VSCROLL = 0x0115;
                    const uint SB_THUMBPOSITION = 4;

                    uint b = ((uint)(lstFont.SelectedIndex) << 16) | (SB_THUMBPOSITION & 0xffff);
                    SendMessage(lstFont.Handle, WM_VSCROLL, b, 0);

                    return;
                }               
            }
        }

        private void txtFont_MouseClick(object sender, MouseEventArgs e)
        {
            txtFont.SelectAll();
        }

        
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, uint wParam, uint lParam);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lstFont_KeyDown(object sender, KeyEventArgs e)
        {
            // if you type alphanumeric characters while focus is on ListBox, it shifts the focus to TextBox.
            if (Char.IsLetterOrDigit((char)e.KeyValue))
            {
                txtFont.Focus();
                txtFont.Text = ((char)e.KeyValue).ToString();
                txtFont.SelectionStart = 1;
            }


            // allows to move between sections using arrow keys
            switch (e.KeyCode)
            {
                case Keys.Left:
                case Keys.Up:
                    if (lstFont.SelectedIndex == AllFontsSectionIndex + 1)
                    {
                        lstFont.SelectedIndex = lstFont.SelectedIndex - 2;
                        e.SuppressKeyPress = true;
                    }
                    break;
                case Keys.Down:
                case Keys.Right:
                    if (lstFont.SelectedIndex == AllFontsSectionIndex - 1)
                    {
                        lstFont.SelectedIndex = lstFont.SelectedIndex + 2;
                        e.SuppressKeyPress = true;
                    }
                    break;

            }
        }

        // ensures that focus is lstFont control whenever the form is loaded
        private void FontList_Load(object sender, EventArgs e)
        {
            this.ActiveControl = lstFont;
        }
               
                       
    }
}
