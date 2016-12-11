using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chatterino.Common;

namespace Chatterino
{
    public class GifEmoteState
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public List<Word> Words { get; set; }
        public bool Selected { get; set; }
        public HighlightType HighlightType { get; set; }
        public bool Disabled { get; set; }
        public int MessageYOffset { get; set; }
        public int MessageXOffset { get; set; }

        public GifEmoteState(int x, int y, int width, int height, List<Word> words, bool selected, HighlightType highlightType, bool disabled, int messageYOffset, int messageXOffset)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Words = words;
            Selected = selected;
            HighlightType = highlightType;
            Disabled = disabled;
            MessageYOffset = messageYOffset;
            MessageXOffset = messageXOffset;
        }
    }
}
