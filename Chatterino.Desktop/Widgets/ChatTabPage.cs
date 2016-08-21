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
        public bool HasCustomTitle { get; set; } = false;

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

        public void RemoveWidget(ChatWidget w)
        {
            if (CanRemoveWidget())
            {
                Columns.FirstOrDefault(x => x.Widgets.Contains(w)).Process(col =>
                {
                    col.RemoveWidget(w);
                    if (col.WidgetCount == 0)
                    {
                        RemoveColumn(col);
                    }
                });
            }
        }

        public bool CanRemoveWidget()
        {
            return columns.Count > 1 || ((columns.FirstOrDefault()?.WidgetCount ?? 1) > 1);
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
            try
            {
                MenuItem item;

                item = new MenuItem { Label = "Remove this Split", Image = getImage("Remove_9x_16x.png"), Tag = "rsplit" };
                item.Clicked += (s, e) =>
                {
                    if (menuWidget != null)
                    {
                        menuPage.RemoveWidget(menuWidget);
                    }
                };
                menu.Items.Add(item);

                item = new MenuItem { Label = "Add Vertical Split", Image = getImage("2Columns_16x.png") };
                item.Clicked += (s, e) =>
                {
                    menuPage?.AddColumn();
                };
                menu.Items.Add(item);

                item = new MenuItem { Label = "Add Horizontal Split", Image = getImage("2Rows_16x.png") };
                item.Clicked += (s, e) =>
                {
                    menuPage?.FindColumn(menuWidget).AddWidget(new ChatWidget());
                };
                menu.Items.Add(item);
            }
            catch (Exception exc)
            {
                Console.WriteLine("error:" + exc);
            }

            Console.WriteLine(menu.Items.Count);
        }

        static Image getImage(string name)
        {
            try
            {
                return Image.FromResource("Chatterino.Desktop.Assets." + name);
            }
            catch { }

            return null;
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

                QueueDraw();
            };
        }

        private void W_ButtonReleased(object sender, ButtonEventArgs e)
        {
            if (e.Button == PointerButton.Right)
            {
                menuPage = this;
                menuWidget = (ChatWidget)sender;

                menu.Items.FirstOrDefault(m => m.Tag.Equals("rsplit")).Sensitive = CanRemoveWidget();
                var b = GetChildBounds((Widget)sender);
                menu.Popup(this, e.Position.X + b.X, e.Position.Y + b.Y);
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
                    if (col.WidgetCount > 0)
                    {
                        double rowHeight = columnHeight / col.WidgetCount;

                        double y = 0;

                        foreach (ChatWidget w in col.Widgets)
                        {
                            SetChildBounds(w, new Rectangle((int)(Padding.Left + x), (Padding.Top + y), (int)columnWidth, (int)rowHeight));

                            y += rowHeight;
                        }

                    }
                    x += columnWidth;
                }
            }
        }
    }
}
