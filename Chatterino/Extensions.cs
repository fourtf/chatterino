using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatterino
{
    public static class Extensions
    {
        public static void Invoke(this System.Windows.Forms.Control c, Action action)
        {
            if (c.InvokeRequired)
            {
                c.Invoke((System.Windows.Forms.MethodInvoker)delegate { action(); });
            }
            else
            {
                action();
            }
        }
    }
}
