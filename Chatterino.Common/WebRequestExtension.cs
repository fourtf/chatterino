using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;


namespace Chatterino.Common
{
    public static class WebRequestExtension
    {
        public static WebRequest AuthorizeHelix(this WebRequest request)
        {
            request.Headers.Add("Client-ID", IrcManager.DefaultClientID);
            return request;
        }

        public static WebRequest AuthorizeV5(this WebRequest request, string oauth = null, string clientId = null)
        {
            (request as HttpWebRequest).Accept = "application/vnd.twitchtv.v5+json";
            request.Headers.Add("Client-ID", clientId ?? IrcManager.DefaultClientID);
            if (oauth != null) { request.Headers.Add("Authorization", "OAuth " + oauth); }
            return request;
        }
    }
}
