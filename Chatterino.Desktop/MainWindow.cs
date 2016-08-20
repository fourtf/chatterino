using Chatterino.Desktop.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xwt;

namespace Chatterino.Desktop
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            Title = "Chatterino";

            var tabs = new TabControl();

            Content = tabs;

            tabs.AddTab(new ChatTabPage() { Title = "pajlada, forsenlol" });
            tabs.AddTab(new ChatTabPage() { Title = "forsenlol" });
            tabs.AddTab(new ChatTabPage() { Title = "nuuls, gempir" });
            tabs.AddTab(new ChatTabPage() { Title = "raysfire" });
            tabs.AddTab(new ChatTabPage() { Title = "5" });
            tabs.AddTab(new ChatTabPage() { Title = "6" });
            tabs.AddTab(new ChatTabPage() { Title = "7" });
            tabs.AddTab(new ChatTabPage() { Title = "8" });
            tabs.AddTab(new ChatTabPage() { Title = "9" });
            tabs.AddTab(new ChatTabPage() { Title = "10" });
            tabs.AddTab(new ChatTabPage() { Title = "11" });

            Padding = new WidgetSpacing(0, 0, 0, 0);

            Size = new Size(600, 500);
        }
    }
}
