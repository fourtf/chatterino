using Chatterino.Common;
using Chatterino.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatterino
{
    public class ChatColumn
    {
        public event EventHandler<ValueEventArgs<ColumnLayoutItem>> WidgetAdded;
        public event EventHandler<ValueEventArgs<ColumnLayoutItem>> WidgetRemoved;

        private List<ColumnLayoutItem> widgets = new List<ColumnLayoutItem>();

        public int WidgetCount
        {
            get
            {
                return widgets.Count;
            }
        }

        public IEnumerable<ColumnLayoutItem> Widgets
        {
            get
            {
                return widgets.AsReadOnly();
            }
        }

        public void AddWidget(ColumnLayoutItem widget)
        {
            InsertWidget(widgets.Count, widget);
        }

        public void InsertWidget(int index, ColumnLayoutItem widget)
        {
            widgets.Insert(index, widget);

            WidgetAdded?.Invoke(this, new ValueEventArgs<ColumnLayoutItem>(widget));
        }

        public void RemoveWidget(ColumnLayoutItem widget)
        {
            var index = widgets.FindIndex(x => x == widget);

            if (index == -1)
            {
                throw new ArgumentException("\"widget\" is not a widget in this column.");
            }

            widgets.RemoveAt(index);

            WidgetRemoved?.Invoke(this, new ValueEventArgs<ColumnLayoutItem>(widget));
        }

        public ChatColumn()
        {

        }

        public ChatColumn(ColumnLayoutItem item)
        {
            widgets.Add(item);
        }
    }
}
