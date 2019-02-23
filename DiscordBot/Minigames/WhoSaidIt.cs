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
    class WhoSaidIt
    {
        public bool isGameGoing;

        private readonly List<string> availableOptions = new List<string>();
        private string Speaker;

        private SocketUser Player;

        public async Task TryToStartGame(SocketCommandContext context)
        {
            if (!await Utilities.CheckForChannel(context, 518846214603669537, context.User)) return;
            else if (isGameGoing)
            {
                await Utilities.PrintError(context.Channel, $"Sorry, {context.User.Mention} a game is currently ongoing.\nYou can ask an admin to `!reset wsi` if there is an issue.");
                return;
            }

            isGameGoing = true;
            Player = context.User;
            WSIQuote Quote = MinigameHandler.WhoSaidItResources.Quotes[Utilities.GetRandomNumber(0, MinigameHandler.WhoSaidItResources.Quotes.Length)];

            Speaker = Quote.Speaker;
            availableOptions.Add(Speaker);

            // Add 3 other random options
            do
            {
                string option = MinigameHandler.WhoSaidItResources.Options[Utilities.GetRandomNumber(1, MinigameHandler.WhoSaidItResources.Options.Length)];
                if (!availableOptions.Contains(option))
                    availableOptions.Add(option);
            } while (availableOptions.Count != 4);

            // Shuffle the list
            Random rng = new Random();
            int n = availableOptions.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                var value = availableOptions[k];
                availableOptions[k] = availableOptions[n];
                availableOptions[n] = value;
            }

            // Write options
            StringBuilder options = new StringBuilder();
            for (int i = 0; i < availableOptions.Count; i++)
                options.AppendLine($"`{i+1}`. {availableOptions[i]}");

            await context.Channel.SendMessageAsync("", false, new EmbedBuilder()
                .WithTitle("Who Said It?")
                .WithDescription(Quote.QuoteQuote)
                .WithColor(Colors.Green)
                .WithFooter($"Only {((SocketGuildUser)context.User).Nickname ?? context.User.Username} can answer.")
                .AddField("Options", options)
                .Build());
        }

        public async Task TryToGuess(SocketCommandContext context, int number)
        {
            if (Player != context.User)
            {
                await Utilities.PrintError(context.Channel, $"Sorry, {Player.Mention} is currently playing.\nYou can ask an admin to `!reset wsi` if there is an issue.");
                return;
            }
            if (availableOptions.ElementAt(number-1) == Speaker)
            {
                await Utilities.SendEmbed(context.Channel, "Who Said It?", "Correct!", Colors.Green, $"{((SocketGuildUser)context.User).Nickname ?? context.User.Username} got 1 coin.", "");
                CoinsHandler.AdjustCoins((SocketGuildUser)context.User, 1);
            }
            else
            {
                await Utilities.SendEmbed(context.Channel, "Who Said It?", "Incorrect.", Colors.Green, $"{((SocketGuildUser)context.User).Nickname ?? context.User.Username} lost 1 coin.", "");
                CoinsHandler.AdjustCoins((SocketGuildUser)context.User, -1);
            }
            Reset();
        }

        public void Reset()
        {
            availableOptions.Clear();
            isGameGoing = false;
            Speaker = "";
        }
    }
}