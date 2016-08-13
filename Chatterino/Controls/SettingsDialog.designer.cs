namespace Chatterino.Controls
{
    partial class SettingsDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabs = new Chatterino.Controls.SettingsTabControl();
            this.RightPanel = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnCustomHighlightOpenFile = new System.Windows.Forms.Button();
            this.chkCustomPingSound = new System.Windows.Forms.CheckBox();
            this.rtbHighlights = new System.Windows.Forms.RichTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkFlashTaskbar = new System.Windows.Forms.CheckBox();
            this.chkHighlight = new System.Windows.Forms.CheckBox();
            this.chkPings = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblFont = new System.Windows.Forms.Label();
            this.btnSelectFont = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.chkHideInput = new System.Windows.Forms.CheckBox();
            this.txtMsgLimit = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.chkDoubleClickLinks = new System.Windows.Forms.CheckBox();
            this.chkEmojis = new System.Windows.Forms.CheckBox();
            this.chkAllowSameMessages = new System.Windows.Forms.CheckBox();
            this.chkGifEmotes = new System.Windows.Forms.CheckBox();
            this.chkBttvEmotes = new System.Windows.Forms.CheckBox();
            this.chkFFzEmotes = new System.Windows.Forms.CheckBox();
            this.lblEmotes = new System.Windows.Forms.Label();
            this.lblTabs = new System.Windows.Forms.Label();
            this.chkTimestampSeconds = new System.Windows.Forms.CheckBox();
            this.chkTimestamps = new System.Windows.Forms.CheckBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.lblProxyPassword = new System.Windows.Forms.Label();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.lblProxyUsername = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.lblProxyType = new System.Windows.Forms.Label();
            this.lblProxy = new System.Windows.Forms.Label();
            this.chkProxyEnabled = new System.Windows.Forms.CheckBox();
            this.spAppearance = new Chatterino.Controls.SettingsTabPage();
            this.spConnection = new Chatterino.Controls.SettingsTabPage();
            this.spHighlighted = new Chatterino.Controls.SettingsTabPage();
            this.btnTextCustomPing = new System.Windows.Forms.Button();
            this.tabs.SuspendLayout();
            this.RightPanel.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabs
            // 
            this.tabs.AllowDrop = true;
            this.tabs.Controls.Add(this.RightPanel);
            this.tabs.Controls.Add(this.spAppearance);
            this.tabs.Controls.Add(this.spConnection);
            this.tabs.Controls.Add(this.spHighlighted);
            this.tabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabs.Location = new System.Drawing.Point(0, 0);
            this.tabs.Name = "tabs";
            this.tabs.Panel = this.RightPanel;
            this.tabs.SelectedIndex = 2;
            this.tabs.SelectedTab = this.spHighlighted;
            this.tabs.Size = new System.Drawing.Size(598, 448);
            this.tabs.TabIndex = 0;
            this.tabs.TabsWidth = 150;
            this.tabs.Text = "tabs";
            // 
            // RightPanel
            // 
            this.RightPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RightPanel.Controls.Add(this.panel3);
            this.RightPanel.Controls.Add(this.panel1);
            this.RightPanel.Controls.Add(this.panel2);
            this.RightPanel.Location = new System.Drawing.Point(150, 0);
            this.RightPanel.Name = "RightPanel";
            this.RightPanel.Padding = new System.Windows.Forms.Padding(0, 0, 0, 42);
            this.RightPanel.Size = new System.Drawing.Size(448, 448);
            this.RightPanel.TabIndex = 0;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.btnTextCustomPing);
            this.panel3.Controls.Add(this.btnCustomHighlightOpenFile);
            this.panel3.Controls.Add(this.chkCustomPingSound);
            this.panel3.Controls.Add(this.rtbHighlights);
            this.panel3.Controls.Add(this.label2);
            this.panel3.Controls.Add(this.chkFlashTaskbar);
            this.panel3.Controls.Add(this.chkHighlight);
            this.panel3.Controls.Add(this.chkPings);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(448, 406);
            this.panel3.TabIndex = 1;
            // 
            // btnCustomHighlightOpenFile
            // 
            this.btnCustomHighlightOpenFile.Image = global::Chatterino.Properties.Resources.OpenFolder_16x;
            this.btnCustomHighlightOpenFile.Location = new System.Drawing.Point(161, 78);
            this.btnCustomHighlightOpenFile.Name = "btnCustomHighlightOpenFile";
            this.btnCustomHighlightOpenFile.Size = new System.Drawing.Size(24, 23);
            this.btnCustomHighlightOpenFile.TabIndex = 19;
            this.btnCustomHighlightOpenFile.UseVisualStyleBackColor = true;
            // 
            // chkCustomPingSound
            // 
            this.chkCustomPingSound.AutoSize = true;
            this.chkCustomPingSound.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.chkCustomPingSound.Location = new System.Drawing.Point(21, 82);
            this.chkCustomPingSound.Name = "chkCustomPingSound";
            this.chkCustomPingSound.Size = new System.Drawing.Size(135, 17);
            this.chkCustomPingSound.TabIndex = 18;
            this.chkCustomPingSound.Text = "Custom highlight sound";
            this.chkCustomPingSound.UseVisualStyleBackColor = true;
            // 
            // rtbHighlights
            // 
            this.rtbHighlights.Location = new System.Drawing.Point(21, 131);
            this.rtbHighlights.Name = "rtbHighlights";
            this.rtbHighlights.Size = new System.Drawing.Size(404, 263);
            this.rtbHighlights.TabIndex = 17;
            this.rtbHighlights.Text = "";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label2.Location = new System.Drawing.Point(18, 108);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(218, 17);
            this.label2.TabIndex = 16;
            this.label2.Text = "Highlighted Keywords (one per line)";
            // 
            // chkFlashTaskbar
            // 
            this.chkFlashTaskbar.AutoSize = true;
            this.chkFlashTaskbar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.chkFlashTaskbar.Location = new System.Drawing.Point(21, 59);
            this.chkFlashTaskbar.Name = "chkFlashTaskbar";
            this.chkFlashTaskbar.Size = new System.Drawing.Size(238, 17);
            this.chkFlashTaskbar.TabIndex = 15;
            this.chkFlashTaskbar.Text = "Flash Taskbar when your Name is mentioned";
            this.chkFlashTaskbar.UseVisualStyleBackColor = true;
            // 
            // chkHighlight
            // 
            this.chkHighlight.AutoSize = true;
            this.chkHighlight.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.chkHighlight.Location = new System.Drawing.Point(21, 13);
            this.chkHighlight.Name = "chkHighlight";
            this.chkHighlight.Size = new System.Drawing.Size(224, 17);
            this.chkHighlight.TabIndex = 13;
            this.chkHighlight.Text = "Highlight Messages containing your Name";
            this.chkHighlight.UseVisualStyleBackColor = true;
            // 
            // chkPings
            // 
            this.chkPings.AutoSize = true;
            this.chkPings.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.chkPings.Location = new System.Drawing.Point(21, 36);
            this.chkPings.Name = "chkPings";
            this.chkPings.Size = new System.Drawing.Size(225, 17);
            this.chkPings.TabIndex = 14;
            this.chkPings.Text = "Play Sound when your Name is mentioned";
            this.chkPings.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lblFont);
            this.panel1.Controls.Add(this.btnSelectFont);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.chkHideInput);
            this.panel1.Controls.Add(this.txtMsgLimit);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.chkDoubleClickLinks);
            this.panel1.Controls.Add(this.chkEmojis);
            this.panel1.Controls.Add(this.chkAllowSameMessages);
            this.panel1.Controls.Add(this.chkGifEmotes);
            this.panel1.Controls.Add(this.chkBttvEmotes);
            this.panel1.Controls.Add(this.chkFFzEmotes);
            this.panel1.Controls.Add(this.lblEmotes);
            this.panel1.Controls.Add(this.lblTabs);
            this.panel1.Controls.Add(this.chkTimestampSeconds);
            this.panel1.Controls.Add(this.chkTimestamps);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(448, 406);
            this.panel1.TabIndex = 0;
            // 
            // lblFont
            // 
            this.lblFont.AutoSize = true;
            this.lblFont.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.lblFont.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.lblFont.Location = new System.Drawing.Point(190, 32);
            this.lblFont.Name = "lblFont";
            this.lblFont.Size = new System.Drawing.Size(54, 13);
            this.lblFont.TabIndex = 15;
            this.lblFont.Text = "font name";
            // 
            // btnSelectFont
            // 
            this.btnSelectFont.Location = new System.Drawing.Point(101, 27);
            this.btnSelectFont.Name = "btnSelectFont";
            this.btnSelectFont.Size = new System.Drawing.Size(75, 23);
            this.btnSelectFont.TabIndex = 14;
            this.btnSelectFont.Text = "Select";
            this.btnSelectFont.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.label5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label5.Location = new System.Drawing.Point(18, 32);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(31, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Font:";
            // 
            // chkHideInput
            // 
            this.chkHideInput.AutoSize = true;
            this.chkHideInput.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.chkHideInput.Location = new System.Drawing.Point(21, 174);
            this.chkHideInput.Name = "chkHideInput";
            this.chkHideInput.Size = new System.Drawing.Size(134, 17);
            this.chkHideInput.TabIndex = 12;
            this.chkHideInput.Text = "Hide input box if Empty";
            this.chkHideInput.UseVisualStyleBackColor = true;
            // 
            // txtMsgLimit
            // 
            this.txtMsgLimit.Location = new System.Drawing.Point(101, 54);
            this.txtMsgLimit.Name = "txtMsgLimit";
            this.txtMsgLimit.Size = new System.Drawing.Size(167, 20);
            this.txtMsgLimit.TabIndex = 10;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label3.Location = new System.Drawing.Point(18, 58);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Message Limit:";
            // 
            // chkDoubleClickLinks
            // 
            this.chkDoubleClickLinks.AutoSize = true;
            this.chkDoubleClickLinks.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.chkDoubleClickLinks.Location = new System.Drawing.Point(21, 151);
            this.chkDoubleClickLinks.Name = "chkDoubleClickLinks";
            this.chkDoubleClickLinks.Size = new System.Drawing.Size(180, 17);
            this.chkDoubleClickLinks.TabIndex = 10;
            this.chkDoubleClickLinks.Text = "Only open Links on Double Click";
            this.chkDoubleClickLinks.UseVisualStyleBackColor = true;
            // 
            // chkEmojis
            // 
            this.chkEmojis.AutoSize = true;
            this.chkEmojis.Checked = true;
            this.chkEmojis.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkEmojis.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.chkEmojis.Location = new System.Drawing.Point(21, 309);
            this.chkEmojis.Name = "chkEmojis";
            this.chkEmojis.Size = new System.Drawing.Size(92, 17);
            this.chkEmojis.TabIndex = 9;
            this.chkEmojis.Text = "Enable Emojis";
            this.chkEmojis.UseVisualStyleBackColor = true;
            // 
            // chkAllowSameMessages
            // 
            this.chkAllowSameMessages.AutoSize = true;
            this.chkAllowSameMessages.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.chkAllowSameMessages.Location = new System.Drawing.Point(21, 128);
            this.chkAllowSameMessages.Name = "chkAllowSameMessages";
            this.chkAllowSameMessages.Size = new System.Drawing.Size(309, 17);
            this.chkAllowSameMessages.TabIndex = 8;
            this.chkAllowSameMessages.Text = "Allow sending Duplicate Messages (add a space at the end)";
            this.chkAllowSameMessages.UseVisualStyleBackColor = true;
            // 
            // chkGifEmotes
            // 
            this.chkGifEmotes.AutoSize = true;
            this.chkGifEmotes.Checked = true;
            this.chkGifEmotes.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGifEmotes.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.chkGifEmotes.Location = new System.Drawing.Point(21, 286);
            this.chkGifEmotes.Name = "chkGifEmotes";
            this.chkGifEmotes.Size = new System.Drawing.Size(133, 17);
            this.chkGifEmotes.TabIndex = 7;
            this.chkGifEmotes.Text = "Enable GIF Animations";
            this.chkGifEmotes.UseVisualStyleBackColor = true;
            // 
            // chkBttvEmotes
            // 
            this.chkBttvEmotes.AutoSize = true;
            this.chkBttvEmotes.Checked = true;
            this.chkBttvEmotes.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBttvEmotes.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.chkBttvEmotes.Location = new System.Drawing.Point(21, 240);
            this.chkBttvEmotes.Name = "chkBttvEmotes";
            this.chkBttvEmotes.Size = new System.Drawing.Size(149, 17);
            this.chkBttvEmotes.TabIndex = 6;
            this.chkBttvEmotes.Text = "Enable BetterTTV Emotes";
            this.chkBttvEmotes.UseVisualStyleBackColor = true;
            // 
            // chkFFzEmotes
            // 
            this.chkFFzEmotes.AutoSize = true;
            this.chkFFzEmotes.Checked = true;
            this.chkFFzEmotes.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkFFzEmotes.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.chkFFzEmotes.Location = new System.Drawing.Point(21, 263);
            this.chkFFzEmotes.Name = "chkFFzEmotes";
            this.chkFFzEmotes.Size = new System.Drawing.Size(167, 17);
            this.chkFFzEmotes.TabIndex = 4;
            this.chkFFzEmotes.Text = "Enable FrankerFaceZ Emotes";
            this.chkFFzEmotes.UseVisualStyleBackColor = true;
            // 
            // lblEmotes
            // 
            this.lblEmotes.AutoSize = true;
            this.lblEmotes.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblEmotes.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.lblEmotes.Location = new System.Drawing.Point(18, 216);
            this.lblEmotes.Name = "lblEmotes";
            this.lblEmotes.Size = new System.Drawing.Size(51, 17);
            this.lblEmotes.TabIndex = 3;
            this.lblEmotes.Text = "Emotes";
            // 
            // lblTabs
            // 
            this.lblTabs.AutoSize = true;
            this.lblTabs.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTabs.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.lblTabs.Location = new System.Drawing.Point(18, 7);
            this.lblTabs.Name = "lblTabs";
            this.lblTabs.Size = new System.Drawing.Size(34, 17);
            this.lblTabs.TabIndex = 2;
            this.lblTabs.Text = "Chat";
            // 
            // chkTimestampSeconds
            // 
            this.chkTimestampSeconds.AutoSize = true;
            this.chkTimestampSeconds.Checked = true;
            this.chkTimestampSeconds.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkTimestampSeconds.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.chkTimestampSeconds.Location = new System.Drawing.Point(21, 105);
            this.chkTimestampSeconds.Name = "chkTimestampSeconds";
            this.chkTimestampSeconds.Size = new System.Drawing.Size(168, 17);
            this.chkTimestampSeconds.TabIndex = 1;
            this.chkTimestampSeconds.Text = "Show Seconds in Timestamps";
            this.chkTimestampSeconds.UseVisualStyleBackColor = true;
            // 
            // chkTimestamps
            // 
            this.chkTimestamps.AutoSize = true;
            this.chkTimestamps.Checked = true;
            this.chkTimestamps.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkTimestamps.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.chkTimestamps.Location = new System.Drawing.Point(21, 82);
            this.chkTimestamps.Name = "chkTimestamps";
            this.chkTimestamps.Size = new System.Drawing.Size(112, 17);
            this.chkTimestamps.TabIndex = 0;
            this.chkTimestamps.Text = "Show Timestamps";
            this.chkTimestamps.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.comboBox1);
            this.panel2.Controls.Add(this.label4);
            this.panel2.Controls.Add(this.textBox4);
            this.panel2.Controls.Add(this.lblProxyPassword);
            this.panel2.Controls.Add(this.textBox3);
            this.panel2.Controls.Add(this.lblProxyUsername);
            this.panel2.Controls.Add(this.textBox2);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.textBox1);
            this.panel2.Controls.Add(this.lblProxyType);
            this.panel2.Controls.Add(this.lblProxy);
            this.panel2.Controls.Add(this.chkProxyEnabled);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(448, 406);
            this.panel2.TabIndex = 0;
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(87, 52);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(167, 21);
            this.comboBox1.TabIndex = 15;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label4.Location = new System.Drawing.Point(18, 107);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 13);
            this.label4.TabIndex = 14;
            this.label4.Text = "Port:";
            // 
            // textBox4
            // 
            this.textBox4.Location = new System.Drawing.Point(87, 103);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(167, 20);
            this.textBox4.TabIndex = 13;
            // 
            // lblProxyPassword
            // 
            this.lblProxyPassword.AutoSize = true;
            this.lblProxyPassword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.lblProxyPassword.Location = new System.Drawing.Point(18, 156);
            this.lblProxyPassword.Name = "lblProxyPassword";
            this.lblProxyPassword.Size = new System.Drawing.Size(56, 13);
            this.lblProxyPassword.TabIndex = 12;
            this.lblProxyPassword.Text = "Password:";
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(87, 153);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(167, 20);
            this.textBox3.TabIndex = 11;
            // 
            // lblProxyUsername
            // 
            this.lblProxyUsername.AutoSize = true;
            this.lblProxyUsername.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.lblProxyUsername.Location = new System.Drawing.Point(18, 131);
            this.lblProxyUsername.Name = "lblProxyUsername";
            this.lblProxyUsername.Size = new System.Drawing.Size(58, 13);
            this.lblProxyUsername.TabIndex = 10;
            this.lblProxyUsername.Text = "Username:";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(87, 128);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(167, 20);
            this.textBox2.TabIndex = 9;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label1.Location = new System.Drawing.Point(18, 81);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Host:";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(87, 78);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(167, 20);
            this.textBox1.TabIndex = 7;
            // 
            // lblProxyType
            // 
            this.lblProxyType.AutoSize = true;
            this.lblProxyType.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.lblProxyType.Location = new System.Drawing.Point(18, 55);
            this.lblProxyType.Name = "lblProxyType";
            this.lblProxyType.Size = new System.Drawing.Size(34, 13);
            this.lblProxyType.TabIndex = 6;
            this.lblProxyType.Text = "Type:";
            // 
            // lblProxy
            // 
            this.lblProxy.AutoSize = true;
            this.lblProxy.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProxy.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.lblProxy.Location = new System.Drawing.Point(18, 7);
            this.lblProxy.Name = "lblProxy";
            this.lblProxy.Size = new System.Drawing.Size(228, 17);
            this.lblProxy.TabIndex = 4;
            this.lblProxy.Text = "Proxy (doesn\'t work right now Keepo)";
            // 
            // chkProxyEnabled
            // 
            this.chkProxyEnabled.AutoSize = true;
            this.chkProxyEnabled.Checked = true;
            this.chkProxyEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkProxyEnabled.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.chkProxyEnabled.Location = new System.Drawing.Point(21, 31);
            this.chkProxyEnabled.Name = "chkProxyEnabled";
            this.chkProxyEnabled.Size = new System.Drawing.Size(88, 17);
            this.chkProxyEnabled.TabIndex = 3;
            this.chkProxyEnabled.Text = "Enable Proxy";
            this.chkProxyEnabled.UseVisualStyleBackColor = true;
            // 
            // spAppearance
            // 
            this.spAppearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.spAppearance.Image = null;
            this.spAppearance.Location = new System.Drawing.Point(0, 0);
            this.spAppearance.Name = "spAppearance";
            this.spAppearance.Panel = this.panel1;
            this.spAppearance.Selected = false;
            this.spAppearance.Size = new System.Drawing.Size(150, 30);
            this.spAppearance.TabIndex = 1;
            this.spAppearance.Text = "Appearance";
            // 
            // spConnection
            // 
            this.spConnection.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.spConnection.Image = null;
            this.spConnection.Location = new System.Drawing.Point(0, 30);
            this.spConnection.Name = "spConnection";
            this.spConnection.Panel = this.panel2;
            this.spConnection.Selected = false;
            this.spConnection.Size = new System.Drawing.Size(150, 30);
            this.spConnection.TabIndex = 2;
            this.spConnection.Text = "Connection";
            // 
            // spHighlighted
            // 
            this.spHighlighted.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.spHighlighted.Image = null;
            this.spHighlighted.Location = new System.Drawing.Point(0, 60);
            this.spHighlighted.Name = "spHighlighted";
            this.spHighlighted.Panel = this.panel3;
            this.spHighlighted.Size = new System.Drawing.Size(150, 30);
            this.spHighlighted.TabIndex = 3;
            this.spHighlighted.Text = "Highlighting";
            // 
            // btnTextCustomPing
            // 
            this.btnTextCustomPing.Location = new System.Drawing.Point(193, 78);
            this.btnTextCustomPing.Name = "btnTextCustomPing";
            this.btnTextCustomPing.Size = new System.Drawing.Size(75, 23);
            this.btnTextCustomPing.TabIndex = 20;
            this.btnTextCustomPing.Text = "Test";
            this.btnTextCustomPing.UseVisualStyleBackColor = true;
            this.btnTextCustomPing.Click += new System.EventHandler(this.btnTextCustomPing_Click);
            // 
            // SettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.ClientSize = new System.Drawing.Size(598, 448);
            this.ControlBox = false;
            this.Controls.Add(this.tabs);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(600, 450);
            this.Name = "SettingsDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Preferences";
            this.tabs.ResumeLayout(false);
            this.RightPanel.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private SettingsTabControl tabs;
        private System.Windows.Forms.Panel RightPanel;
        private System.Windows.Forms.Panel panel1;
        private SettingsTabPage spAppearance;
        private System.Windows.Forms.CheckBox chkTimestamps;
        private System.Windows.Forms.CheckBox chkTimestampSeconds;
        private System.Windows.Forms.Label lblTabs;
        private System.Windows.Forms.Label lblEmotes;
        private System.Windows.Forms.CheckBox chkFFzEmotes;
        private System.Windows.Forms.CheckBox chkGifEmotes;
        private System.Windows.Forms.CheckBox chkBttvEmotes;
        private System.Windows.Forms.CheckBox chkAllowSameMessages;
        private SettingsTabPage spConnection;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label lblProxy;
        private System.Windows.Forms.CheckBox chkProxyEnabled;
        private System.Windows.Forms.Label lblProxyType;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.Label lblProxyPassword;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Label lblProxyUsername;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.CheckBox chkEmojis;
        private System.Windows.Forms.Panel panel3;
        private SettingsTabPage spHighlighted;
        private System.Windows.Forms.CheckBox chkFlashTaskbar;
        private System.Windows.Forms.CheckBox chkHighlight;
        private System.Windows.Forms.CheckBox chkPings;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox rtbHighlights;
        private System.Windows.Forms.CheckBox chkDoubleClickLinks;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtMsgLimit;
        private System.Windows.Forms.CheckBox chkHideInput;
        private System.Windows.Forms.Button btnSelectFont;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblFont;
        private System.Windows.Forms.CheckBox chkCustomPingSound;
        private System.Windows.Forms.Button btnCustomHighlightOpenFile;
        private System.Windows.Forms.Button btnTextCustomPing;
    }
}