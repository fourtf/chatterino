using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CustomFontDialog
{
    public partial class FontDialog : Form
    {
        public FontDialog()
        {
            InitializeComponent();

            lstFont.SelectedFontFamilyChanged += lstFont_SelectedFontFamilyChanged;
            lstFont.SelectedFontFamily = FontFamily.GenericSansSerif;
            lstFont.AddSelectedFontToRecent();
            txtSize.Text = Convert.ToString(10);
        }

        public new Font Font
        {
            get
            {
                return lblSampleText.Font;
            }
            set
            {
                lstFont.AddFontToRecent(value.FontFamily);
                lstFont.SelectedFontFamily = value.FontFamily;
                txtSize.Text = value.Size.ToString();
                chbBold.Checked = value.Bold;
                chbItalic.Checked = value.Italic;
                chbStrikeout.Checked = value.Strikeout;
            }
        }

        public void AddFontToRecentList(FontFamily ff)
        {
            lstFont.AddFontToRecent(ff);
        }

        private void lstFont_SelectedFontFamilyChanged(object sender, EventArgs e)
        {
            UpdateSampleText();
        }

        private void lstSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(lstSize.SelectedItem != null)
                txtSize.Text = lstSize.SelectedItem.ToString();
        }

        private void txtSize_TextChanged(object sender, EventArgs e)
        {
            if (lstSize.Items.Contains(txtSize.Text))
                lstSize.SelectedItem = txtSize.Text;
            else
                lstSize.ClearSelected();
            
            UpdateSampleText();
        }

        
        private void txtSize_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyData)
            {                   
                case Keys.D0:
                case Keys.D1:
                case Keys.D2:
                case Keys.D3:
                case Keys.D4:
                case Keys.D5:
                case Keys.D6:
                case Keys.D7:
                case Keys.D8:
                case Keys.D9:
                case Keys.End:
                case Keys.Enter:
                case Keys.Home:                
                case Keys.Back:
                case Keys.Delete:
                case Keys.Escape:
                case Keys.Left:
                case Keys.Right:
                    break;
                case Keys.Decimal:
                case (Keys)190: //decimal point
                    if (txtSize.Text.Contains("."))
                    {
                        e.SuppressKeyPress = true;
                        e.Handled = true;
                    }
                    break;
                default:
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                    break;
            }
            
        }

        private void UpdateSampleText()
        {
            float size = txtSize.Text != "" ? float.Parse(txtSize.Text) : 1;
            FontStyle style = chbBold.Checked ? FontStyle.Bold : FontStyle.Regular;
            if (chbItalic.Checked)  style |= FontStyle.Italic;
            if (chbStrikeout.Checked) style |= FontStyle.Strikeout;
            Font tmp = lblSampleText.Font;
            lblSampleText.Font = new Font(lstFont.SelectedFontFamily, size, style);
            tmp.Dispose();
        }

        /// <summary>
        /// Handles CheckedChanged event for Bold, 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chb_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSampleText();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            lstFont.AddSelectedFontToRecent();
        }

        
        

        
    }
}
