using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Chatterino.Common
{
    public static class AccountManager
    {
        private static ConcurrentDictionary<string, Account> _accounts = new ConcurrentDictionary<string, Account>();

        public static IEnumerable<Account> Accounts => _accounts.Values;

        public static void LoadFromJson(string path)
        {
            try
            {
                var accounts = JsonConvert.DeserializeObject<Account[]>(File.ReadAllText(path));

                foreach (var account in accounts)
                {
                    AddAccount(account);
                }
            }
            catch (Exception)
            {

            }
        }

        public static void SaveToJson(string path)
        {
            var s = JsonConvert.SerializeObject(Accounts);

            File.WriteAllText(path, s);
        }

        public static void RemoveAccount(string name)
        {
            Account account;

            if (_accounts.TryGetValue(name.ToLowerInvariant(), out account))
            {
                Account tmp;
                _accounts.TryRemove(account.Username.ToLowerInvariant(), out tmp);

                if (account == IrcManager.Account)
                {
                    IrcManager.Account = Accounts.FirstOrDefault() ?? Account.AnonAccount;
                }
            }
        }

        public static void AddAccount(Account account)
        {
            if (!string.IsNullOrWhiteSpace(account.Username))
            {
                _accounts[account.Username.ToLowerInvariant()] = account;
            }
        }

        public static Account FromUsername(string username)
        {
            Account account;

            _accounts.TryGetValue(username.ToLowerInvariant(), out account);

            return account;
        }
    }
}
