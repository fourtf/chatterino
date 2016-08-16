using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatterino.Common
{
    public class IrcClient
    {
        public IrcConnection ReadConnection { get; private set; }
        public IrcConnection WriteConnection { get; private set; }

        public bool SingleConnection { get; private set; }

        public IrcClient(bool singleConnection = false)
        {
            SingleConnection = singleConnection;

            ReadConnection = new IrcConnection();

            if (singleConnection)
            {
                WriteConnection = ReadConnection;
            }
            else
            {
                WriteConnection = new IrcConnection();
            }
        }

        public void Connect(string username, string password)
        {
            ReadConnection.Connect(username, password);

            if (!SingleConnection)
            {
                WriteConnection.Connect(username, password);
            }
        }

        public void Say(string message, string channel)
        {
            WriteConnection.WriteLine("PRIVMSG #" + channel + " :" + message);
        }

        public void WriteLine(string value)
        {
            WriteConnection.WriteLine(value);
        }

        public void Disconnect()
        {

        }

        public void Join(string channel)
        {
            ReadConnection.WriteLine("JOIN " + channel);

            if (!SingleConnection)
                WriteConnection.WriteLine("JOIN " + channel);
        }

        public void Part(string channel)
        {
            ReadConnection.WriteLine("PART " + channel);

            if (!SingleConnection)
                WriteConnection.WriteLine("PART " + channel);
        }
    }
}
