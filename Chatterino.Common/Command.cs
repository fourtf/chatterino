using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatterino.Common
{
    public class Command
    {
        public string Name { get; set; }
        public string Raw { get; set; }

        object[] objects;

        public Command(string command)
        {
            Raw = command;
            var index = command.IndexOf(" ");

            if (index == -1)
            {
                Name = command;
            }
            else
            {
                Name = command.Remove(index).TrimStart('/');

                command = command.Substring(index + 1).Trim();

                var objects = new List<object>();

                var builder = new StringBuilder();

                for (var i = 0; i < command.Length; i++)
                {
                    if (command[i] == '{')
                    {
                        var j = i + 1;

                        var number = "";
                        var allAfter = false;

                        for (; j < command.Length; j++)
                        {
                            if (command[j] >= '0' && command[j] <= '9')
                            {
                                number += command[j];
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (j < command.Length && command[j] == '+')
                        {
                            allAfter = true;
                            j++;
                        }

                        if (j < command.Length && command[j] == '}' && int.TryParse(number, out index) && index > 0)
                        {
                            i = j;

                            if (builder.Length != 0)
                            {
                                objects.Add(builder.ToString());
                                builder.Clear();
                            }

                            objects.Add(new WordSelection { StartIndex = index - 1, After = allAfter });
                        }
                        else
                        {
                            builder.Append('{');
                        }
                    }
                    else
                    {
                        builder.Append(command[i]);
                    }
                }

                if (builder.Length != 0)
                {
                    objects.Add(builder.ToString());
                }

                this.objects = objects.ToArray();
            }
        }

        public string Execute(string message)
        {
            var S = message.SplitWords();

            var text = "";

            foreach (var o in objects)
            {
                var s = o as string;

                if (s != null)
                {
                    text += s;
                    continue;
                }

                var selection = o as WordSelection;

                if (selection != null)
                {
                    if (selection.StartIndex < S.Length)
                    {
                        if (selection.After)
                        {
                            text += message.SubstringFromWordIndex(selection.StartIndex);
                        }
                        else
                        {
                            text += S[selection.StartIndex];
                        }
                    }
                }
            }

            return text;
        }

        class WordSelection
        {
            public int StartIndex { get; set; }
            public bool After { get; set; }
        }
    }
}
