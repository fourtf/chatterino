using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Chatterino.Controls
{
    public class ColumnLayoutControl : Control
    {
        public ColumnLayoutControl()
        {
            AllowDrop = true;

            LayoutPreviewItem = new ColumnLayoutPreviewItem
            {
                Visible = false
            };
            LayoutPreviewItem.SetBounds(25, 25, 100, 100);
            Controls.Add(LayoutPreviewItem);

            // drag and drop
            bool dragging = false;

            int dragColumn = -1;
            int dragRow = -1;

            Point lastDragPoint = new Point(10000, 10000);
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

                    if (container != null && dragColumn != -1)
                    {
                        var control = container.Control;
                        if (dragRow == -1)
                        {
                            List<Control> row = new List<Control>
                            {
                                control
                            };
                            Columns.Insert(dragColumn, row);
                            Controls.Add(control);
                        }
                        else
                        {
                            List<Control> row = Columns[dragColumn];
                            row.Insert(dragRow, control);
                            Controls.Add(control);
                        }
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

                        double columnWidth = (double)totalWidth / Columns.Count;

                        dragColumn = -1;
                        dragRow = -1;

                        // insert new column
                        for (int i = (Columns.Count >= MaxColumns ? 1 : 0); i < (Columns.Count >= MaxColumns ? Columns.Count : Columns.Count + 1); i++)
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
                            for (int i = 0; i < Columns.Count; i++)
                            {
                                if (mouse.X < (i + 1) * columnWidth)
                                {
                                    var rows = Columns[i];
                                    double rowHeight = (double)Height / rows.Count;

                                    for (int j = 0; j < rows.Count + 1; j++)
                                    {
                                        if (mouse.Y > j * rowHeight - rowHeight / 2 &&
                                            mouse.Y < j * rowHeight + rowHeight / 2)
                                        {
                                            if (rows.Count < MaxRows)
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
            };

            // redraw layout preview
            SizeChanged += (s, e) =>
            {
                if (LayoutPreviewItem.Visible)
                    LayoutPreviewItem.Invalidate();
            };
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int totalWidth = ClientRectangle.Width;
            int totalHeight = ClientRectangle.Height;
            double x = ClientRectangle.X;
            double columnWidth = (double)totalWidth / Columns.Count;


            foreach (var column in Columns)
            {
                double y = ClientRectangle.Y;
                double rowHeight = (double)totalHeight / column.Count;

                foreach (var row in column)
                {
                    row.Location = new Point((int)x, (int)y);
                    row.Size = new Size((int)(x + columnWidth) - (int)x, (int)(y + rowHeight) - (int)y);
                    y += rowHeight;
                }

                x += columnWidth;
            }
        }

        public void AddToGrid(Control control, int column = -1, int row = -1)
        {
            if (row == -1)
            {
                if (column == -1)
                    Columns.Add(new List<Control> { control });
                else
                    Columns.Insert(column, new List<Control> { control });
            }
            else
            {
                if (column == Columns.Count)
                    Columns.Add(new List<Control>());
                Columns[column].Insert(row, control);
            }
            Controls.Add(control);
        }

        public Tuple<int, int> RemoveFromGrid(Control control)
        {
            List<Control> toRemove = null;

            int c = 0, r;

            foreach (var column in Columns)
            {
                r = 0;
                foreach (var row in column)
                {
                    if (row == control)
                    {
                        column.Remove(control);
                        Controls.Remove(control);

                        if (column.Count == 0)
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
                Columns.Remove(toRemove);

            PerformLayout();

            return Tuple.Create(c, r);
        }

        public void ClearGrid()
        {
            foreach (Control c in Columns.SelectMany(x => x))
            {
                Controls.Remove(c);
                c.Dispose();
            }

            Columns.Clear();
        }

        public ColumnLayoutPreviewItem LayoutPreviewItem { get; set; }

        public List<List<Control>> Columns = new List<List<Control>>();

        public int MaxColumns { get; set; } = 4;
        public int MaxRows { get; set; } = 4;
    }
}
