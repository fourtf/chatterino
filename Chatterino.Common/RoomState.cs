using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatterino.Common
{
    [Flags]
    public enum RoomState
    {
        None = 0,
        SubOnly = 1,
        SlowMode = 2,
        EmoteOnly = 4,
        R9k = 8,
    }
}
