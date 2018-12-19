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
    class WhoSaidIt
    {
        public bool isGameGoing = false;
        private static readonly Color color = new Color(31, 139, 76);

        private List<string> availableOptions = new List<string>();
        private string Speaker;

        private static SocketUser Player;

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
            int quoteIndex = Utilities.GetRandomNumber(0, Config.whoSaidItResources.Quotes.Count);
            string options = "";

            Speaker = Config.whoSaidItResources.Quotes[quoteIndex].Speaker;
            availableOptions.Add(Speaker);

            // Add 3 other random options
            do
            {
                string option = Config.whoSaidItResources.Options[Utilities.GetRandomNumber(1, Config.whoSaidItResources.Options.Count)];
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
            for (int i = 0; i < availableOptions.Count; i++)
                options += $"`{i+1}`. {availableOptions[i]}\n";

            EmbedBuilder embed = new EmbedBuilder();
            {
                embed.WithTitle("Who Said It?");
                embed.WithColor(color);
                embed.WithFooter($"Only {((SocketGuildUser)context.User).Nickname ?? context.User.Username} can answer.");
                embed.AddField("Quote", Config.whoSaidItResources.Quotes[quoteIndex].Quote);
                embed.AddField("Options", options);
            }
            await context.Channel.SendMessageAsync("", false, embed);
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
                await context.Channel.SendMessageAsync("", false, Utilities.Embed("Who Said It?", "Correct!", color, $"{((SocketGuildUser)context.User).Nickname ?? context.User.Username} got 1 coin.", ""));
                CoinsHandler.AdjustCoins((SocketGuildUser)context.User, 1);
            }
            else
            {
                await context.Channel.SendMessageAsync("", false, Utilities.Embed("Who Said It?", "Incorrect.", color, $"{((SocketGuildUser)context.User).Nickname ?? context.User.Username} lost 1 coin.", ""));
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