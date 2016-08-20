using Chatterino.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xwt;
using Xwt.Drawing;

namespace Chatterino.Desktop.Widgets
{
    public class ChatTabPage : TabPage
    {
        // COLUMNS
        public event EventHandler<ValueEventArgs<ChatColumn>> ColumnAdded;
        public event EventHandler<ValueEventArgs<ChatColumn>> ColumnRemoved;

        public int ColumnCount
        {
            get
            {
                return columns.Count;
            }
        }

        public IEnumerable<ChatColumn> Columns
        {
            get
            {
                return columns.AsReadOnly();
            }
        }

        private List<ChatColumn> columns = new List<ChatColumn>();

        public ChatColumn AddColumn()
        {
            var col = new ChatColumn();

            col.AddWidget(new ChatWidget());
            AddColumn(col);

            return col;
        }

        public void AddColumn(ChatColumn column)
        {
            InsertColumn(columns.Count, column);
        }

        public void InsertColumn(int index, ChatColumn column)
        {
            columns.Insert(index, column);

            ColumnAdded?.Invoke(this, new ValueEventArgs<ChatColumn>(column));
        }

        public void RemoveColumn(ChatColumn column)
        {
            int index = columns.FindIndex(x => x == column);

            if (index == -1)
            {
                throw new ArgumentException("\"widget\" is not a widget in this column.");
            }

            columns.RemoveAt(index);

            ColumnRemoved?.Invoke(this, new ValueEventArgs<ChatColumn>(column));
        }

        public ChatColumn FindColumn(ChatWidget w)
        {
            return Columns.FirstOrDefault(x => x.Widgets.Contains(w));
        }

        // MENU
        static Menu menu = new Menu();
        static ChatTabPage menuPage = null;
        static ChatWidget menuWidget = null;

        static ChatTabPage()
        {
            MenuItem item;

            item = new MenuItem { Label = "Add Vertical Split", Image = Image.FromResource("Chatterino.Desktop.Assets.2Columns_16x.png") };
            item.Clicked += (s, e) =>
            {
                menuPage?.AddColumn();
            };
            menu.Items.Add(item);

            item = new MenuItem { Label = "Add Horizontal Split", Image = Image.FromResource("Chatterino.Desktop.Assets.2Rows_16x.png") };
            item.Clicked += (s, e) =>
            {
                menuPage?.FindColumn(menuWidget).AddWidget(new ChatWidget());
            };
            menu.Items.Add(item);
        }

        // CONSTRUCTOR
        public ChatTabPage()
        {
            // layout on item added/removed/bounds changed
            ColumnAdded += (s, e) =>
            {
                foreach (ChatWidget w in e.Value.Widgets)
                {
                    AddChild(w);

                    w.ButtonReleased += W_ButtonReleased;
                }

                layout();

                e.Value.WidgetAdded += Value_WidgetAdded;
                e.Value.WidgetRemoved += Value_WidgetRemoved;
            };

            ColumnRemoved += (s, e) =>
            {
                foreach (ChatWidget w in e.Value.Widgets)
                {
                    RemoveChild(w);

                    w.ButtonReleased -= W_ButtonReleased;
                }

                layout();

                e.Value.WidgetAdded -= Value_WidgetAdded;
                e.Value.WidgetRemoved -= Value_WidgetRemoved;
            };

            BoundsChanged += (s, e) =>
            {
                layout();
            };

            // add first column
            var col = new ChatColumn();
            col.AddWidget(new ChatWidget());
            AddColumn(col);
        }

        private void W_ButtonReleased(object sender, ButtonEventArgs e)
        {
            if (e.Button == PointerButton.Right)
            {
                menuPage = this;
                menuWidget = (ChatWidget)sender;

                menu.Popup((Widget)sender, e.Position.X, e.Position.Y);
            }
        }

        private void Value_WidgetAdded(object sender, ValueEventArgs<ChatWidget> e)
        {
            AddChild(e.Value);
            e.Value.ButtonReleased += W_ButtonReleased;

            layout();
        }

        private void Value_WidgetRemoved(object sender, ValueEventArgs<ChatWidget> e)
        {
            RemoveChild(e.Value);
            e.Value.ButtonReleased -= W_ButtonReleased;

            layout();
        }

        private void layout()
        {
            if (Bounds.Width > 0 && Bounds.Height > 0)
            {
                int columnHeight = (int)(Bounds.Height - Padding.Top - Padding.Bottom);
                double columnWidth = (Bounds.Width - Padding.Left - Padding.Right) / ColumnCount;
                double x = 0;

                for (int i = 0; i < ColumnCount; i++)
                {
                    ChatColumn col = columns[i];
                    double rowHeight = columnHeight / col.WidgetCount;

                    double y = 0;

                    foreach (ChatWidget w in col.Widgets)
                    {
                        SetChildBounds(w, new Rectangle((int)(Padding.Left + x), (Padding.Top + y), (int)columnWidth, (int)rowHeight));

                        y += rowHeight;
                    }

                    x += columnWidth;
                }
            }
        }
    }
}
