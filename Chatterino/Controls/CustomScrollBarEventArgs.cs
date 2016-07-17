using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chatterino.Controls
{
    public class CustomScrollBarEventArgs : EventArgs
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

        public CustomScrollBarEventArgs(double oldValue, double newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
