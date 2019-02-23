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

        SocketTextChannel GeneralChat;

        public async Task InitializeAsync(DiscordSocketClient client)
        {
            _client = client;
            _service = new CommandService();

            await Task.Delay(3000).ConfigureAwait(false); // Allow time to log in
            GeneralChat = client.GetGuild(294699220743618561).GetTextChannel(294699220743618561);

            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), null);

            _client.MessageReceived += HandleCommandAsync;

            _client.UserBanned += HandleUserBanned;
            _client.UserUnbanned += HandleUserUnbanned;

            _client.UserJoined += HandleUserJoining;
            _client.UserLeft += HandleUserLeaving;

            _client.GuildMemberUpdated += HandleGuildMemberUpdate;

            _service.Log += Log;
        }

        // Notify me when one of MY bots goes offline
        private async Task HandleGuildMemberUpdate(SocketGuildUser arg1, SocketGuildUser arg2)
        {
            if (arg1.IsBot && arg1.Status == UserStatus.Online && arg2.Status == UserStatus.Offline && Config.MyBots.Contains(arg1.Id))
            {
                await GeneralChat.SendMessageAsync("@Reverse#0666 ");
                await Utilities.SendEmbed(GeneralChat, "Bot Error", $"{arg1.Mention} has gone offline!", Colors.Red, "", arg1.GetAvatarUrl());
            }
        }

        private async Task HandleUserUpdate(SocketUser arg1, SocketUser arg2)
        {
            //Console.WriteLine("X");
        }

        private Task Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        // Send a message in #general saying a user has been unbanned
        private async Task HandleUserUnbanned(SocketUser arg1, SocketGuild arg2)
        {
            await Utilities.SendEmbed(arg2.GetTextChannel(294699220743618561), "Pardon", $"{arg1} has been unbanned.", Colors.Green, arg1.Id.ToString(), arg1.GetAvatarUrl());
        }

        // Send a message in #general saying a user has been banned
        private async Task HandleUserBanned(SocketUser arg1, SocketGuild arg2)
        {
            var bans = arg2.GetBansAsync().Result.ToList();
            string reason = "";
            foreach (var ban in bans)
                if (ban.User.Id == arg1.Id)
                    reason = ban.Reason;
            if (reason == "")
                await Utilities.SendEmbed(arg2.GetTextChannel(294699220743618561), "Ban", $"{arg1} has been banned.", Colors.Red, arg1.Id.ToString(), arg1.GetAvatarUrl());
            else
                await Utilities.SendEmbed(arg2.GetTextChannel(294699220743618561), "Ban", $"{arg1} has been banned for {reason}.", Colors.Red, arg1.Id.ToString(), arg1.GetAvatarUrl());
        }

        // Send a message in #general saying a user as joined
        private async Task HandleUserJoining(SocketGuildUser arg)
        {
            if (arg.IsBot)
            {
                await (arg as IGuildUser).AddRoleAsync(arg.Guild.Roles.FirstOrDefault(x => x.Name == "Bot"));
                await Utilities.SendEmbed(arg.Guild.GetTextChannel(294699220743618561), "New Bot", $"The {arg.Username} bot has been added to the server.", Colors.Green, arg.Id.ToString(), arg.GetAvatarUrl());
                return;
            }
            string desc = $"{arg} has joined the server.";
            if (UserAccounts.GetAccount(arg).level != 0)
            {
                string rank = RankHandler.LevelToRank(UserAccounts.GetAccount(arg).level);
                await (arg as IGuildUser).AddRoleAsync(arg.Guild.Roles.FirstOrDefault(x => x.Name == rank));
                desc += $" Their rank has been restored to {rank}.";
            }
            await Utilities.SendEmbed(arg.Guild.GetTextChannel(294699220743618561), "New User", desc, Colors.Green, arg.Id.ToString(), arg.GetAvatarUrl());
        }

        // Send a message in #general saying a user as left
        private async Task HandleUserLeaving(SocketGuildUser arg)
        {
            // If they're banned, don't show that they're leaving
            var bans = arg.Guild.GetBansAsync().Result.ToList();
            foreach (var ban in bans)
                if (ban.User.Id == arg.Id)
                    return;

            if (arg.IsBot)
                await Utilities.SendEmbed(arg.Guild.GetTextChannel(294699220743618561), "Bot Removed", $"The {arg.Username} bot has been removed from the server.", Colors.Red, arg.Id.ToString(), arg.GetAvatarUrl());
            else
                await Utilities.SendEmbed(arg.Guild.GetTextChannel(294699220743618561), "User Left", $"{arg} has left the server.", Colors.Red, arg.Id.ToString(), arg.GetAvatarUrl());
        }

        // Spelling mistakes and corresponding fixes
        readonly string[] spellingMistakes = { "should of", "would of", "wouldnt of", "wouldn't of", "would not of", "couldnt of", "couldn't of", "could not of", "better of", "shouldnt of", "shouldn't of", "should not of", "alot", "could of" };
        readonly string[] spellingFix = { "should have", "would have", "wouldn't have", "wouldn't have", "would not have", "couldn't have", "couldn't have", "could not have", "better have", "shouldn't have", "shouldn't have", "should not have", "a lot", "could have" };

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