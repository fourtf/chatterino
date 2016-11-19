using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Chatterino.Common;

namespace Chatterino
{
    public class UserSwitchPopup : Form
    {
        public UserSwitchPopup()
        {
            FormBorderStyle = FormBorderStyle.None;

            var listView = new ListView()
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 14f),
                View = View.List,
            };

            Controls.Add(listView);

            listView.Items.Add("anonymous user");

            foreach (var account in AccountManager.Accounts)
            {
                listView.Items.Add(account.Username);
            }

            listView.ItemActivate += (s, e) =>
            {
                IrcManager.Account = AccountManager.FromUsername(listView.FocusedItem.Text) ?? Account.AnonAccount;
                IrcManager.Connect();

                Close();
            };
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);

            Close();
        }
    }
}
