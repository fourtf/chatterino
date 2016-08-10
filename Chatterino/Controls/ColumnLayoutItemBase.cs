using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Chatterino.Controls
{
    public class ColumnLayoutItemBase : Control
    {
        static Random random = new Random();

        public ColumnLayoutItemBase()
        {
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Selectable, true);

            // Focus
            GotFocus += (s, e) => { Invalidate(); };
            LostFocus += (s, e) => { Invalidate(); };
            MouseDown += (s, e) =>
            {
                //if (e.Button == MouseButtons.Left)
                    Select();
            };

            // Mousedown
            bool mouseDown = false;

            MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                    mouseDown = true;
            };
            MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                    mouseDown = false;
            };

            // Drag + Drop
            MouseMove += (s, e) =>
            {
                if (mouseDown)
                {
                    if (e.X < 0 || e.Y < 0 || e.X > Width || e.Y > Height)
                    {
                        ColumnLayoutControl layout = Parent as ColumnLayoutControl;
                        if (layout != null)
                        {
                            var position = layout.RemoveFromGrid(this);
                            OnSplitDragStart();
                            if (DoDragDrop(new ColumnLayoutDragDropContainer { Control = this }, DragDropEffects.Move) == DragDropEffects.None)
                            {
                                layout.AddToGrid(this, position.Item1, position.Item2);
                            }
                        }
                    }
                }
            };

            //BackColor = Color.FromArgb(-16777216 | (0x404040 | 0x808080 | random.Next(0xFFFFFF)));
        }

        protected virtual void OnSplitDragStart()
        {

        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(App.ColorScheme.ChatBackground, e.ClipRectangle);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //e.Graphics.DrawRectangle(Focused ? Pens.Red : Pens.Black, 0, 0, Width - 1, Height - 1);
        }
    }
}
