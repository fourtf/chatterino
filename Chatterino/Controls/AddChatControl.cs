using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chatterino.Controls
{
    public class AddChatControl : Control
    {
        public AddChatControl()
        {
            SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);

            Cursor = Cursors.Hand;

            Dock = DockStyle.Fill;

            BackColor = (App.ColorScheme.ChatBackground as SolidBrush)?.Color ?? Color.White;

            App.ColorSchemeChanged += (s, e) => BackColor = (App.ColorScheme.ChatBackground as SolidBrush)?.Color ?? Color.White;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            TextRenderer.DrawText(e.Graphics, "Add Chat", Font, new Rectangle(0,0,Width, Height), App.ColorScheme.Text, App.DefaultTextFormatFlags | TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
        }
    }
}
