using System;
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
    class Player : IEquatable<Player>
    {
        public SocketGuildUser user { get; set; }
        public bool hasAnswered { get; set; }
        public int guess { get; set; }

        public bool Equals(Player other)
        {
            return user.Id == other.user.Id;
        }
    }
    class NumberGuess
    {
        private int number, playerSlots = 2;
        private List<Player> Players = new List<Player>();

        public bool isGamingGoing;

        private Embed embed(string description, string footer, bool showPlayers)
        {
            var embed = new EmbedBuilder()
                .WithTitle("Number Guess")
                .WithDescription(description)
                .WithColor(Colors.Black)
                .WithFooter(footer);
            if (showPlayers)
            {
                StringBuilder PlayerDesc = new StringBuilder();
                for (int i = 0; i < Players.Count; i++)
                    PlayerDesc.AppendLine($"Player {i + 1}: {(Players.ElementAt(i).user.Nickname ?? Players.ElementAt(i).user.Username)}");
                embed.AddField("Players", PlayerDesc);
            }
            return embed.Build();
        }

        private void AddPlayer(SocketGuildUser user) => Players.Add(new Player { hasAnswered = false, user = user });

        public async Task TryToStartGame(int RandomNumber, SocketGuildUser user, SocketCommandContext context, int players)
        {
            if (!await Utilities.CheckForChannel(context, 518846214603669537, context.User)) return;
            if (isGamingGoing) return;
            isGamingGoing = true;
            number = RandomNumber;
            AddPlayer(user);
            playerSlots = players;
            if (playerSlots == 0)
                await context.Channel.SendMessageAsync("", false, embed($"{user.Mention} has started a solo game.\n\nI am thinking of a number between 1 and 100. You get one try.", "!g number to guess.", false));
            else
                await context.Channel.SendMessageAsync("", false, embed($"Starting a game!\n\nType `!join ng` to join!", "", true));
        }

        public async Task JoinGame(SocketGuildUser user, SocketCommandContext context)
        {
            if (!await Utilities.CheckForChannel(context, 518846214603669537, context.User)) return;
            if (!isGamingGoing)
            {
                await context.Channel.SendMessageAsync("", false, embed("There is no game currently going.\n\nType `!help ng` for Number Guess game help.", "", false));
                return;
            }
            if (playerSlots == Players.Count)
                return;
            foreach(Player p in Players)
                if (p.user == user)
                    return;
            AddPlayer(user);
            if (playerSlots == Players.Count)
                await StartGame(context);
            else
                await context.Channel.SendMessageAsync("", false, embed($"{playerSlots - (Players.Count)} more player(s) needed!", "", true));
        }

        public async Task StartGame(SocketCommandContext context) => await context.Channel.SendMessageAsync("", false, embed("I am thinking of a number between 1 and 100. You get one try.", "!g number to guess.", false));

        public async Task TryToGuess(SocketGuildUser user, SocketCommandContext context, int input)
        {
            if (!await Utilities.CheckForChannel(context, 518846214603669537, context.User)) return;
            if (!isGamingGoing) return;
            if (playerSlots != Players.Count && playerSlots != 0)
                return;
            bool isPlaying = false;
            foreach(Player p in Players)
            {
                if (p.user == user)
                {
                    isPlaying = true;
                    break;
                }
            }
            if (!isPlaying) return;
            for (int i = 0; i < Players.Count; i++)
            {
                if(Players.ElementAt(i).user == user)
                {
                    var p = Players.ElementAt(i);
                    p.hasAnswered = true;
                    p.guess = input;
                    Players.RemoveAt(i);
                    Players.Add(p);
                }
            }
            
            foreach(Player p in Players)
                if (p.hasAnswered == false)
                    return;

            var embed = new EmbedBuilder()
                .WithTitle("Number Guess")
                .WithColor(new Color(0, 0, 0));
            if(playerSlots == 0)
            {
                if(Players.ElementAt(0).guess == number)
                {
                    embed.WithDescription($"Great job, {Players.ElementAt(0).user.Mention}! You got it exactly right and won 101 Coins!");
                    CoinsHandler.AdjustCoins(Players.ElementAt(0).user, 101);
                }
                else
                {
                    embed.WithDescription($"Sorry, {Players.ElementAt(0).user.Mention}. You did not get it right.");
                    embed.WithFooter("Lost 1 Coin.");
                    CoinsHandler.AdjustCoins(Players.ElementAt(0).user, -1);
                }
                await context.Channel.SendMessageAsync("", false, embed.Build());
                Reset();
                return;
            }

            StringBuilder Description = new StringBuilder().AppendLine($"Everyone has answered!\n\nThe answer was...{number}!").AppendLine();
            bool lost10 = false;
            bool didSomeoneGetIt = false;
            SocketGuildUser winner = null;
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players.ElementAt(i).guess == number)
                {
                    didSomeoneGetIt = true;
                    Description.AppendLine($"{Players.ElementAt(i).user.Mention} got it exactly right and won {100 + (2* playerSlots)} Coins!").AppendLine();
                    Description.AppendLine("Everyone else lost 10 coins!");
                    lost10 = true;
                    embed.WithFooter("100 + 2 * Players played.");
                    CoinsHandler.AdjustCoins(Players.ElementAt(i).user, 100 + (2 * playerSlots));
                    winner = Players.ElementAt(i).user;
                    break;
                }
            }
            List<int> list = new List<int>();
            foreach (Player p in Players) list.Add(p.guess);
            int Closest = list.Aggregate((x, y) => Math.Abs(x - number) < Math.Abs(y - number) ? x : y);
            if(!didSomeoneGetIt)
            {
                for (int i = 0; i < Players.Count; i++)
                {
                    if (Players.ElementAt(i).guess == Closest)
                    {
                        Description.AppendLine($"{Players.ElementAt(i).user.Mention} got the closest with {Players.ElementAt(i).guess}!").AppendLine();
                        Description.AppendLine("Everyone else lost 1 coin.");
                        winner = Players.ElementAt(i).user;
                        break;
                    }
                }
            }
            embed.WithDescription(Description.ToString());

            for (int i = 0; i < Players.Count; i++)
                if (!(winner == Players.ElementAt(i).user))
                    CoinsHandler.AdjustCoins(Players.ElementAt(i).user, lost10 ? -10 : -1);

            await context.Channel.SendMessageAsync("", false, embed.Build());
            Reset();
        }

        public void Reset()
        {
            isGamingGoing = false;
            Players.Clear();
        }
    }
}