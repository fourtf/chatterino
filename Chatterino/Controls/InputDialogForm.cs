using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chatterino.Controls
{
    public partial class InputDialogForm : Form
    {
        public string Value
        {
            get { return textBox.Text; }
            set { textBox.Text = value; }
        }

        public InputDialogForm(string title)
        {
            InitializeComponent();

            StartPosition = FormStartPosition.CenterScreen;

            Text = title;

            KeyPreview = true;

            KeyDown += (s, e) =>
            {
                if (textBox.Focused)
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        e.Handled = true;

                        okButton.PerformClick();
                    }
                }
            };
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }
    }
}
