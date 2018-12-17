using System;
using Discord;
using System.Linq;
using Discord.Rest;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Gideon.Minigames
{
    class RockPaperScissors
    {
        public ulong MessageID;
        private bool isPlaying;
        private static readonly Color color = new Color(251, 233, 231);
        private SocketGuildUser Player;
        private List<string> Plays = new List<string>(new string[] { "Rock", "Paper", "Scissors" });

        private Embed embed(string description, string footer) => Config.Utilities.Embed("Rock-Paper-Scissors", description, color, footer, "https://i.imgur.com/VXdDjho.png");

        public async Task StartRPS(SocketCommandContext context)
        {
            if (!await Config.Utilities.CheckForChannel(context, 518846214603669537, context.User)) return;
            string name = ((SocketGuildUser)context.User).Nickname ?? context.User.Username;
            RestUserMessage m = await context.Channel.SendMessageAsync("", false, embed("Pick your play and see if you can beat me!", $"{name} is playing."));
            await m.AddReactionAsync(new Emoji("📰"));
            await m.AddReactionAsync(new Emoji("✂"));
            await m.AddReactionAsync(new Emoji("🌑"));
            isPlaying = true;
            MessageID = m.Id;
            Player = (SocketGuildUser)context.User;
        }

        public async Task ViewPlay(string emote, ISocketMessageChannel channel, Optional<IUser> user)
        {
            if (!isPlaying || Player != user.Value) return;

            string playOne = "";
            if (emote == "📰")
                playOne = "Paper";
            else if (emote == "✂")
                playOne = "Scissors";
            else if (emote == "🌑")
                playOne = "Rock";

            string playTwo = Plays.ElementAt(Config.Utilities.GetRandomNumber(0, 3));
            string result = GetWinner(playOne[0], playTwo[0]);

            await channel.SendMessageAsync("", false, embed($"{Player.Mention} chose {playOne}!\n\nI chose {playTwo}.\n\n{result}", ""));

            if(result.Contains("lose 3 coins"))
                Config.CoinHandler.AdjustCoins(Player, -3);
            else if (result.Contains("got 3 coins"))
                Config.CoinHandler.AdjustCoins(Player, 3);

            Player = null;
            isPlaying = false;
        }

        private string GetWinner(char p1, char p2)
        {
            if (p1 == p2) return "It's a draw!";
            if ((p1 == 'S' && p2 == 'P') ||
                (p1 == 'P' && p2 == 'R') ||
                (p1 == 'R' && p2 == 'S'))
                return $"{Player.Mention} won and got 3 coins!";
            return "I won! You lose 3 coins.";
        }

    }
}
