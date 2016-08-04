using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatterino.Common
{
    public class Selection
    {
        public MessagePosition Start { get; set; }
        public MessagePosition End { get; set; }
        public MessagePosition First { get; set; }
        public MessagePosition Last { get; set; }

        public Selection(MessagePosition start, MessagePosition end)
        {
            if (end.MessageIndex < start.MessageIndex || (end.MessageIndex == start.MessageIndex &&
                (end.WordIndex < start.WordIndex || (end.WordIndex == start.WordIndex &&
                (end.SplitIndex < start.SplitIndex || (end.SplitIndex == start.SplitIndex &&
                end.CharIndex < start.CharIndex))))))
            {
                First = end;
                Last = start;
            }
            else
            {
                First = start;
                Last = end;
            }

            Start = start;
            End = end;
        }

        public bool Equals(Selection other)
        {
            return other.Start.MessageIndex == Start.MessageIndex
                && other.Start.WordIndex == Start.WordIndex
                && other.Start.SplitIndex == Start.SplitIndex
                && other.Start.CharIndex == Start.CharIndex
                && other.End.MessageIndex == End.MessageIndex
                && other.End.WordIndex == End.WordIndex
                && other.End.SplitIndex == End.SplitIndex
                && other.End.CharIndex == End.CharIndex;
        }

        public bool IsEmpty
        {
            get
            {
                return !(Start.MessageIndex != End.MessageIndex ||
                    Start.WordIndex != End.WordIndex ||
                    Start.CharIndex != End.CharIndex);
            }
        }
    }

    public struct MessagePosition
    {
        public int MessageIndex { get; set; }
        public int WordIndex { get; set; }
        public int SplitIndex { get; set; }
        public int CharIndex { get; set; }

        public MessagePosition(int messageIndex, int wordIndex, int splitIndex, int charIndex)
        {
            MessageIndex = messageIndex;
            WordIndex = wordIndex;
            SplitIndex = splitIndex;
            CharIndex = charIndex;
        }

        public MessagePosition WithMessageIndex(int v)
        {
            return new MessagePosition(v, WordIndex, SplitIndex, CharIndex);
        }
    }
}
