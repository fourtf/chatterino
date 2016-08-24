using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchIrc
{
    public class ExceptionEventArgs : EventArgs
    {
        public Exception Exception { get; private set; }

        public ExceptionEventArgs(Exception exc)
        {
            Exception = exc;
        }
    }
}
