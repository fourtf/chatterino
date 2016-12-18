using Chatterino.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chatterino.Controls
{
    public class EmoteListPopup : Form
    {
        EmoteList container;

        public EmoteListPopup()
        {
            Icon = App.Icon;

            Text = "Emotes - Chatterino";

            //TopMost = true;

            container = new EmoteList();
            container.Dock = DockStyle.Fill;

            Controls.Add(container);

            Width = 500;
            Height = 600;

            TopMost = AppSettings.WindowTopMost;

            AppSettings.WindowTopMostChanged += (s, e) =>
            {
                TopMost = AppSettings.WindowTopMost;
            };
        }

        public void SetChannel(TwitchChannel channel)
        {
            Text = (channel == null ? "Global" : (channel.Name + "'" + (channel.Name.EndsWith("s") ? "" : "s"))) + " Emotes - Chatterino";

            container.LoadChannel(channel);
        }
    }
}
