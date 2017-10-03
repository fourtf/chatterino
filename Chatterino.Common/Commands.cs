using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TwitchIrc;

namespace Chatterino.Common
{
    public static class Commands
    {
        public static ConcurrentDictionary<string, Func<string, TwitchChannel, bool, string>> ChatCommands = new ConcurrentDictionary<string, Func<string, TwitchChannel, bool, string>>();

        public static readonly object CustomCommandsLock = new object();
        public static List<Command> CustomCommands = new List<Command>();

        // static ctor
        static Commands()
        {
            // Chat Commands
            ChatCommands.TryAdd("w", (s, channel, execute) =>
            {
                if (execute)
                {
                    var S = s.SplitWords();

                    if (S.Length > 1)
                    {
                        var name = S[0];

                        IrcMessage message;
                        IrcMessage.TryParse($":{name}!{name}@{name}.tmi.twitch.tv PRIVMSG #whispers :" + s.SubstringFromWordIndex(1), out message);

                        TwitchChannel.WhisperChannel.AddMessage(new Message(message, TwitchChannel.WhisperChannel, isSentWhisper: true));

                        if (AppSettings.ChatEnableInlineWhispers)
                        {
                            var inlineMessage = new Message(message, TwitchChannel.WhisperChannel, true, false, isSentWhisper: true) { HighlightTab = false };

                            inlineMessage.HighlightType = HighlightType.Whisper;

                            foreach (var c in TwitchChannel.Channels)
                            {
                                c.AddMessage(inlineMessage);
                            }
                        }
                    }
                }

                return "/w " + s;
            });

            ChatCommands.TryAdd("ignore", (s, channel, execute) =>
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
            ChatCommands.TryAdd("unignore", (s, channel, execute) =>
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

            ChatCommands.TryAdd("uptime", (s, channel, execute) =>
            {
                if (execute && channel != null)
                {
                    try
                    {
                        var request = WebRequest.Create($"https://api.twitch.tv/kraken/streams/{channel.Name}?client_id={IrcManager.DefaultClientID}");
                        if (AppSettings.IgnoreSystemProxy)
                        {
                            request.Proxy = null;
                        }
                        using (var resp = request.GetResponse())
                        using (var stream = resp.GetResponseStream())
                        {
                            var parser = new JsonParser();

                            dynamic json = parser.Parse(stream);

                            dynamic root = json["stream"];

                            string createdAt = root["created_at"];

                            var streamStart = DateTime.Parse(createdAt);

                            var uptime = DateTime.Now - streamStart;

                            var text = "Stream uptime: ";

                            if (uptime.TotalDays > 1)
                            {
                                text += (int)uptime.TotalDays + " days, " + uptime.ToString("hh\\h\\ mm\\m\\ ss\\s");
                            }
                            else
                            {
                                text += uptime.ToString("hh\\h\\ mm\\m\\ ss\\s");
                            }

                            channel.AddMessage(new Message(text));
                        }
                    }
                    catch { }
                }

                return null;
            });

            ChatCommands.TryAdd("testcheering", (s, channel, execute) =>
            {
                if (execute)
                {
                    foreach (var x in new[] { "1", "100", "1000", "5000", "10000" })
                    {
                        IrcMessage msg;
                        IrcMessage.TryParse($"@badges=subscriber/1;bits={x};color=;display-name=FOURTF;emotes=;mod=0;subscriber=1;turbo=0;user-type= :fourtf!fourtf@fourtf.tmi.twitch.tv PRIVMSG #fourtf :cheer{x} xD donation", out msg);

                        foreach (var c in TwitchChannel.Channels)
                        {
                            var message = new Message(msg, c);
                            c.AddMessage(message);
                        }
                    }
                }

                return null;
            });

            ChatCommands.TryAdd("testchannels", (s, channel, execute) =>
            {
                TwitchChannel.WhisperChannel.AddMessage(new Message(string.Join(", ", TwitchChannel.Channels.Select(x => x.Name))));
                return null;
            });
        }

        // public
        public static string ProcessMessage(string text, TwitchChannel channel, bool executeCommands)
        {
            string _command = null;
            string args = null;

            if (text.Length > 1)
            {
                if (text[0] == '/')
                {
                    var index = text.IndexOf(' ');
                    _command = index == -1 ? text.Substring(1) : text.Substring(1, index - 1);
                    args = index == -1 ? "" : text.Substring(index + 1);
                }
                else if (AppSettings.ChatAllowCommandsAtEnd)
                {
                    var index = text.LastIndexOf(' ');

                    if (index != -1)
                    {
                        var s = text.Substring(index + 1);

                        if (s.Length > 0 && s[0] == '/')
                        {
                            _command = s.Substring(1);
                            args = text.Remove(index);
                        }
                    }
                }
            }

            if (_command != null)
            {
                Func<string, TwitchChannel, bool, string> command;
                if (ChatCommands.TryGetValue(_command, out command))
                {
                    text = command(args, channel, executeCommands);
                }
                else
                {
                    lock (CustomCommandsLock)
                    {
                        foreach (var c in CustomCommands)
                        {
                            if (c.Name == _command)
                            {
                                text = c.Execute(args);
                                break;
                            }
                        }
                    }
                }
            }

            if (text != null)
            {
                text = Emojis.ReplaceShortCodes(text);

                text = Regex.Replace(text, " +", " ");
                text = text.Trim();

                if (AppSettings.ChatAllowSameMessage)
                {
                    if (!executeCommands)
                    {
                        text = text + " ";
                    }
                }

                return text;
            }
            else
            {
                return null;
            }
        }

        // io
        public static void LoadOrDefault(string path)
        {
            lock (CustomCommandsLock)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        using (var reader = new StreamReader(path))
                        {
                            CustomCommands.Clear();

                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                CustomCommands.Add(new Command(line));
                            }
                        }
                    }
                    else
                    {
                        CustomCommands.Add(new Command("/slap /me slaps {1} around a bit with a large trout"));
                        CustomCommands.Add(new Command("/shrug {1+} ¯\\_(ツ)_/¯"));
                        CustomCommands.Add(new Command("/brainpower {1+} O-oooooooooo AAAAE-A-A-I-A-U- JO-oooooooooooo AAE-O-A-A-U-U-A- E-eee-ee-eee AAAAE-A-E-I-E-A- JO-ooo-oo-oo-oo EEEEO-A-AAA-AAAA {1+}"));
                    }
                }
                catch (Exception exc)
                {
                    exc.Message.Log("commands");
                }
            }
        }

        public static void Save(string path)
        {
            try
            {
                using (var writer = new StreamWriter(path))
                {
                    lock (CustomCommandsLock)
                    {
                        foreach (var command in CustomCommands)
                        {
                            writer.WriteLine(command.Raw);
                        }
                    }
                }
            }
            catch { }
        }
    }
}
