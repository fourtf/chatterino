using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchIrc
{
    public class IrcMessage
    {
        private string _tags = null;
        private Dictionary<string, string> tags = null;

        public Dictionary<string, string> Tags
        {
            get
            {
                if (tags == null)
                {
                    tags = new Dictionary<string, string>();

                    if (_tags != null)
                    {
                        var keyBuilder = new StringBuilder();
                        var valueBuilder = new StringBuilder();

                        var text = _tags;

                        for (var i = 0; i < text.Length; i++)
                        {
                            var c = text[i];

                            if (c == '=')
                            {
                                i++;

                                for (; i < text.Length; i++)
                                {
                                    c = text[i];

                                    if (c == '\\' && i + 1 < text.Length)
                                    {
                                        c = text[++i];

                                        if (c == 's')
                                        {
                                            valueBuilder.Append(' ');
                                        }
                                        else if (c == 'n')
                                        {
                                            valueBuilder.Append('\n');
                                        }
                                        else if (c == 'r')
                                        {
                                            valueBuilder.Append('\r');
                                        }
                                        else if (c == '\\')
                                        {
                                            valueBuilder.Append('\\');
                                        }
                                        else if (c == ':')
                                        {
                                            valueBuilder.Append(';');
                                        }
                                        else
                                        {
                                            valueBuilder.Append(c);
                                        }
                                    }
                                    else if (c == ';')
                                    {
                                        tags[keyBuilder.ToString()] = valueBuilder.ToString();
                                        keyBuilder.Clear();
                                        valueBuilder.Clear();

                                        break;
                                    }
                                    else
                                    {
                                        valueBuilder.Append(c);
                                    }
                                }
                            }
                            else if (c == ';')
                            {
                                tags[keyBuilder.ToString()] = "";
                                keyBuilder.Clear();
                            }
                            else
                            {
                                keyBuilder.Append(c);
                            }
                        }

                        if (keyBuilder.Length != 0)
                        {
                            tags[keyBuilder.ToString()] = valueBuilder.ToString();
                        }
                    }
                }

                return tags;
            }
        }

        public string RawPrefix { get; private set; }

        public string PrefixNickname { get; private set; }
        public string PrefixServer { get; private set; }
        public string PrefixUser { get; private set; }
        public string PrefixHost { get; private set; }

        public string Command { get; private set; }
        public string Middle { get; private set; }
        public string Params { get; private set; }

        private IrcMessage(string tags, string prefix, string command, string channel, string @params)
        {
            _tags = tags;

            // prefix
            RawPrefix = prefix;

            // servername / ( nickname [ [ "!" user ] "@" host ] )
            if (prefix != null)
            {
                var at = prefix.IndexOf('@');

                // servername
                if (at == -1 && prefix.IndexOf('.') != -1)
                {
                    PrefixServer = prefix;
                }
                // nickname[ ["!" user] "@" host ]
                else
                {
                    // nickname
                    if (at == -1)
                    {
                        PrefixNickname = prefix;
                    }
                    // nickname ["!" user] "@" host
                    else
                    {
                        var exclamation = prefix.IndexOf('!');

                        // nickname "@" host
                        if (exclamation == -1)
                        {
                            PrefixNickname = prefix.Remove(at);
                            PrefixHost = prefix.Substring(at + 1);
                        }
                        // nickname "!" user "@" host
                        else if (exclamation < at)
                        {
                            PrefixNickname = prefix.Remove(exclamation);
                            PrefixUser = prefix.Substring(exclamation + 1, at - exclamation - 1);
                            PrefixHost = prefix.Substring(at + 1);
                        }
                    }
                }
            }

            Command = command;
            Middle = channel;
            Params = @params;
        }

        public static bool TryParse(string line, out IrcMessage message)
        {
            string tags = null, prefix = null, command = null, middle = null, @params = null;

            int i = 0, end;

            // tags
            if (i >= line.Length) goto error;

            if (line[i] == '@')
            {
                i++;
                end = line.IndexOf(' ', i);

                if (end == -1) goto error;

                tags = line.Substring(i, end - i);
                i = end + 1;
            }

            // prefix
            if (i >= line.Length) goto error;

            if (line[i] == ':')
            {
                i++;
                end = line.IndexOf(' ', i);

                if (end == -1) goto error;

                prefix = line.Substring(i, end - i);
                i = end + 1;
            }

            // command
            if (i >= line.Length) goto error;

            end = line.IndexOf(' ', i);

            if (end == -1)
                end = line.Length;

            if (end == i) goto error;

            command = line.Substring(i, end - i);
            i = end + 1;

            // params
            if (i < line.Length)
            {
                if (line[i] != ':')
                {
                    end = line.IndexOf(' ', i);

                    if (end == -1)
                        end = line.Length;

                    middle = line.Substring(i, end - i);
                    i = end + 1;
                }

                if (i < line.Length)
                {
                    if (line[i] == ':')
                    {
                        @params = line.Substring(i + 1);
                    }
                }
            }

            message = new IrcMessage(tags, prefix, command, middle, @params);
            return true;

            error:
            message = null;
            return false;
        }
    }
}
