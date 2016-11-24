using Chatterino.Common;
using Gtk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatterino.Gtk.Controls
{
    public class MainWindow : Window
    {
        public MainWindow()
            : base("Chatterino")
        {
            // window bounds
            try
            {
                SetPosition(WindowPosition.None);
                Move(AppSettings.WindowX, AppSettings.WindowY);
                Resize(AppSettings.WindowWidth, AppSettings.WindowHeight);
            }
            catch { }

            // top most
            KeepAbove = AppSettings.WindowTopMost;

            AppSettings.WindowTopMostChanged += (s, e) =>
            {
                KeepAbove = AppSettings.WindowTopMost;
            };

            // icon
            //Icon = App.Icon;

            // show login dialog
            //if (!File.Exists("./login.ini"))
            //{
            //    using (var login = new LoginForm())
            //    {
            //        login.ShowDialog();
            //    }
            //}

            // load layout
            LoadLayout("./layout.xml");

            // set title
            SetTitle();

            IrcManager.LoggedIn += (s, e) => SetTitle();

            // gtk specific
            var widget = new ChatWidget();
            var channel = TwitchChannel.AddChannel("forsenlol");
            widget.TwitchChannel = channel;
            Add(widget);
        }

        protected override void OnHidden()
        {
            SaveLayout("./layout.xml");

            int x, y;
            GetPosition(out x, out y);
            AppSettings.WindowX = x;
            AppSettings.WindowY = y;

            int width, height;
            GetSize(out width, out height);
            AppSettings.WindowWidth = width;
            AppSettings.WindowHeight = height;

            base.OnHidden();
        }

        public void SetTitle()
        {
            Application.Invoke((s, e) => Title = ($"{IrcManager.Account.Username ?? "<not logged in>"} - Chatterino for Twitch (v?"
#if DEBUG
            + " dev"
#endif
            + ")")
            );
        }

        public void LoadLayout(string path)
        {
            //try
            //{
            //    if (File.Exists(path))
            //    {
            //        XDocument doc = XDocument.Load(path);

            //        doc.Root.Process(root =>
            //        {
            //            foreach (var tab in doc.Elements().First().Elements("tab"))
            //            {
            //                Console.WriteLine("tab");

            //                ColumnTabPage page = new ColumnTabPage();

            //                page.CustomTitle = tab.Attribute("title")?.Value;

            //                foreach (var col in tab.Elements("column"))
            //                {
            //                    ChatColumn column = new ChatColumn();

            //                    foreach (var chat in col.Elements("chat"))
            //                    {
            //                        if (chat.Attribute("type")?.Value == "twitch")
            //                        {
            //                            Console.WriteLine("added chat");

            //                            string channel = chat.Attribute("channel")?.Value;

            //                            ChatControl widget = new ChatControl();
            //                            widget.ChannelName = channel;

            //                            column.AddWidget(widget);
            //                        }
            //                    }

            //                    if (column.WidgetCount == 0)
            //                    {
            //                        column.AddWidget(new ChatControl());
            //                    }

            //                    page.AddColumn(column);
            //                }

            //                tabControl.AddTab(page);
            //            }
            //        });
            //    }
            //}
            //catch (Exception exc)
            //{
            //    Console.WriteLine(exc.Message);
            //}
        }

        public void SaveLayout(string path)
        {
            //try
            //{
            //    XDocument doc = new XDocument();
            //    XElement root = new XElement("layout");
            //    doc.Add(root);

            //    foreach (ColumnTabPage page in tabControl.TabPages)
            //    {
            //        root.Add(new XElement("tab").With(xtab =>
            //        {
            //            if (page.CustomTitle != null)
            //            {
            //                xtab.SetAttributeValue("title", page.Title);
            //            }

            //            foreach (ChatColumn col in page.Columns)
            //            {
            //                xtab.Add(new XElement("column").With(xcol =>
            //                {
            //                    foreach (ChatControl widget in col.Widgets.Where(x => x is ChatControl))
            //                    {
            //                        xcol.Add(new XElement("chat").With(x =>
            //                        {
            //                            x.SetAttributeValue("type", "twitch");
            //                            x.SetAttributeValue("channel", widget.ChannelName ?? "");
            //                        }));
            //                    }
            //                }));
            //            }
            //        }));
            //    }

            //    doc.Save(path);
            //}
            //catch { }
        }
    }
}
