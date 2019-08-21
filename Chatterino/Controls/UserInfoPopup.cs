using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Chatterino.Common;
using Message = System.Windows.Forms.Message;

namespace Chatterino.Controls
{
    public class UserInfoPopup : Form
    {
        private string username;

        public string Username
        {
            get { return username; }
            set { username = value; }
        }

        void setControlFont(Control control)
        {
            foreach (Control c in control.Controls)
            {
                var label = c as Label;

                if (label != null)
                {
                    label.Font = Fonts.GetFont(Common.FontType.Medium);
                }

                var btn = c as FlatButton;

                if (btn != null)
                {
                    btn.Font = Fonts.GetFont(Common.FontType.Medium);
                }

                setControlFont(c);
            }
        }

        public UserInfoPopup(Common.UserInfoData data)
        {
            InitializeComponent();

            TopMost = Common.AppSettings.WindowTopMost;

            Common.AppSettings.WindowTopMostChanged += (s, e) =>
            {
                TopMost = Common.AppSettings.WindowTopMost;
            };

            lblCreatedAt.Text = "";
            lblViews.Text = "";

            setControlFont(this);

            Task.Run(() =>
            {
                try
                {
                    //var request = WebRequest.Create($"https://api.twitch.tv/kraken/channels/{data.UserName}?client_id={Common.IrcManager.DefaultClientID}");
                    var request = WebRequest.Create($"https://api.twitch.tv/helix/users?login={data.UserName}")
                        .AuthorizeHelix();
                    if (AppSettings.IgnoreSystemProxy)
                    {
                        request.Proxy = null;
                    }

                    using (var response = request.GetResponse())
                    using (var stream = response.GetResponseStream())
                    {
                        var parser = new JsonParser();

                        dynamic json = parser.Parse(stream);
                        dynamic user = json["data"]?[0];

                        string logo = user["profile_image_url"];
                        //string createdAt = user["created_at"];
                        //string followerCount = user["followers"];
                        string viewCount = user["view_count"];

                        lblViews.Invoke(() => lblViews.Text = $"Channel Views: {viewCount}");

                        DateTime createAtTime;

                        //if (DateTime.TryParse(createdAt, out createAtTime))
                        //{
                        //    lblCreatedAt.Invoke(() => lblCreatedAt.Text = $"Created at: {createAtTime.ToString()}");
                        //}

                        Task.Run(() =>
                        {
                            try
                            {
                                var req = WebRequest.Create(logo);
                                if (AppSettings.IgnoreSystemProxy)
                                {
                                    request.Proxy = null;
                                }

                                using (var res = req.GetResponse())
                                using (var s = res.GetResponseStream())
                                {
                                    var image = Image.FromStream(s);

                                    picAvatar.Invoke(() => picAvatar.Image = image);
                                }
                            }
                            catch { }
                        });
                    }
                }
                catch { }
            });

            string displayName;

            if (!data.Channel.Users.TryGetValue(data.UserName, out displayName))
            {
                displayName = data.UserName;
            }

            lblUsername.Text = data.UserName;

            btnCopyUsername.Font = Fonts.GdiSmall;
            btnCopyUsername.Click += (s, e) =>
            {
                try
                {
                    Clipboard.SetText(displayName);
                }
                catch { }
            };

            btnCopyUsername.SetTooltip("Copy Username");
            btnBan.SetTooltip("Ban User");
            btnFollow.SetTooltip("Follow User");
            btnIgnore.SetTooltip("Ignore User");
            btnMessage.SetTooltip("Send Private Message");
            btnProfile.SetTooltip("Show Profile");
            btnUnban.SetTooltip("Unban User");
            btnWhisper.SetTooltip("Whisper User");
            btnPurge.SetTooltip("Timeout User for 1 Second");

            btnTimeout2Hours.SetTooltip("Timeout User for 2 Hours");
            btnTimeout30Mins.SetTooltip("Timeout User for 30 Minutes");
            btnTimeout5Min.SetTooltip("Timeout User for 5 Minutes");
            btnTimeout1Day.SetTooltip("Timeout User for 1 Day");
            btnTimeout3Days.SetTooltip("Timeout User for 3 Days");
            btnTimeout7Days.SetTooltip("Timeout User for 7 Days");
            btnTimeout1Month.SetTooltip("Timeout User for 1 Month");

            btnPurge.Click += (s, e) => data.Channel.SendMessage($"/timeout {data.UserName} 1");
            btnTimeout5Min.Click += (s, e) => data.Channel.SendMessage($"/timeout {data.UserName} 300");
            btnTimeout30Mins.Click += (s, e) => data.Channel.SendMessage($"/timeout {data.UserName} 1800");
            btnTimeout2Hours.Click += (s, e) => data.Channel.SendMessage($"/timeout {data.UserName} 7200");
            btnTimeout1Day.Click += (s, e) => data.Channel.SendMessage($"/timeout {data.UserName} 86400");
            btnTimeout3Days.Click += (s, e) => data.Channel.SendMessage($"/timeout {data.UserName} 259200");
            btnTimeout7Days.Click += (s, e) => data.Channel.SendMessage($"/timeout {data.UserName} 604800");
            btnTimeout1Month.Click += (s, e) => data.Channel.SendMessage($"/timeout {data.UserName} 2592000");

            // show profile
            btnProfile.Click += (s, e) =>
            {
                Common.GuiEngine.Current.HandleLink(new Common.Link(Common.LinkType.Url, "https://www.twitch.tv/" + data.UserName));
            };

            if (Common.IrcManager.Account.IsAnon || string.Equals(data.UserName, Common.IrcManager.Account.Username, StringComparison.OrdinalIgnoreCase))
            {
                btnBan.Visible = false;
                btnUnban.Visible = false;
                btnMessage.Visible = false;
                btnWhisper.Visible = false;
                btnIgnore.Visible = false;
                btnFollow.Visible = false;
                btnPurge.Visible = false;

                btnMod.Visible = false;
                btnUnmod.Visible = false;
                btnIgnoreHighlights.Visible = false;

                btnTimeout1Day.Visible = false;
                btnTimeout1Month.Visible = false;
                btnTimeout2Hours.Visible = false;
                btnTimeout30Mins.Visible = false;
                btnTimeout3Days.Visible = false;
                btnTimeout5Min.Visible = false;
                btnTimeout7Days.Visible = false;
                btnPurge.Visible = false;
            }
            else
            {
                if (data.Channel.IsModOrBroadcaster && !string.Equals(data.UserName, data.Channel.Name, StringComparison.OrdinalIgnoreCase))
                {

                }
                else
                {
                    btnBan.Visible = false;
                    btnUnban.Visible = false;

                    btnTimeout1Day.Visible = false;
                    btnTimeout1Month.Visible = false;
                    btnTimeout2Hours.Visible = false;
                    btnTimeout30Mins.Visible = false;
                    btnTimeout3Days.Visible = false;
                    btnTimeout5Min.Visible = false;
                    btnTimeout7Days.Visible = false;
                    btnPurge.Visible = false;
                }

                if (data.Channel.IsBroadcaster && !string.Equals(data.UserName, data.Channel.Name, StringComparison.OrdinalIgnoreCase))
                {
                    btnMod.Click += (s, e) =>
                    {
                        data.Channel.SendMessage("/mod " + data.UserName);
                    };
                    btnUnmod.Click += (s, e) =>
                    {
                        data.Channel.SendMessage("/unmod " + data.UserName);
                    };
                }
                else
                {
                    btnMod.Visible = false;
                    btnUnmod.Visible = false;
                }

                // ban
                btnBan.Click += (s, e) =>
                {
                    data.Channel.SendMessage("/ban " + data.UserName);
                };

                btnUnban.Click += (s, e) =>
                {
                    data.Channel.SendMessage("/unban " + data.UserName);
                };

                // purge user
                btnPurge.Click += (s, e) =>
                {
                    data.Channel.SendMessage("/timeout " + data.UserName + " 1");
                };

                // ignore user
                btnIgnore.Text = Common.IrcManager.IsIgnoredUser(data.UserName) ? "Unignore" : "Ignore";

                btnIgnore.Click += (s, e) =>
                {
                    if (Common.IrcManager.IsIgnoredUser(data.UserName))
                    {
                        string message;

                        if (!Common.IrcManager.TryRemoveIgnoredUser(data.UserName, out message))
                        {
                            MessageBox.Show(message, "Error while ignoring user.");
                        }
                    }
                    else
                    {
                        string message;

                        if (!Common.IrcManager.TryAddIgnoredUser(data.UserName, out message))
                        {
                            MessageBox.Show(message, "Error while unignoring user.");
                        }
                    }

                    btnIgnore.Text = Common.IrcManager.IsIgnoredUser(data.UserName) ? "Unignore" : "Ignore";
                };

                // message user
                btnMessage.Click += (s, e) =>
                {
                    Common.GuiEngine.Current.HandleLink(new Common.Link(Common.LinkType.Url, "https://www.twitch.tv/message/compose?to=" + data.UserName));
                };

                // highlight ignore
                btnIgnoreHighlights.Click += (s, e) =>
                {
                    if (AppSettings.HighlightIgnoredUsers.ContainsKey(data.UserName))
                    {
                        object tmp;

                        AppSettings.HighlightIgnoredUsers.TryRemove(data.UserName, out tmp);

                        btnIgnoreHighlights.Text = "Disable Highlights";
                    }
                    else
                    {
                        AppSettings.HighlightIgnoredUsers[data.UserName] = null;

                        btnIgnoreHighlights.Text = "Enable Highlights";
                    }
                };

                btnIgnoreHighlights.Text = AppSettings.HighlightIgnoredUsers.ContainsKey(data.UserName) ? "Enable Highlights" : "Disable Highlights";

                // follow user
                var isFollowing = false;

                this.btnFollow.Visible = false;
                //Task.Run(() =>
                //{
                //    bool result;
                //    string message;

                //    Common.IrcManager.TryCheckIfFollowing(data.UserName, out result, out message);

                //    isFollowing = result;

                //    btnFollow.Invoke(() => btnFollow.Text = isFollowing ? "Unfollow" : "Follow");
                //});

                btnFollow.Click += (s, e) =>
                {
                    if (isFollowing)
                    {
                        string message;

                        if (Common.IrcManager.TryUnfollowUser(data.UserName, out message))
                        {
                            isFollowing = false;

                            btnFollow.Text = "Follow";
                        }
                        else
                        {
                            MessageBox.Show(message, "Error while unfollowing user.");
                        }
                    }
                    else
                    {
                        string message;

                        if (Common.IrcManager.TryUnfollowUser(data.UserName, out message))
                        {
                            isFollowing = true;

                            btnFollow.Text = "Unfollow";
                        }
                        else
                        {
                            MessageBox.Show(message, "Error while following user.");
                        }
                    }
                };
            }
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);

            Close();
        }

        private void InitializeComponent()
        {
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.picAvatar = new System.Windows.Forms.PictureBox();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.lblUsername = new System.Windows.Forms.Label();
            this.lblViews = new System.Windows.Forms.Label();
            this.lblCreatedAt = new System.Windows.Forms.Label();
            this.btnMod = new Chatterino.Controls.FlatButton();
            this.btnUnmod = new Chatterino.Controls.FlatButton();
            this.btnBan = new Chatterino.Controls.FlatButton();
            this.btnUnban = new Chatterino.Controls.FlatButton();
            this.btnPurge = new Chatterino.Controls.FlatButton();
            this.btnTimeout5Min = new Chatterino.Controls.FlatButton();
            this.btnTimeout30Mins = new Chatterino.Controls.FlatButton();
            this.btnTimeout2Hours = new Chatterino.Controls.FlatButton();
            this.btnTimeout1Day = new Chatterino.Controls.FlatButton();
            this.btnTimeout3Days = new Chatterino.Controls.FlatButton();
            this.btnTimeout7Days = new Chatterino.Controls.FlatButton();
            this.btnTimeout1Month = new Chatterino.Controls.FlatButton();
            this.btnCopyUsername = new Chatterino.Controls.FlatButton();
            this.btnProfile = new Chatterino.Controls.FlatButton();
            this.btnFollow = new Chatterino.Controls.FlatButton();
            this.btnIgnore = new Chatterino.Controls.FlatButton();
            this.btnIgnoreHighlights = new Chatterino.Controls.FlatButton();
            this.btnWhisper = new Chatterino.Controls.FlatButton();
            this.btnMessage = new Chatterino.Controls.FlatButton();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picAvatar)).BeginInit();
            this.flowLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.picAvatar);
            this.flowLayoutPanel1.Controls.Add(this.flowLayoutPanel2);
            this.flowLayoutPanel1.Controls.Add(this.btnMod);
            this.flowLayoutPanel1.Controls.Add(this.btnUnmod);
            this.flowLayoutPanel1.Controls.Add(this.btnBan);
            this.flowLayoutPanel1.Controls.Add(this.btnUnban);
            this.flowLayoutPanel1.Controls.Add(this.btnPurge);
            this.flowLayoutPanel1.Controls.Add(this.btnTimeout5Min);
            this.flowLayoutPanel1.Controls.Add(this.btnTimeout30Mins);
            this.flowLayoutPanel1.Controls.Add(this.btnTimeout2Hours);
            this.flowLayoutPanel1.Controls.Add(this.btnTimeout1Day);
            this.flowLayoutPanel1.Controls.Add(this.btnTimeout3Days);
            this.flowLayoutPanel1.Controls.Add(this.btnTimeout7Days);
            this.flowLayoutPanel1.Controls.Add(this.btnTimeout1Month);
            this.flowLayoutPanel1.Controls.Add(this.btnCopyUsername);
            this.flowLayoutPanel1.Controls.Add(this.btnProfile);
            this.flowLayoutPanel1.Controls.Add(this.btnFollow);
            this.flowLayoutPanel1.Controls.Add(this.btnIgnore);
            this.flowLayoutPanel1.Controls.Add(this.btnIgnoreHighlights);
            this.flowLayoutPanel1.Controls.Add(this.btnWhisper);
            this.flowLayoutPanel1.Controls.Add(this.btnMessage);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Padding = new System.Windows.Forms.Padding(8);
            this.flowLayoutPanel1.Size = new System.Drawing.Size(284, 261);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // picAvatar
            // 
            this.picAvatar.Location = new System.Drawing.Point(11, 11);
            this.picAvatar.Name = "picAvatar";
            this.picAvatar.Size = new System.Drawing.Size(64, 64);
            this.picAvatar.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picAvatar.TabIndex = 11;
            this.picAvatar.TabStop = false;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.AutoSize = true;
            this.flowLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel2.Controls.Add(this.lblUsername);
            this.flowLayoutPanel2.Controls.Add(this.lblViews);
            this.flowLayoutPanel2.Controls.Add(this.lblCreatedAt);
            this.flowLayoutPanel1.SetFlowBreak(this.flowLayoutPanel2, true);
            this.flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(81, 11);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(61, 39);
            this.flowLayoutPanel2.TabIndex = 12;
            // 
            // lblUsername
            // 
            this.lblUsername.AutoSize = true;
            this.lblUsername.Location = new System.Drawing.Point(3, 0);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.Size = new System.Drawing.Size(35, 13);
            this.lblUsername.TabIndex = 0;
            this.lblUsername.Text = "label1";
            // 
            // lblViews
            // 
            this.lblViews.AutoSize = true;
            this.lblViews.Location = new System.Drawing.Point(3, 13);
            this.lblViews.Name = "lblViews";
            this.lblViews.Size = new System.Drawing.Size(34, 13);
            this.lblViews.TabIndex = 1;
            this.lblViews.Text = "views";
            // 
            // lblCreatedAt
            // 
            this.lblCreatedAt.AutoSize = true;
            this.lblCreatedAt.Location = new System.Drawing.Point(3, 26);
            this.lblCreatedAt.Name = "lblCreatedAt";
            this.lblCreatedAt.Size = new System.Drawing.Size(55, 13);
            this.lblCreatedAt.TabIndex = 2;
            this.lblCreatedAt.Text = "created at";
            // 
            // btnMod
            // 
            this.btnMod.Image = null;
            this.btnMod.Location = new System.Drawing.Point(11, 81);
            this.btnMod.Name = "btnMod";
            this.btnMod.Size = new System.Drawing.Size(35, 18);
            this.btnMod.TabIndex = 13;
            this.btnMod.Text = "Mod";
            // 
            // btnUnmod
            // 
            this.btnUnmod.Image = null;
            this.btnUnmod.Location = new System.Drawing.Point(52, 81);
            this.btnUnmod.Name = "btnUnmod";
            this.btnUnmod.Size = new System.Drawing.Size(48, 18);
            this.btnUnmod.TabIndex = 14;
            this.btnUnmod.Text = "Unmod";
            // 
            // btnBan
            // 
            this.btnBan.Image = null;
            this.btnBan.Location = new System.Drawing.Point(106, 81);
            this.btnBan.Name = "btnBan";
            this.btnBan.Size = new System.Drawing.Size(33, 18);
            this.btnBan.TabIndex = 4;
            this.btnBan.Text = "Ban";
            // 
            // btnUnban
            // 
            this.flowLayoutPanel1.SetFlowBreak(this.btnUnban, true);
            this.btnUnban.Image = null;
            this.btnUnban.Location = new System.Drawing.Point(145, 81);
            this.btnUnban.Name = "btnUnban";
            this.btnUnban.Size = new System.Drawing.Size(46, 18);
            this.btnUnban.TabIndex = 5;
            this.btnUnban.Text = "Unban";
            // 
            // btnPurge
            // 
            this.btnPurge.Image = null;
            this.btnPurge.Location = new System.Drawing.Point(11, 105);
            this.btnPurge.Name = "btnPurge";
            this.btnPurge.Size = new System.Drawing.Size(42, 18);
            this.btnPurge.TabIndex = 10;
            this.btnPurge.Text = "Purge";
            // 
            // btnTimeout5Min
            // 
            this.btnTimeout5Min.Image = null;
            this.btnTimeout5Min.Location = new System.Drawing.Point(59, 105);
            this.btnTimeout5Min.Name = "btnTimeout5Min";
            this.btnTimeout5Min.Size = new System.Drawing.Size(39, 18);
            this.btnTimeout5Min.TabIndex = 3;
            this.btnTimeout5Min.Text = "5 min";
            // 
            // btnTimeout30Mins
            // 
            this.btnTimeout30Mins.Image = null;
            this.btnTimeout30Mins.Location = new System.Drawing.Point(104, 105);
            this.btnTimeout30Mins.Name = "btnTimeout30Mins";
            this.btnTimeout30Mins.Size = new System.Drawing.Size(45, 18);
            this.btnTimeout30Mins.TabIndex = 15;
            this.btnTimeout30Mins.Text = "30 min";
            // 
            // btnTimeout2Hours
            // 
            this.btnTimeout2Hours.Image = null;
            this.btnTimeout2Hours.Location = new System.Drawing.Point(155, 105);
            this.btnTimeout2Hours.Name = "btnTimeout2Hours";
            this.btnTimeout2Hours.Size = new System.Drawing.Size(44, 18);
            this.btnTimeout2Hours.TabIndex = 20;
            this.btnTimeout2Hours.Text = "2 hour";
            // 
            // btnTimeout1Day
            // 
            this.btnTimeout1Day.Image = null;
            this.btnTimeout1Day.Location = new System.Drawing.Point(205, 105);
            this.btnTimeout1Day.Name = "btnTimeout1Day";
            this.btnTimeout1Day.Size = new System.Drawing.Size(40, 18);
            this.btnTimeout1Day.TabIndex = 16;
            this.btnTimeout1Day.Text = "1 day";
            // 
            // btnTimeout3Days
            // 
            this.btnTimeout3Days.Image = null;
            this.btnTimeout3Days.Location = new System.Drawing.Point(11, 129);
            this.btnTimeout3Days.Name = "btnTimeout3Days";
            this.btnTimeout3Days.Size = new System.Drawing.Size(40, 18);
            this.btnTimeout3Days.TabIndex = 17;
            this.btnTimeout3Days.Text = "3 day";
            // 
            // btnTimeout7Days
            // 
            this.btnTimeout7Days.Image = null;
            this.btnTimeout7Days.Location = new System.Drawing.Point(57, 129);
            this.btnTimeout7Days.Name = "btnTimeout7Days";
            this.btnTimeout7Days.Size = new System.Drawing.Size(40, 18);
            this.btnTimeout7Days.TabIndex = 18;
            this.btnTimeout7Days.Text = "7 day";
            // 
            // btnTimeout1Month
            // 
            this.flowLayoutPanel1.SetFlowBreak(this.btnTimeout1Month, true);
            this.btnTimeout1Month.Image = null;
            this.btnTimeout1Month.Location = new System.Drawing.Point(103, 129);
            this.btnTimeout1Month.Name = "btnTimeout1Month";
            this.btnTimeout1Month.Size = new System.Drawing.Size(52, 18);
            this.btnTimeout1Month.TabIndex = 19;
            this.btnTimeout1Month.Text = "1 month";
            // 
            // btnCopyUsername
            // 
            this.btnCopyUsername.Image = global::Chatterino.Properties.Resources.CopyLongTextToClipboard_16x;
            this.btnCopyUsername.Location = new System.Drawing.Point(11, 153);
            this.btnCopyUsername.Name = "btnCopyUsername";
            this.btnCopyUsername.Size = new System.Drawing.Size(24, 23);
            this.btnCopyUsername.TabIndex = 1;
            // 
            // btnProfile
            // 
            this.btnProfile.Image = null;
            this.btnProfile.Location = new System.Drawing.Point(41, 153);
            this.btnProfile.Name = "btnProfile";
            this.btnProfile.Size = new System.Drawing.Size(43, 18);
            this.btnProfile.TabIndex = 8;
            this.btnProfile.Text = "Profile";
            // 
            // btnFollow
            // 
            this.btnFollow.Image = null;
            this.btnFollow.Location = new System.Drawing.Point(90, 153);
            this.btnFollow.Name = "btnFollow";
            this.btnFollow.Size = new System.Drawing.Size(44, 18);
            this.btnFollow.TabIndex = 9;
            this.btnFollow.Text = "Follow";
            // 
            // btnIgnore
            // 
            this.btnIgnore.Image = null;
            this.btnIgnore.Location = new System.Drawing.Point(140, 153);
            this.btnIgnore.Name = "btnIgnore";
            this.btnIgnore.Size = new System.Drawing.Size(44, 18);
            this.btnIgnore.TabIndex = 2;
            this.btnIgnore.Text = "Ignore";
            // 
            // btnIgnoreHighlights
            // 
            this.btnIgnoreHighlights.Image = null;
            this.btnIgnoreHighlights.Location = new System.Drawing.Point(11, 182);
            this.btnIgnoreHighlights.Name = "btnIgnoreHighlights";
            this.btnIgnoreHighlights.Size = new System.Drawing.Size(98, 18);
            this.btnIgnoreHighlights.TabIndex = 21;
            this.btnIgnoreHighlights.Text = "Disable Highlights";
            // 
            // btnWhisper
            // 
            this.btnWhisper.Image = null;
            this.btnWhisper.Location = new System.Drawing.Point(115, 182);
            this.btnWhisper.Name = "btnWhisper";
            this.btnWhisper.Size = new System.Drawing.Size(53, 18);
            this.btnWhisper.TabIndex = 6;
            this.btnWhisper.Text = "Whisper";
            this.btnWhisper.Visible = false;
            // 
            // btnMessage
            // 
            this.btnMessage.Image = null;
            this.btnMessage.Location = new System.Drawing.Point(174, 182);
            this.btnMessage.Name = "btnMessage";
            this.btnMessage.Size = new System.Drawing.Size(57, 18);
            this.btnMessage.TabIndex = 7;
            this.btnMessage.Text = "Message";
            // 
            // UserInfoPopup
            // 
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.flowLayoutPanel1);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "UserInfoPopup";
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picAvatar)).EndInit();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_NCHITTEST)
                m.Result = (IntPtr)(HT_CAPTION);
        }

        private const int WM_NCHITTEST = 0x84;
        private const int HT_CLIENT = 0x1;

        private FlowLayoutPanel flowLayoutPanel1;
        private Label lblUsername;
        private FlatButton btnCopyUsername;
        private FlatButton btnIgnore;
        private FlatButton btnTimeout5Min;
        private FlatButton btnBan;
        private FlatButton btnUnban;
        private FlatButton btnWhisper;
        private FlatButton btnMessage;
        private FlatButton btnProfile;
        private FlatButton btnFollow;
        private FlatButton btnPurge;
        private PictureBox picAvatar;
        private FlowLayoutPanel flowLayoutPanel2;
        private Label lblViews;
        private Label lblCreatedAt;
        private FlatButton btnMod;
        private FlatButton btnUnmod;
        private FlatButton btnTimeout30Mins;
        private FlatButton btnTimeout1Day;
        private FlatButton btnTimeout3Days;
        private FlatButton btnTimeout7Days;
        private FlatButton btnTimeout1Month;
        private FlatButton btnTimeout2Hours;
        private FlatButton btnIgnoreHighlights;
        private const int HT_CAPTION = 0x2;
    }
}
