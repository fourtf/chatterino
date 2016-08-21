using Chatterino.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xwt;

namespace Chatterino.Desktop
{
    public static class App
    {
        public static MainWindow Window { get; private set; }
        public static ColorScheme ColorScheme { get; private set; }

        public static VersionNumber CurrentVersion { get; private set; }

        public static void Run(ToolkitType toolkit)
        {
            // Set working directory
            Directory.SetCurrentDirectory(new FileInfo(Assembly.GetEntryAssembly().Location).Directory.FullName);

            // Parse current version from file metadata
            CurrentVersion = VersionNumber.Parse(AssemblyName.GetAssemblyName(Assembly.GetExecutingAssembly().Location).Version.ToString());

            // Make SSL on mono happy
            ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;

            // Handle unhandled exceptions
            Application.UnhandledException += (s, e) =>
            {
                e.ErrorException.Log("exception", "{0}\n");
            };
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                (e.ExceptionObject as Exception).Log("exception", "{0}\n");
            };

            // Initialize Xwt
            Application.Initialize(toolkit);

            // GuiEngine
            if (GuiEngine.Current == null)
            {
                GuiEngine.Initialize(new XwtGuiEngine());
            }

            // Load settings
            AppSettings.Load("./Settings.ini");

            // Load color scheme
            ColorScheme = new ColorScheme();

            // Start irc
            IrcManager.Connect();

            // Load global emotes and badges
            Emotes.LoadGlobalEmotes();
            Badges.LoadGlobalBadges();

            // Create main window
            Window = new MainWindow();
            Window.Closed += (s, e) => Application.Exit();
            Window.Show();

            // Start the main loop
            Application.Run();

            // Save settings
            AppSettings.Save("./Settings.ini");



            // ---> not implemented yet

            // Update gif emotes
            //new Timer { Interval = 20, Enabled = true }.Tick += (s, e) =>
            //{
            //    if (AppSettings.ChatEnableGifAnimations)
            //    {
            //        GifEmoteFramesUpdating?.Invoke(null, EventArgs.Empty);
            //        GifEmoteFramesUpdated?.Invoke(null, EventArgs.Empty);
            //    }
            //};


            // Check for updates
            //            try
            //            {
            //                if (Directory.Exists("Updater.new"))
            //                {
            //                    if (Directory.Exists("Updater"))
            //                        Directory.Delete("Updater", true);

            //                    Directory.Move("Updater.new", "Updater");

            //                }
            //            }
            //            catch { }

            //            Updates.UpdateFound += (s, e) =>
            //            {
            //                try
            //                {
            //                    using (Controls.UpdateDialog dialog = new Controls.UpdateDialog())
            //                    {
            //                        if (File.Exists("./Updater/Chatterino.Updater.exe"))
            //                        {
            //                            var result = dialog.ShowDialog();

            //                            // OK -> install now
            //                            // Yes -> install on exit
            //                            if (result == DialogResult.OK || result == DialogResult.Yes)
            //                            {
            //                                using (WebClient client = new WebClient())
            //                                {
            //                                    client.DownloadFile(e.Url, "./Updater/update.zip");
            //                                }

            //                                installUpdatesOnExit = true;

            //                                if (result == DialogResult.OK)
            //                                {
            //                                    restartAfterUpdates = true;
            //                                    MainForm?.Close();
            //                                }
            //                            }
            //                        }
            //                        else
            //                        {
            //                            MessageBox.Show("An update is available but the update executable could not be found. If you want to update chatterino you will have to reinstall it.");
            //                        }
            //                    }
            //                }
            //                catch { }
            //            };

            //#if DEBUG
            //            Updates.CheckForUpdate("win-dev", CurrentVersion);
            //#else
            //            Updates.CheckForUpdate("win-release", CurrentVersion);
            //#endif

            // Install updates
            //if (installUpdatesOnExit)
            //{
            //    Process.Start(Path.Combine(new FileInfo(Assembly.GetEntryAssembly().Location).Directory.FullName, "Updater", "Chatterino.Updater.exe"), restartAfterUpdates ? "--restart" : "");
            //    System.Threading.Thread.Sleep(1000);
            //}
        }
    }
}
