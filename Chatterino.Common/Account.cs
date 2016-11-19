using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Chatterino.Common
{
    public class Account
    {
        public string Username { get; set; }
        public string OauthToken { get; set; }
        public string ClientId { get; set; }
        [JsonIgnore]
        public bool IsAnon { get; private set; }

        public Account(string username, string oauthToken, string clientId)
        {
            Username = username;
            OauthToken = oauthToken;
            ClientId = clientId;
        }

        public Account()
        {
            
        }

        public static Account AnonAccount { get; } = new Account("justinfan123", string.Empty, string.Empty) { IsAnon = true};
    }
}
