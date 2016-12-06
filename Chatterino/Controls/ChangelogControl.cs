using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chatterino.Common;
using System.IO;
using Message = Chatterino.Common.Message;
using System.Windows.Forms;

namespace Chatterino.Controls
{
    public class ChangelogControl : MessageContainerControl
    {
        Message[] messages = new Message[0];

        protected override Message[] Messages
        {
            get
            {
                return messages;
            }
        }

        public ChangelogControl(string md)
        {
            lock (MessageLock)
            {
                messages = Message.ParseMarkdown(md);
            }

            scrollAtBottom = false;

            updateMessageBounds();

            AllowMessageSeperator = false;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                base.OnMouseDown(e);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                base.OnMouseUp(e);
            }
        }
    }
}
