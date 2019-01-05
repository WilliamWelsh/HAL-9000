using Discord;
using System.Text;
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
        private int round = 0, PlayerSlots = 0, currentChamber = 0, badChamber = 0, currentTurn = 0;
        private bool isGameGoing = false;

        private SocketGuildUser host = null;
        private List<SocketGuildUser> Players = new List<SocketGuildUser>();

        private int RandomChamber() => Utilities.GetRandomNumber(0, 6);

        private Embed embed(string description, string footer, bool showPlayers)
        {
            var embed = new EmbedBuilder()
                .WithTitle($":gun: Russian Roulette")
                .WithColor(Colors.Black)
                .WithDescription(description)
                .WithFooter(footer);
            if (showPlayers)
            {
                StringBuilder PlayerDesc = new StringBuilder();
                for (int i = 0; i < Players.Count; i++)
                    PlayerDesc.AppendLine($"Player {i + 1}: {Players.ElementAt(i).Mention}");
                embed.AddField("Players", PlayerDesc);
            }
            return embed.Build();
        }

        private Embed gameEmbed(string description, string footer) => Utilities.Embed($":gun: Russian Roulette - Round {round}", description, Colors.Black, footer, "");

        public async Task TryToStartGame(SocketCommandContext context, string input)
        {
            if (!await Utilities.CheckForChannel(context, 518846214603669537, context.User)) return;
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
            if (!await Utilities.CheckForChannel(context, 518846214603669537, context.User)) return;
            if (!isGameGoing || Players.Count == PlayerSlots) return;

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
            if (!await Utilities.CheckForChannel(context, 518846214603669537, context.User) || !isGameGoing) return;

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
                CoinsHandler.AdjustCoins(player, -3);
            }
            else
                await context.Channel.SendMessageAsync("", false, gameEmbed($"The cylinder spins...\n\n*click*\n\n{player.Mention} survived!\n\nWaiting for {Players.ElementAt(currentTurn).Mention} to pull the trigger. (`!pt`)", ""));
        }

        private async Task DieAndCheckForWin(SocketGuildUser player, SocketCommandContext context)
        {
            Players.Remove(player);
            if(Players.Count == 1)
            {
                CoinsHandler.AdjustCoins(Players.ElementAt(0), 3 + (2 * PlayerSlots));
                CoinsHandler.AdjustCoins(player, -3);

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