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
        public double Height { get; set; }
        public object Tag { get; set; }

        public ScrollBarHighlight(double position, Color color, double height = 1, object tag = null)
        {
            Position = position;
            Color = color;
            Height = height;
            Tag = tag;
        }
    }
}
