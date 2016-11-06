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
        private ColumnTabPage lastTabPage = null;

        public MainForm()
        {
            InitializeComponent();

            // set window bounds
            try
            {
                StartPosition = FormStartPosition.Manual;
                Location = new Point(AppSettings.WindowX, AppSettings.WindowY);
                Size = new Size(AppSettings.WindowWidth, AppSettings.WindowHeight);
            }
            catch { }

            // top most
            TopMost = AppSettings.WindowTopMost;

            AppSettings.WindowTopMostChanged += (s, e) =>
            {
                TopMost = AppSettings.WindowTopMost;
            };

            // icon
            Icon = App.Icon;

            // show login dialog
            if (!File.Exists("./login.ini"))
            {
                using (var login = new LoginForm())
                {
                    login.ShowDialog();
                }
            }

            // load layout
            LoadLayout("./layout.xml");

#if !DEBUG
            if (AppSettings.CurrentVersion != App.CurrentVersion.ToString())
            {
                AppSettings.CurrentVersion = App.CurrentVersion.ToString();

                ShowChangelog();
            }
#endif

            // set title
            SetTitle();

            IrcManager.LoggedIn += (s, e) => SetTitle();

            // winforms specific
            BackColor = Color.Black;

            KeyPreview = true;

            Activated += (s, e) =>
            {
                App.WindowFocused = true;
            };

            Deactivate += (s, e) =>
            {
                App.WindowFocused = false;
                App.HideToolTip();
            };

            tabControl.TabPageSelected += (s, e) =>
            {
                var tab = e.Value as ColumnTabPage;

                if (lastTabPage != null)
                {
                    lastTabPage.LastSelected = Selected;
                }

                if (tab != null)
                {
                    if (tab.LastSelected != null && tab.Columns.SelectMany(x => x.Widgets).Contains(tab.LastSelected))
                        Selected = tab.LastSelected;
                    else
                        Selected = tab?.Columns.FirstOrDefault()?.Widgets.FirstOrDefault();

                    Selected?.Focus();
                }

                lastTabPage = tab;
            };

            lastTabPage = tabControl.Selected as ColumnTabPage;
        }

        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.T:
                    AddNewSplit();
                    break;
                case Keys.Control | Keys.W:
                    RemoveSelectedSplit();
                    break;
                case Keys.Control | Keys.P:
                    App.ShowSettings();
                    break;
                case Keys.Control | Keys.L:
                    new LoginForm().ShowDialog();
                    break;
                case Keys.Alt | Keys.Left:
                    {
                        var tab = tabControl.Selected as ColumnTabPage;

                        if (tab != null && selected != null)
                        {
                            var index = tab.Columns.TakeWhile(x => !x.Widgets.Contains(selected)).Count();

                            if (index > 0)
                            {
                                var newCol = tab.Columns.ElementAt(index - 1);
                                Selected = newCol.Widgets.ElementAtOrDefault(tab.Columns.ElementAt(index).Widgets.TakeWhile(x => x != selected).Count()) ?? newCol.Widgets.Last();
                            }
                        }
                    }
                    break;
                case Keys.Alt | Keys.Right:
                    {
                        var tab = tabControl.Selected as ColumnTabPage;

                        if (tab != null && selected != null)
                        {
                            var index = tab.Columns.TakeWhile(x => !x.Widgets.Contains(selected)).Count();

                            if (index + 1 < tab.ColumnCount)
                            {
                                var newCol = tab.Columns.ElementAt(index + 1);
                                Selected = newCol.Widgets.ElementAtOrDefault(tab.Columns.ElementAt(index).Widgets.TakeWhile(x => x != selected).Count()) ?? newCol.Widgets.Last();
                            }
                        }
                    }
                    break;
                case Keys.Alt | Keys.Up:
                    {
                        var tab = tabControl.Selected as ColumnTabPage;

                        if (tab != null && selected != null)
                        {
                            var col = tab.Columns.First(x => x.Widgets.Contains(selected));

                            var index = col.Widgets.TakeWhile(x => x != selected).Count();

                            if (index > 0)
                            {
                                Selected = col.Widgets.ElementAt(index - 1);
                            }
                        }
                    }
                    break;
                case Keys.Alt | Keys.Down:
                    {
                        var tab = tabControl.Selected as ColumnTabPage;

                        if (tab != null && selected != null)
                        {
                            var col = tab.Columns.First(x => x.Widgets.Contains(selected));

                            var index = col.Widgets.TakeWhile(x => x != selected).Count();

                            if (index + 1 < col.WidgetCount)
                            {
                                Selected = col.Widgets.ElementAt(index + 1);
                            }
                        }
                    }
                    break;
                case Keys.Control | Keys.D1:
                case Keys.Control | Keys.D2:
                case Keys.Control | Keys.D3:
                case Keys.Control | Keys.D4:
                case Keys.Control | Keys.D5:
                case Keys.Control | Keys.D6:
                case Keys.Control | Keys.D7:
                case Keys.Control | Keys.D8:
                case Keys.Control | Keys.D9:
                    {
                        int tab = (keyData & ~Keys.Modifiers) - Keys.D0;

                        var t = tabControl.TabPages.ElementAtOrDefault(tab - 1);

                        if (t != null)
                        {
                            TabControl.Select(t);
                        }
                    }
                    break;
                case Keys.Control | Keys.Tab:
                    {
                        int index = tabControl.TabPages.TakeWhile(x => !x.Selected).Count();

                        if (tabControl.TabPages.Count() > index + 1)
                        {
                            tabControl.Select(tabControl.TabPages.ElementAt(index + 1));
                        }
                        else
                        {
                            tabControl.Select(tabControl.TabPages.ElementAt(0));
                        }
                    }
                    break;
                case Keys.Control | Keys.Shift | Keys.Tab:
                    {
                        int index = tabControl.TabPages.TakeWhile(x => !x.Selected).Count();

                        if (index > 0)
                        {
                            tabControl.Select(tabControl.TabPages.ElementAt(index - 1));
                        }
                        else
                        {
                            tabControl.Select(tabControl.TabPages.ElementAt(tabControl.TabPages.Count() - 1));
                        }
                    }
                    break;
                case Keys.Tab:
                case Keys.Shift | Keys.Tab:
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                    selected?.HandleKeys(keyData);
                    break;
                default:

                    return false;
            }

            return true;
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            e.IsInputKey = true;
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

        ColumnLayoutItem selected = null;

        public ColumnLayoutItem Selected
        {
            get
            {
                return selected;
                //return tabControl.TabPages.SelectMany(a => ((ColumnTabPage)a).Columns.SelectMany(b => b.Widgets)).FirstOrDefault(c => c.Focused);
            }
            internal set
            {
                if (selected != value)
                {
                    selected = value;

                    value?.Focus();

                    App.SetEmoteListChannel((selected as ChatControl)?.Channel);
                }
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

            //if ((e.KeyCode & ~Keys.Modifiers) != Keys.ControlKey)
            //{
            //    ;
            //}

            //if (e.Modifiers == Keys.Control)
            //{
            //    switch (e.KeyCode)
            //    {
            //        case Keys.T:
            //            AddNewSplit();
            //            break;
            //        case Keys.W:
            //            RemoveSelectedSplit();
            //            break;
            //        case Keys.R:
            //            {
            //                ChatControl focused = Selected as ChatControl;
            //                if (focused != null)
            //                {
            //                    using (InputDialogForm dialog = new InputDialogForm("channel name") { Value = focused.ChannelName })
            //                    {
            //                        if (dialog.ShowDialog() == DialogResult.OK)
            //                        {
            //                            focused.ChannelName = dialog.Value;
            //                        }
            //                    }
            //                }
            //            }
            //            break;
            //        case Keys.P:
            //            App.ShowSettings();
            //            break;
            //        case Keys.C:
            //            (App.MainForm?.Selected as ChatControl)?.CopySelection(false);
            //            break;
            //        case Keys.X:
            //            (App.MainForm?.Selected as ChatControl)?.CopySelection(true);
            //            break;
            //        case Keys.V:
            //            try
            //            {
            //                if (Clipboard.ContainsText())
            //                {
            //                    (App.MainForm?.Selected as ChatControl)?.PasteText(Clipboard.GetText());
            //                }
            //            }
            //            catch { }
            //            break;
            //        case Keys.L:
            //            new LoginForm().ShowDialog();
            //            break;
            //        case Keys.D1:
            //        case Keys.D2:
            //        case Keys.D3:
            //        case Keys.D4:
            //        case Keys.D5:
            //        case Keys.D6:
            //        case Keys.D7:
            //        case Keys.D8:
            //        case Keys.D9:
            //            {
            //                int tab = e.KeyCode - Keys.D0;

            //                var t = tabControl.TabPages.ElementAtOrDefault(tab - 1);

            //                if (t != null)
            //                {
            //                    TabControl.Select(t);
            //                }
            //            }
            //            break;
            //        case Keys.Left:
            //            var page = tabControl.Selected as ColumnTabPage;
            //            if (page != null)
            //            {
            //                bool cont = false;
            //                int index = page.Columns.TakeWhile(x => !(cont = x.Widgets.Contains(selected))).Count();

            //                if (cont && index != 0)
            //                {
            //                    var newCol = page.Columns.ElementAt(index - 1);

            //                    var newSelected = newCol.Widgets.ElementAtOrDefault(page.Columns.ElementAt(index).Widgets.TakeWhile(x => x != selected).Count()) ?? newCol.Widgets.Last();

            //                    Selected = newSelected;
            //                }
            //            }
            //            break;
            //    }
            //}
            //else if (e.Modifiers == Keys.None)
            //{
            //    switch (e.KeyCode)
            //    {
            //        case Keys.Home:

            //            break;
            //        case Keys.End:

            //            break;
            //    }
            //}

            //e.Handled = true;
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
