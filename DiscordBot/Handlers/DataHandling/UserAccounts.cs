using System.Linq;
using Discord.WebSocket;
using System.Collections.Generic;

namespace DiscordBot
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
                         where a.UserID == id
                         select a;
            var account = result.FirstOrDefault();
            if (account == null) account = CreateUserAccount(id);
            return account;
        }

        private static UserAccount CreateUserAccount(ulong id)
        {
            var newAccount = new UserAccount
            {
                UserID = id,
                Coins = 0
            };
            accounts.Add(newAccount);
            SaveAccounts();
            return newAccount;
        }
    }
}