using System.Linq;
using Discord.WebSocket;
using System.Collections.Generic;

namespace Gideon
{
    public static class UserAccounts
    {
        private static List<UserAccount> accounts;

        private static string accountsFile = "Resources/user_data.json";

        static UserAccounts()
        {
            if(DataStoreage.SaveExists(accountsFile))
            {
                accounts = DataStoreage.LoadUserAccounts(accountsFile).ToList();
            }
            else
            {
                accounts = new List<UserAccount>();
                SaveAccounts();
            }
        }

        public static void SaveAccounts() => DataStoreage.SaveUserAccounts(accounts, accountsFile);

        public static UserAccount GetAccount(SocketUser user) => GetOrCreateUserAccount(user.Id);

        private static UserAccount GetOrCreateUserAccount(ulong id)
        {
            var result = from a in accounts
                         where a.UserID == id
                         select a;
            var account = result.FirstOrDefault();
            if(account == null)account = CreateUserAccount(id);
            return account;
        }

        private static UserAccount CreateUserAccount(ulong id)
        {
            var newAccount = new UserAccount()
            {
                UserID = id,
                superadmin = false,
                localTime = 999,
                nationality = "Not set.",
                Tecos = 0,
                hasDoubleTecoBoost = false
            };
            accounts.Add(newAccount);
            SaveAccounts();
            return newAccount;
        }
    }
}