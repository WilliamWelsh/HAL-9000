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
        public SocketGuildUser User { get; set; }
        public bool HasAnswered { get; set; }
        public int Guess { get; set; }

        public bool Equals(Player other) => User.Id == other.User.Id;

        public override bool Equals(object obj) => Equals(obj as Player);

        public override int GetHashCode() => 0;
    }

    class NumberGuess
    {
        private int number, playerSlots = 2;
        private readonly List<Player> Players = new List<Player>();

        public bool isGamingGoing;

        private Embed Embed(string description, string footer, bool showPlayers)
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
                    PlayerDesc.AppendLine($"Player {i + 1}: {(Players.ElementAt(i).User.Nickname ?? Players.ElementAt(i).User.Username)}");
                embed.AddField("Players", PlayerDesc);
            }
            return embed.Build();
        }

        private void AddPlayer(SocketGuildUser user) => Players.Add(new Player { HasAnswered = false, User = user });

        public async Task TryToStartGame(int RandomNumber, SocketGuildUser user, SocketCommandContext context, int players)
        {
            if (isGamingGoing) return;
            isGamingGoing = true;
            number = RandomNumber;
            AddPlayer(user);
            playerSlots = players;
            if (playerSlots == 0)
                await context.Channel.SendMessageAsync("", false, Embed($"{user.Mention} has started a solo game.\n\nI am thinking of a number between 1 and 100. You get one try.", "!g number to guess.", false));
            else
                await context.Channel.SendMessageAsync("", false, Embed($"Starting a game!\n\nType `!join ng` to join!", "", true));
        }

        public async Task JoinGame(SocketGuildUser user, SocketCommandContext context)
        {
            if (!isGamingGoing)
            {
                await context.Channel.SendMessageAsync("", false, Embed("There is no game currently going.\n\nType `!help ng` for Number Guess game help.", "", false)).ConfigureAwait(false);
                return;
            }
            if (playerSlots == Players.Count)
                return;
            foreach(Player p in Players)
                if (p.User == user)
                    return;
            AddPlayer(user);
            if (playerSlots == Players.Count)
                await StartGame(context).ConfigureAwait(false);
            else
                await context.Channel.SendMessageAsync("", false, Embed($"{playerSlots - (Players.Count)} more player(s) needed!", "", true));
        }

        public async Task StartGame(SocketCommandContext context) => await context.Channel.SendMessageAsync("", false, Embed("I am thinking of a number between 1 and 100. You get one try.", "!g number to guess.", false));

        public async Task TryToGuess(SocketGuildUser user, SocketCommandContext context, int input)
        {
            if (!isGamingGoing) return;
            else if (playerSlots != Players.Count && playerSlots != 0) return;

            bool isPlaying = false;
            foreach(Player p in Players)
            {
                if (p.User == user)
                {
                    isPlaying = true;
                    break;
                }
            }
            if (!isPlaying) return;
            for (int i = 0; i < Players.Count; i++)
            {
                if(Players.ElementAt(i).User == user)
                {
                    var p = Players.ElementAt(i);
                    p.HasAnswered = true;
                    p.Guess = input;
                    Players.RemoveAt(i);
                    Players.Add(p);
                }
            }
            
            foreach(Player p in Players)
                if (p.HasAnswered == false)
                    return;

            var embed = new EmbedBuilder()
                .WithTitle("Number Guess")
                .WithColor(new Color(0, 0, 0));
            if(playerSlots == 0)
            {
                if(Players.ElementAt(0).Guess == number)
                {
                    embed.WithDescription($"Great job, {Players.ElementAt(0).User.Mention}! You got it exactly right and won 101 Coins!");
                    CoinsHandler.AdjustCoins(Players.ElementAt(0).User, 101);
                }
                else
                {
                    embed.WithDescription($"Sorry, {Players.ElementAt(0).User.Mention}. You did not get it right.\n\nThe number was {number}.");
                    embed.WithFooter("Lost 1 Coin.");
                    CoinsHandler.AdjustCoins(Players.ElementAt(0).User, -1);
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
                if (Players.ElementAt(i).Guess == number)
                {
                    didSomeoneGetIt = true;
                    Description.AppendLine($"{Players.ElementAt(i).User.Mention} got it exactly right and won {100 + (2* playerSlots)} Coins!").AppendLine();
                    Description.AppendLine("Everyone else lost 10 coins!");
                    lost10 = true;
                    embed.WithFooter("100 + 2 * Players played.");
                    CoinsHandler.AdjustCoins(Players.ElementAt(i).User, 100 + (2 * playerSlots));
                    winner = Players.ElementAt(i).User;
                    break;
                }
            }
            List<int> list = new List<int>();
            foreach (Player p in Players) list.Add(p.Guess);
            int Closest = list.Aggregate((x, y) => Math.Abs(x - number) < Math.Abs(y - number) ? x : y);
            if(!didSomeoneGetIt)
            {
                for (int i = 0; i < Players.Count; i++)
                {
                    if (Players.ElementAt(i).Guess == Closest)
                    {
                        Description.AppendLine($"{Players.ElementAt(i).User.Mention} got the closest with {Players.ElementAt(i).Guess}!").AppendLine();
                        Description.AppendLine("Everyone else lost 1 coin.");
                        winner = Players.ElementAt(i).User;
                        break;
                    }
                }
            }
            embed.WithDescription(Description.ToString());

            for (int i = 0; i < Players.Count; i++)
                if (winner != Players.ElementAt(i).User)
                    CoinsHandler.AdjustCoins(Players.ElementAt(i).User, lost10 ? -10 : -1);

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