/*using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chatterino.Common
{
    public static class IrcManager2
    {
        // public properties
        public static string Username { get; private set; }

        public static event EventHandler<MessageEventArgs> MessageReceived;
        public static event EventHandler<ChatClearedEventArgs> ChatCleared;
        public static event EventHandler Disconnected;
        public static event EventHandler Connected;

        // networking
        static TcpClient readClient = null;
        static NetworkStream readStream = null;
        static StreamReader readReader = null;

        static TcpClient writeClient = null;
        static NetworkStream writeStream = null;

        // connected -> client, stream, reader != null
        static bool readConnected = false;
        static bool readConnecting = false;

        static bool writeConnected = false;
        static bool writeConnecting = false;

        //Timer timer = new Timer()

        public static void Connect(string username, string oauth)
        {
            if (!readConnected && !readConnecting)
            {
                Task.Run(() =>
                {
                    readConnecting = true;

                    try
                    {
                        readClient = new TcpClient("irc.chat.twitch.tv", 6667);
                        readStream = readClient.GetStream();
                        read

                    }
                    catch { }
                });
            }

            if (!readConnecting)
            {
                readConnecting = true;

                // dispose old tcpclient
                readReader?.Dispose();
                readReader = null;
                readStream?.Dispose();
                readStream = null;
                try
                {
                    readClient?.Close();
                }
                catch { }
                readClient = null;

                // connect
                readClient = new TcpClient()


                readConnected = true;
                readConnecting = false;
            }
        }

        public static void Disconnect()
        {
            Disconnected?.Invoke(null, EventArgs.Empty);
        }

        public static void SendMessage(string channel, string message)
        {
            if (channel != null)
            {
                if (message.Length > 1 && message[0] == '/')
                {
                    int index = message.IndexOf(' ');
                    string _command = index == -1 ? message.Substring(1) : message.Substring(1, index - 1);
                    message = index == -1 ? "" : message.Substring(index + 1);

                    Func<string, string> command;
                    if (ChatCommands.TryGetValue(_command, out command))
                    {
                        message = command(message) ?? message;
                    }
                }

                if (AppSettings.ChatAllowSameMessage)
                {
                    message = message + " ";
                }

                IrcWriteClient?.SendMessage(SendType.Message, "#" + channel.TrimStart('#'), Common.Emojis.ReplaceShortCodes(message));
            }
        }
    }
}
*/