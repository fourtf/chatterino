using Chatterino.Common;
using Chatterino.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Chatterino
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            LoadLayout("./layout.xml");

            BackColor = Color.Black;

            KeyPreview = true;

            SetTitle();

            Activated += (s, e) =>
            {
                App.EnableTooltips = true;
            };

            Deactivate += (s, e) =>
            {
                App.EnableTooltips = false;
                App.ToolTip?.Hide();
            };

            IrcManager.Connected += (s, e) => SetTitle();

            StartPosition = FormStartPosition.Manual;
            Location = new Point(AppSettings.WindowX, AppSettings.WindowY);
            Size = new Size(AppSettings.WindowWidth, AppSettings.WindowHeight);
        }

        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, Keys keyData)
        {
            if (keyData == Keys.Tab || keyData == (Keys.Shift | Keys.Tab))
            {
                (App.MainForm?.SelectedControl as ChatControl)?.HandleTabCompletion((keyData & Keys.Shift) != Keys.Shift);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        public void SetTitle()
        {
            Text = $"{IrcManager.Username ?? "<not logged in>"} - Chatterino for Twitch";
        }

        //protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        //{
        //    base.OnPreviewKeyDown(e);
        //}

        public Control SelectedControl
        {
            get
            {
                return columnLayoutControl1.Columns.SelectMany(x => x).FirstOrDefault(x => x.Focused);
            }
        }

        public ColumnLayoutControl ColumnLayout
        {
            get { return columnLayoutControl1; }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Modifiers == Keys.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.T:
                        AddNewSplit();
                        break;
                    case Keys.W:
                        RemoveSelectedSplit();
                        break;
                    case Keys.R:
                        {
                            var focused = columnLayoutControl1.Columns.SelectMany(x => x).FirstOrDefault(x => x.Focused) as ChatControl;
                            if (focused != null)
                            {
                                using (InputDialogForm dialog = new InputDialogForm("channel name") { Value = focused.ChannelName })
                                {
                                    if (dialog.ShowDialog() == DialogResult.OK)
                                    {
                                        focused.ChannelName = dialog.Value;
                                    }
                                }
                            }
                        }
                        break;
                    case Keys.P:
                        App.ShowSettings();
                        break;
                    case Keys.C:
                        (App.MainForm?.SelectedControl as ChatControl)?.CopySelection();
                        break;
                    case Keys.V:
                        try
                        {
                            if (Clipboard.ContainsText())
                            {
                                (App.MainForm?.SelectedControl as ChatControl)?.PasteText(Clipboard.GetText());
                            }
                        }
                        catch { }
                        break;
                    case Keys.L:
                        new LoginForm().ShowDialog();
                        break;
                }
            }
        }

        public void AddNewSplit()
        {
            var chat = new ChatControl();

            columnLayoutControl1.AddToGrid(chat);
            chat.Select();

            RenameSelectedSplit();
        }

        public void RemoveSelectedSplit()
        {
            Control focused = columnLayoutControl1.Columns.SelectMany(x => x).FirstOrDefault(x => x.Focused);
            if (focused != null)
            {
                columnLayoutControl1.RemoveFromGrid(focused);
                focused.Dispose();
            }
        }

        public void RenameSelectedSplit()
        {
            ChatControl focused = columnLayoutControl1.Columns.SelectMany(x => x).FirstOrDefault(x => x.Focused) as ChatControl;
            if (focused != null)
            {
                using (InputDialogForm dialog = new InputDialogForm("channel name") { Value = focused.ChannelName })
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        focused.ChannelName = dialog.Value;
                    }
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            SaveLayout("./layout.xml");

            AppSettings.WindowX = Location.X;
            AppSettings.WindowY = Location.Y;
            AppSettings.WindowWidth = Width;
            AppSettings.WindowHeight = Height;

            //timeEndPeriod(timerAccuracy);

            base.OnClosing(e);
        }

        public void LoadLayout(string path)
        {
            columnLayoutControl1.ClearGrid();

            try
            {
                if (File.Exists(path))
                {
                    XDocument doc = XDocument.Load(path);
                    doc.Root.Process(root =>
                    {
                        int columnIndex = 0;
                        root.Elements("column").Do(xD =>
                        {
                            int rowIndex = 0;
                            xD.Elements().Do(x =>
                            {
                                switch (x.Name.LocalName)
                                {
                                    case "chat":
                                        if (x.Attribute("type").Value == "twitch")
                                        {
                                            AddChannel(x.Attribute("channel").Value, columnIndex, rowIndex == 0 ? -1 : rowIndex);
                                        }
                                        break;
                                }
                                rowIndex++;
                            });
                            columnIndex++;
                        });
                    });
                }
            }
            catch { }

            if (columnLayoutControl1.Columns.Count == 0)
                AddChannel("fourtf");
        }

        public void SaveLayout(string path)
        {
            try
            {
                XDocument doc = new XDocument();
                XElement root = new XElement("layout");
                doc.Add(root);

                foreach (var column in columnLayoutControl1.Columns)
                {
                    root.Add(new XElement("column").With(xcol =>
                    {
                        foreach (var row in column)
                        {
                            var chat = row as ChatControl;

                            if (chat != null)
                            {
                                xcol.Add(new XElement("chat").With(x =>
                                {
                                    x.SetAttributeValue("type", "twitch");
                                    x.SetAttributeValue("channel", chat.ChannelName ?? "");
                                }));
                            }
                        }
                    }));
                }

                doc.Save(path);
            }
            catch { }
        }

        public void AddChannel(string name, int column = -1, int row = -1)
        {
            columnLayoutControl1.AddToGrid(new ChatControl { ChannelName = name }, column, row);
        }
    }
}
