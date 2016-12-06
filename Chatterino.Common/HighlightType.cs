using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatterino.Common
{
    [Flags]
    public enum HighlightType
    {
        None, 
        Highlighted = 1,
        Resub = 2,
        Whisper = 4,
        SearchResult = 8,
    }
}
