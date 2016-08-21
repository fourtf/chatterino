using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatterino.Desktop.Widgets
{
    public class ScrollEventArgs : EventArgs
    {
        public double OldValue { get; private set; }
        public double NewValue { get; private set; }

        public double Delta
        {
            get
            {
                return NewValue - OldValue;
            }
        }

        public ScrollEventArgs(double oldValue, double newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
