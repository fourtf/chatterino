using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchIrc
{
    public class MessageEventArgs : EventArgs
    {
        public IrcMessage Message { get; private set; }

        public MessageEventArgs(IrcMessage message)
        {
            Message = message;
        }
    }
}
