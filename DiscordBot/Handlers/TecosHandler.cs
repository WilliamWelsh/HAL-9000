using Discord.WebSocket;

namespace Gideon.Modules
{
    class TecosHandler
    {
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

        public void AdjustTecos(SocketGuildUser user, int amount)
        {
            var account = UserAccounts.GetAccount(user);
            account.Tecos += amount;
            if (account.Tecos < 0)
                account.Tecos = 0;
            UserAccounts.SaveAccounts();
        }
    }
}
