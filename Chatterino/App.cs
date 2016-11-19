using Chatterino.Common;
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
        public static event EventHandler ColorSchemeChanged;

        private static ColorScheme colorScheme;

        public static ColorScheme ColorScheme
        {
            get { return colorScheme; }
            set
            {
                if (colorScheme != value)
                {
                    colorScheme = value;
                    ColorSchemeChanged?.Invoke(null, null);
                }
            }
        }

        // Window
        public static MainForm MainForm { get; set; }

        public static Icon Icon { get; private set; }

        public static Controls.SettingsDialog SettingsDialog { get; set; }

        static Controls.ToolTip ToolTip { get; set; } = null;
        static Controls.EmoteListPopup EmoteList { get; set; } = null;

        private static bool windowFocused = true;
        public static bool WindowFocused
        {
            get { return windowFocused; }
            set { windowFocused = value; Common.Message.EnablePings = !value; }
        }

        // Emotes
        public static event EventHandler EmoteLoaded;

        [System.Runtime.InteropServices.DllImport("shcore.dll")]
        static extern int SetProcessDpiAwareness(_Process_DPI_Awareness value);

        enum _Process_DPI_Awareness
        {
            Process_DPI_Unaware = 0,
            Process_System_DPI_Aware = 1,
            Process_Per_Monitor_DPI_Aware = 2
        }

        // Main Entry Point
        [STAThread]
        static void Main()
        {
            CurrentVersion = VersionNumber.Parse(AssemblyName.GetAssemblyName(Assembly.GetExecutingAssembly().Location).Version.ToString());

            Directory.SetCurrentDirectory(new FileInfo(Assembly.GetEntryAssembly().Location).Directory.FullName);

            GuiEngine.Initialize(new WinformsGuiEngine());

            ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;
            ServicePointManager.DefaultConnectionLimit = 1000;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            AccountManager.LoadFromJson(Path.Combine(Util.GetUserDataPath(), "Login.json"));

            //SetProcessDpiAwareness(_Process_DPI_Awareness.Process_Per_Monitor_DPI_Aware);

            Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);

            // Fonts
            if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 1)
            {
                UseDirectX = true;
            }

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
            new Timer { Interval = 33, Enabled = true }.Tick += (s, e) =>
            {
                if (AppSettings.ChatEnableGifAnimations)
                {
                    GifEmoteFramesUpdating?.Invoke(null, EventArgs.Empty);
                    GifEmoteFramesUpdated?.Invoke(null, EventArgs.Empty);
                }
            };

            // Settings/Colors
            AppSettings.Load(Path.Combine(Util.GetUserDataPath(), "Settings.ini"));
            Commands.LoadOrDefault(Path.Combine(Util.GetUserDataPath(), "Custom", "Commands.txt"));
            Cache.Load();

            updateTheme();

            AppSettings.ThemeChanged += (s, e) => updateTheme();

            // Check for updates
            try
            {
                string updaterPath = Path.Combine(Util.GetUserDataPath(), "Updater");
                string newUpdaterPath = Path.Combine(Util.GetUserDataPath(), "Updater.new");

                if (Directory.Exists(newUpdaterPath))
                {
                    if (Directory.Exists(updaterPath))
                        Directory.Delete(updaterPath, true);

                    Directory.Move(newUpdaterPath, updaterPath);
                }
            }
            catch { }

            Updates.UpdateFound += (s, e) =>
            {
                try
                {
                    using (Controls.UpdateDialog dialog = new Controls.UpdateDialog())
                    {
                        if (File.Exists(Path.Combine(Util.GetUserDataPath(), "Updater", "Chatterino.Updater.exe")))
                        {
                            var result = dialog.ShowDialog();

                            // OK -> install now
                            // Yes -> install on exit
                            if (result == DialogResult.OK || result == DialogResult.Yes)
                            {
                                using (WebClient client = new WebClient())
                                {
                                    client.DownloadFile(e.Url, Path.Combine(Util.GetUserDataPath(), "Updater", "update.zip"));
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

            Net.StartHttpServer();

            // Show form
            MainForm = new MainForm();

            Application.Run(MainForm);

            // Save settings
            Cache.Save();

            if (!Directory.Exists(Path.Combine(Util.GetUserDataPath(), "Custom")))
                Directory.CreateDirectory(Path.Combine(Util.GetUserDataPath(), "Custom"));

            Commands.Save(Path.Combine(Util.GetUserDataPath(), "Custom", "Commands.txt"));

            // Install updates
            if (installUpdatesOnExit)
            {
                Process.Start(Path.Combine(Util.GetUserDataPath(), "Updater", "Chatterino.Updater.exe"), restartAfterUpdates ? "--restart" : "");
                System.Threading.Thread.Sleep(1000);
            }

            Environment.Exit(0);
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

                SettingsDialog.StartPosition = FormStartPosition.Manual;
                SettingsDialog.Location = new Point(MainForm.Location.X + 32, MainForm.Location.Y + 64);
                SettingsDialog.Show(MainForm);
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

        public static void ShowToolTip(Point point, string text, bool force = false)
        {
            if (force || WindowFocused || (EmoteList?.ContainsFocus ?? false))
            {
                if (ToolTip == null)
                {
                    ToolTip = new Controls.ToolTip() { Enabled = false };
                }

                ToolTip.TooltipText = text;
                ToolTip.Location = point;

                if (!ToolTip.Visible)
                {
                    ToolTip.Show();
                }
            }
        }

        public static void HideToolTip()
        {
            if (ToolTip != null)
            {
                ToolTip.Close();
                ToolTip.Dispose();
                ToolTip = null;
            }
        }

        public static void ShowEmoteList(TwitchChannel channel)
        {
            if (EmoteList == null)
            {
                EmoteList = new Controls.EmoteListPopup();
            }

            EmoteList.SetChannel(channel);

            EmoteList.Show();

            EmoteList.FormClosed += (s, e) =>
            {
                EmoteList = null;
            };
        }

        public static void SetEmoteListChannel(TwitchChannel channel)
        {
            EmoteList?.SetChannel(channel);
        }

        static void updateTheme()
        {
            float multiplier = -0.8f;

            switch (AppSettings.Theme)
            {
                case "White":
                    multiplier = 1f;
                    break;
                case "Light":
                    multiplier = 0.8f;
                    break;
                case "Dark":
                    multiplier = -0.8f;
                    break;
                case "Black":
                    multiplier = -1f;
                    break;
            }


            ColorScheme = ColorScheme.FromHue((float)Math.Max(Math.Min(AppSettings.ThemeHue, 1), 0), multiplier);

            MainForm?.Refresh();

            //if (MainForm != null)
            //{
            //    Action<Control> invalidate = null;

            //    invalidate = c =>
            //    {
            //        foreach (Control C in c.Controls)
            //        {
            //            C.Invalidate();
            //            invalidate(C);
            //        }
            //    };

            //    invalidate(MainForm);
            //}
        }
    }
}
