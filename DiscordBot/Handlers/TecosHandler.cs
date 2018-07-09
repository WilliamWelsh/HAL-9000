using Discord.WebSocket;
using System.Threading.Tasks;

namespace Gideon.Handlers
{
    class TecosHandler
    {
        private static Discord.Color EmbedColor = new Discord.Color(215, 154, 14);
        private static string TecosIcon = "https://i.imgur.com/w09rWQg.png";

        public string GiveTecos(SocketGuildUser sender, SocketGuildUser reciever, int amount)
        {
            var SenderAccount = UserAccounts.GetAccount(sender);
            var RecieverAccount = UserAccounts.GetAccount(reciever);
            if (amount < 1)
                return $"You must enter an amount greater than 1, {sender.Mention}.";
            if (amount > SenderAccount.Tecos)
                return $"You do not have that many Tecos to send, {sender.Mention}.";
            SenderAccount.Tecos -= amount;
            RecieverAccount.Tecos += amount;
            UserAccounts.SaveAccounts();
            return $"{sender.Mention} gave {reciever.Mention} {amount} Tecos.";
        }

        public string SpawnTecos(SocketGuildUser user, int amount)
        {
            var account = UserAccounts.GetAccount(user);
            account.Tecos += amount;
            UserAccounts.SaveAccounts();
            return $"spawned {user.Mention} {amount} Tecos.";
        }

        public string RemoveTecos(SocketGuildUser user, int amount)
        {
            var account = UserAccounts.GetAccount(user);
            account.Tecos -= amount;
            if (account.Tecos < 0)
                account.Tecos = 0;
            UserAccounts.SaveAccounts();
            return $"{user.Mention} lost {amount} Tecos.";
        }

        public void AdjustTecos(SocketGuildUser user, int amount)
        {
            var account = UserAccounts.GetAccount(user);
            if(account.hasDoubleTecoBoost && amount > 0)
            {
                account.Tecos += (amount * 2);
            }
            else
            {
                account.Tecos += amount;
            }
            if (account.Tecos < 0)
                account.Tecos = 0;
            UserAccounts.SaveAccounts();
        }

        public void AdjustBoost(SocketGuildUser user, bool boolean)
        {
            var account = UserAccounts.GetAccount(user);
            account.hasDoubleTecoBoost = boolean;
            UserAccounts.SaveAccounts();
        }

        public async Task DisplayTecos(SocketGuildUser user, ISocketMessageChannel channel)
        {
            string name = user.Nickname != null ? user.Nickname : user.Username;
            string footer = UserAccounts.GetAccount(user).hasDoubleTecoBoost ? "Double Teco Boost is active." : "";
            await channel.SendMessageAsync("", false, Config.Utilities.Embed($"{name}", $"{UserAccounts.GetAccount(user).Tecos.ToString("#,##0")} Tecos", EmbedColor, footer, TecosIcon));
        }

        public async Task DisplayTecosStore(SocketGuildUser user, ISocketMessageChannel channel)
        {
            await channel.SendMessageAsync("", false, Config.Utilities.Embed($"Tecos Store", $"Double Teco Boost - ??? Tecos\n`Doubles your Tecos income for 24 hours!`", EmbedColor, $"You have {UserAccounts.GetAccount(user).Tecos} Tecos.", TecosIcon));
        }
    }
}