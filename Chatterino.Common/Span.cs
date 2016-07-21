using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatterino.Common
{
    public class Span
    {
        public SpanType Type { get; set; }
        public object Value { get; set; }
        public int? Color { get; set; }
        public string Link { get; set; }

        public FontType Font { get; set; }
        public int Height { get; set; } = 16;
        public int Width { get; set; } = 16;
        public int X { get; set; }
        public int Y { get; set; }

        public Tuple<string, CommonRectangle>[] SplitSegments { get; set; } = null;
    }

    public enum SpanType
    {
        Text,
        Emote,
        Image
    }
}
