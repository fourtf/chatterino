using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TwitchIrc;

namespace Chatterino.Common
{
    public static class Commands
    {
        public static ConcurrentDictionary<string, Func<string, bool, string>> ChatCommands = new ConcurrentDictionary<string, Func<string, bool, string>>();

        // static ctor
        static Commands()
        {
            // Chat Commands
            ChatCommands.TryAdd("shrug", (s, execute) => ". " + s + " ¯\\_(ツ)_/¯");
            ChatCommands.TryAdd("brainpower", (s, execute) => ". " + s + " O-oooooooooo AAAAE-A-A-I-A-U- JO-oooooooooooo AAE-O-A-A-U-U-A- E-eee-ee-eee AAAAE-A-E-I-E-A- JO-ooo-oo-oo-oo EEEEO-A-AAA-AAAA " + s);

            ChatCommands.TryAdd("ignore", (s, execute) =>
            {
                if (execute)
                {
                    var S = s.SplitWords();
                    if (S.Length > 0)
                    {
                        IrcManager.AddIgnoredUser(S[0]);
                    }
                }
                return null;
            });
            ChatCommands.TryAdd("unignore", (s, execute) =>
            {
                if (execute)
                {
                    var S = s.SplitWords();
                    if (S.Length > 0)
                    {
                        IrcManager.RemoveIgnoredUser(S[0]);
                    }
                }
                return null;
            });

            ChatCommands.TryAdd("cheertest312", (s, execute) =>
            {
                if (execute)
                {
                    foreach (string x in new[] { "1", "100", "1000", "5000", "10000" })
                    {
                        IrcMessage msg;
                        IrcMessage.TryParse($"@badges=subscriber/1;bits={x};color=;display-name=FOURTF;emotes=;mod=0;subscriber=1;turbo=0;user-type= :fourtf!fourtf@fourtf.tmi.twitch.tv PRIVMSG #fourtf :cheer{x} xD donation", out msg);

                        foreach (TwitchChannel c in TwitchChannel.Channels)
                        {
                            Message message = new Message(msg, c);
                            c.AddMessage(message);
                        }
                    }
                }

                return null;
            });


        }

        // public
        public static string ProcessMessage(string text, bool executeCommands)
        {
            if (text.Length > 1 && text[0] == '/')
            {
                int index = text.IndexOf(' ');
                string _command = index == -1 ? text.Substring(1) : text.Substring(1, index - 1);
                var args  = index == -1 ? "" : text.Substring(index + 1);

                Func<string, bool, string> command;
                if (ChatCommands.TryGetValue(_command, out command))
                {
                    text = command(args, executeCommands);
                }
            }

            if (text != null)
            {
                text = Emojis.ReplaceShortCodes(text);

                text = Regex.Replace(text, " +", " ");
                text = text.Trim();

                if (AppSettings.ChatAllowSameMessage)
                {
                    text = text + " ";
                }

                return text;
            }
            else
            {
                return null;
            }

            //var message = _message;

            //if (_message.Length > 1 && _message[0] == '/')
            //{
            //    int index = _message.IndexOf(' ');
            //    string _command = index == -1 ? _message.Substring(1) : _message.Substring(1, index - 1);
            //    _message = index == -1 ? "" : _message.Substring(index + 1);

            //    Func<string, string> command;
            //    if (ChatCommands.TryGetValue(_command, out command))
            //    {
            //        message = command(_message) ?? message;
            //    }
            //}

            //if (message != null)
            //{
            //    if (AppSettings.ChatAllowSameMessage)
            //    {
            //        message = message + " ";
            //    }
            //}
        }
    }
}
