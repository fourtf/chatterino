using Chatterino.Common;
using Meebey.SmartIrc4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chatterino
{
    public static class App
    {
        [STAThread]
        static void Main()
        {
            GuiEngine.Initialize(new WinformsGuiEngine());

            ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);

            // Exceptions
            Application.ThreadException += (s, e) =>
            {
                e.Exception.Log("exception", "{0}\n");
            };
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                (e.ExceptionObject as Exception).Log("exception", "{0}\n");
            };

            // Update gif emotes
            new Timer { Interval = 20, Enabled = true }.Tick += (s, e) =>
            {
                if (AppSettings.ChatEnableGifAnimations)
                {
                    GifEmoteFramesUpdating?.Invoke(null, EventArgs.Empty);
                    GifEmoteFramesUpdated?.Invoke(null, EventArgs.Empty);
                }
            };

            // Settings/Colors
            AppSettings.Load("./settings.ini");
            ColorScheme.Load("./colors.ini");

            // Start irc
            IrcManager.Connect();
            Emotes.LoadGlobalEmotes();
            Badges.LoadGlobalBadges();

            // Show form
            MainForm = new MainForm();

            Application.Run(MainForm);

            // Save settings
            AppSettings.Save("./settings.ini");
        }

        public const TextFormatFlags DefaultTextFormatFlags = TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;

        public static event EventHandler GifEmoteFramesUpdating;
        public static event EventHandler GifEmoteFramesUpdated;

        //public static void TriggerGifEmoteFramesUpdated()
        //{
        //    GifEmoteFramesUpdated?.Invoke(null, EventArgs.Empty);
        //}


        // COLOR SCHEME
        public static ColorScheme ColorScheme { get; set; } = new ColorScheme();
        public static event EventHandler ColorSchemeChanged;
        public static void TriggerColorSchemeChanged()
        {
            ColorSchemeChanged?.Invoke(null, EventArgs.Empty);
        }

        // WINDOW
        public static MainForm MainForm { get; set; }

        public static Icon Icon { get; private set; }

        public static Controls.SettingsDialog SettingsDialog { get; set; }

        public static void ShowSettings()
        {
            if (SettingsDialog == null)
            {
                SettingsDialog = new Controls.SettingsDialog();
                SettingsDialog.Show();
                SettingsDialog.FormClosing += (s, e) =>
                {
                    SettingsDialog = null;
                };
            }
            else
            {
                SettingsDialog.Focus();
            }
        }

        public static Controls.ToolTip ToolTip { get; private set; } = null;
        public static bool EnableTooltips { get; set; } = true;

        public static void ShowToolTip(System.Drawing.Point point, string text)
        {
            if (EnableTooltips)
            {
                if (ToolTip == null)
                {
                    ToolTip = new Controls.ToolTip() { Enabled = false };
                }

                ToolTip.TooltipText = text;
                ToolTip.Location = point;

                if (!ToolTip.Visible)
                    ToolTip.Show();
            }
        }

        // EMOTES
        public static event EventHandler EmoteLoaded;

        public static void TriggerEmoteLoaded()
        {
            EmoteLoaded?.Invoke(null, EventArgs.Empty);
        }
    }
}
