using Chatterino.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xwt;

namespace Chatterino.Windows
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            App.Run(ToolkitType.Wpf);
        }
    }
}
