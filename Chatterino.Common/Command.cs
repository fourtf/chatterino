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
            int index = command.IndexOf(" ");

            if (index == -1)
            {
                Name = command;
            }
            else
            {
                Name = command.Remove(index).TrimStart('/');

                command = command.Substring(index + 1).Trim();

                List<object> objects = new List<object>();

                StringBuilder builder = new StringBuilder();

                for (int i = 0; i < command.Length; i++)
                {
                    if (command[i] == '{')
                    {
                        int j = i + 1;

                        string number = "";
                        bool allAfter = false;

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
            string[] S = message.SplitWords();

            string text = "";

            foreach (var o in objects)
            {
                string s = o as string;

                if (s != null)
                {
                    text += s;
                    continue;
                }

                WordSelection selection = o as WordSelection;

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
