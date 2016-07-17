using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatterino
{
    public class Span
    {
        public SpanType Type { get; set; }
        public object Value { get; set; }
        public Color? Color { get; set; }
        public string Link { get; set; }

        public int Height { get; set; }
        public int Width { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }

    public enum SpanType
    {
        Text,
        SmallText,
        Emote,
        Image
    }
}
