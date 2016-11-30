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
using System.Web.SessionState;
using System.Windows.Forms;
using Chatterino.Controls;

namespace Chatterino
{
    public static class App
    {
        // Updates
        public static VersionNumber CurrentVersion { get; private set; }
        private static bool installUpdatesOnExit = false;
        private static bool restartAfterUpdates = false;

        public static bool CanShowChangelogs { get; private set; } = true;
        public static string UpdaterPath { get; private set; } = Path.Combine(new FileInfo(Assembly.GetEntryAssembly().Location).Directory.FullName, "Updater", "Chatterino.Updater.exe");

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
        public static Controls.EmoteListPopup EmoteList { get; set; } = null;

        private static bool windowFocused = true;

        public static bool WindowFocused
        {
            get { return windowFocused; }
            set
            {
                windowFocused = value;
                Common.Message.EnablePings = !value;
            }
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
            CurrentVersion = VersionNumber.Parse(
                    AssemblyName.GetAssemblyName(Assembly.GetExecutingAssembly().Location).Version.ToString());

            if (!File.Exists("./removeupdatenew") && Directory.Exists("./Updater.new"))
            {
                UpdaterPath = Path.Combine(new FileInfo(Assembly.GetEntryAssembly().Location).Directory.FullName,
                    "Updater.new", "Chatterino.Updater.exe");
            }
            else
            {
                if (File.Exists("./update2"))
                {
                    UpdaterPath = Path.Combine(new FileInfo(Assembly.GetEntryAssembly().Location).Directory.FullName,
                        "Updater2", "Chatterino.Updater.exe");
                }
            }

            Directory.SetCurrentDirectory(new FileInfo(Assembly.GetEntryAssembly().Location).Directory.FullName);

            GuiEngine.Initialize(new WinformsGuiEngine());

            ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;
            ServicePointManager.DefaultConnectionLimit = 1000;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

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
            new Timer { Interval = 30, Enabled = true }.Tick += (s, e) =>
                {
                    if (AppSettings.ChatEnableGifAnimations)
                    {
                        GifEmoteFramesUpdating?.Invoke(null, EventArgs.Empty);
                        GifEmoteFramesUpdated?.Invoke(null, EventArgs.Empty);
                    }
                };

            // Settings/Colors
            try
            {
                if (!Directory.Exists(Util.GetUserDataPath()))
                {
                    Directory.CreateDirectory(Util.GetUserDataPath());
                }

                if (!Directory.Exists(Path.Combine(Util.GetUserDataPath(), "Custom")))
                {
                    Directory.CreateDirectory(Path.Combine(Util.GetUserDataPath(), "Custom"));
                }
            }
            catch
            {

            }

            AppSettings.SavePath = Path.Combine(Util.GetUserDataPath(), "Settings.ini");

            bool showWelcomeForm = false;

            try
            {
                if (!File.Exists(AppSettings.SavePath))
                {
                    CanShowChangelogs = false;

                    showWelcomeForm = true;

                    if (File.Exists("./Settings.ini") && !File.Exists(AppSettings.SavePath))
                    {
                        File.Move("./Settings.ini", AppSettings.SavePath);

                        try

                        {
                            File.Delete("./Settings.ini");
                        }
                        catch { }
                    }

                    if (File.Exists("./Custom/Commands.txt") &&
                        !File.Exists(Path.Combine(Util.GetUserDataPath(), "Custom", "Commands.txt")))
                    {
                        File.Move("./Custom/Commands.txt", Path.Combine(Util.GetUserDataPath(), "Custom", "Commands.txt"));

                        try
                        {
                            File.Delete("./Custom/Commands.txt");
                        }
                        catch { }
                    }

                    if (File.Exists("./Custom/Ping.wav") &&
                        !File.Exists(Path.Combine(Util.GetUserDataPath(), "Custom", "Ping.wav")))
                    {
                        File.Move("./Custom/Ping.wav", Path.Combine(Util.GetUserDataPath(), "Custom", "Ping.wav"));

                        try
                        {
                            File.Delete("./Custom/Ping.wav");
                        }
                        catch { }
                    }

                    if (File.Exists("./Layout.xml") &&
                        !File.Exists(Path.Combine(Util.GetUserDataPath(), "Layout.xml")))
                    {
                        File.Move("./Layout.xml", Path.Combine(Util.GetUserDataPath(), "Layout.xml"));

                        try
                        {
                            File.Delete("./Layout.xml");
                        }
                        catch { }
                    }
                }
            }
            catch
            {

            }

            AppSettings.Load();

            AccountManager.LoadFromJson(Path.Combine(Util.GetUserDataPath(), "Login.json"));

            IrcManager.Account = AccountManager.FromUsername(AppSettings.SelectedUser) ?? Account.AnonAccount;
            IrcManager.Connect();

            Commands.LoadOrDefault(Path.Combine(Util.GetUserDataPath(), "Custom", "Commands.txt"));
            Cache.Load();

            updateTheme();

            AppSettings.ThemeChanged += (s, e) => updateTheme();

            // Check for updates
            //try
            //{
            //    string updaterPath = "./Updater";
            //    string newUpdaterPath = "./Updater.new";

            //    if (Directory.Exists(newUpdaterPath))
            //    {
            //        if (Directory.Exists(updaterPath))
            //            Directory.Delete(updaterPath, true);

            //        Directory.Move(newUpdaterPath, updaterPath);
            //    }
            //}
            //catch { }

            Updates.UpdateFound += (s, e) =>
            {
                try
                {
                    using (UpdateDialog dialog = new UpdateDialog())
                    {
                        if (File.Exists(UpdaterPath))
                        {
                            var result = dialog.ShowDialog();

                            // OK -> install now
                            // Yes -> install on exit
                            if (result == DialogResult.OK || result == DialogResult.Yes)
                            {
                                using (WebClient client = new WebClient())
                                {
                                    client.DownloadFile(e.Url, Path.Combine(Util.GetUserDataPath(), "update.zip"));
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
            Emotes.LoadGlobalEmotes();
            Badges.LoadGlobalBadges();

            Net.StartHttpServer();

            // Show form
            MainForm = new MainForm();

            MainForm.Show();

            if (showWelcomeForm)
            {
                new WelcomeForm().Show();
            }

            MainForm.Closed += (s, e) =>
            {
                Application.Exit();
            };

            Application.Run();

            // Save settings
            AppSettings.Save();

            Cache.Save();

            Commands.Save(Path.Combine(Util.GetUserDataPath(), "Custom", "Commands.txt"));

            // Install updates
            if (installUpdatesOnExit)
            {
                Process.Start(UpdaterPath, restartAfterUpdates ? "--restart" : "");
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
                SettingsDialog = new Controls.SettingsDialog
                {
                    StartPosition = FormStartPosition.Manual,
                    Location = new Point(MainForm.Location.X + 32, MainForm.Location.Y + 64)
                };

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

                Screen.FromPoint(point);

                var screen = Screen.FromPoint(Cursor.Position);

                ToolTip.TooltipText = text;

                if (!ToolTip.Visible)
                {
                    ToolTip.Show();
                }

                int x = point.X, y = point.Y;

                if (point.X < screen.WorkingArea.X)
                {
                    x = screen.WorkingArea.X;
                }
                else if (point.X + ToolTip.Width > screen.WorkingArea.Right)
                {
                    x = screen.WorkingArea.Right - ToolTip.Width;
                }

                if (point.Y < screen.WorkingArea.Y)
                {
                    y = screen.WorkingArea.Y;
                }
                else if (point.Y + ToolTip.Height > screen.WorkingArea.Bottom)
                {
                    y = y - 24 - ToolTip.Height;
                }

                point = new Point(x, y);

                if (ToolTip.Location != point)
                {
                    ToolTip.Location = point;
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
