using Chatterino.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chatterino
{
    public static class Net
    {
        public static event EventHandler<ValueEventArgs<string>> CurrentChannelChanged;

        private static string currentChannel = null;

        public static string CurrentChannel
        {
            get { return currentChannel; }
            set
            {
                if (currentChannel != value)
                {
                    currentChannel = value;

                    CurrentChannelChanged?.Invoke(null, new ValueEventArgs<string>(value));
                }
            }
        }

        static HttpListener listener = null;

        public static void StartHttpServer()
        {
            //try
            //{
            //    listener = new HttpListener();

            //    listener.Prefixes.Add("http://127.0.0.1:5012/");

            //    listener.Start();
            //    new Thread(HandleRequests).Start();
            //}
            //catch
            //{

            //}
        }

        static void HandleRequests()
        {
            while (listener.IsListening)
            {
                var context = listener.GetContext();

                Task.Run(() =>
                {
                    try
                    {
                        var c = context;

                        if (c.Request.QueryString.Get("key") == AppSettings.BrowserExtensionKey)
                        {
                            switch (c.Request.QueryString.Get("action"))
                            {
                                case "currentChannel":
                                    var channel = c.Request.QueryString.Get("channel");
                                    CurrentChannel = channel;
                                    break;
                            }
                        }
                    }
                    catch { }

                    context.Response.StatusCode = 404;
                    context.Response.Close();
                });
            }
        }
    }
}
