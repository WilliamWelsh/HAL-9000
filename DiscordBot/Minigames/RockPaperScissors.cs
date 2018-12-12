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
        bool isPlaying;
        private SocketGuildUser Player;
        private List<string> Plays = new List<string>(new string[] { "Rock", "Paper", "Scissors" });
        static Random rnd = new Random();

        private Embed Embed (string description, string footer)
        {
            var Embed = new EmbedBuilder();
            Embed.WithTitle("Rock-Paper-Scissors");
            Embed.WithDescription(description);
            Embed.WithFooter(footer);
            Embed.WithColor(new Color(251, 233, 231));
            Embed.WithThumbnailUrl("https://i.imgur.com/VXdDjho.png");
            return Embed;
        }

        public async Task StartRPS(SocketCommandContext context)
        {
            if (context.Channel.Id != 518846214603669537)
            {
                await Config.Utilities.PrintError(context, $"Please use the {context.Guild.GetTextChannel(518846214603669537).Mention} chat for that, {context.User.Mention}.");
                return;
            }
            string name = ((SocketGuildUser)context.User).Nickname ?? context.User.Username;
            RestUserMessage m = await context.Channel.SendMessageAsync("", false, Embed("Pick your play and see if you can beat me!", $"{name} is playing."));
            await m.AddReactionAsync(new Emoji("📰"));
            await m.AddReactionAsync(new Emoji("✂"));
            await m.AddReactionAsync(new Emoji("🌑"));
            MessageID = m.Id;
            isPlaying = true;
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

            string playTwo = Plays.ElementAt(rnd.Next(Plays.Count));

            string result = GetWinner(playOne, playTwo);
            await channel.SendMessageAsync("", false, Embed($"{Player.Mention} chose {playOne}!\n\nI chose {playTwo}.\n\n{result}", ""));

            if(result.Contains("lose 3 Coins"))
                Config.CoinHandler.AdjustCoins(Player, -3);
            else if (result.Contains("got 3 Coins"))
                Config.CoinHandler.AdjustCoins(Player, 3);

            Player = null;
            isPlaying = false;
        }

        private string GetWinner(string p1, string p2)
        {
            if (p1 == p2) return "It's a draw!";
            if ((p1[0] == 'S' && p2[0] == 'P') ||
                (p1[0] == 'P' && p2[0] == 'R') ||
                (p1[0] == 'R' && p2[0] == 'S'))
                return $"{Player.Mention} won and got 3 Coins!";
            return "I won! You lose 3 Coins.";
        }

    }
}
