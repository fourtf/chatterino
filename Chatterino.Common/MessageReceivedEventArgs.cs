using Meebey.SmartIrc4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatterino.Common
{
    public class MessageEventArgs : EventArgs
    {
        public Message Message { get; private set; }

        public MessageEventArgs(Message message)
        {
            Message = message;
        }
    }
}
