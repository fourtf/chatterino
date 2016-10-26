using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatterino.Common
{
    public class MessageInputLogic
    {
        public event EventHandler Changed;

        // properties
        public TwitchChannel Channel { get; set; }

        public Message Message { get; private set; } = null;

        public Selection Selection
        {
            get
            {
                return new Selection(Message.PositionFromIndex(SelectionStart), Message.PositionFromIndex(SelectionStart + SelectionLength));
            }
        }

        private string text = "";
        public string Text
        {
            get { return text; }
            private set
            {
                text = value;
                Message = new Message(value);

                string sendmessage = Commands.ProcessMessage(value, Channel, false) ?? "";

                int messageLength = 0;
                for (int j = 0; j < sendmessage.Length; j++)
                {
                    messageLength++;

                    if (char.IsHighSurrogate(sendmessage[j]))
                        j += 1;
                }

                MessageLength = messageLength;

                invokeChanged();
            }
        }

        public int MessageLength { get; private set; } = 0;

        public void SetText(string text)
        {
            Text = text;
            SelectionStart = CaretPosition = Text.Length;
            SelectionLength = 0;
            invokeChanged();
        }

        public int CaretPosition { get; private set; } = 0;
        public int SelectionStart { get; set; } = 0;
        public int SelectionLength { get; set; } = 0;

        // public functions
        public void InsertText(string text)
        {
            removeSelection();

            Text = Text.Insert(CaretPosition, text);

            CaretPosition = CaretPosition + text.Length;
            SelectionLength = 0;
            SelectionStart = CaretPosition;

            invokeChanged();
        }

        public void SetCaretPosition(int position)
        {
            CaretPosition = SelectionStart = position;
            SelectionLength = 0;

            invokeChanged();
        }

        public void ClearSelection()
        {
            SelectionStart = CaretPosition;
            SelectionLength = 0;

            invokeChanged();
        }

        public void SelectAll()
        {
            SelectionStart = 0;
            SelectionLength = Text.Length;

            invokeChanged();
        }

        public void Clear()
        {
            SelectionLength = SelectionStart = CaretPosition = 0;
            Text = "";

            invokeChanged();
        }

        public void MoveCursorLeft(bool ctrl, bool selecting)
        {
            if (ctrl)
            {
                if (CaretPosition != 0)
                {
                    var text = Text;

                    CaretPosition = findNextCtrlPositionLeft();
                }
            }
            else
            {
                if (CaretPosition != 0)
                {
                    CaretPosition--;
                }
            }

            if (selecting)
            {
                SelectionLength = CaretPosition - SelectionStart;
            }
            else
            {
                SelectionStart = CaretPosition;
                SelectionLength = 0;
            }

            invokeChanged();
        }

        public void MoveCursorRight(bool ctrl, bool selecting)
        {
            if (ctrl)
            {
                var text = Text;
                if (CaretPosition <= text.Length)
                {
                    CaretPosition = findNextCtrlPositionRight();
                }
            }
            else
            {
                if (CaretPosition < Text.Length)
                {
                    CaretPosition++;
                }
            }

            if (selecting)
            {
                SelectionLength = CaretPosition - SelectionStart;
            }
            else
            {
                SelectionStart = CaretPosition;
                SelectionLength = 0;
            }

            invokeChanged();
        }

        public void SetSelectionEnd(int position)
        {
            SelectionLength = -SelectionStart + position;
            CaretPosition = position;

            invokeChanged();
        }

        public void Delete(bool ctrl, bool forward)
        {
            if (!removeSelection())
            {
                if (ctrl)
                {
                    int start, length;

                    if (forward)
                    {
                        if (CaretPosition > text.Length)
                            return;

                        start = CaretPosition;
                        length = findNextCtrlPositionRight() - start;
                    }
                    else
                    {
                        if (CaretPosition == 0)
                            return;

                        start = findNextCtrlPositionLeft();
                        length = CaretPosition - start;

                        SelectionStart = CaretPosition = start;
                    }

                    Text = (start < Text.Length ? Text.Remove(start) : Text) + (start + length < Text.Length ? Text.Substring(start + length) : "");
                }
                else
                {
                    if (forward)
                    {
                        if (CaretPosition <= text.Length)
                        {
                            Text = (CaretPosition < Text.Length ? Text.Remove(CaretPosition) : Text) + (CaretPosition + 1 < Text.Length ? Text.Substring(CaretPosition + 1) : "");
                        }
                    }
                    else
                    {
                        if (CaretPosition != 0)
                        {
                            CaretPosition--;
                            SelectionStart--;
                            Text = (CaretPosition < Text.Length ? Text.Remove(CaretPosition) : Text) + (CaretPosition + 1 < Text.Length ? Text.Substring(CaretPosition + 1) : "");
                        }
                    }
                }
            }
            invokeChanged();
        }

        public string SelectedText
        {
            get
            {
                if (SelectionLength == 0)
                    return "";

                if (SelectionLength > 0)
                    return Text.Substring(SelectionStart, SelectionLength);
                else
                    return Text.Substring(SelectionStart + SelectionLength, -SelectionLength);
            }
        }

        // private helpers
        private void invokeChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }

        private bool removeSelection()
        {
            if (SelectionLength == 0)
                return false;

            int start, length;

            if (SelectionLength > 0)
            {
                start = SelectionStart;
                length = SelectionLength;
            }
            else
            {
                start = SelectionStart + SelectionLength;
                length = -SelectionLength;
            }

            SelectionLength = 0;
            SelectionStart = start;
            CaretPosition = start;

            Text = (start < Text.Length ? Text.Remove(start) : Text) + (start + length < Text.Length ? Text.Substring(start + length) : "");

            return true;
        }

        private int findNextCtrlPositionLeft()
        {
            int i = CaretPosition - 1;

            for (; i >= 0; i--)
            {
                var c = text[i];

                if (!(c == ' ' || (c >= '[' && c <= '`') || (c >= '{' && c <= '~') || (c >= ':' && c <= '@') || (c >= '!' && c <= '/')))
                {
                    break;
                }
            }

            for (; i >= 0; i--)
            {
                var c = text[i];

                if (c == ' ' || (c >= '[' && c <= '`') || (c >= '{' && c <= '~') || (c >= ':' && c <= '@') || (c >= '!' && c <= '/'))
                {
                    i++;
                    break;
                }
            }

            i = i > 0 ? i : 0;

            return i;
        }

        private int findNextCtrlPositionRight()
        {
            int i = CaretPosition;

            for (; i < text.Length; i++)
            {
                var c = text[i];

                if (c == ' ' || (c >= '[' && c <= '`') || (c >= '{' && c <= '~') || (c >= ':' && c <= '@') || (c >= '!' && c <= '/'))
                {
                    break;
                }
            }

            for (; i < text.Length; i++)
            {
                var c = text[i];

                if (!(c == ' ' || (c >= '[' && c <= '`') || (c >= '{' && c <= '~') || (c >= ':' && c <= '@') || (c >= '!' && c <= '/')))
                {
                    break;
                }
            }

            return i;
        }
    }
}
