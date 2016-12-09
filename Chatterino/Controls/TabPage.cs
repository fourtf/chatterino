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
        private string _title;

        public string Title
        {
            get { return _title; }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    TitleChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private bool _selected;

        public bool Selected
        {
            get { return _selected; }
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    Invalidate();
                }
            }
        }

        public event EventHandler<ValueEventArgs<TabPageHighlightType>> HighlightTypeChanged;

        private TabPageHighlightType _highlightType;

        public TabPageHighlightType HighlightType
        {
            get { return _highlightType; }
            set
            {
                if (!Selected)
                {
                    if (_highlightType != value)
                    {
                        _highlightType = value;
                        HighlightTypeChanged?.Invoke(this, new ValueEventArgs<TabPageHighlightType>(value));
                    }
                }

                _highlightType = value;
            }
        }

        public bool EnableNewMessageHighlights { get; set; } = true;

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
