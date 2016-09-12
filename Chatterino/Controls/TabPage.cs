using Chatterino.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chatterino.Controls
{
    public class TabPage : Control
    {
        private string title;

        public string Title
        {
            get { return title; }
            set
            {
                if (title != value)
                {
                    title = value;
                    TitleChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private bool selected;

        public bool Selected
        {
            get { return selected; }
            set
            {
                if (selected != value)
                {
                    selected = value;
                    Invalidate();
                }
            }
        }

        public event EventHandler<ValueEventArgs<bool>> HighlightedChanged;

        private bool highlighted = false;

        public bool Highlighted
        {
            get { return highlighted; }
            set
            {
                if (!value || !Selected)
                {
                    if (highlighted != value)
                    {
                        highlighted = value;
                        HighlightedChanged?.Invoke(this, new ValueEventArgs<bool>(value));
                    }
                }
            }
        }

        public event EventHandler TitleChanged;

        public TabPage()
        {
            Padding = new Padding(0, 4, 0, 0);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(App.ColorScheme.TabSelectedBG, 0, 0, Width, 2);
        }
    }
}
