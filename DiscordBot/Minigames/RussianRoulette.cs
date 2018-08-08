using System;
using Discord;
using System.Linq;
using Gideon.Handlers;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Gideon.Minigames
{
    class RussianRoulette
    {
        TecosHandler TH = new TecosHandler();

        int round = 0;
        int PlayerSlots = 0;
        int currentChamber = 0;
        int badChamber = 0;
        int currentTurn = 0;
        bool isGameGoing = false;
        bool canPlaceBets = false;
        bool canGameProgress = false;
        SocketGuildUser host = null;
        List<SocketGuildUser> Players = new List<SocketGuildUser>();

        struct Gambler { public SocketGuildUser user; public int amount; public SocketGuildUser UserBettingOn; };
        List<Gambler> Gamblers = new List<Gambler>();

        private static readonly Random getrandom = new Random();
        public static int RandomChamber()
        {
            lock (getrandom) { return getrandom.Next(1, 6); }
        }

        Embed Embed(string Description, string Footer, bool showPlayers)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle($":gun: Russian Roulette");
            embed.WithColor(new Color(0, 0, 0));
            embed.WithDescription(Description);
            if (showPlayers)
            {
                string PlayerDesc = "";
                for (int i = 0; i < Players.Count; i++)
                    PlayerDesc += $"Player {i + 1}: {Players.ElementAt(i).Mention}\n";
                embed.AddField("Players", PlayerDesc);
            }
            embed.WithFooter(Footer);
            return embed;
        }

        Embed GameEmbed(string Description, string Footer)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle($":gun: Russian Roulette - Round {round}");
            embed.WithColor(new Color(0, 0, 0));
            embed.WithDescription(Description);
            embed.WithFooter(Footer);
            return embed;
        }

        public async Task TryToPlaceBet(SocketGuildUser UserBeingBetOn, SocketCommandContext context, int amount)
        {
            if (context.Channel.Id != 443205778656985089)
            {
                await Config.Utilities.PrintError(context, $"Please use the {context.Guild.GetTextChannel(443205778656985089).Mention} chat for that, {context.User.Mention}.");
                return;
            }

            if (!canPlaceBets)
            {
                await context.Channel.SendMessageAsync("", false, Embed($"You cannot place bets until the player slots are filled up.", "", false));
                return;
            }

            SocketGuildUser user = (SocketGuildUser)context.User;
            if(amount <= 0)
            {
                await context.Channel.SendMessageAsync("", false, Embed($"You can only bet an amount over 0.", "", false));
                return;
            }
            else if (UserAccounts.GetAccount(user).Tecos < amount)
            {
                await context.Channel.SendMessageAsync("", false, Embed($"You do not have that many Tecos to bet.", "", false));
                return;
            }

            await context.Channel.SendMessageAsync("", false, Embed($"{user.Mention} bet {amount} Tecos on {UserBeingBetOn.Mention}.", "", false));
            Gambler newGambler = new Gambler();
            newGambler.user = user;
            newGambler.amount = amount;
            newGambler.UserBettingOn = UserBeingBetOn;
            Gamblers.Add(newGambler);
        }

        public async Task TryToStartGame(SocketCommandContext context, string input)
        {
            if (context.Channel.Id != 443205778656985089)
            {
                await Config.Utilities.PrintError(context, $"Please use the {context.Guild.GetTextChannel(443205778656985089).Mention} chat for that, {context.User.Mention}.");
                return;
            }
            if (input == "start")
            {
                await HostStart(context);
                return;
            }
            if (input.StartsWith("!rr bet")) return;
            if (isGameGoing)
            {
                await context.Channel.SendMessageAsync("", false, Embed($"Sorry, {host.Mention} is currently hosting a game.", "", false));
                return;
            }
            if (input == "!rr")
            {
                await context.Channel.SendMessageAsync("", false, Embed("Please enter an amount of players.\n\n`!rr #` - # = Players\n\nExample: \n`!rr 5` will start a game with 5 players.", "", false));
                return;
            }
            input = input.Replace("!rr ", "");
            Int32.TryParse(input, out PlayerSlots);
            if (PlayerSlots > 6)
            {
                await context.Channel.SendMessageAsync("", false, Embed($"Sorry, 6 is the max amount of players for Russian Roulette.", "", false));
                return;
            }
            else if (PlayerSlots < 2)
            {
                await context.Channel.SendMessageAsync("", false, Embed($"Sorry, 2 is the minimum amount of players for Russian Roulette.", "", false));
                return;
            }
            await StartGame(context);
        }

        public async Task StartGame(SocketCommandContext context)
        {
            host = (SocketGuildUser)context.User;
            await context.Channel.SendMessageAsync("", false, Embed($"{host.Mention} has started a game of Russian Roulette with {PlayerSlots} players!\n\nType `!join rr` to play!", "", false));
            Players.Add(host);
            isGameGoing = true;
        }

        public async Task TryToJoin(SocketCommandContext context)
        {
            if (context.Channel.Id != 443205778656985089)
            {
                await Config.Utilities.PrintError(context, $"Please use the {context.Guild.GetTextChannel(443205778656985089).Mention} chat for that, {context.User.Mention}.");
                return;
            }
            if (!isGameGoing) return;
            if (Players.Count == PlayerSlots) return;

            SocketGuildUser newPlayer = (SocketGuildUser)context.User;
            Players.Add(newPlayer);
            if (Players.Count != PlayerSlots)
            {
                await context.Channel.SendMessageAsync("", false, Embed($"{PlayerSlots - Players.Count} more player(s) needed!\n\nType `!join rr` to play!", "", true));
                return;
            }
            else
            {
                canPlaceBets = true;
                await context.Channel.SendMessageAsync("", false, GameEmbed($"Everyone has joined!\n\nPlace bets to win tecos!\n`!rr bet @user amount`\n\nWaiting for {host.Mention} to `!rr start`...", ""));
            }
        }

        public async Task HostStart(SocketCommandContext context)
        {
            if ((SocketGuildUser)context.User != host) return;
            await context.Channel.SendMessageAsync("", false, GameEmbed($"Initial round.\n\nWaiting for {Players.ElementAt(0).Mention} to pull the trigger. (`!pt`)", ""));
            canGameProgress = true;
            return;
        }

        public async Task PullTrigger(SocketCommandContext context)
        {
            if (context.Channel.Id != 443205778656985089)
            {
                await Config.Utilities.PrintError(context, $"Please use the {context.Guild.GetTextChannel(443205778656985089).Mention} chat for that, {context.User.Mention}.");
                return;
            }
            if (!isGameGoing) return;
            if (!canGameProgress) return;

            SocketGuildUser player = (SocketGuildUser)context.User;
            if (Players.ElementAt(currentTurn) != player) return;
            await DoRound(player, context);
        }

        private async Task DoRound(SocketGuildUser player, SocketCommandContext context)
        {
            round++;
            badChamber = RandomChamber();
            currentChamber = RandomChamber();
            currentTurn = currentTurn == (Players.Count-1) ? currentTurn = 0 : currentTurn + 1;

            if (currentChamber == badChamber)
            {
                await DieAndCheckForWin(player, context);
                await context.Channel.SendMessageAsync("", false, GameEmbed($"The cylinder spins...\n\n*BANG*\n\n{player.Mention} died and lost 3 Tecos!\n\nWaiting for {Players.ElementAt(currentTurn).Mention} to pull the trigger. (`!pt`)", ""));
                TH.AdjustTecos(player, -3);
            }
            else
            {
                Console.WriteLine(currentTurn);
                await context.Channel.SendMessageAsync("", false, GameEmbed($"The cylinder spins...\n\n*click*\n\n{player.Mention} survived!\n\nWaiting for {Players.ElementAt(currentTurn).Mention} to pull the trigger. (`!pt`)", ""));
            }
        }

        private async Task DieAndCheckForWin(SocketGuildUser player, SocketCommandContext context)
        {
            Players.Remove(player);
            if(Players.Count == 1)
            {
                int ExtraTecos = 0;
                if (Gamblers.Count != 0)
                {
                    for (int i = 0; i < Gamblers.Count; i++)
                    {
                        if (Gamblers.ElementAt(i).UserBettingOn != Players.ElementAt(0))
                        {
                            TH.AdjustTecos(Gamblers.ElementAt(i).user, -Gamblers.ElementAt(i).amount);
                            ExtraTecos += Gamblers.ElementAt(i).amount;
                        }
                    }

                    int AmountOfPeopleWhoBetCorrectly = 0;
                    for (int i = 0; i < Gamblers.Count; i++)
                    {
                        if (Gamblers.ElementAt(i).UserBettingOn == Players.ElementAt(0))
                        {
                            AmountOfPeopleWhoBetCorrectly += 1;
                        }
                    }

                    ExtraTecos /= AmountOfPeopleWhoBetCorrectly;
                    for (int i = 0; i < Gamblers.Count; i++)
                    {
                        if (Gamblers.ElementAt(i).UserBettingOn == Players.ElementAt(0))
                        {
                            TH.AdjustTecos(Gamblers.ElementAt(i).user, ExtraTecos);
                        }
                    }
                }

                TH.AdjustTecos(Players.ElementAt(0), 3 + (2 * PlayerSlots));
                TH.AdjustTecos(player, -3);

                await context.Channel.SendMessageAsync("", false, GameEmbed($"The cylinder spins...\n\n*BANG*\n\n{player.Mention} died and lost 3 Tecos!\n\n{Players.ElementAt(0).Mention} won the game and got {3 + (2*PlayerSlots)} Tecos!\n\nEveryone who bet correctly got their Tecos back plus {ExtraTecos} extra Tecos!", ""));
                Reset();
                return;
            }
        }

        public void Reset()
        {
            round = 0;
            host = null;
            currentTurn = 0;
            isGameGoing = false;
            canPlaceBets = false;
            canGameProgress = false;
            Players.Clear();
            Gamblers.Clear();
        }
    }
}