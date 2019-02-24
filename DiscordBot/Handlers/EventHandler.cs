using System;
using Discord;
using System.Linq;
using Discord.Rest;
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

        SocketGuild Guild;
        SocketTextChannel GeneralChat;

        public async Task InitializeAsync(DiscordSocketClient client)
        {
            _client = client;
            _service = new CommandService();

            await Task.Delay(3000).ConfigureAwait(false); // Allow time to log in

            Guild = client.GetGuild(294699220743618561);
            GeneralChat = Guild.GetTextChannel(294699220743618561);

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
        private async Task HandleGuildMemberUpdate(SocketGuildUser before, SocketGuildUser after)
        {
            // If the SocketGuildUser isn't my bot, then we don't care
            if (!before.IsBot || !Config.MyBots.Contains(before.Id)) return;

            if (before.Status == UserStatus.Online && after.Status == UserStatus.Offline)
            {
                await GeneralChat.SendMessageAsync("<@354458973572956160> ");
                await Utilities.SendEmbed(GeneralChat, "Bot Error", $"{before.Mention} has gone offline!", Colors.Red, "", before.GetAvatarUrl());
            }

            if (before.Status == UserStatus.Offline && after.Status == UserStatus.Online)
            {
                await Utilities.SendEmbed(GeneralChat, "Bot Online", $"{before.Mention} is back online!", Colors.Green, "", before.GetAvatarUrl());
            }
        }

        // Log command-related logs
        private Task Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        // Send a message in #general saying a user has been unbanned
        private async Task HandleUserUnbanned(SocketUser arg1, SocketGuild arg2)
        {
            await Utilities.SendEmbed(GeneralChat, "Pardon", $"{arg1} has been unbanned.", Colors.Green, "", arg1.GetAvatarUrl());
        }

        // Send a message in #general saying a user has been banned
        private async Task HandleUserBanned(SocketUser arg1, SocketGuild arg2)
        {
            var bans = arg2.GetBansAsync().Result.ToList();
            string reason = "";
            foreach (var ban in bans)
                if (ban.User.Id == arg1.Id)
                    reason = string.IsNullOrEmpty(ban.Reason) ? "" : $" for `{ban.Reason}`";
            await Utilities.SendEmbed(GeneralChat, "Ban", $"{arg1} has been banned{reason}.", Colors.Red, "", arg1.GetAvatarUrl());
        }

        // Send a message in #general saying a user as joined
        private async Task HandleUserJoining(SocketGuildUser arg)
        {
            if (arg.IsBot)
            {
                await (arg as IGuildUser).AddRoleAsync(arg.Guild.Roles.FirstOrDefault(x => x.Name == "Bot"));
                await Utilities.SendEmbed(GeneralChat, "New Bot", $"The {arg.Username} bot has been added to the server.", Colors.Green, "", arg.GetAvatarUrl());
                return;
            }
            string desc = $"{arg} has joined the server.";
            if (UserAccounts.GetAccount(arg).level != 0)
            {
                string rank = RankHandler.LevelToRank(UserAccounts.GetAccount(arg).level);
                await (arg as IGuildUser).AddRoleAsync(arg.Guild.Roles.FirstOrDefault(x => x.Name == rank));
                desc += $" Their rank has been restored to {rank}.";
            }
            await Utilities.SendEmbed(GeneralChat, "New User", desc, Colors.Green, "", arg.GetAvatarUrl());
        }

        // Send a message in #general saying a user as left or has been kicked
        private async Task HandleUserLeaving(SocketGuildUser arg)
        {
            // If they're banned, don't show that they're leaving
            var bans = arg.Guild.GetBansAsync().Result.ToList();
            foreach (var ban in bans)
                if (ban.User.Id == arg.Id)
                    return;

            // If they're kicked, send a kick message instead
            foreach (var log in await Guild.GetAuditLogsAsync(5).ToArray())
                foreach (var item in log.ToList())
                    if (item.Action == ActionType.Kick)
                        if ((item.Data as KickAuditLogData).Target.Id == arg.Id)
                        {
                            await HandleUserKicked(item).ConfigureAwait(false);
                            return;
                        }

            if (arg.IsBot)
                await Utilities.SendEmbed(GeneralChat, "Bot Removed", $"The {arg.Username} bot has been removed from the server.", Colors.Red, "", arg.GetAvatarUrl());
            else
                await Utilities.SendEmbed(GeneralChat, "User Left", $"{arg} has left the server.", Colors.Red, "", arg.GetAvatarUrl());
        }

        // Send a message in #general saying a use has been kicked
        private async Task HandleUserKicked(RestAuditLogEntry Log)
        {
            var Target = (Log.Data as KickAuditLogData).Target;
            var Kicker = Log.User.Mention;
            var Reason = Log.Reason ?? "No reason specified";
            await Utilities.SendEmbed(GeneralChat, "User Kicked", $"{Target} has been kicked from the server by {Kicker} for `{Reason}`.", Color.Red, "", Target.GetAvatarUrl());
        }

        // Spelling mistakes and corresponding fixes
        readonly string[] spellingMistakes = { "should of", "would of", "wouldnt of", "wouldn't of", "would not of", "couldnt of", "couldn't of", "could not of", "better of", "shouldnt of", "shouldn't of", "should not of", "alot", "could of" };
        readonly string[] spellingFix = { "should have", "would have", "wouldn't have", "wouldn't have", "would not have", "couldn't have", "couldn't have", "could not have", "better have", "shouldn't have", "shouldn't have", "should not have", "a lot", "could have" };

        private async Task HandleCommandAsync(SocketMessage s)
        {
            if (!(s is SocketUserMessage msg) || msg.Author.IsBot) return;

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
                if (int.TryParse(m, out int x))
                    if (x <= 4 && x >= 1 && MinigameHandler.WSI.isGameGoing)
                        await MinigameHandler.WSI.TryToGuess(context, x);
            }

            // Print a lennyface
            if (m.Contains("lennyface"))
                await context.Channel.SendMessageAsync("( ͡° ͜ʖ ͡°)");

            // Fix some spelling mistakes
            for (int i = 0; i < spellingMistakes.Length; i++)
                if (m.Contains(spellingMistakes[i]))
                    await msg.Channel.SendMessageAsync($"{spellingFix[i]}*");

            // Print a DM message to console
            if (s.Channel.Name.StartsWith("@"))
                Console.WriteLine($" ----------\n DIRECT MESSAGE\n From: {s.Channel}\n {s}\n ----------");
        }
    }
}