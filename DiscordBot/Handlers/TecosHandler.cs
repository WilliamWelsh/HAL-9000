using System;
using Discord;
using System.Linq;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Gideon.Handlers
{
    class TecosHandler
    {
        private static Color EmbedColor = new Color(215, 154, 14);
        private static string TecosIcon = "https://i.imgur.com/w09rWQg.png";

        private async Task PrintEmbed(SocketCommandContext context, string description) => await context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("Tecos", description, EmbedColor, "", TecosIcon));
        private async Task PrintEmbedNoFooter(SocketCommandContext context, string description) => await context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("Tecos", description, EmbedColor, "", ""));

        public async Task GiveTecos(SocketCommandContext context, SocketGuildUser sender, SocketGuildUser reciever, int amount)
        {
            if (context.Channel.Id != 443205778656985089)
            {
                await Config.Utilities.PrintError(context, $"Please use the {context.Guild.GetTextChannel(443205778656985089).Mention} chat for that, {context.User.Mention}.");
                return;
            }
            var SenderAccount = UserAccounts.GetAccount(sender);
            var RecieverAccount = UserAccounts.GetAccount(reciever);
            if (amount < 1)
            {
                await PrintEmbed(context, $"You must enter an amount greater than 1, {sender.Mention}.");
                return;
            }
            else if (amount > SenderAccount.Tecos)
            {
                await PrintEmbed(context, $"You do not have that many Tecos to send, {sender.Mention}.");
                return;
            }
            SenderAccount.Tecos -= amount;
            RecieverAccount.Tecos += amount;
            UserAccounts.SaveAccounts();
            await PrintEmbedNoFooter(context, $"{sender.Mention} gave {reciever.Mention} {amount} Tecos.");
        }

        public async Task SpawnTecos(SocketCommandContext context, SocketGuildUser user, int amount)
        {
            if (context.Channel.Id != 443205778656985089)
            {
                await Config.Utilities.PrintError(context, $"Please use the {context.Guild.GetTextChannel(443205778656985089).Mention} chat for that, {context.User.Mention}.");
                return;
            }
            var account = UserAccounts.GetAccount(user);
            account.Tecos += amount;
            UserAccounts.SaveAccounts();
            await PrintEmbedNoFooter(context, $"spawned {user.Mention} {amount} Tecos.");
        }

        public async Task RemoveTecos(SocketCommandContext context, SocketGuildUser user, int amount)
        {
            if (context.Channel.Id != 443205778656985089)
            {
                await Config.Utilities.PrintError(context, $"Please use the {context.Guild.GetTextChannel(443205778656985089).Mention} chat for that, {context.User.Mention}.");
                return;
            }
            var account = UserAccounts.GetAccount(user);
            account.Tecos -= amount;
            if (account.Tecos < 0)
                account.Tecos = 0;
            UserAccounts.SaveAccounts();
            await PrintEmbedNoFooter(context, $"{user.Mention} lost {amount} Tecos.");
        }

        public void AdjustTecos(SocketGuildUser user, int amount)
        {
            var account = UserAccounts.GetAccount(user);
            if(account.hasDoubleTecoBoost && amount > 0)
                account.Tecos += (amount * 2);
            else
                account.Tecos += amount;
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

        public async Task DisplayTecos(SocketCommandContext context, SocketGuildUser user, ISocketMessageChannel channel)
        {
            if (context.Channel.Id != 443205778656985089)
            {
                await Config.Utilities.PrintError(context, $"Please use the {context.Guild.GetTextChannel(443205778656985089).Mention} chat for that, {context.User.Mention}.");
                return;
            }
            string name = user.Nickname != null ? user.Nickname : user.Username;
            string footer = UserAccounts.GetAccount(user).hasDoubleTecoBoost ? "Double Teco Boost is active." : "";
            await channel.SendMessageAsync("", false, Config.Utilities.Embed($"{name}", $"{UserAccounts.GetAccount(user).Tecos.ToString("#,##0")} Tecos", EmbedColor, footer, TecosIcon));
        }

        public async Task DisplayTecosStore(SocketCommandContext context, SocketGuildUser user, ISocketMessageChannel channel)
        {
            if (context.Channel.Id != 443205778656985089)
            {
                await Config.Utilities.PrintError(context, $"Please use the {context.Guild.GetTextChannel(443205778656985089).Mention} chat for that, {context.User.Mention}.");
                return;
            }
            await channel.SendMessageAsync("", false, Config.Utilities.Embed($"Tecos Store", $"Double Teco Boost - ??? Tecos\n`Doubles your Tecos income for 24 hours!`", EmbedColor, $"You have {UserAccounts.GetAccount(user).Tecos} Tecos.", TecosIcon));
        }

        private struct PickPocketUser { public SocketGuildUser user; public DateTime timeStamp; }
        private List<PickPocketUser> PickPocketHistory = new List<PickPocketUser>();
        public async Task PickPocket(SocketCommandContext context, SocketGuildUser target)
        {
            if (context.Channel.Id != 443205778656985089)
            {
                await Config.Utilities.PrintError(context, $"Please use the {context.Guild.GetTextChannel(443205778656985089).Mention} chat for that, {context.User.Mention}.");
                return;
            }
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
                        await context.Channel.SendMessageAsync("", false, Config.Utilities.Embed($"PickPocket", $"You must wait {timeLeft} before pickpocketing again.", EmbedColor, "", TecosIcon));
                        return;
                    }
                    PickPocketHistory.Remove(ppu);
                }
            }
            switch (Config.Utilities.GetRandomNumber(0,2))
            {
                // Success
                case 0:
                    int TecosGained = (int)(UserAccounts.GetAccount(target).Tecos * 0.1);
                    await context.Channel.SendMessageAsync("", false, Config.Utilities.Embed($"PickPocket", $"{self.Mention} successfully pickpocketed {TecosGained} Tecos from {target.Mention}.", EmbedColor, "", TecosIcon));
                    AdjustTecos(self, TecosGained);
                    AdjustTecos(target, -TecosGained);
                    break;
                // Fail
                case 1:
                    int TecosLost = (int)(UserAccounts.GetAccount(self).Tecos * 0.1);
                    await context.Channel.SendMessageAsync("", false, Config.Utilities.Embed($"PickPocket", $"{self.Mention} attempted to pickpocket {target.Mention} and failed, losing {TecosLost} Tecos.", EmbedColor, "", TecosIcon));
                    AdjustTecos(self, -TecosLost);
                    break;
            }
            PickPocketUser p = new PickPocketUser();
            p.user = self;
            p.timeStamp = DateTime.Now;
            PickPocketHistory.Add(p);
        }

        public async Task PrintTecosLeaderboard(SocketCommandContext context)
        {
            if (context.Channel.Id != 443205778656985089)
            {
                await Config.Utilities.PrintError(context, $"Please use the {context.Guild.GetTextChannel(443205778656985089).Mention} chat for that, {context.User.Mention}.");
                return;
            }
            List<int> list = new List<int>();
            for (int i = 0; i < context.Guild.Users.Count; i++)
            {
                list.Add(UserAccounts.GetAccount(context.Guild.Users.ElementAt(i)).Tecos);
            }

            int[] MostTecosArray = new int[5];
            int indexMin = 0;
            var IntArray = list.ToArray();
            MostTecosArray[indexMin] = IntArray[0];
            int min = MostTecosArray[indexMin];

            for (int i = 1; i < IntArray.Length; i++)
            {
                if (i < 5)
                {
                    MostTecosArray[i] = IntArray[i];
                    if (MostTecosArray[i] < min)
                    {
                        min = MostTecosArray[i];
                        indexMin = i;
                    }
                }
                else if (IntArray[i] > min)
                {
                    min = IntArray[i];
                    MostTecosArray[indexMin] = min;
                    for (int r = 0; r < 5; r++)
                    {
                        if (MostTecosArray[r] < min)
                        {
                            min = MostTecosArray[r];
                            indexMin = r;
                        }
                    }
                }
            }

            var embed = new EmbedBuilder();
            embed.WithTitle("Tecos Leaderboard");
            embed.WithColor(new Color(215, 154, 14));

            Array.Sort(MostTecosArray);
            Array.Reverse(MostTecosArray);
            List<SocketGuildUser> PeopleOnLB = new List<SocketGuildUser>();
            for (int i = 0; i < 5; i++)
            {
                for (int n = 0; n < context.Guild.Users.Count; n++)
                {
                    if (UserAccounts.GetAccount(context.Guild.Users.ElementAt(n)).Tecos == MostTecosArray[i] && !PeopleOnLB.Contains(context.Guild.Users.ElementAt(n)))
                    {
                        string name = context.Guild.Users.ElementAt(n).Nickname != null ? context.Guild.Users.ElementAt(n).Nickname : context.Guild.Users.ElementAt(n).Username;
                        embed.AddField($"{i + 1} - {name}", MostTecosArray[i] + " Tecos");
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

        public async Task StartTecosLottery(SocketCommandContext context, int amount, int cost)
        {
            if (context.Channel.Id != 443205778656985089)
            {
                await Config.Utilities.PrintError(context, $"Please use the {context.Guild.GetTextChannel(443205778656985089).Mention} chat for that, {context.User.Mention}.");
                return;
            }
            if (!UserAccounts.GetAccount(context.User).superadmin)
            {
                await Config.Utilities.PrintError(context, $"You do not have permission for that command, {context.User.Mention}.");
                return;
            }
            if(isLotteryGoing)
            {
                await Config.Utilities.PrintError(context, $"A Tecos Lottery is already active, {context.User.Mention}.");
                return;
            }

            isLotteryGoing = true;
            LotteryFee = cost;
            LotteryPrize = amount;
            PeopleEnteredInLottery = new List<SocketGuildUser>();
            await PrintEmbed(context, $"{context.User.Mention} has started a Tecos lottery for {amount} Tecos!\n\nType `!tecos lottery join` to enter!\n\nIt cost `{cost}` Tecos!");
        }

        public async Task JoinTecosLottery(SocketCommandContext context)
        {
            if (!isLotteryGoing)
            {
                await Config.Utilities.PrintError(context, $"There is no active Tecos Lottery, {context.User.Mention}.");
                return;
            }
            if (UserAccounts.GetAccount(context.User).Tecos < LotteryFee)
            {
                await Config.Utilities.PrintError(context, $"You do not have enough Tecos to enter the lottery, {context.User.Mention}.");
                return;
            }

            AdjustTecos((SocketGuildUser)context.User, -LotteryFee);
            PeopleEnteredInLottery.Add((SocketGuildUser)context.User);
            await PrintEmbed(context, $"{context.User.Mention} has joined the lottery!\n\n{PeopleEnteredInLottery.Count} currently entered.");
        }

        public async Task DrawLottery(SocketCommandContext context)
        {
            if (context.Channel.Id != 443205778656985089)
            {
                await Config.Utilities.PrintError(context, $"Please use the {context.Guild.GetTextChannel(443205778656985089).Mention} chat for that, {context.User.Mention}.");
                return;
            }
            if (!UserAccounts.GetAccount(context.User).superadmin)
            {
                await Config.Utilities.PrintError(context, $"You do not have permission for that command, {context.User.Mention}.");
                return;
            }
            if (!isLotteryGoing)
            {
                await Config.Utilities.PrintError(context, $"There is no active Tecos Lottery, {context.User.Mention}.");
                return;
            }

            SocketGuildUser winner;
            winner = PeopleEnteredInLottery.ElementAt(Config.Utilities.GetRandomNumber(1, PeopleEnteredInLottery.Count));
            AdjustTecos(winner, LotteryPrize);
            await PrintEmbed(context, $"{winner.Mention} has won {LotteryPrize} Tecos!\n\nThanks for playing!");
            await ResetTecosLottery(context, false);
        }

        public async Task ResetTecosLottery(SocketCommandContext context, bool isFromUser)
        {
            if (isFromUser)
            {
                if (!UserAccounts.GetAccount(context.User).superadmin)
                {
                    await Config.Utilities.PrintError(context, $"You do not have permission for that command, {context.User.Mention}.");
                    return;
                }
                await PrintEmbed(context, $"{context.User.Mention} has reset the Tecos Lottery.");
            }
            isLotteryGoing = false;
            LotteryFee = 0;
            LotteryPrize = 0;
            PeopleEnteredInLottery = null;
        }
    }
}