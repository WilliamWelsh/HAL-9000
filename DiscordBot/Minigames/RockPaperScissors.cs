using Discord;
using System.Linq;
using Discord.Rest;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Handlers;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DiscordBot.Minigames
{
    class RockPaperScissors
    {
        private RestUserMessage gameMessage;
        public ulong MessageID;
        private bool isPlaying;
        public SocketGuildUser Player;
        private readonly List<string> Plays = new List<string>(new[] { "Rock", "Paper", "Scissors" });

        private Embed Embed(string description, string footer) => Utilities.Embed("Rock-Paper-Scissors", description, Utilities.ClearColor, footer, "https://i.imgur.com/VXdDjho.png");

        public async Task StartRPS(SocketCommandContext context)
        {
            string name = ((SocketGuildUser)context.User).Nickname ?? context.User.Username;
            gameMessage = await context.Channel.SendMessageAsync("", false, Embed("Pick your play and see if you can beat me!", $"{name} is playing."));
            await gameMessage.AddReactionAsync(new Emoji("📰"));
            await gameMessage.AddReactionAsync(new Emoji("✂"));
            await gameMessage.AddReactionAsync(new Emoji("🌑"));
            isPlaying = true;
            MessageID = gameMessage.Id;
            Player = (SocketGuildUser)context.User;
        }

        public async Task ViewPlay(string emote)
        {
            if (!isPlaying) return;

            string playOne = "";
            if (emote == "📰")
                playOne = "Paper";
            else if (emote == "✂")
                playOne = "Scissors";
            else if (emote == "🌑")
                playOne = "Rock";

            string playTwo = Plays.ElementAt(Utilities.GetRandomNumber(0, 3));
            string result = GetWinner(playOne[0], playTwo[0]);

            // Update the game message to show the winner and remove the reactions
            await gameMessage.ModifyAsync(m => { m.Embed = Embed($"{Player.Mention} chose {playOne}!\n\nI chose {playTwo}.\n\n{result}", ""); });
            await gameMessage.RemoveAllReactionsAsync();

            if(result.Contains("lose 3 coins"))
                CoinsHandler.AdjustCoins(Player, -3);
            else if (result.Contains("got 3 coins"))
                CoinsHandler.AdjustCoins(Player, 3);

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
