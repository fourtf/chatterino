﻿using Chatterino.Common;
using Microsoft.Win32;
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

            TopMost = AppSettings.WindowTopMost;

            AppSettings.WindowTopMostChanged += (s, e) =>
            {
                TopMost = AppSettings.WindowTopMost;
            };

            Icon = App.Icon;

            try
            {
                //this.Icon = Program.AppIcon;
            }
            catch { }

            tabs.SelectedIndex = 0;
            tabs.PageSelected += tabs_PageSelected;
            tabs_PageSelected(this, EventArgs.Empty);

            // Appearance
            string originalTheme = comboTheme.Text = AppSettings.Theme;
            double originalThemeHue = AppSettings.ThemeHue;

            comboTheme.SelectedValueChanged += (s, e) =>
            {
                AppSettings.Theme = comboTheme.Text;
            };

            trackBar1.Value = Math.Max(Math.Min((int)Math.Round(originalThemeHue * 360), 360), 0);

            trackBar1.ValueChanged += (s, e) =>
            {
                AppSettings.ThemeHue = trackBar1.Value / 360.0;
                App.MainForm.Refresh();
            };

            double defaultScrollSpeed = AppSettings.ScrollMultiplyer;

            trackBar2.Value = Math.Min(400, Math.Max(100, (int)(AppSettings.ScrollMultiplyer * 200)));

            onCancel += (s, e) =>
            {
                AppSettings.ScrollMultiplyer = defaultScrollSpeed;
            };

            lblScrollSpeed.Text = (int)(AppSettings.ScrollMultiplyer * 100) + "%";

            trackBar2.ValueChanged += (s, e) =>
            {
                AppSettings.ScrollMultiplyer = (double)trackBar2.Value / 200;

                lblScrollSpeed.Text = (int)(AppSettings.ScrollMultiplyer * 100) + "%";
            };

            onCancel += (s, e) =>
            {
                AppSettings.Theme = originalTheme;
                AppSettings.ThemeHue = originalThemeHue;
                App.MainForm.Refresh();
            };

            btnSelectFont.Click += (s, e) =>
            {
                using (CustomFontDialog.FontDialog dialog = new CustomFontDialog.FontDialog())
                {
                    if (dialog.ShowDialog(this) == DialogResult.OK)
                    {
                        AppSettings.SetFont(dialog.Font.Name, dialog.Font.Size);
                    }
                }

                updateFontName();
            };

            BindCheckBox(chkTimestamps, "ChatShowTimestamps");
            BindCheckBox(chkTimestampSeconds, "ChatShowTimestampSeconds");
            BindCheckBox(chkAllowSameMessages, "ChatAllowSameMessage");
            BindCheckBox(chkDoubleClickLinks, "ChatLinksDoubleClickOnly");
            BindCheckBox(chkHideInput, "ChatHideInputIfEmpty");
            BindCheckBox(chkMessageSeperators, "ChatSeperateMessages");

            chkMessageSeperators.CheckedChanged += (s, e) =>
            {
                App.MainForm.Refresh();
            };

            BindTextBox(txtMsgLimit, "ChatMessageLimit");

            BindCheckBox(chkHighlight, "ChatEnableHighlight");
            BindCheckBox(chkPings, "ChatEnableHighlightSound");
            BindCheckBox(chkFlashTaskbar, "ChatEnableHighlightTaskbar");
            BindCheckBox(chkCustomPingSound, "ChatCustomHighlightSound");

            BindCheckBox(chkInputShowMessageLength, "ChatInputShowMessageLength");

            BindCheckBox(chkMentionUserWithAt, "ChatMentionUsersWithAt");

            BindCheckBox(chkTabLocalizedNames, "ChatTabLocalizedNames");
            BindCheckBox(chkTopMost, "WindowTopMost");


            // Commands
            lock (Commands.CustomCommandsLock)
            {
                foreach (Command c in Commands.CustomCommands)
                {
                    dgvCommands.Rows.Add(c.Raw);
                }
            }
            //ChatAllowCommandsAtEnd
            bool defaultAllowCommandAtEnd = AppSettings.ChatAllowCommandsAtEnd;

            chkAllowCommandAtEnd.Checked = AppSettings.ChatAllowCommandsAtEnd;

            chkAllowCommandAtEnd.CheckedChanged += (s, e) =>
            {
                AppSettings.ChatAllowCommandsAtEnd = chkAllowCommandAtEnd.Checked;
            };

            onCancel += (s, e) =>
            {
                AppSettings.ChatAllowCommandsAtEnd = defaultAllowCommandAtEnd;
            };

            dgvCommands.MultiSelect = false;
            dgvCommands.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvCommands.Columns[0].DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            dgvCommands.KeyDown += (s, e) =>
            {
                if (e.KeyData == (Keys.Control | Keys.Back))
                {
                    e.Handled = true;
                }
            };

            btnCommandAdd.Click += (s, e) =>
            {
                dgvCommands.Rows.Add();
                dgvCommands.Rows[dgvCommands.Rows.Count - 1].Selected = true;
            };

            Action updateCustomCommands = () =>
            {
                lock (Commands.CustomCommandsLock)
                {
                    Commands.CustomCommands.Clear();

                    foreach (DataGridViewRow row in dgvCommands.Rows)
                    {
                        Commands.CustomCommands.Add(new Command((string)row.Cells[0].Value));
                    }
                }
            };

            btnCommandRemove.Click += (s, e) =>
            {
                if (dgvCommands.SelectedCells.Count != 0)
                {
                    dgvCommands.Rows.RemoveAt(dgvCommands.SelectedCells[0].RowIndex);
                }

                updateCustomCommands();
            };

            List<Command> originalCustomCommand = Commands.CustomCommands;

            lock (Commands.CustomCommandsLock)
            {
                Commands.CustomCommands = new List<Command>(Commands.CustomCommands);
            }

            dgvCommands.CellValueChanged += (s, e) =>
            {
                updateCustomCommands();
            };

            onCancel += (s, e) =>
            {
                lock (Commands.CustomCommandsLock)
                {
                    Commands.CustomCommands = originalCustomCommand;
                }
            };

            // Emotes
            BindCheckBox(chkTwitchEmotes, "ChatEnableTwitchEmotes");
            BindCheckBox(chkBttvEmotes, "ChatEnableBttvEmotes");
            BindCheckBox(chkFFzEmotes, "ChatEnableFfzEmotes");
            BindCheckBox(chkEmojis, "ChatEnableEmojis");
            BindCheckBox(chkGifEmotes, "ChatEnableGifAnimations");

            string originalIgnoredEmotes = rtbIngoredEmotes.Text = string.Join(Environment.NewLine, AppSettings.ChatIgnoredEmotes.Keys);

            rtbIngoredEmotes.LostFocus += (s, e) =>
            {
                AppSettings.ChatIgnoredEmotes.Clear();
                var reader = new StringReader(rtbIngoredEmotes.Text);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    AppSettings.ChatIgnoredEmotes[line.Trim()] = null;
                }
            };

            onCancel += (s, e) =>
            {
                AppSettings.ChatIgnoredEmotes.Clear();
                var reader = new StringReader(originalIgnoredEmotes);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    AppSettings.ChatIgnoredEmotes[line.Trim()] = null;
                }
            };

            // Ignored Users
            BindCheckBox(chkTwitchIgnores, "EnableTwitchUserIgnores");

            foreach (string user in IrcManager.IgnoredUsers)
            {
                dgvIgnoredUsers.Rows.Add(user);
            }

            dgvIgnoredUsers.MultiSelect = false;
            dgvIgnoredUsers.ReadOnly = true;
            dgvIgnoredUsers.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvIgnoredUsers.Columns[0].DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            btnIgnoredUserAdd.Click += (s, e) =>
            {
                using (var dialog = new InputDialogForm("Input Username"))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        string message;

                        if (IrcManager.TryAddIgnoredUser(dialog.Value.Trim(), out message))
                        {
                            dgvIgnoredUsers.Rows.Add(dialog.Value.Trim());
                        }
                        else
                        {
                            MessageBox.Show(message, "Error while ignoring user");
                        }
                    }
                }
            };

            btnIgnoredUserRemove.Click += (s, e) =>
            {
                if (dgvIgnoredUsers.SelectedCells.Count != 0)
                {
                    string message;
                    string username = (string)dgvIgnoredUsers.SelectedCells[0].Value;

                    if (IrcManager.TryRemoveIgnoredUser(username, out message))
                    {
                        dgvIgnoredUsers.Rows.RemoveAt(dgvIgnoredUsers.SelectedCells[0].RowIndex);
                    }
                    else
                    {
                        MessageBox.Show(message, "Error while unignoring user");
                    }
                }
            };

            // Ignored Messages
            string ignoreKeywordsOriginal = rtbIgnoreKeywords.Text = string.Join(Environment.NewLine, AppSettings.ChatIgnoredKeywords);

            rtbIgnoreKeywords.LostFocus += (s, e) =>
            {
                List<string> list = new List<string>();
                StringReader reader = new StringReader(rtbIgnoreKeywords.Text);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    list.Add(line.Trim());
                }
                AppSettings.ChatIgnoredKeywords = list.ToArray();
            };

            onCancel += (s, e) =>
            {
                // highlight keywords
                {
                    List<string> list = new List<string>();
                    StringReader reader = new StringReader(ignoreKeywordsOriginal);
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        list.Add(line.Trim());
                    }
                    AppSettings.ChatIgnoredKeywords = list.ToArray();
                }
            };

            // Links

            //RegistryKey browserKeys;
            ////on 64bit the browsers are in a different location
            //browserKeys = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Clients\StartMenuInternet");
            //if (browserKeys == null)
            //    browserKeys = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Clients\StartMenuInternet");
            //string[] browserNames = browserKeys.GetSubKeyNames();

            // Proxy
            BindCheckBox(chkProxyEnabled, "ProxyEnable");
            BindTextBox(textBox1, "ProxyHost");
            BindTextBox(textBox4, "ProxyPort");
            BindTextBox(textBox2, "ProxyUsername");
            BindTextBox(textBox3, "ProxyPassword");

            // Highlights
            string customHighlightsOriginal = rtbHighlights.Text = string.Join(Environment.NewLine, AppSettings.ChatCustomHighlights);
            string highlightIgnoredUsersOriginal = rtbUserBlacklist.Text = string.Join(Environment.NewLine, AppSettings.HighlightIgnoredUsers.Keys);

            rtbHighlights.LostFocus += (s, e) =>
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

            rtbUserBlacklist.LostFocus += (s, e) =>
            {
                AppSettings.HighlightIgnoredUsers.Clear();
                var reader = new StringReader(rtbUserBlacklist.Text);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    AppSettings.HighlightIgnoredUsers[line.Trim().ToLower()] = null;
                }
            };

            onCancel += (s, e) =>
            {
                // highlight keywords
                {
                    List<string> list = new List<string>();
                    StringReader reader = new StringReader(customHighlightsOriginal);
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        list.Add(line.Trim());
                    }
                    AppSettings.ChatCustomHighlights = list.ToArray();
                }

                // user blacklist
                {
                    AppSettings.HighlightIgnoredUsers.Clear();
                    var reader = new StringReader(highlightIgnoredUsersOriginal);
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        AppSettings.HighlightIgnoredUsers[line.Trim().ToLower()] = null;
                    }
                }
            };

            updateFontName();

            btnCustomHighlightOpenFile.Click += (s, e) =>
            {
                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    dialog.Filter = "wave sound file|*.wav";

                    if (dialog.ShowDialog(this) == DialogResult.OK)
                    {
                        try
                        {
                            (GuiEngine.Current as WinformsGuiEngine).HighlightSound?.Dispose();

                            if (!Directory.Exists("./Custom"))
                                Directory.CreateDirectory("./Custom");

                            File.Copy(dialog.FileName, "./Custom/Ping.wav", true);
                        }
                        catch (Exception exc)
                        {
                            MessageBox.Show(exc.Message, "Error copying the highlight sound");
                        }
                    }
                }
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

        //event EventHandler onSave;
        event EventHandler onCancel;

        void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            onCancel?.Invoke(this, EventArgs.Empty);
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
            bool original;

            PropertyInfo prop;
            if (AppSettings.Properties.TryGetValue(name, out prop))
            {
                original = (bool)prop.GetValue(null);
                c.Checked = original;
            }
            else
                throw new ArgumentException($"The settings {name} doesn't exist.");

            c.CheckedChanged += (s, e) =>
            {
                prop.SetValue(null, c.Checked);
            };

            onCancel += (s, e) =>
            {
                prop.SetValue(null, original);
            };
        }

        private void BindTextBox(TextBox c, string name)
        {
            PropertyInfo prop;
            bool isNumeric;

            object original;

            if (AppSettings.Properties.TryGetValue(name, out prop))
            {
                isNumeric = prop.PropertyType == typeof(int);

                original = prop.GetValue(null);
                c.Text = original.ToString(); ;
            }
            else
            {
                throw new ArgumentException($"The settings {name} doesn't exist.");
            }

            c.TextChanged += (s, e) =>
            {
                if (isNumeric)
                {
                    try
                    {
                        prop.SetValue(null, int.Parse(c.Text));
                    }
                    catch { }
                }
                else
                {
                    prop.SetValue(null, c.Text);
                }
            };

            onCancel += (s, e) =>
            {
                prop.SetValue(null, original);
            };

            if (isNumeric)
            {
                c.TextChanged += (s, e) =>
                {
                    c.Text = Regex.Replace(c.Text, "[^0-9]+", "");
                };
            }
        }

        private void tabs_PageSelected(object sender, EventArgs e)
        {
            Text = "Preferences - " + tabs.SelectedTab.Text;
        }

        private void updateFontName()
        {
            lblFont.Text = $"{Fonts.GetFont(FontType.Medium).Name}, {Fonts.GetFont(FontType.Medium).Size}";
        }

        private void btnTextCustomPing_Click(object sender, EventArgs e)
        {
            GuiEngine.Current.PlaySound(NotificationSound.Ping, true);
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
