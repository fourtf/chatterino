using Chatterino.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chatterino.Controls
{
    public class ColumnTabPage : TabPage
    {
        private string customTitle = null;

        public string CustomTitle
        {
            get { return customTitle; }
            set
            {
                customTitle = value;

                Title = customTitle ?? defaultTitle;
            }
        }

        private string defaultTitle = "empty";

        public string DefaultTitle
        {
            get { return defaultTitle; }
            set
            {
                defaultTitle = value;

                Title = customTitle ?? defaultTitle;
            }
        }

        // COLUMNS
        public event EventHandler<ValueEventArgs<ChatColumn>> ColumnAdded;
        public event EventHandler<ValueEventArgs<ChatColumn>> ColumnRemoved;

        public ColumnLayoutItem LastSelected { get; set; } = null;

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

        public Tuple<int, int> RemoveWidget(ColumnLayoutItem w)
        {
            if (CanRemoveWidget())
            {
                ChatColumn toRemove = null;

                int c = 0, r;

                foreach (var column in Columns)
                {
                    r = 0;
                    foreach (var row in column.Widgets)
                    {
                        if (row == w)
                        {
                            column.RemoveWidget(w);

                            if (column.WidgetCount == 0)
                            {
                                toRemove = column;
                                r = -1;
                            }
                            goto end;
                        }
                        r++;
                    }
                    c++;
                }
                return Tuple.Create(0, 0);

                end:
                if (toRemove != null)
                    RemoveColumn(toRemove);

                layout();

                return Tuple.Create(c, r);

                //Columns.FirstOrDefault(x => x.Widgets.Contains(w)).Process(col =>
                //{
                //    col.RemoveWidget(w);
                //    if (col.WidgetCount == 0)
                //    {
                //        RemoveColumn(col);
                //    }
                //});
            }
            return null;
        }

        public bool CanRemoveWidget()
        {
            return true;
            //return columns.Count > 1 || ((columns.FirstOrDefault()?.WidgetCount ?? 1) > 1);
        }

        public ChatColumn FindColumn(ColumnLayoutItem w)
        {
            return Columns.FirstOrDefault(x => x.Widgets.Contains(w));
        }

        AddChatControl addChatControl = new AddChatControl();

        // DRAG DROP
        bool dragging = false;

        public ColumnLayoutPreviewItem LayoutPreviewItem { get; set; }

        // MENU
        static ContextMenu menu = new ContextMenu();
        static ColumnTabPage menuPage = null;
        static ColumnLayoutItem menuWidget = null;

        static ColumnTabPage()
        {
            try
            {
                MenuItem item;

                item = new MenuItem { Text = "Remove this Split",/* Image = getImage("Remove_9x_16x.png"),*/ Tag = "rsplit" };
                item.Click += (s, e) =>
                {
                    if (menuWidget != null)
                    {
                        menuPage.RemoveWidget(menuWidget);
                    }
                };
                menu.MenuItems.Add(item);

                item = new MenuItem { Text = "Add Vertical Split", /*Image = getImage("2Columns_16x.png")*/ };
                item.Click += (s, e) =>
                {
                    menuPage?.AddColumn();
                };
                menu.MenuItems.Add(item);

                item = new MenuItem { Text = "Add Horizontal Split", /*Image = getImage("2Rows_16x.png") */ };
                item.Click += (s, e) =>
                {
                    menuPage?.FindColumn(menuWidget).AddWidget(new ColumnLayoutItem());
                };
                menu.MenuItems.Add(item);
            }
            catch (Exception exc)
            {
                Console.WriteLine("error:" + exc);
            }

            Console.WriteLine(menu.MenuItems.Count);
        }

        //static Image getImage(string name)
        //{
        //    try
        //    {
        //        return Image.FromResource("Chatterino.Desktop.Assets." + name);
        //    }
        //    catch { }

        //    return null;
        //}

        // CONSTRUCTOR
        public ColumnTabPage()
        {
            AllowDrop = true;

            LayoutPreviewItem = new ColumnLayoutPreviewItem
            {
                Visible = false
            };
            LayoutPreviewItem.SetBounds(25, 25, 100, 100);
            Controls.Add(LayoutPreviewItem);

            // layout on item added/removed/bounds changed
            ColumnAdded += (s, e) =>
            {
                foreach (ColumnLayoutItem w in e.Value.Widgets)
                {
                    Controls.Add(w);

                    //w.MouseUp += W_ButtonReleased;
                }

                layout();

                e.Value.WidgetAdded += Value_WidgetAdded;
                e.Value.WidgetRemoved += Value_WidgetRemoved;
            };

            ColumnRemoved += (s, e) =>
            {
                foreach (ColumnLayoutItem w in e.Value.Widgets)
                {
                    Controls.Remove(w);

                    //w.MouseUp -= W_ButtonReleased;
                }

                layout();

                e.Value.WidgetAdded -= Value_WidgetAdded;
                e.Value.WidgetRemoved -= Value_WidgetRemoved;
            };

            SizeChanged += (s, e) =>
            {
                layout();

                Invalidate();
            };

            // Drag drop
            Point lastDragPoint = new Point(10000, 10000);

            int dragColumn = -1;
            int dragRow = -1;

            int MaxColumns = 10;
            int MaxRows = 10;

            DragEnter += (s, e) =>
            {
                try
                {
                    var control = (ColumnLayoutDragDropContainer)e.Data.GetData(typeof(ColumnLayoutDragDropContainer));

                    if (control != null)
                    {
                        dragging = true;

                        lastDragPoint = new Point(10000, 10000);

                        e.Effect = e.AllowedEffect;
                        LayoutPreviewItem.Visible = true;
                    }
                }
                catch
                {

                }
            };
            DragLeave += (s, e) =>
            {
                if (dragging)
                {
                    dragging = false;
                    LayoutPreviewItem.Visible = false;
                }
            };
            DragDrop += (s, e) =>
            {
                if (dragging)
                {
                    dragging = false;
                    LayoutPreviewItem.Visible = false;

                    var container = (ColumnLayoutDragDropContainer)e.Data.GetData(typeof(ColumnLayoutDragDropContainer));

                    if (container != null/* && dragColumn != -1*/)
                    {
                        var control = container.Control;

                        AddWidget(control, dragColumn, dragRow);

                        //if (dragRow == -1)
                        //{
                        //    InsertColumn(dragColumn, new ChatColumn(control));
                        //}
                        //else
                        //{
                        //    ChatColumn row = Columns.ElementAt(dragColumn);
                        //    row.InsertWidget(dragRow, control);
                        //}
                    }
                }
            };

            DragOver += (s, e) =>
            {
                if (dragging)
                {
                    var mouse = PointToClient(new Point(e.X, e.Y));

                    if (lastDragPoint != mouse)
                    {
                        lastDragPoint = mouse;
                        int totalWidth = Width;

                        if (ColumnCount == 0)
                        {
                            LayoutPreviewItem.Bounds = new Rectangle(8, 8, Width - 16, Height - 16);
                            LayoutPreviewItem.Visible = true;

                            dragColumn = -1;
                            dragRow = -1;

                            e.Effect = DragDropEffects.Move;
                            LayoutPreviewItem.IsError = false;
                        }
                        else
                        {
                            double columnWidth = (double)totalWidth / ColumnCount;

                            dragColumn = -1;
                            dragRow = -1;

                            // insert new column
                            for (int i = (ColumnCount >= MaxColumns ? 1 : 0); i < (ColumnCount >= MaxColumns ? ColumnCount : ColumnCount + 1); i++)
                            {
                                if (mouse.X > i * columnWidth - columnWidth / 4 &&
                                    mouse.X < i * columnWidth + columnWidth / 4)
                                {
                                    dragColumn = i;

                                    Rectangle bounds = new Rectangle((int)(i * columnWidth - columnWidth / 4), 0, (int)(columnWidth / 2), Height);

                                    if (LayoutPreviewItem.Bounds != bounds)
                                    {
                                        LayoutPreviewItem.Bounds = bounds;
                                        LayoutPreviewItem.Invalidate();
                                    }
                                    break;
                                }
                            }

                            // insert new row
                            if (dragColumn == -1)
                            {
                                for (int i = 0; i < ColumnCount; i++)
                                {
                                    if (mouse.X < (i + 1) * columnWidth)
                                    {
                                        var rows = Columns.ElementAt(i);
                                        double rowHeight = (double)Height / rows.WidgetCount;

                                        for (int j = 0; j < rows.WidgetCount + 1; j++)
                                        {
                                            if (mouse.Y > j * rowHeight - rowHeight / 2 &&
                                                mouse.Y < j * rowHeight + rowHeight / 2)
                                            {
                                                if (rows.WidgetCount < MaxRows)
                                                {
                                                    dragColumn = i;
                                                    dragRow = j;
                                                }

                                                Rectangle bounds = new Rectangle((int)(i * columnWidth), (int)(j * rowHeight - rowHeight / 2), (int)columnWidth, (int)(rowHeight));
                                                if (LayoutPreviewItem.Bounds != bounds)
                                                {
                                                    LayoutPreviewItem.Bounds = bounds;
                                                    LayoutPreviewItem.Invalidate();
                                                }
                                            }
                                        }
                                        break;
                                    }
                                }
                            }

                            LayoutPreviewItem.IsError = dragColumn == -1;
                            e.Effect = dragColumn == -1 ? DragDropEffects.None : DragDropEffects.Move;
                        }
                    }
                }
            };

            // Add chat control
            checkAddChatControl();

            addChatControl.Click += (s, e) =>
            {
                ChatControl chatControl = new ChatControl();

                using (InputDialogForm dialog = new InputDialogForm("channel name") { Value = "" })
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        chatControl.ChannelName = dialog.Value;
                    }
                }

                AddColumn(new ChatColumn(chatControl));
            };
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);

            UpdateDefaultTitle();
            checkAddChatControl();
        }

        protected override void OnControlRemoved(ControlEventArgs e)
        {
            base.OnControlRemoved(e);

            UpdateDefaultTitle();
            checkAddChatControl();
        }

        public void UpdateDefaultTitle()
        {
            string title = "";
            bool any = false;

            foreach (ChatColumn c in columns)
            {
                foreach (var w in c.Widgets)
                {
                    ChatControl chat = w as ChatControl;
                    if (chat != null)
                    {
                        if (!string.IsNullOrWhiteSpace(chat.ChannelName))
                        {
                            title += chat.ChannelName + ", ";
                            any = true;
                        }
                    }
                }
            }

            DefaultTitle = any ? title.TrimEnd(',', ' ') : "empty";
        }

        void checkAddChatControl()
        {
            foreach (Control c in Controls)
            {
                if (c is ColumnLayoutItem)
                {
                    if (addChatControl.Parent != null)
                    {
                        Controls.Remove(addChatControl);
                    }

                    return;
                }
            }

            if (addChatControl.Parent == null)
            {
                Controls.Add(addChatControl);
            }
        }

        //private void W_ButtonReleased(object sender, MouseEventArgs e)
        //{
        //    if (e.Button == MouseButtons.Right)
        //    {
        //        menuPage = this;
        //        menuWidget = (ColumnLayoutItem)sender;

        //        foreach (MenuItem x in menu.MenuItems)
        //        {
        //            if (x.Tag == "rsplit")
        //            {
        //                x.Enabled = CanRemoveWidget();
        //            }
        //        }

        //        menu.Show((Control)sender, new Point(0, ((Control)sender).Height));
        //    }
        //}

        private void Value_WidgetAdded(object sender, ValueEventArgs<ColumnLayoutItem> e)
        {
            Controls.Add(e.Value);
            //e.Value.MouseUp += W_ButtonReleased;

            layout();
        }

        private void Value_WidgetRemoved(object sender, ValueEventArgs<ColumnLayoutItem> e)
        {
            Controls.Remove(e.Value);
            //e.Value.MouseUp -= W_ButtonReleased;

            layout();
        }

        private void layout()
        {
            if (Bounds.Width > 0 && Bounds.Height > 0 && ColumnCount != 0)
            {
                int columnHeight = (Bounds.Height - Padding.Top - Padding.Bottom - 1);
                double columnWidth = (Bounds.Width - Padding.Left - Padding.Right - 1) / ColumnCount;
                double x = 0;

                for (int i = 0; i < ColumnCount; i++)
                {
                    ChatColumn col = columns[i];
                    if (col.WidgetCount > 0)
                    {
                        double rowHeight = columnHeight / col.WidgetCount;

                        double y = 0;

                        foreach (ColumnLayoutItem w in col.Widgets)
                        {
                            w.SetBounds((int)(Padding.Left + x), (int)(Padding.Top + y), (int)columnWidth, (int)rowHeight);

                            y += rowHeight + 1;
                        }
                    }
                    x += columnWidth + 1;
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(App.ColorScheme.ChatSeperator);

            base.OnPaint(e);
        }

        public void AddWidget(ColumnLayoutItem control, int column = -1, int row = -1)
        {
            if (row == -1)
            {
                if (column == -1)
                    AddColumn(new ChatColumn(control));
                else
                    InsertColumn(column, new ChatColumn(control));
            }
            else
            {
                if (column == ColumnCount)
                    AddColumn(new ChatColumn());
                Columns.ElementAt(column).InsertWidget(row, control);
            }
            Controls.Add(control);
        }
    }
}
