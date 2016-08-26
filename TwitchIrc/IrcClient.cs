using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchIrc
{
    public class IrcClient
    {
        public IrcConnection ReadConnection { get; private set; }
        public IrcConnection WriteConnection { get; private set; }

        public bool SingleConnection { get; private set; }

        // ratelimiting
        static ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();
        static Queue<DateTime> lastMessagesPleb = new Queue<DateTime>();
        static Queue<DateTime> lastMessagesMod = new Queue<DateTime>();

        static object lastMessagesLock = new object();

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

        public bool Say(string message, string channel, bool isMod)
        {
            lock (lastMessagesLock)
            {
                var now = DateTime.Now;

                while (lastMessagesMod.Count > 0 && lastMessagesMod.Peek() < now)
                {
                    lastMessagesMod.Dequeue();
                }

                while (lastMessagesPleb.Count > 0 && lastMessagesPleb.Peek() < now)
                {
                    lastMessagesPleb.Dequeue();
                }

                if (isMod)
                {
                    if (lastMessagesMod.Count < 100)
                    {
                        WriteConnection.WriteLine("PRIVMSG #" + channel + " :" + message);

                        lastMessagesMod.Enqueue(now + TimeSpan.FromSeconds(30));
                        lastMessagesPleb.Enqueue(now + TimeSpan.FromSeconds(30));
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (lastMessagesPleb.Count < 20)
                    {
                        WriteConnection.WriteLine("PRIVMSG #" + channel + " :" + message);

                        lastMessagesPleb.Enqueue(now + TimeSpan.FromSeconds(30));
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
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
