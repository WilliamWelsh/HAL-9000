using Discord;
using System.Linq;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Gideon.Minigames
{
    class RussianRoulette
    {
        private int round = 0, PlayerSlots = 0, currentChamber = 0, badChamber = 0, currentTurn = 0;
        private bool isGameGoing = false;

        private SocketGuildUser host = null;
        private List<SocketGuildUser> Players = new List<SocketGuildUser>();

        private int RandomChamber() => Config.Utilities.GetRandomNumber(1, 6);

        private Embed embed(string Description, string Footer, bool showPlayers)
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

        private Embed gameEmbed(string Description, string Footer)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle($":gun: Russian Roulette - Round {round}");
            embed.WithColor(new Color(0, 0, 0));
            embed.WithDescription(Description);
            embed.WithFooter(Footer);
            return embed;
        }

        public async Task TryToStartGame(SocketCommandContext context, string input)
        {
            if (context.Channel.Id != 518846214603669537)
            {
                await Config.Utilities.PrintError(context, $"Please use the {context.Guild.GetTextChannel(518846214603669537).Mention} chat for that, {context.User.Mention}.");
                return;
            }
            if (isGameGoing)
            {
                await context.Channel.SendMessageAsync("", false, embed($"Sorry, {host.Mention} is currently hosting a game.", "", false));
                return;
            }
            if (input == "!rr")
            {
                await context.Channel.SendMessageAsync("", false, embed("Please enter an amount of players.\n\n`!rr #` - # = Players\n\nExample: \n`!rr 5` will start a game with 5 players.", "", false));
                return;
            }
            input = input.Replace("!rr ", "");
            int.TryParse(input, out PlayerSlots);
            if (PlayerSlots > 6)
            {
                await context.Channel.SendMessageAsync("", false, embed($"Sorry, 6 is the max amount of players for Russian Roulette.", "", false));
                return;
            }
            else if (PlayerSlots < 2)
            {
                await context.Channel.SendMessageAsync("", false, embed($"Sorry, 2 is the minimum amount of players for Russian Roulette.", "", false));
                return;
            }
            await StartGame(context);
        }

        public async Task StartGame(SocketCommandContext context)
        {
            host = (SocketGuildUser)context.User;
            await context.Channel.SendMessageAsync("", false, embed($"{host.Mention} has started a game of Russian Roulette with {PlayerSlots} players!\n\nType `!join rr` to play!", "", false));
            Players.Add(host);
            isGameGoing = true;
        }

        public async Task TryToJoin(SocketCommandContext context)
        {
            if (context.Channel.Id != 518846214603669537)
            {
                await Config.Utilities.PrintError(context, $"Please use the {context.Guild.GetTextChannel(518846214603669537).Mention} chat for that, {context.User.Mention}.");
                return;
            }
            if (!isGameGoing) return;
            if (Players.Count == PlayerSlots) return;

            SocketGuildUser newPlayer = (SocketGuildUser)context.User;
            Players.Add(newPlayer);
            if (Players.Count != PlayerSlots)
            {
                await context.Channel.SendMessageAsync("", false, embed($"{PlayerSlots - Players.Count} more player(s) needed!\n\nType `!join rr` to play!", "", true));
                return;
            }
            else
                await context.Channel.SendMessageAsync("", false, gameEmbed($"Initial round.\n\nWaiting for {Players.ElementAt(0).Mention} to pull the trigger. (`!pt`)", ""));
        }

        public async Task PullTrigger(SocketCommandContext context)
        {
            if (context.Channel.Id != 518846214603669537)
            {
                await Config.Utilities.PrintError(context, $"Please use the {context.Guild.GetTextChannel(518846214603669537).Mention} chat for that, {context.User.Mention}.");
                return;
            }
            if (!isGameGoing) return;

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
                await context.Channel.SendMessageAsync("", false, gameEmbed($"The cylinder spins...\n\n*BANG*\n\n{player.Mention} died and lost 3 Coins!\n\nWaiting for {Players.ElementAt(currentTurn).Mention} to pull the trigger. (`!pt`)", ""));
                Config.CoinHandler.AdjustCoins(player, -3);
            }
            else
                await context.Channel.SendMessageAsync("", false, gameEmbed($"The cylinder spins...\n\n*click*\n\n{player.Mention} survived!\n\nWaiting for {Players.ElementAt(currentTurn).Mention} to pull the trigger. (`!pt`)", ""));
        }

        private async Task DieAndCheckForWin(SocketGuildUser player, SocketCommandContext context)
        {
            Players.Remove(player);
            if(Players.Count == 1)
            {
                Config.CoinHandler.AdjustCoins(Players.ElementAt(0), 3 + (2 * PlayerSlots));
                Config.CoinHandler.AdjustCoins(player, -3);

                await context.Channel.SendMessageAsync("", false, gameEmbed($"The cylinder spins...\n\n*BANG*\n\n{player.Mention} died and lost 3 Coins!\n\n{Players.ElementAt(0).Mention} won the game and got {3 + (2*PlayerSlots)} coins!", ""));
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
            Players.Clear();
        }
    }
}