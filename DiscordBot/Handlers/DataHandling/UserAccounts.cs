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
            if(DateStorage.SaveExists(accountsFile))
                accounts = DateStorage.LoadUserAccounts(accountsFile).ToList();
            else
            {
                accounts = new List<UserAccount>();
                SaveAccounts();
            }
        }

        public static void SaveAccounts() => DateStorage.SaveUserAccounts(accounts, accountsFile);

        public static UserAccount GetAccount(SocketUser user) => GetOrCreateUserAccount(user.Id);

        private static UserAccount GetOrCreateUserAccount(ulong id)
        {
            var result = from a in accounts
                         where a.userID == id
                         select a;
            var account = result.FirstOrDefault();
            if (account == null) account = CreateUserAccount(id);
            return account;
        }

        private static UserAccount CreateUserAccount(ulong id)
        {
            var newAccount = new UserAccount()
            {
                userID = id,
                superadmin = false,
                localTime = 999,
                country = "Not set.",
                coins = 0,
                xp = 0,
                level = 0
            };
            accounts.Add(newAccount);
            SaveAccounts();
            return newAccount;
        }
    }
}