using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatterino.Common
{
    [Flags]
    public enum MessageBadges : byte
    {
        None = 0,
        Mod = 1,
        Turbo = 2,
        Sub = 4,
        Staff = 8,
        GlobalMod = 16,
        Admin = 32,
        Broadcaster = 64,
    }
}
