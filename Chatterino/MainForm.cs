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

            Icon = App.Icon;

            if (!File.Exists("./login.ini"))
            {
                using (var login = new LoginForm())
                {
                    login.ShowDialog();
                }
            }

            LoadLayout("./layout.xml");

#if !DEBUG
            if (AppSettings.CurrentVersion != App.CurrentVersion.ToString())
            {
                AppSettings.CurrentVersion = App.CurrentVersion.ToString();

                ShowChangelog();
            }
#endif

            BackColor = Color.Black;

            KeyPreview = true;

            SetTitle();

            Activated += (s, e) =>
            {
                App.WindowFocused = true;
            };

            Deactivate += (s, e) =>
            {
                App.WindowFocused = false;
                App.HideToolTip();
            };

            IrcManager.LoggedIn += (s, e) => SetTitle();

            StartPosition = FormStartPosition.Manual;
            Location = new Point(AppSettings.WindowX, AppSettings.WindowY);
            Size = new Size(AppSettings.WindowWidth, AppSettings.WindowHeight);

            tabControl.TabPageSelected += (s, e) => { (e.Value as ColumnTabPage)?.Columns.FirstOrDefault()?.Widgets.FirstOrDefault()?.Focus(); };
        }

        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, Keys keyData)
        {
            var chatControl = (App.MainForm?.Selected as ChatControl);

            if (keyData == Keys.Tab || keyData == (Keys.Shift | Keys.Tab))
            {
                chatControl?.HandleTabCompletion((keyData & Keys.Shift) != Keys.Shift);
                return true;
            }
            else if (((keyData & ~(Keys.Control | Keys.Shift)) == Keys.Left) || ((keyData & ~(Keys.Control | Keys.Shift)) == Keys.Right)
                || keyData == Keys.Up || keyData == Keys.Down)
            {
                chatControl?.HandleArrowKey(keyData);

                return true;
            }
            else if (keyData == (Keys.Control | Keys.A))
            {
                chatControl?.Input.Logic.SelectAll();

                return true;
            }
            else if (keyData == Keys.Back || keyData == (Keys.Back | Keys.Control) || keyData == (Keys.Back | Keys.Shift) || keyData == Keys.Delete || keyData == (Keys.Delete | Keys.Control) || keyData == (Keys.Delete | Keys.Shift))
            {
                chatControl?.Input.Logic.Delete((keyData & Keys.Control) == Keys.Control, (keyData & ~Keys.Control) == Keys.Delete);
            }
            else if (keyData == Keys.Home)
                (App.MainForm?.Selected as ChatControl).Process(c => c.Input.Logic.SetCaretPosition(0));
            else if (keyData == (Keys.Home | Keys.Shift))
                (App.MainForm?.Selected as ChatControl).Process(c => c.Input.Logic.SetSelectionEnd(0));
            else if (keyData == Keys.End)
                (App.MainForm?.Selected as ChatControl).Process(c => c.Input.Logic.SetCaretPosition(c.Input.Logic.Text.Length));
            else if (keyData == (Keys.End | Keys.Shift))
                (App.MainForm?.Selected as ChatControl).Process(c => c.Input.Logic.SetSelectionEnd(c.Input.Logic.Text.Length));

            return base.ProcessCmdKey(ref msg, keyData);
        }

        public void SetTitle()
        {
            this.Invoke(() => Text = $"{IrcManager.Username ?? "<not logged in>"} - Chatterino for Twitch (v" + App.CurrentVersion.ToString()
#if DEBUG
            + " dev"
#endif
            + ")"
            );
        }

        public ColumnLayoutItem Selected
        {
            get
            {
                return tabControl.TabPages.SelectMany(a => ((ColumnTabPage)a).Columns.SelectMany(b => b.Widgets)).FirstOrDefault(c => c.Focused);
            }
            set
            {
                value.Focus();
            }
        }

        public Controls.TabControl TabControl
        {
            get
            {
                return tabControl;
            }
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
                            ChatControl focused = Selected as ChatControl;
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
                        (App.MainForm?.Selected as ChatControl)?.CopySelection(false);
                        break;
                    case Keys.X:
                        (App.MainForm?.Selected as ChatControl)?.CopySelection(true);
                        break;
                    case Keys.V:
                        try
                        {
                            if (Clipboard.ContainsText())
                            {
                                (App.MainForm?.Selected as ChatControl)?.PasteText(Clipboard.GetText());
                            }
                        }
                        catch { }
                        break;
                    case Keys.L:
                        new LoginForm().ShowDialog();
                        break;
                }
            }
            else if (e.Modifiers == Keys.None)
            {
                switch (e.KeyCode)
                {
                    case Keys.Home:

                        break;
                    case Keys.End:

                        break;
                }
            }
        }

        public void AddNewSplit()
        {
            ChatControl chatControl = new ChatControl();

            (tabControl.Selected as ColumnTabPage)?.AddColumn()?.Process(col =>
            {
                col.AddWidget(chatControl);
                col.Widgets.FirstOrDefault()?.Focus();
            });

            RenameSelectedSplit();
        }

        public void RemoveSelectedSplit()
        {
            var selected = Selected;

            if (selected != null)
            {
                ChatColumn column = null;
                ColumnTabPage _page = null;

                foreach (ColumnTabPage page in tabControl.TabPages.Where(x => x is ColumnTabPage))
                {
                    foreach (var c in page.Columns)
                    {
                        if (c.Widgets.Contains(selected))
                        {
                            _page = page;
                            column = c;
                            break;
                        }
                    }
                }

                if (column != null)
                {
                    column.RemoveWidget(selected);

                    if (column.WidgetCount == 0)
                        _page.RemoveColumn(column);
                }
            }
        }

        public void RenameSelectedSplit()
        {
            ChatControl focused = Selected as ChatControl;
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

            base.OnClosing(e);
        }

        public void LoadLayout(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    XDocument doc = XDocument.Load(path);

                    doc.Root.Process(root =>
                    {
                        foreach (var tab in doc.Elements().First().Elements("tab"))
                        {
                            Console.WriteLine("tab");

                            ColumnTabPage page = new ColumnTabPage();

                            page.CustomTitle = tab.Attribute("title")?.Value;

                            foreach (var col in tab.Elements("column"))
                            {
                                ChatColumn column = new ChatColumn();

                                foreach (var chat in col.Elements("chat"))
                                {
                                    if (chat.Attribute("type")?.Value == "twitch")
                                    {
                                        Console.WriteLine("added chat");

                                        string channel = chat.Attribute("channel")?.Value;

                                        ChatControl widget = new ChatControl();
                                        widget.ChannelName = channel;

                                        column.AddWidget(widget);
                                    }
                                }

                                if (column.WidgetCount == 0)
                                {
                                    column.AddWidget(new ChatControl());
                                }

                                page.AddColumn(column);
                            }

                            tabControl.AddTab(page);
                        }
                    });
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }

            //columnLayoutControl1.ClearGrid();

            //try
            //{
            //    if (File.Exists(path))
            //    {
            //        XDocument doc = XDocument.Load(path);
            //        doc.Root.Process(root =>
            //        {
            //            root.Elements("tab").Do(tab =>
            //            {
            //                int columnIndex = 0;
            //                tab.Elements("column").Do(xD =>
            //                {
            //                    int rowIndex = 0;
            //                    xD.Elements().Do(x =>
            //                    {
            //                        switch (x.Name.LocalName)
            //                        {
            //                            case "chat":
            //                                if (x.Attribute("type").Value == "twitch")
            //                                {
            //                                    AddChannel(x.Attribute("channel").Value, columnIndex, rowIndex == 0 ? -1 : rowIndex);
            //                                }
            //                                break;
            //                        }
            //                        rowIndex++;
            //                    });
            //                    columnIndex++;
            //                });
            //            });
            //        });
            //    }
            //}
            //catch { }

            //if (columnLayoutControl1.Columns.Count == 0)
            //    AddChannel("fourtf");
        }

        public void SaveLayout(string path)
        {
            try
            {
                XDocument doc = new XDocument();
                XElement root = new XElement("layout");
                doc.Add(root);

                foreach (ColumnTabPage page in tabControl.TabPages)
                {
                    root.Add(new XElement("tab").With(xtab =>
                    {
                        if (page.CustomTitle != null)
                        {
                            xtab.SetAttributeValue("title", page.Title);
                        }

                        foreach (ChatColumn col in page.Columns)
                        {
                            xtab.Add(new XElement("column").With(xcol =>
                            {
                                foreach (ChatControl widget in col.Widgets.Where(x => x is ChatControl))
                                {
                                    xcol.Add(new XElement("chat").With(x =>
                                    {
                                        x.SetAttributeValue("type", "twitch");
                                        x.SetAttributeValue("channel", widget.ChannelName ?? "");
                                    }));
                                }
                            }));
                        }
                    }));
                }

                doc.Save(path);
            }
            catch { }

            //try
            //{
            //    XDocument doc = new XDocument();
            //    XElement root = new XElement("layout");
            //    doc.Add(root);

            //    foreach (var column in columnLayoutControl1.Columns)
            //    {
            //        root.Add(new XElement("column").With(xcol =>
            //        {
            //            foreach (var row in column)
            //            {
            //                var chat = row as ChatControl;

            //                if (chat != null)
            //                {
            //                    xcol.Add(new XElement("chat").With(x =>
            //                    {
            //                        x.SetAttributeValue("type", "twitch");
            //                        x.SetAttributeValue("channel", chat.ChannelName ?? "");
            //                    }));
            //                }
            //            }
            //        }));
            //    }

            //    doc.Save(path);
            //}
            //catch { }
        }

        public void ShowChangelog()
        {
            try
            {
                if (File.Exists("./Changelog.md"))
                {
                    var page = new ColumnTabPage();
                    page.CustomTitle = "Changelog";

                    page.AddColumn(new ChatColumn(new ChangelogControl(File.ReadAllText("./Changelog.md"))));

                    tabControl.InsertTab(0, page, true);
                }
            }
            catch { }
        }

        //public void AddChannel(string name, int column = -1, int row = -1)
        //{
        //    //columnLayoutControl1.AddToGrid(new ChatControl { ChannelName = name }, column, row);
        //}

        //public void AddChangelog()
        //{
        //    try
        //    {
        //        ColumnLayout.AddToGrid(new ChangelogControl(File.ReadAllText("Changelog.md")));
        //    }
        //    catch { }
        //}
    }
}
