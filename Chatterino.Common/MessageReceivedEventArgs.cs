using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatterino.Common
{
    public class MessageAddedEventArgs : EventArgs
    {
        public Message Message { get; private set; }
        public Message RemovedMessage { get; private set; }

        public MessageAddedEventArgs(Message message, Message removedMessage)
        {
            Message = message;
            RemovedMessage = removedMessage;
        }
    }
}
