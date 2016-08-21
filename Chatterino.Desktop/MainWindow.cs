using Chatterino.Common;
using Chatterino.Desktop.Widgets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xwt;

namespace Chatterino.Desktop
{
    public class MainWindow : Window
    {
        TabControl tabControl;

        public MainWindow()
        {
            Title = "Chatterino " + App.CurrentVersion.ToString();

            tabControl = new TabControl();

            Content = tabControl;

            Padding = new WidgetSpacing(0, 0, 0, 0);

            Size = new Size(600, 500);

            LoadLayout("./Layout.xml");
        }

        protected override void OnClosed()
        {
            //SaveLayout("./Layout.xml");

            base.OnClosed();
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

                            ChatTabPage page = new ChatTabPage();

                            page.Title = tab.Attribute("title")?.Value ?? "<no title>";

                            foreach (var col in tab.Elements("column"))
                            {
                                ChatColumn column = new ChatColumn();

                                foreach (var chat in col.Elements("chat"))
                                {
                                    if (chat.Attribute("type")?.Value == "twitch")
                                    {
                                        Console.WriteLine("added chat");

                                        string channel = chat.Attribute("channel")?.Value;

                                        ChatWidget widget = new ChatWidget();
                                        widget.ChannelName = channel;

                                        column.AddWidget(widget);
                                    }
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
            //            int columnIndex = 0;
            //            root.Elements("column").Do(col =>
            //            {
            //                int rowIndex = 0;
            //                col.Elements().Do(x =>
            //                {
            //                    switch (x.Name.LocalName)
            //                    {
            //                        case "chat":
            //                            if (x.Attribute("type").Value == "twitch")
            //                            {
            //                                AddChannel(x.Attribute("channel").Value, columnIndex, rowIndex == 0 ? -1 : rowIndex);
            //                            }
            //                            break;
            //                    }
            //                    rowIndex++;
            //                });
            //                columnIndex++;
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

                foreach (ChatTabPage page in tabControl.TabPages)
                {
                    root.Add(new XElement("tab").With(xtab =>
                    {
                        foreach (ChatColumn col in page.Columns)
                        {
                            xtab.Add(new XElement("column").With(xcol =>
                            {
                                foreach (ChatWidget widget in col.Widgets)
                                {
                                    xcol.Add(new XElement("chat").With(x =>
                                    {
                                        x.SetAttributeValue("type", "twitch");
                                        x.SetAttributeValue("channel", widget.ChannelName ?? "");
                                    }));
                                }
                            }));
                        }

                        if (page.HasCustomTitle)
                        {
                            xtab.SetAttributeValue("title", page.Title);
                        }
                    }));
                }

                doc.Save(path);
            }
            catch { }
        }
    }
}
