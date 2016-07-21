using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatterino.Common
{
    public struct CommonPoint
    {
        public int X;
        public int Y;

        public CommonPoint(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public struct CommonSize
    {
        public int Width;
        public int Height;

        public CommonSize(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }

    public struct CommonRectangle
    {
        public int X;
        public int Y;

        public int Width;
        public int Height;

        public CommonRectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public bool Contains(CommonPoint point)
        {
            return X < point.X && Y < point.Y && X + Width > point.X && Y + Height > point.Y;
        }
    }
}
