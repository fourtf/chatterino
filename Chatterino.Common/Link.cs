using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatterino.Common
{
    public class Link
    {
        public LinkType Type { get; set; }
        public object Value { get; set; }

        public Link(LinkType type, object value)
        {
            Type = type;
            Value = value;
        }
    }
}
