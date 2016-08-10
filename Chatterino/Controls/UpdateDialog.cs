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
    public partial class UpdateDialog : Form
    {
        public UpdateDialog()
        {
            InitializeComponent();

            Icon = App.Icon;
        }
    }
}
