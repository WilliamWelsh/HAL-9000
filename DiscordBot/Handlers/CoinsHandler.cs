using System;
using Discord;
using System.Linq;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Gideon.Handlers
{
    class CoinsHandler
    {
        private static Color embedColor = new Color(215, 154, 14);
        private const string icon = "https://i.imgur.com/w09rWQg.png";

        private async Task PrintEmbed(SocketCommandContext context, string description) => await context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("Coins", description, embedColor, "", icon));
        private async Task PrintEmbedNoFooter(SocketCommandContext context, string description) => await context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("Coins", description, embedColor, "", ""));

        public async Task GiveCoins(SocketCommandContext context, SocketGuildUser sender, SocketGuildUser reciever, int amount)
        {
            var SenderAccount = UserAccounts.GetAccount(sender);
            var RecieverAccount = UserAccounts.GetAccount(reciever);
            if (amount < 1)
            {
                await PrintEmbed(context, $"You must enter an amount greater than 1, {sender.Mention}.");
                return;
            }
            else if (amount > SenderAccount.coins)
            {
                await PrintEmbed(context, $"You do not have that many Coins to send, {sender.Mention}.");
                return;
            }
            SenderAccount.coins -= amount;
            RecieverAccount.coins += amount;
            UserAccounts.SaveAccounts();
            await PrintEmbedNoFooter(context, $"{sender.Mention} gave {reciever.Mention} {amount} Coins.");
        }

        public async Task SpawnCoins(SocketCommandContext context, SocketGuildUser user, int amount)
        {
            UserAccounts.GetAccount(user).coins += amount;
            UserAccounts.SaveAccounts();
            await PrintEmbedNoFooter(context, $"Spawned {user.Mention} {amount} Coins.");
        }

        public async Task RemoveCoins(SocketCommandContext context, SocketGuildUser user, int amount)
        {
            AdjustCoins(user, -amount);
            await PrintEmbedNoFooter(context, $"{user.Mention} lost {amount} Coins.");
        }

        public void AdjustCoins(SocketGuildUser user, int amount)
        {
            var account = UserAccounts.GetAccount(user);
            account.coins += amount;
            if (account.coins < 0)
                account.coins = 0;
            UserAccounts.SaveAccounts();
        }

        public async Task DisplayCoins(SocketCommandContext context, SocketGuildUser user, ISocketMessageChannel channel)
        {
            string name = user.Nickname != null ? user.Nickname : user.Username;
            await channel.SendMessageAsync("", false, Config.Utilities.Embed($"{name}", $"{UserAccounts.GetAccount(user).coins.ToString("#,##0")} Coins", embedColor, "", icon));
        }

        public async Task DisplayCoinsStore(SocketCommandContext context, SocketGuildUser user, ISocketMessageChannel channel)
        {
            await channel.SendMessageAsync("", false, Config.Utilities.Embed($"Coins Store", $"500 XP - ??? Coins\n1000 XP - ???", embedColor, $"You have {UserAccounts.GetAccount(user).coins} Coins.", icon));
        }

        private struct PickPocketUser { public SocketGuildUser user; public DateTime timeStamp; }
        private List<PickPocketUser> PickPocketHistory = new List<PickPocketUser>();
        public async Task PickPocket(SocketCommandContext context, SocketGuildUser target)
        {
            SocketGuildUser self = (SocketGuildUser)context.User;
            if(self == target)
            {
                await PrintEmbed(context, "You cannot pickpocket yourself");
                return;
            }
            foreach(PickPocketUser ppu in PickPocketHistory)
            {
                if (ppu.user == self)
                {
                    if((DateTime.Now - ppu.timeStamp).TotalHours <= 12)
                    {
                        string timeLeft = "";
                        if((12 - ((DateTime.Now - ppu.timeStamp).TotalHours)) < 1)
                            timeLeft = $"{Math.Round(12 - ((DateTime.Now - ppu.timeStamp).TotalMinutes), 0)} minutes";
                        else
                            timeLeft = $"{Math.Round(12 - ((DateTime.Now - ppu.timeStamp).TotalHours), 0)} hours";
                        await context.Channel.SendMessageAsync("", false, Config.Utilities.Embed($"PickPocket", $"You must wait {timeLeft} before pickpocketing again.", embedColor, "", icon));
                        return;
                    }
                    PickPocketHistory.Remove(ppu);
                }
            }
            switch (Config.Utilities.GetRandomNumber(0,2))
            {
                // Success
                case 0:
                    int CoinsGained = (int)(UserAccounts.GetAccount(target).coins * 0.1);
                    await context.Channel.SendMessageAsync("", false, Config.Utilities.Embed($"PickPocket", $"{self.Mention} successfully pickpocketed {CoinsGained} Coins from {target.Mention}.", embedColor, "", icon));
                    AdjustCoins(self, CoinsGained);
                    AdjustCoins(target, -CoinsGained);
                    break;
                // Fail
                case 1:
                    int CoinsLost = (int)(UserAccounts.GetAccount(self).coins * 0.1);
                    await context.Channel.SendMessageAsync("", false, Config.Utilities.Embed($"PickPocket", $"{self.Mention} attempted to pickpocket {target.Mention} and failed, losing {CoinsLost} Coins.", embedColor, "", icon));
                    AdjustCoins(self, -CoinsLost);
                    break;
            }
            PickPocketUser p = new PickPocketUser();
            p.user = self;
            p.timeStamp = DateTime.Now;
            PickPocketHistory.Add(p);
        }

        public async Task PrintCoinsLeaderboard(SocketCommandContext context)
        {
            List<int> list = new List<int>();
            for (int i = 0; i < context.Guild.Users.Count; i++)
                list.Add(UserAccounts.GetAccount(context.Guild.Users.ElementAt(i)).coins);

            int[] MostCoinsArray = new int[5];
            int indexMin = 0;
            var IntArray = list.ToArray();
            MostCoinsArray[indexMin] = IntArray[0];
            int min = MostCoinsArray[indexMin];

            for (int i = 1; i < IntArray.Length; i++)
            {
                if (i < 5)
                {
                    MostCoinsArray[i] = IntArray[i];
                    if (MostCoinsArray[i] < min)
                    {
                        min = MostCoinsArray[i];
                        indexMin = i;
                    }
                }
                else if (IntArray[i] > min)
                {
                    min = IntArray[i];
                    MostCoinsArray[indexMin] = min;
                    for (int r = 0; r < 5; r++)
                    {
                        if (MostCoinsArray[r] < min)
                        {
                            min = MostCoinsArray[r];
                            indexMin = r;
                        }
                    }
                }
            }

            var embed = new EmbedBuilder();
            embed.WithTitle("Coins Leaderboard");
            embed.WithColor(new Color(215, 154, 14));

            Array.Sort(MostCoinsArray);
            Array.Reverse(MostCoinsArray);
            List<SocketGuildUser> PeopleOnLB = new List<SocketGuildUser>();
            for (int i = 0; i < 5; i++)
            {
                for (int n = 0; n < context.Guild.Users.Count; n++)
                {
                    if (UserAccounts.GetAccount(context.Guild.Users.ElementAt(n)).coins == MostCoinsArray[i] && !PeopleOnLB.Contains(context.Guild.Users.ElementAt(n)))
                    {
                        string name = context.Guild.Users.ElementAt(n).Nickname != null ? context.Guild.Users.ElementAt(n).Nickname : context.Guild.Users.ElementAt(n).Username;
                        embed.AddField($"{i + 1} - {name}", MostCoinsArray[i] + " Coins");
                        PeopleOnLB.Add(context.Guild.Users.ElementAt(n));
                        break;
                    }
                }
            }

            await context.Channel.SendMessageAsync("", false, embed);
        }

        private bool isLotteryGoing = false;
        private List<SocketGuildUser> PeopleEnteredInLottery;
        private int LotteryFee = 0;
        private int LotteryPrize = 0;

        public async Task StartCoinsLottery(SocketCommandContext context, int amount, int cost)
        {
            if (context.Channel.Id != 518846214603669537)
            {
                await Config.Utilities.PrintError(context, $"Please use the {context.Guild.GetTextChannel(518846214603669537).Mention} chat for that, {context.User.Mention}.");
                return;
            }
            if (!UserAccounts.GetAccount(context.User).superadmin)
            {
                await Config.Utilities.PrintError(context, $"You do not have permission for that command, {context.User.Mention}.");
                return;
            }
            if(isLotteryGoing)
            {
                await Config.Utilities.PrintError(context, $"A lottery is already active, {context.User.Mention}.");
                return;
            }

            isLotteryGoing = true;
            LotteryFee = cost;
            LotteryPrize = amount;
            PeopleEnteredInLottery = new List<SocketGuildUser>();
            await PrintEmbed(context, $"{context.User.Mention} has started a lottery for {amount} coins!\n\nType `!coins lottery join` to enter!\n\nIt cost `{cost}` Coins!");
        }

        public async Task JoinCoinsLottery(SocketCommandContext context)
        {
            if (!isLotteryGoing)
            {
                await Config.Utilities.PrintError(context, $"There is no active lottery, {context.User.Mention}.");
                return;
            }
            if (UserAccounts.GetAccount(context.User).coins < LotteryFee)
            {
                await Config.Utilities.PrintError(context, $"You do not have enough Coins to enter the lottery, {context.User.Mention}.");
                return;
            }

            AdjustCoins((SocketGuildUser)context.User, -LotteryFee);
            PeopleEnteredInLottery.Add((SocketGuildUser)context.User);
            await PrintEmbed(context, $"{context.User.Mention} has joined the lottery!\n\n{PeopleEnteredInLottery.Count} currently entered.");
        }

        public async Task DrawLottery(SocketCommandContext context)
        {
            if (context.Channel.Id != 518846214603669537)
            {
                await Config.Utilities.PrintError(context, $"Please use the {context.Guild.GetTextChannel(518846214603669537).Mention} chat for that, {context.User.Mention}.");
                return;
            }
            if (!UserAccounts.GetAccount(context.User).superadmin)
            {
                await Config.Utilities.PrintError(context, $"You do not have permission for that command, {context.User.Mention}.");
                return;
            }
            if (!isLotteryGoing)
            {
                await Config.Utilities.PrintError(context, $"There is no active Coins Lottery, {context.User.Mention}.");
                return;
            }

            SocketGuildUser winner;
            winner = PeopleEnteredInLottery.ElementAt(Config.Utilities.GetRandomNumber(1, PeopleEnteredInLottery.Count));
            AdjustCoins(winner, LotteryPrize);
            await PrintEmbed(context, $"{winner.Mention} has won {LotteryPrize} Coins!\n\nThanks for playing!");
            await ResetCoinsLottery(context, false);
        }

        public async Task ResetCoinsLottery(SocketCommandContext context, bool isFromUser)
        {
            if (isFromUser)
            {
                if (!UserAccounts.GetAccount(context.User).superadmin)
                {
                    await Config.Utilities.PrintError(context, $"You do not have permission for that command, {context.User.Mention}.");
                    return;
                }
                await PrintEmbed(context, $"{context.User.Mention} has reset the Coins Lottery.");
            }
            isLotteryGoing = false;
            LotteryFee = 0;
            LotteryPrize = 0;
            PeopleEnteredInLottery = null;
        }
    }
}