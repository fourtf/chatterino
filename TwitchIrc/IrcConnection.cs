using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
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

        public bool IsConnected { get; private set; } = false;

        // private variables
        private bool connecting = false;

        private bool receivedPong = true;

        private TcpClient client = null;
        private NetworkStream stream = null;

        private string username, password = null;

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

            if (!connecting)
            {
                if (client != null)
                {
                    client.Close();
                    client = null;
                }

                try
                {
                    client = new TcpClient();
                    client.NoDelay = true;
                    client.ReceiveBufferSize = 8096;
                    client.SendBufferSize = 8096;
                    client.Connect("irc.chat.twitch.tv", 6667);

                    stream = client.GetStream();

                    StreamReader reader = new StreamReader(stream);

                    new Task(() =>
                    {
                        try
                        {
                            string line;

                            while ((line = reader.ReadLine()) != null)
                            {
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

                                    MessageReceived?.Invoke(this, new MessageEventArgs(msg));
                                }
                            }
                        }
                        catch { }
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
        }

        private void writeLine(Stream stream, string value)
        {
            if (stream != null)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(value);

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
        private static System.Timers.Timer pingTimer = new System.Timers.Timer { Enabled = true, Interval = 15000 };
    }
}
