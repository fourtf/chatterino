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
            this.panel4 = new System.Windows.Forms.Panel();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.label8 = new System.Windows.Forms.Label();
            this.rtbIngoredEmotes = new System.Windows.Forms.RichTextBox();
            this.chkEmojis = new System.Windows.Forms.CheckBox();
            this.chkGifEmotes = new System.Windows.Forms.CheckBox();
            this.chkBttvEmotes = new System.Windows.Forms.CheckBox();
            this.chkFFzEmotes = new System.Windows.Forms.CheckBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.label2 = new System.Windows.Forms.Label();
            this.rtbHighlights = new System.Windows.Forms.RichTextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.rtbUserBlacklist = new System.Windows.Forms.RichTextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.btnTextCustomPing = new System.Windows.Forms.Button();
            this.btnCustomHighlightOpenFile = new System.Windows.Forms.Button();
            this.chkCustomPingSound = new System.Windows.Forms.CheckBox();
            this.chkFlashTaskbar = new System.Windows.Forms.CheckBox();
            this.chkHighlight = new System.Windows.Forms.CheckBox();
            this.chkPings = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.comboTheme = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.chkMessageSeperators = new System.Windows.Forms.CheckBox();
            this.lblFont = new System.Windows.Forms.Label();
            this.btnSelectFont = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.chkHideInput = new System.Windows.Forms.CheckBox();
            this.txtMsgLimit = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.chkDoubleClickLinks = new System.Windows.Forms.CheckBox();
            this.chkAllowSameMessages = new System.Windows.Forms.CheckBox();
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
            this.spEmotes = new Chatterino.Controls.SettingsTabPage();
            this.spHighlighted = new Chatterino.Controls.SettingsTabPage();
            this.spConnection = new Chatterino.Controls.SettingsTabPage();
            this.tabs.SuspendLayout();
            this.RightPanel.SuspendLayout();
            this.panel4.SuspendLayout();
            this.tabControl2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.panel3.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabs
            // 
            this.tabs.AllowDrop = true;
            this.tabs.Controls.Add(this.RightPanel);
            this.tabs.Controls.Add(this.spAppearance);
            this.tabs.Controls.Add(this.spEmotes);
            this.tabs.Controls.Add(this.spHighlighted);
            this.tabs.Controls.Add(this.spConnection);
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
            this.RightPanel.Controls.Add(this.panel4);
            this.RightPanel.Controls.Add(this.panel1);
            this.RightPanel.Controls.Add(this.panel2);
            this.RightPanel.Location = new System.Drawing.Point(150, 0);
            this.RightPanel.Name = "RightPanel";
            this.RightPanel.Padding = new System.Windows.Forms.Padding(0, 0, 0, 42);
            this.RightPanel.Size = new System.Drawing.Size(448, 448);
            this.RightPanel.TabIndex = 0;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.tabControl2);
            this.panel4.Controls.Add(this.chkEmojis);
            this.panel4.Controls.Add(this.chkGifEmotes);
            this.panel4.Controls.Add(this.chkBttvEmotes);
            this.panel4.Controls.Add(this.chkFFzEmotes);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(0, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(448, 406);
            this.panel4.TabIndex = 2;
            // 
            // tabControl2
            // 
            this.tabControl2.Controls.Add(this.tabPage3);
            this.tabControl2.Location = new System.Drawing.Point(17, 110);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(415, 283);
            this.tabControl2.TabIndex = 22;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.label8);
            this.tabPage3.Controls.Add(this.rtbIngoredEmotes);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(407, 257);
            this.tabPage3.TabIndex = 0;
            this.tabPage3.Text = "Ignored Emotes";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.label8.ForeColor = System.Drawing.Color.Black;
            this.label8.Location = new System.Drawing.Point(3, 5);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(230, 13);
            this.label8.TabIndex = 22;
            this.label8.Text = "Emotes that will be shown as text (one per line):";
            // 
            // rtbIngoredEmotes
            // 
            this.rtbIngoredEmotes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbIngoredEmotes.Location = new System.Drawing.Point(6, 21);
            this.rtbIngoredEmotes.Name = "rtbIngoredEmotes";
            this.rtbIngoredEmotes.Size = new System.Drawing.Size(395, 230);
            this.rtbIngoredEmotes.TabIndex = 17;
            this.rtbIngoredEmotes.Text = "";
            // 
            // chkEmojis
            // 
            this.chkEmojis.AutoSize = true;
            this.chkEmojis.Checked = true;
            this.chkEmojis.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkEmojis.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.chkEmojis.Location = new System.Drawing.Point(16, 82);
            this.chkEmojis.Name = "chkEmojis";
            this.chkEmojis.Size = new System.Drawing.Size(92, 17);
            this.chkEmojis.TabIndex = 13;
            this.chkEmojis.Text = "Enable Emojis";
            this.chkEmojis.UseVisualStyleBackColor = true;
            // 
            // chkGifEmotes
            // 
            this.chkGifEmotes.AutoSize = true;
            this.chkGifEmotes.Checked = true;
            this.chkGifEmotes.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGifEmotes.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.chkGifEmotes.Location = new System.Drawing.Point(16, 59);
            this.chkGifEmotes.Name = "chkGifEmotes";
            this.chkGifEmotes.Size = new System.Drawing.Size(133, 17);
            this.chkGifEmotes.TabIndex = 12;
            this.chkGifEmotes.Text = "Enable GIF Animations";
            this.chkGifEmotes.UseVisualStyleBackColor = true;
            // 
            // chkBttvEmotes
            // 
            this.chkBttvEmotes.AutoSize = true;
            this.chkBttvEmotes.Checked = true;
            this.chkBttvEmotes.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBttvEmotes.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.chkBttvEmotes.Location = new System.Drawing.Point(16, 13);
            this.chkBttvEmotes.Name = "chkBttvEmotes";
            this.chkBttvEmotes.Size = new System.Drawing.Size(149, 17);
            this.chkBttvEmotes.TabIndex = 11;
            this.chkBttvEmotes.Text = "Enable BetterTTV Emotes";
            this.chkBttvEmotes.UseVisualStyleBackColor = true;
            // 
            // chkFFzEmotes
            // 
            this.chkFFzEmotes.AutoSize = true;
            this.chkFFzEmotes.Checked = true;
            this.chkFFzEmotes.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkFFzEmotes.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.chkFFzEmotes.Location = new System.Drawing.Point(16, 36);
            this.chkFFzEmotes.Name = "chkFFzEmotes";
            this.chkFFzEmotes.Size = new System.Drawing.Size(167, 17);
            this.chkFFzEmotes.TabIndex = 10;
            this.chkFFzEmotes.Text = "Enable FrankerFaceZ Emotes";
            this.chkFFzEmotes.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.tabControl1);
            this.panel3.Controls.Add(this.btnTextCustomPing);
            this.panel3.Controls.Add(this.btnCustomHighlightOpenFile);
            this.panel3.Controls.Add(this.chkCustomPingSound);
            this.panel3.Controls.Add(this.chkFlashTaskbar);
            this.panel3.Controls.Add(this.chkHighlight);
            this.panel3.Controls.Add(this.chkPings);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(448, 406);
            this.panel3.TabIndex = 1;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(16, 105);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(415, 298);
            this.tabControl1.TabIndex = 21;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.rtbHighlights);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(407, 272);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Highlight Keywords";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.label2.ForeColor = System.Drawing.Color.Black;
            this.label2.Location = new System.Drawing.Point(3, 5);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(163, 13);
            this.label2.TabIndex = 22;
            this.label2.Text = "Highlight keywords (one per line):";
            // 
            // rtbHighlights
            // 
            this.rtbHighlights.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbHighlights.Location = new System.Drawing.Point(6, 21);
            this.rtbHighlights.Name = "rtbHighlights";
            this.rtbHighlights.Size = new System.Drawing.Size(395, 245);
            this.rtbHighlights.TabIndex = 17;
            this.rtbHighlights.Text = "";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.rtbUserBlacklist);
            this.tabPage2.Controls.Add(this.label6);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(407, 272);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "User Highlight Blacklist";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // rtbUserBlacklist
            // 
            this.rtbUserBlacklist.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbUserBlacklist.Location = new System.Drawing.Point(6, 21);
            this.rtbUserBlacklist.Name = "rtbUserBlacklist";
            this.rtbUserBlacklist.Size = new System.Drawing.Size(395, 246);
            this.rtbUserBlacklist.TabIndex = 24;
            this.rtbUserBlacklist.Text = "";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.label6.ForeColor = System.Drawing.Color.Black;
            this.label6.Location = new System.Drawing.Point(3, 5);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(204, 13);
            this.label6.TabIndex = 23;
            this.label6.Text = "Users that can not ping you (one per line):";
            // 
            // btnTextCustomPing
            // 
            this.btnTextCustomPing.Location = new System.Drawing.Point(188, 78);
            this.btnTextCustomPing.Name = "btnTextCustomPing";
            this.btnTextCustomPing.Size = new System.Drawing.Size(75, 23);
            this.btnTextCustomPing.TabIndex = 20;
            this.btnTextCustomPing.Text = "Test";
            this.btnTextCustomPing.UseVisualStyleBackColor = true;
            this.btnTextCustomPing.Click += new System.EventHandler(this.btnTextCustomPing_Click);
            // 
            // btnCustomHighlightOpenFile
            // 
            this.btnCustomHighlightOpenFile.Image = global::Chatterino.Properties.Resources.OpenFolder_16x;
            this.btnCustomHighlightOpenFile.Location = new System.Drawing.Point(156, 78);
            this.btnCustomHighlightOpenFile.Name = "btnCustomHighlightOpenFile";
            this.btnCustomHighlightOpenFile.Size = new System.Drawing.Size(24, 23);
            this.btnCustomHighlightOpenFile.TabIndex = 19;
            this.btnCustomHighlightOpenFile.UseVisualStyleBackColor = true;
            // 
            // chkCustomPingSound
            // 
            this.chkCustomPingSound.AutoSize = true;
            this.chkCustomPingSound.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.chkCustomPingSound.Location = new System.Drawing.Point(16, 82);
            this.chkCustomPingSound.Name = "chkCustomPingSound";
            this.chkCustomPingSound.Size = new System.Drawing.Size(135, 17);
            this.chkCustomPingSound.TabIndex = 18;
            this.chkCustomPingSound.Text = "Custom highlight sound";
            this.chkCustomPingSound.UseVisualStyleBackColor = true;
            // 
            // chkFlashTaskbar
            // 
            this.chkFlashTaskbar.AutoSize = true;
            this.chkFlashTaskbar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.chkFlashTaskbar.Location = new System.Drawing.Point(16, 59);
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
            this.chkHighlight.Location = new System.Drawing.Point(16, 13);
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
            this.chkPings.Location = new System.Drawing.Point(16, 36);
            this.chkPings.Name = "chkPings";
            this.chkPings.Size = new System.Drawing.Size(225, 17);
            this.chkPings.TabIndex = 14;
            this.chkPings.Text = "Play Sound when your Name is mentioned";
            this.chkPings.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.comboTheme);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.chkMessageSeperators);
            this.panel1.Controls.Add(this.lblFont);
            this.panel1.Controls.Add(this.btnSelectFont);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.chkHideInput);
            this.panel1.Controls.Add(this.txtMsgLimit);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.chkDoubleClickLinks);
            this.panel1.Controls.Add(this.chkAllowSameMessages);
            this.panel1.Controls.Add(this.chkTimestampSeconds);
            this.panel1.Controls.Add(this.chkTimestamps);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(448, 406);
            this.panel1.TabIndex = 0;
            // 
            // comboTheme
            // 
            this.comboTheme.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboTheme.FormattingEnabled = true;
            this.comboTheme.Items.AddRange(new object[] {
            "White",
            "Light",
            "Dark",
            "Black"});
            this.comboTheme.Location = new System.Drawing.Point(97, 11);
            this.comboTheme.Name = "comboTheme";
            this.comboTheme.Size = new System.Drawing.Size(167, 21);
            this.comboTheme.TabIndex = 16;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.label7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label7.Location = new System.Drawing.Point(14, 14);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(43, 13);
            this.label7.TabIndex = 17;
            this.label7.Text = "Theme:";
            // 
            // chkMessageSeperators
            // 
            this.chkMessageSeperators.AutoSize = true;
            this.chkMessageSeperators.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.chkMessageSeperators.Location = new System.Drawing.Point(16, 202);
            this.chkMessageSeperators.Name = "chkMessageSeperators";
            this.chkMessageSeperators.Size = new System.Drawing.Size(119, 17);
            this.chkMessageSeperators.TabIndex = 16;
            this.chkMessageSeperators.Text = "Seperate messages";
            this.chkMessageSeperators.UseVisualStyleBackColor = true;
            // 
            // lblFont
            // 
            this.lblFont.AutoSize = true;
            this.lblFont.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.lblFont.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.lblFont.Location = new System.Drawing.Point(178, 39);
            this.lblFont.Name = "lblFont";
            this.lblFont.Size = new System.Drawing.Size(54, 13);
            this.lblFont.TabIndex = 15;
            this.lblFont.Text = "font name";
            // 
            // btnSelectFont
            // 
            this.btnSelectFont.Location = new System.Drawing.Point(97, 34);
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
            this.label5.Location = new System.Drawing.Point(14, 39);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(31, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Font:";
            // 
            // chkHideInput
            // 
            this.chkHideInput.AutoSize = true;
            this.chkHideInput.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.chkHideInput.Location = new System.Drawing.Point(16, 179);
            this.chkHideInput.Name = "chkHideInput";
            this.chkHideInput.Size = new System.Drawing.Size(134, 17);
            this.chkHideInput.TabIndex = 12;
            this.chkHideInput.Text = "Hide input box if Empty";
            this.chkHideInput.UseVisualStyleBackColor = true;
            // 
            // txtMsgLimit
            // 
            this.txtMsgLimit.Location = new System.Drawing.Point(97, 61);
            this.txtMsgLimit.Name = "txtMsgLimit";
            this.txtMsgLimit.Size = new System.Drawing.Size(167, 20);
            this.txtMsgLimit.TabIndex = 10;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label3.Location = new System.Drawing.Point(14, 65);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Message Limit:";
            // 
            // chkDoubleClickLinks
            // 
            this.chkDoubleClickLinks.AutoSize = true;
            this.chkDoubleClickLinks.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.chkDoubleClickLinks.Location = new System.Drawing.Point(16, 156);
            this.chkDoubleClickLinks.Name = "chkDoubleClickLinks";
            this.chkDoubleClickLinks.Size = new System.Drawing.Size(180, 17);
            this.chkDoubleClickLinks.TabIndex = 10;
            this.chkDoubleClickLinks.Text = "Only open Links on Double Click";
            this.chkDoubleClickLinks.UseVisualStyleBackColor = true;
            // 
            // chkAllowSameMessages
            // 
            this.chkAllowSameMessages.AutoSize = true;
            this.chkAllowSameMessages.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.chkAllowSameMessages.Location = new System.Drawing.Point(16, 133);
            this.chkAllowSameMessages.Name = "chkAllowSameMessages";
            this.chkAllowSameMessages.Size = new System.Drawing.Size(309, 17);
            this.chkAllowSameMessages.TabIndex = 8;
            this.chkAllowSameMessages.Text = "Allow sending Duplicate Messages (add a space at the end)";
            this.chkAllowSameMessages.UseVisualStyleBackColor = true;
            // 
            // chkTimestampSeconds
            // 
            this.chkTimestampSeconds.AutoSize = true;
            this.chkTimestampSeconds.Checked = true;
            this.chkTimestampSeconds.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkTimestampSeconds.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.chkTimestampSeconds.Location = new System.Drawing.Point(16, 110);
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
            this.chkTimestamps.Location = new System.Drawing.Point(16, 87);
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
            // spEmotes
            // 
            this.spEmotes.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.spEmotes.Image = null;
            this.spEmotes.Location = new System.Drawing.Point(0, 30);
            this.spEmotes.Name = "spEmotes";
            this.spEmotes.Panel = this.panel4;
            this.spEmotes.Selected = false;
            this.spEmotes.Size = new System.Drawing.Size(150, 30);
            this.spEmotes.TabIndex = 4;
            this.spEmotes.Text = "Emotes";
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
            // spConnection
            // 
            this.spConnection.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.spConnection.Image = null;
            this.spConnection.Location = new System.Drawing.Point(0, 90);
            this.spConnection.Name = "spConnection";
            this.spConnection.Panel = this.panel2;
            this.spConnection.Selected = false;
            this.spConnection.Size = new System.Drawing.Size(150, 30);
            this.spConnection.TabIndex = 2;
            this.spConnection.Text = "Connection";
            this.spConnection.Visible = false;
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
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.tabControl2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
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
        private System.Windows.Forms.Panel panel3;
        private SettingsTabPage spHighlighted;
        private System.Windows.Forms.CheckBox chkFlashTaskbar;
        private System.Windows.Forms.CheckBox chkHighlight;
        private System.Windows.Forms.CheckBox chkPings;
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
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox rtbUserBlacklist;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox chkMessageSeperators;
        private System.Windows.Forms.ComboBox comboTheme;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Panel panel4;
        private SettingsTabPage spEmotes;
        private System.Windows.Forms.CheckBox chkEmojis;
        private System.Windows.Forms.CheckBox chkGifEmotes;
        private System.Windows.Forms.CheckBox chkBttvEmotes;
        private System.Windows.Forms.CheckBox chkFFzEmotes;
        private System.Windows.Forms.TabControl tabControl2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.RichTextBox rtbIngoredEmotes;
    }
}