using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatterino.Controls
{
    public struct ScrollBarHighlight
    {
        public double Position { get; private set; }
        public double Height { get; set; }
        public Color Color { get; private set; }

        public ScrollBarHighlight(double position, double height, Color color)
        {
            Position = position;
            Height = height;
            Color = color;
        }
    }
}
