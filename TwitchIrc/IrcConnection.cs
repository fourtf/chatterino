using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TwitchIrc
{
    public class IrcConnection : IDisposable
    {
        // public properties
        public event EventHandler<MessageEventArgs> MessageReceived;
        public event EventHandler Connected;
        public event EventHandler Disconnected;
        public event EventHandler<ExceptionEventArgs> ConnectionException;

        public bool IsConnected { get; private set; }

        // private variables
        private bool connecting = false;

        private bool receivedPong = true;

        private TcpClient client;
        private NetworkStream stream;

        private string username, password;

        // constructor
        public IrcConnection()
        {
            pingTimer.Elapsed += pingTimer_Elapsed;
        }

        // public methods
        public void Connect(string username, string password)
        {
            this.username = username;
            this.password = password;

            connect();
        }

        public void WriteLine(string value)
        {
            if (IsConnected)
            {
                writeLine(stream, value);
            }
        }

        public void Dispose()
        {
            pingTimer.Elapsed -= pingTimer_Elapsed;
            pingTimer.Dispose();

            client?.Close();
        }

        public void Reconnect()
        {
            connect();
        }

        // private methods
        private void connect()
        {
            IsConnected = false;

            if (connecting) return;

            if (client != null)
            {
                client.Close();
                client = null;
            }

            try
            {
                client = new TcpClient
                {
                    NoDelay = true,
                    ReceiveBufferSize = 8192,
                    SendBufferSize = 8192
                };

                client.Connect("irc.chat.twitch.tv", 6667);

                stream = client.GetStream();

                var reader = new StreamReader(stream);

                var messageQueue = new ConcurrentQueue<IrcMessage>();
                var messageQueueAddedEvent = new AutoResetEvent(false);

                new Thread(() =>
                {
                    try
                    {
                        string line;

                        while ((line = reader.ReadLine()) != null)
                        {
#if DEBUG
                            System.Diagnostics.Debug.WriteLine(line);
#endif
                            IrcMessage msg;

                            if (IrcMessage.TryParse(line, out msg))
                            {
                                if (msg.Command == "PING")
                                {
                                    WriteLine("PONG");
                                }
                                else if (msg.Command == "PONG")
                                {
                                    receivedPong = true;
                                }

                                messageQueue.Enqueue(msg);
                                messageQueueAddedEvent.Set();
                            }
                        }
                    }
                    catch { }
                }).Start();

                new Thread(() =>
                {
                    bool inQueue = false;

                    IrcMessage message;

                    while (true)
                    {
                        messageQueueAddedEvent.WaitOne();

                        while (messageQueue.TryDequeue(out message))
                        {
                            int count = messageQueue.Count;

                            if (messageQueue.Count > 50)
                            {
                                IrcMessage.TryParse($"@system-msg=ignored\\s{count}\\smessages USERNOTICE #{message.Middle}", out message);

                                IrcMessage nil;
                                for (int i = 0; i < count; i++)
                                {
                                    messageQueue.TryDequeue(out nil);
                                }

                                MessageReceived?.Invoke(this, new MessageEventArgs(message));
                            }
                            else
                            {
                                MessageReceived?.Invoke(this, new MessageEventArgs(message));
                            }
                        }
                    }
                }).Start();

                if (!string.IsNullOrEmpty(password))
                {
                    writeLine(stream, "PASS " + password);
                }

                writeLine(stream, "NICK " + username);

                writeLine(stream, "CAP REQ :twitch.tv/commands");
                writeLine(stream, "CAP REQ :twitch.tv/tags");

                receivedPong = true;
                IsConnected = true;

                Connected?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception exc)
            {
                ConnectionException?.Invoke(this, new ExceptionEventArgs(exc));
            }
        }

        private void writeLine(Stream stream, string value)
        {
            if (stream != null)
            {
                var bytes = Encoding.UTF8.GetBytes(value);

                stream.Write(bytes, 0, bytes.Length);
                stream.WriteByte((byte)'\r');
                stream.WriteByte((byte)'\n');
                stream.Flush();
            }
        }

        private void pingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (IsConnected)
            {
                if (receivedPong)
                {
                    receivedPong = false;

                    writeLine(stream, "PING");
                }
                else
                {
                    Disconnected?.Invoke(this, EventArgs.Empty);

                    connect();
                }
            }
        }

        // static
        private static System.Timers.Timer pingTimer = new System.Timers.Timer { Enabled = true, Interval = 12000 };
    }
    
    static class MessageTrimmer
    {
        private static string last;
        public static string TrimAll(this string s, bool isMod)
        {
            return s.StartsWith(".") || s.StartsWith("/") ? s : s + (isMod || (last = last == s ? null : s) != null ? "" : " ⁭");
        }
    }
}
