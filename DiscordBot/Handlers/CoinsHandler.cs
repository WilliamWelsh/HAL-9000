// Handles the currency system: giving, adding, spawning, pickpocketing, and a lottery
using System;
using System.Linq;
using System.Text;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Gideon.Handlers
{
    class CoinsHandler
    {
        private const string icon = "https://i.imgur.com/w09rWQg.png";

        private static async Task PrintEmbed(ISocketMessageChannel channel, string description) => await Utilities.SendEmbed(channel, "Coins", description, Colors.Gold, "", icon);
        private static async Task PrintEmbedNoFooter(ISocketMessageChannel channel, string description) => await Utilities.SendEmbed(channel, "Coins", description, Colors.Gold, "", "");

        // Give coins to another user (from your own amount)
        public static async Task GiveCoins(SocketCommandContext context, SocketGuildUser sender, SocketGuildUser reciever, int amount)
        {
            var SenderAccount = UserAccounts.GetAccount(sender);
            var RecieverAccount = UserAccounts.GetAccount(reciever);
            if (amount < 1)
            {
                await PrintEmbed(context.Channel, $"You must enter an amount greater than 1, {sender.Mention}.").ConfigureAwait(false);
                return;
            }
            else if (amount > SenderAccount.coins)
            {
                await PrintEmbed(context.Channel, $"You do not have that many coins to send, {sender.Mention}.").ConfigureAwait(false);
                return;
            }
            SenderAccount.coins -= amount;
            RecieverAccount.coins += amount;
            UserAccounts.SaveAccounts();
            await PrintEmbedNoFooter(context.Channel, $"{sender.Mention} gave {reciever.Mention} {amount} coins.").ConfigureAwait(false);
        }

        // Spawn coins for a user
        public static async Task SpawnCoins(SocketCommandContext context, SocketGuildUser user, int amount)
        {
            UserAccounts.GetAccount(user).coins += amount;
            UserAccounts.SaveAccounts();
            await PrintEmbedNoFooter(context.Channel, $"Spawned {user.Mention} {amount} coins.").ConfigureAwait(false);
        }

        // Remove coins from a user
        public static async Task RemoveCoins(SocketCommandContext context, SocketGuildUser user, int amount)
        {
            AdjustCoins(user, -amount);
            await PrintEmbedNoFooter(context.Channel, $"{user.Mention} lost {amount} coins.").ConfigureAwait(false);
        }

        // Give or take coins from a user
        public static void AdjustCoins(SocketGuildUser user, int amount)
        {
            var account = UserAccounts.GetAccount(user);
            account.coins += amount;
            if (account.coins < 0)
                account.coins = 0;
            UserAccounts.SaveAccounts();
        }

        // Display how many coins a user has
        public static async Task DisplayCoins(SocketCommandContext context, SocketGuildUser user, ISocketMessageChannel channel)
        {
            await Utilities.SendEmbed(channel, user.Nickname ?? user.Username, $"{UserAccounts.GetAccount(user).coins.ToString("#,##0")} Coins", Colors.Gold, "", icon);
        }

        // Still in development
        public static async Task DisplayCoinsStore(SocketCommandContext context, SocketGuildUser user, ISocketMessageChannel channel)
        {
            await Utilities.SendEmbed(channel, "Coins Store", $"500 XP - ??? Coins\n1000 XP - ???", Colors.Gold, $"You have {UserAccounts.GetAccount(user).coins} Coins.", icon);
        }

        #region Pickpocket Related
        private static List<PickPocketUser> PickPocketHistory = new List<PickPocketUser>();
        public static async Task PickPocket(SocketCommandContext context, SocketGuildUser target)
        {
            if (target == null)
            {
                await Utilities.SendEmbed(context.Channel, "PickPocket", "Attempt to pickpocket others with `!pickpocket @user`", Colors.Gold, "", icon);
                return;
            }

            SocketGuildUser self = (SocketGuildUser)context.User;
            if (self == target)
            {
                await PrintEmbed(context.Channel, "You cannot pickpocket yourself").ConfigureAwait(false);
                return;
            }
            foreach (PickPocketUser ppu in PickPocketHistory)
            {
                if (ppu.User == self)
                {
                    if((DateTime.Now - ppu.TimeStamp).TotalHours <= 12)
                    {
                        string timeLeft = "";
                        if((12 - ((DateTime.Now - ppu.TimeStamp).TotalHours)) < 1)
                            timeLeft = $"{Math.Round(12 - ((DateTime.Now - ppu.TimeStamp).TotalMinutes), 0)} minutes";
                        else
                            timeLeft = $"{Math.Round(12 - ((DateTime.Now - ppu.TimeStamp).TotalHours), 0)} hours";
                        await Utilities.SendEmbed(context.Channel, "PickPocket", $"You must wait {timeLeft} before pickpocketing again.", Colors.Gold, "", icon);
                        return;
                    }
                    PickPocketHistory.Remove(ppu);
                }
            }
            if (Utilities.GetRandomNumber(0, 2) == 0)
            {
                // Successful pickpocket
                int CoinsGained = (int)(UserAccounts.GetAccount(target).coins * 0.1);
                await Utilities.SendEmbed(context.Channel, "PickPocket", $"{self.Mention} successfully pickpocketed {CoinsGained} coins from {target.Mention}.", Colors.Gold, "", icon);
                AdjustCoins(self, CoinsGained);
                AdjustCoins(target, -CoinsGained);
            }
            else
            {
                // Failed pickpocket
                int CoinsLost = (int)(UserAccounts.GetAccount(self).coins * 0.1);
                await Utilities.SendEmbed(context.Channel, "PickPocket", $"{self.Mention} attempted to pickpocket {target.Mention} and failed, losing {CoinsLost} coins.", Colors.Gold, "", icon);
                AdjustCoins(self, -CoinsLost);
            }
            PickPocketHistory.Add(new PickPocketUser(self, DateTime.Now));
        }
        #endregion

        // Print the top 10 users with the most coins (and how much they have)
        public static async Task PrintCoinsLeaderboard(SocketCommandContext context)
        {
            List<int> coinList = new List<int>();
            for (int i = 0; i < context.Guild.Users.Count; i++)
                coinList.Add(UserAccounts.GetAccount(context.Guild.Users.ElementAt(i)).coins);

            coinList.Sort();
            coinList.Reverse();

            StringBuilder description = new StringBuilder();
            List<SocketGuildUser> PeopleOnLB = new List<SocketGuildUser>();
            for (int coinListIndex = 0; coinListIndex < 10; coinListIndex++)
            {
                for (int userIndex = 0; userIndex < context.Guild.Users.Count; userIndex++)
                {
                    if (UserAccounts.GetAccount(context.Guild.Users.ElementAt(userIndex)).coins == coinList[coinListIndex] && !PeopleOnLB.Contains(context.Guild.Users.ElementAt(userIndex)))
                    {
                        string name = context.Guild.Users.ElementAt(userIndex).Nickname ?? context.Guild.Users.ElementAt(userIndex).Username;
                        description.AppendLine($"`{coinListIndex + 1}.` **{name}**, `{coinList[coinListIndex]} Coins`");
                        PeopleOnLB.Add(context.Guild.Users.ElementAt(userIndex));
                        break;
                    }
                }
            }

            await Utilities.SendEmbed(context.Channel, "Top 10 Users With The Most Coins", description.ToString(), Colors.Gold, "", "");
        }

        #region Lottery Related
        private static bool isLotteryGoing;
        private static List<SocketGuildUser> PeopleEnteredInLottery;
        private static int LotteryFee;
        private static int LotteryPrize;

        public static async Task StartCoinsLottery(SocketCommandContext context, int amount, int cost)
        {
            if (!await Utilities.CheckForSuperadmin(context, context.User)) return;
            if (!await Utilities.CheckForChannel(context, 518846214603669537, context.User)) return;
            if (isLotteryGoing)
            {
                await Utilities.PrintError(context.Channel, $"A lottery is already active, {context.User.Mention}.");
                return;
            }

            isLotteryGoing = true;
            LotteryFee = cost;
            LotteryPrize = amount;
            PeopleEnteredInLottery = new List<SocketGuildUser>();
            await PrintEmbed(context.Channel, $"{context.User.Mention} has started a lottery for {amount} coins!\n\nType `!coins lottery join` to enter!\n\nIt cost `{cost}` Coins!").ConfigureAwait(false);
        }

        public static async Task JoinCoinsLottery(SocketCommandContext context)
        {
            if (!isLotteryGoing)
            {
                await Utilities.PrintError(context.Channel, $"There is no active lottery, {context.User.Mention}.");
                return;
            }
            if (UserAccounts.GetAccount(context.User).coins < LotteryFee)
            {
                await Utilities.PrintError(context.Channel, $"You do not have enough Coins to enter the lottery, {context.User.Mention}.");
                return;
            }

            AdjustCoins((SocketGuildUser)context.User, -LotteryFee);
            PeopleEnteredInLottery.Add((SocketGuildUser)context.User);
            await PrintEmbed(context.Channel, $"{context.User.Mention} has joined the lottery!\n\n{PeopleEnteredInLottery.Count} currently entered.").ConfigureAwait(false);
        }

        public static async Task DrawLottery(SocketCommandContext context)
        {
            if (!await Utilities.CheckForSuperadmin(context, context.User)) return;
            if (!await Utilities.CheckForChannel(context, 518846214603669537, context.User)) return;
            if (!isLotteryGoing)
            {
                await Utilities.PrintError(context.Channel, $"There is no active Lottery, {context.User.Mention}.");
                return;
            }

            SocketGuildUser winner;
            winner = PeopleEnteredInLottery.ElementAt(Utilities.GetRandomNumber(1, PeopleEnteredInLottery.Count));
            AdjustCoins(winner, LotteryPrize);
            await PrintEmbed(context.Channel, $"{winner.Mention} has won {LotteryPrize} coins!\n\nThanks for playing!").ConfigureAwait(false);
            await ResetCoinsLottery(context, false).ConfigureAwait(false);
        }

        public static async Task ResetCoinsLottery(SocketCommandContext context, bool isFromUser)
        {
            if (isFromUser)
            {
                if (!await Utilities.CheckForSuperadmin(context, context.User)) return;
                await PrintEmbed(context.Channel, $"{context.User.Mention} has reset the Lottery.").ConfigureAwait(false);
            }
            isLotteryGoing = false;
            LotteryFee = 0;
            LotteryPrize = 0;
            PeopleEnteredInLottery = null;
        }
        #endregion
    }

    // A user that has pickpocketed
    public class PickPocketUser : IEquatable<PickPocketUser>
    {
        public SocketGuildUser User { get; }
        public DateTime TimeStamp { get; }

        public PickPocketUser(SocketGuildUser user, DateTime timeStamp)
        {
            User = user;
            TimeStamp = timeStamp;
        }

        public bool Equals(PickPocketUser other)
        {
            return User.Id == other.User.Id;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PickPocketUser);
        }

        public override int GetHashCode()
        {
            return 0; // Sorry
        }
    }
}
