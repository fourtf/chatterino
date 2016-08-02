using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatterino.Common
{
    public class ChatClearedEventArgs : EventArgs
    {
        public string Channel { get; private set; }
        public string User { get; private set; }
        public string Message { get; private set; }
        public int Duration { get; private set; }
        public string Reason { get; private set; }

        public ChatClearedEventArgs(string user, string reason, int duration)
        {
            User = user;
            Duration = duration;
            Reason = reason;

            Message = $"{user} was timed out for {duration} second{(duration == 1 ? "" : "s")}{(string.IsNullOrEmpty(reason) ? "." : ": " + reason)}";
        }
    }
}
