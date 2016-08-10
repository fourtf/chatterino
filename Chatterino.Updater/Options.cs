using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatterino.Updater
{
    class Options
    {
        [Option('r', "restart", Required = false, DefaultValue = false, HelpText = "Restart chatterino.")]
        public bool RestartChatterino { get; set; } = false;
    }
}
