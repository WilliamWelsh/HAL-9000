using System;
using Discord;
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
        SocketGuildUser Owner;
        SocketTextChannel LogChat;

        public async Task InitializeAsync(DiscordSocketClient client)
        {
            _client = client;
            _service = new CommandService();

            await Task.Delay(3000).ConfigureAwait(false); // Allow time to log in

            Guild = client.GetGuild(294699220743618561); // My server
            Owner = Guild.GetUser(354458973572956160); // Me (the owner, bot master or whatever)
            LogChat = Guild.GetTextChannel(575737318904823808); // General channel

            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), null);

            _client.MessageReceived += HandleCommandAsync;

            _client.GuildMemberUpdated += HandleGuildMemberUpdate;

            _service.Log += Log;
        }

        // Notify me when one of MY bots goes offline
        private async Task HandleGuildMemberUpdate(SocketGuildUser before, SocketGuildUser after)
        {
            // If the SocketGuildUser isn't my bot, then we don't care
            if (!Config.MyBots.Contains(before.Id)) return;

            // One of my bots has gone offline
            if (before.Status == UserStatus.Online && after.Status == UserStatus.Offline)
            {
                await Utilities.SendEmbed(LogChat, "", $"Restarted {before.Mention}.", Utilities.ClearColor, "", before.GetAvatarUrl());
                Config.RestartBot(before.Id);
            }
        }

        // Log command-related logs
        private Task Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        // Spelling mistakes and corresponding fixes
        readonly string[] spellingMistakes = { "should of", "would of", "wouldnt of", "wouldn't of", "would not of", "couldnt of", "couldn't of", "could not of", "better of", "shouldnt of", "shouldn't of", "should not of", "alot", "could of" };
        readonly string[] spellingFix = { "should have", "would have", "wouldn't have", "wouldn't have", "would not have", "couldn't have", "couldn't have", "could not have", "better have", "shouldn't have", "shouldn't have", "should not have", "a lot", "could have" };

        private async Task HandleCommandAsync(SocketMessage s)
        {
            if (!(s is SocketUserMessage msg) || msg.Author.IsBot) return;

            var context = new SocketCommandContext(_client, msg);

            string m = msg.Content.ToLower();

            int argPos = 0;
            if (msg.HasStringPrefix("!", ref argPos))
                await _service.ExecuteAsync(context, argPos, null, MultiMatchHandling.Exception);

            // Answer minigames
            if (context.Channel.Id == 518846214603669537)
            {
                // Answer Trivia
                if (m == "a" || m == "b" || m == "c" || m == "d")
                    await MinigameHandler.Trivia.AnswerTrivia((SocketGuildUser)msg.Author, context, m);

                // Answer "Who Said It?"
                int x;
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
                    await msg.Channel.SendMessageAsync($"{spellingFix[i]}*");

            // Print a DM message to console
            if (s.Channel.Name.StartsWith("@"))
                if (s.Author.Id != 354458973572956160) // Me
                    await Owner.SendMessageAsync($"FROM: `{s.Author}`\n\n{s}");
        }
    }
}