using Chatterino.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatterino.Desktop.Widgets
{
    public class ChatColumn
    {
        public event EventHandler<ValueEventArgs<ChatWidget>> WidgetAdded;
        public event EventHandler<ValueEventArgs<ChatWidget>> WidgetRemoved;

        private List<ChatWidget> widgets = new List<ChatWidget>();

        public int WidgetCount
        {
            get
            {
                return widgets.Count;
            }
        }

        public IEnumerable<ChatWidget> Widgets
        {
            get
            {
                return widgets.AsReadOnly();
            }
        }

        public void AddWidget(ChatWidget widget)
        {
            InsertWidget(widgets.Count, widget);
        }

        public void InsertWidget(int index, ChatWidget widget)
        {
            widgets.Insert(index, widget);

            WidgetAdded?.Invoke(this, new ValueEventArgs<ChatWidget>(widget));
        }

        public void RemoveWidget(ChatWidget widget)
        {
            int index = widgets.FindIndex(x => x == widget);

            if (index == -1)
            {
                throw new ArgumentException("\"widget\" is not a widget in this column.");
            }

            widgets.RemoveAt(index);

            WidgetRemoved?.Invoke(this, new ValueEventArgs<ChatWidget>(widget));
        }
    }
}
