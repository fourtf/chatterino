using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Chatterino.Common;

namespace Chatterino.Controls
{
    public partial class WelcomeForm : Form
    {
        public WelcomeForm()
        {
            InitializeComponent();

            Icon = App.Icon;

            comboTheme.Text = AppSettings.Theme;

            comboTheme.SelectedValueChanged += (s, e) =>
            {
                AppSettings.Theme = comboTheme.Text;
            };

            button1.Click += (s, e) =>
            {
                Close();
            };
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (var loginForm = new LoginForm())
            {
                loginForm.ShowDialog();

                loginForm.FormClosed += delegate
                {
                    if (loginForm.Account != null)
                    {
                        AccountManager.AddAccount(loginForm.Account);

                        IrcManager.Account = loginForm.Account;
                        IrcManager.Connect();
                    }
                };
            }
        }
    }
}
