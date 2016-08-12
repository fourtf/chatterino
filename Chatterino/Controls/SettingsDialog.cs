using Chatterino.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;


namespace Chatterino.Controls
{
    public partial class SettingsDialog : Form
    {
        public string OriginalSettings = "";
        private Button btnCancel;
        private Button BtnOK;
        //private Button btnResetCurrent;
        //private Button btnResetAll;

        //CTOR
        public SettingsDialog()
        {
            InitializeComponent();

            Icon = App.Icon;

            try
            {
                //this.Icon = Program.AppIcon;
            }
            catch { }

            // Meebey.SmartIrc4net.ProxyType.

            tabs.SelectedIndex = 0;
            tabs.PageSelected += tabs_PageSelected;
            tabs_PageSelected(this, EventArgs.Empty);

            BindCheckBox(chkTimestamps, "ChatShowTimestamps");
            BindCheckBox(chkTimestampSeconds, "ChatShowTimestampSeconds");
            BindCheckBox(chkAllowSameMessages, "ChatAllowSameMessage");
            BindCheckBox(chkDoubleClickLinks, "ChatLinksDoubleClickOnly");
            BindCheckBox(chkHideInput, "ChatHideInputIfEmpty");

            BindTextBox(txtMsgLimit, "ChatMessageLimit");

            BindCheckBox(chkHighlight, "ChatEnableHighlight");
            BindCheckBox(chkPings, "ChatEnableHighlightSound");
            BindCheckBox(chkFlashTaskbar, "ChatEnableHighlightTaskbar");

            BindCheckBox(chkBttvEmotes, "ChatEnableBttvEmotes");
            BindCheckBox(chkFFzEmotes, "ChatEnableFfzEmotes");
            BindCheckBox(chkEmojis, "ChatEnableEmojis");
            BindCheckBox(chkGifEmotes, "ChatEnableGifAnimations");

            BindCheckBox(chkProxyEnabled, "ProxyEnable");
            BindTextBox(textBox1, "ProxyHost");
            BindTextBox(textBox4, "ProxyPort");
            BindTextBox(textBox2, "ProxyUsername");
            BindTextBox(textBox3, "ProxyPassword");

            rtbHighlights.Text = string.Join(Environment.NewLine, AppSettings.ChatCustomHighlights);
            onSave += (s, e) =>
            {
                List<string> list = new List<string>();
                StringReader reader = new StringReader(rtbHighlights.Text);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    list.Add(line.Trim());
                }
                AppSettings.ChatCustomHighlights = list.ToArray();
            };

            rtbIgnoredUsernames.Text = string.Join(Environment.NewLine, AppSettings.chatHighlightsIgnoreUsernames.Keys);
            onSave += (s, e) =>
            {
                List<string> list = new List<string>();
                StringReader reader = new StringReader(rtbIgnoredUsernames.Text);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    list.Add(line.Trim().ToLower());
                }
                AppSettings.ChatHighlightsIgnoreUsernames = list.ToArray();
            };

            //Buttons
            int x = 0;

            ///Cancel
            btnCancel = new Button();
            btnCancel.AutoSize = true;
            btnCancel.Text = "Cancel";
            btnCancel.Location = new Point(tabs.Panel.Width - 12 - btnCancel.Width - x, tabs.Panel.Height - 12 - btnCancel.Height);
            btnCancel.Anchor = (AnchorStyles.Right | AnchorStyles.Bottom);
            btnCancel.BackColor = Color.FromArgb(0);
            btnCancel.Click += new EventHandler(btnCancel_Click);
            tabs.Panel.Controls.Add(btnCancel);
            x += 12 + btnCancel.Width;

            ///OK
            BtnOK = new Button();
            BtnOK.AutoSize = true;
            BtnOK.Text = "Apply";
            BtnOK.Location = new Point(tabs.Panel.Width - 12 - BtnOK.Width - x, tabs.Panel.Height - 12 - btnCancel.Height);
            BtnOK.Anchor = (AnchorStyles.Right | AnchorStyles.Bottom);
            BtnOK.BackColor = Color.FromArgb(0);
            BtnOK.Click += new EventHandler(btnOK_Click);
            tabs.Panel.Controls.Add(BtnOK);
            x += 12 + BtnOK.Width;

            /////ResetCurrent
            //btnResetCurrent = new Button();
            //btnResetCurrent.AutoSize = true;
            //btnResetCurrent.Text = "Reset Current Page";
            //btnResetCurrent.Location = new Point(tabs.Panel.Width - 12 - btnResetCurrent.Width - x, tabs.Panel.Height - 12 - btnOK.Height);
            //btnResetCurrent.Anchor = (AnchorStyles.Right | AnchorStyles.Bottom);
            //btnResetCurrent.BackColor = Color.FromArgb(0);
            //btnResetCurrent.Click += new EventHandler(btnResetCurrent_Click);
            //tabs.Panel.Controls.Add(btnResetCurrent);
            //x += 12 + btnResetCurrent.Width;

            /////ResetAll
            //btnResetAll = new Button();
            //btnResetAll.AutoSize = true;
            //btnResetAll.Text = "Reset All";
            //btnResetAll.Location = new Point(tabs.Panel.Width - 12 - btnResetAll.Width - x, tabs.Panel.Height - 12 - btnOK.Height);
            //btnResetAll.Anchor = (AnchorStyles.Right | AnchorStyles.Bottom);
            //btnResetAll.BackColor = Color.FromArgb(0);
            //btnResetAll.Click += new EventHandler(btnResetAll_Click);
            //tabs.Panel.Controls.Add(btnResetAll);
            //x += 12 + btnResetAll.Width;
        }

        event EventHandler onSave;

        void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            onSave?.Invoke(this, EventArgs.Empty);
            Close();
        }

        void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        //SHOW
        public void Show(string key)
        {
            base.Show();

            //OriginalSettings = Options.Settings.GetRawData();

            for (int i = 1; i < tabs.Controls.Count; i++)
            {
                if (tabs.Controls[i].Name == key)
                {
                    tabs.SelectedIndex = i - 1;
                }
            }
        }

        //BIND
        Dictionary<Control, KeyValuePair<string, object>> bindings = new Dictionary<Control, KeyValuePair<string, object>>();
        Dictionary<PropertyInfo, object> originalValue = new Dictionary<PropertyInfo, object>();

        private void BindCheckBox(CheckBox c, string name)
        {
            PropertyInfo prop;
            if (AppSettings.Properties.TryGetValue(name, out prop))
            {
                var value = prop.GetValue(null);
                c.Checked = (bool)value;
            }
            else
                throw new ArgumentException($"The settings {name} doesn't exist.");

            onSave += (s, e) => { prop.SetValue(null, c.Checked); };
        }

        private void BindTextBox(TextBox c, string name)
        {
            PropertyInfo prop;
            bool isNumeric;

            if (AppSettings.Properties.TryGetValue(name, out prop))
            {
                isNumeric = prop.PropertyType == typeof(int);

                var value = prop.GetValue(null);
                c.Text = value.ToString();
            }
            else
                throw new ArgumentException($"The settings {name} doesn't exist.");

            onSave += (s, e) =>
            {
                try
                {
                    prop.SetValue(null, int.Parse(c.Text));
                }
                catch { }
            };

            if (isNumeric)
                c.TextChanged += (s, e) =>
                {
                    c.Text = Regex.Replace(c.Text, "[^0-9]+", "");
                };
        }

        private void tabs_PageSelected(object sender, EventArgs e)
        {
            Text = "Preferences - " + tabs.SelectedTab.Text;
        }

        //RESET
        //void btnResetAll_Click(object sender, EventArgs e)
        //{
        //    for (int i = 0; i < tabs.Count; i++)
        //    {
        //        ResetPage(i);
        //    }
        //}

        //void btnResetCurrent_Click(object sender, EventArgs e)
        //{
        //    ResetPage(tabs.SelectedIndex);
        //}

        //private void ResetPage(int index)
        //{
        //    foreach (Control c in tabs[index].Panel.Controls)
        //    {
        //        KeyValuePair<string, object> k;
        //        if (bindings.TryGetValue(c, out k))
        //        {
        //            if (c is CheckBox)
        //                ((CheckBox)c).Checked = (bool)k.Value;
        //        }
        //    }
        //}
    }
}
