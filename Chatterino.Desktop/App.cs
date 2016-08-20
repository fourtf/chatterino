using Chatterino.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xwt;

namespace Chatterino.Desktop
{
    public static class App
    {
        public static MainWindow Window { get; private set; }
        public static ColorScheme ColorScheme { get; private set; }

        public static void Run(ToolkitType toolkit)
        {
            Application.Initialize(ToolkitType.Wpf);

            // GuiEngine
            if (GuiEngine.Current == null)
            {
                GuiEngine.Initialize(new XwtGuiEngine());
            }

            // Color Scheme
            ColorScheme = new ColorScheme();

            // Main Window
            Window = new MainWindow();
            Window.Closed += (s, e) => Application.Exit();
            Window.Show();

            // Start the main loop
            Application.Run();
        }
    }
}
