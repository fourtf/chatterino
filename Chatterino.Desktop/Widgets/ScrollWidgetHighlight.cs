using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xwt.Drawing;

namespace Chatterino.Desktop.Widgets
{
    public class ScrollBarHighlight
    {
        public double Position { get; set; }
        public Color Color { get; set; }
        public double Height { get; set; }

        public ScrollBarHighlight(double position, Color color, double height = 1)
        {
            Position = position;
            Color = color;
            Height = height;
        }
    }
}
