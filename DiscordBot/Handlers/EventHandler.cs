using System;
using Discord;
using System.Linq;
using Gideon.Handlers;
using Discord.Commands;
using System.Reflection;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Gideon
{
    class EventHandler
    {
        DiscordSocketClient _client;
        CommandService _service;

        public async Task InitializeAsync(DiscordSocketClient client)
        {
            _client = client;
            _service = new CommandService();
            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), null);

            _client.MessageReceived += HandleCommandAsync;

            _client.UserBanned += HandleUserBanned;
            _client.UserUnbanned += HandleUserUnbanned;

            _client.UserJoined += HandleUserJoining;
            _client.UserLeft += HandleUserLeaving;

            _service.Log += Log;
        }

        private Task Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        // Send a message in #general saying a user has been banned
        private async Task HandleUserUnbanned(SocketUser arg1, SocketGuild arg2)
        {
            await Utilities.SendEmbed(arg2.GetTextChannel(294699220743618561), "Pardon", $"{arg1} has been unbanned.", Colors.Green, "", arg1.GetAvatarUrl());
        }

        private async Task HandleUserBanned(SocketUser arg1, SocketGuild arg2)
        {
            var bans = arg2.GetBansAsync().Result.ToList();
            string reason = "";
            foreach (var ban in bans)
                if (ban.User.Id == arg1.Id)
                    reason = ban.Reason;
            if (reason == "")
                await Utilities.SendEmbed(arg2.GetTextChannel(294699220743618561), "Ban", $"{arg1} has been banned.", Colors.Red, "", arg1.GetAvatarUrl());
            else
                await Utilities.SendEmbed(arg2.GetTextChannel(294699220743618561), "Ban", $"{arg1} has been banned for {reason}.", Colors.Red, "", arg1.GetAvatarUrl());
        }

        private async Task HandleUserJoining(SocketGuildUser arg)
        {
            if (arg.IsBot)
            {
                await (arg as IGuildUser).AddRoleAsync(arg.Guild.Roles.FirstOrDefault(x => x.Name == "Bot"));
                await Utilities.SendEmbed(arg.Guild.GetTextChannel(294699220743618561), "New Bot", $"The {arg.Username} bot has been added to the server.", Colors.Green, "", arg.GetAvatarUrl());
                return;
            }
            string desc = $"{arg} has joined the server.";
            if (UserAccounts.GetAccount(arg).level != 0)
            {
                string rank = RankHandler.LevelToRank(UserAccounts.GetAccount(arg).level);
                await (arg as IGuildUser).AddRoleAsync(arg.Guild.Roles.FirstOrDefault(x => x.Name == rank));
                desc += $" Their rank has been restored to {rank}.";
            }
            await Utilities.SendEmbed(arg.Guild.GetTextChannel(294699220743618561), "New User", desc, Colors.Green, "", arg.GetAvatarUrl());
        }

        private async Task HandleUserLeaving(SocketGuildUser arg)
        {
            if (arg.IsBot)
                await Utilities.SendEmbed(arg.Guild.GetTextChannel(294699220743618561), "Bot Removed", $"The {arg.Username} bot has been removed from the server.", Colors.Red, "", arg.GetAvatarUrl());
            else
                await Utilities.SendEmbed(arg.Guild.GetTextChannel(294699220743618561), "User Left", $"{arg} has left the server.", Colors.Red, "", arg.GetAvatarUrl());
        }

        string[] spellingMistakes = { "should of", "would of", "wouldnt of", "wouldn't of", "would not of", "couldnt of", "couldn't of", "could not of", "better of", "shouldnt of", "shouldn't of", "should not of", "alot", "could of" };
        string[] spellingFix = { "should have", "would have", "wouldn't have", "wouldn't have", "would not have", "couldn't have", "couldn't have", "could not have", "better have", "shouldn't have", "shouldn't have", "should not have", "a lot", "could have" };

        private async Task HandleCommandAsync(SocketMessage s)
        {
            SocketUserMessage msg = s as SocketUserMessage;
            if (msg == null || msg.Author.IsBot) return;

            var context = new SocketCommandContext(_client, msg);

            await RankHandler.TryToGiveUserXP(context, msg.Author);

            int argPos = 0;
            if (msg.HasStringPrefix("!", ref argPos))
                await _service.ExecuteAsync(context, argPos, null, MultiMatchHandling.Exception);

            string m = msg.Content.ToLower();

            // Answer minigames
            if (context.Channel.Id == 518846214603669537)
            {
                // Answer Trivia
                if (m == "a" || m == "b" || m == "c" || m == "d")
                    await MinigameHandler.Trivia.AnswerTrivia((SocketGuildUser)msg.Author, context, m);

                // Answer "Who Said It?"
                int x = 0;
                if (int.TryParse(m, out x))
                    if (x <= 4 && x >= 1 && MinigameHandler.WSI.isGameGoing)
                        await MinigameHandler.WSI.TryToGuess(context, x);
            }

            // Print a lennyface
            if (m.Contains("lennyface"))
                await context.Channel.SendMessageAsync("( ͡° ͜ʖ ͡°)");

            // Fix some spelling mistakes
            for (int i = 0; i < spellingMistakes.Length; i++)
                if (m.Contains(spellingMistakes[i]))
                    await msg.Channel.SendMessageAsync(spellingFix[i] + "*");

            // Print a DM message to console
            if (s.Channel.Name.StartsWith("@"))
                Console.WriteLine($" ----------\n DIRECT MESSAGE\n From: {s.Channel}\n {s}\n ----------");
        }
    }
}