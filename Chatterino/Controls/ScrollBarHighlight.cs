using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatterino.Controls
{
    public class ScrollBarHighlight
    {
        public double Position { get; set; }
        public Color Color { get; set; }
        public ScrollBarHighlightStyle Style { get; set; }
        public object Tag { get; set; }

        public ScrollBarHighlight(double position, Color color, ScrollBarHighlightStyle style = ScrollBarHighlightStyle.Default, object tag = null)
        {
            Position = position;
            Color = color;
            Style = style;
            Tag = tag;
        }
    }

    public enum ScrollBarHighlightStyle
    {
        Default,
        Right,
        SingleLine
    }
}
