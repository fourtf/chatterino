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
        // Updates
        public static VersionNumber CurrentVersion { get; private set; }
        private static bool installUpdatesOnExit = false;
        private static bool restartAfterUpdates = false;

        // Drawing
        public static bool UseDirectX { get; private set; } = false;

        public const TextFormatFlags DefaultTextFormatFlags = TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;

        public static event EventHandler GifEmoteFramesUpdating;
        public static event EventHandler GifEmoteFramesUpdated;

        // Color Scheme
        public static ColorScheme ColorScheme { get; set; } = new ColorScheme();
        public static event EventHandler ColorSchemeChanged;

        // Window
        public static MainForm MainForm { get; set; }

        public static Icon Icon { get; private set; }

        public static Controls.SettingsDialog SettingsDialog { get; set; }

        public static Controls.ToolTip ToolTip { get; private set; } = null;

        private static bool windowFocused = true;
        public static bool WindowFocused
        {
            get { return windowFocused; }
            set { windowFocused = value; Common.Message.EnablePings = !value; }
        }

        // Emotes
        public static event EventHandler EmoteLoaded;

        // Main Entry Point
        [STAThread]
        static void Main()
        {
            CurrentVersion = VersionNumber.Parse(AssemblyName.GetAssemblyName(Assembly.GetExecutingAssembly().Location).Version.ToString());
            
            Directory.SetCurrentDirectory(new FileInfo(Assembly.GetEntryAssembly().Location).Directory.FullName);

            GuiEngine.Initialize(new WinformsGuiEngine());

            ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);

            // Fonts
            if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 1)
                UseDirectX = true;

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
            AppSettings.Load("./Settings.ini");
            ColorScheme.Load("./Custom/Colors.ini");

            // Check for updates
            try
            {
                if (Directory.Exists("Updater.new"))
                {
                    if (Directory.Exists("Updater"))
                        Directory.Delete("Updater", true);

                    Directory.Move("Updater.new", "Updater");

                }
            }
            catch { }

            Updates.UpdateFound += (s, e) =>
            {
                try
                {
                    using (Controls.UpdateDialog dialog = new Controls.UpdateDialog())
                    {
                        if (File.Exists("./Updater/Chatterino.Updater.exe"))
                        {
                            var result = dialog.ShowDialog();

                            // OK -> install now
                            // Yes -> install on exit
                            if (result == DialogResult.OK || result == DialogResult.Yes)
                            {
                                using (WebClient client = new WebClient())
                                {
                                    client.DownloadFile(e.Url, "./Updater/update.zip");
                                }

                                installUpdatesOnExit = true;

                                if (result == DialogResult.OK)
                                {
                                    restartAfterUpdates = true;
                                    MainForm?.Close();
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("An update is available but the update executable could not be found. If you want to update chatterino you will have to reinstall it.");
                        }
                    }
                }
                catch { }
            };

#if DEBUG
            Updates.CheckForUpdate("win-dev", CurrentVersion);
#else
            Updates.CheckForUpdate("win-release", CurrentVersion);
#endif

            // Start irc
            IrcManager.Connect();
            Emotes.LoadGlobalEmotes();
            Badges.LoadGlobalBadges();

            // Show form
            MainForm = new MainForm();

            Application.Run(MainForm);

            // Save settings
            AppSettings.Save("./Settings.ini");

            // Install updates
            if (installUpdatesOnExit)
            {
                Process.Start(Path.Combine(new FileInfo(Assembly.GetEntryAssembly().Location).Directory.FullName, "Updater", "Chatterino.Updater.exe"), restartAfterUpdates ? "--restart" : "");
                System.Threading.Thread.Sleep(1000);
            }
        }

        // Public Functions
        public static void TriggerEmoteLoaded()
        {
            EmoteLoaded?.Invoke(null, EventArgs.Empty);
        }

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

        public static void ShowToolTip(Point point, string text)
        {
            if (WindowFocused)
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

        public static void TriggerColorSchemeChanged()
        {
            ColorSchemeChanged?.Invoke(null, EventArgs.Empty);
        }
    }
}
