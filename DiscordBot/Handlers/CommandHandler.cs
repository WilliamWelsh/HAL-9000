using System;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Gideon
{
    class CommandHandler
    {
        DiscordSocketClient _client;
        CommandService _service;

        public async Task InitializeAsync(DiscordSocketClient client)
        {
            _client = client;
            _service = new CommandService();
            await _service.AddModulesAsync(Assembly.GetEntryAssembly());
            _client.MessageReceived += HandleCommandAsync;
			_client.UserJoined += HandleUserJoining;
			_client.UserLeft += HandleUserLeaving;
            _client.UserBanned += HandleUserBanned;
            _client.UserUnbanned += HandleUserUnbanned;
        }

        private async Task HandleUserUnbanned(SocketUser arg1, SocketGuild arg2)
        {
            await arg2.GetTextChannel(294699220743618561).SendMessageAsync("", false, Config.Utilities.Embed("Pardon", $"{arg1} has been unbanned.", Config.Utilities.DomColorFromURL(arg1.GetAvatarUrl()), "", arg1.GetAvatarUrl()));
        }

        private async Task HandleUserBanned(SocketUser arg1, SocketGuild arg2)
        {
            await arg2.GetTextChannel(294699220743618561).SendMessageAsync("", false, Config.Utilities.Embed("Ban", $"{arg1} has been banned.", Config.Utilities.DomColorFromURL(arg1.GetAvatarUrl()), "", arg1.GetAvatarUrl()));
        }

        private async Task HandleUserJoining(SocketGuildUser arg)
		{
			await arg.Guild.GetTextChannel(294699220743618561).SendMessageAsync("", false, Config.Utilities.Embed("New User", $"{arg} has joined the server.", Config.Utilities.DomColorFromURL(arg.GetAvatarUrl()), "", arg.GetAvatarUrl()));
		}

		private async Task HandleUserLeaving(SocketGuildUser arg)
		{
            await arg.Guild.GetTextChannel(294699220743618561).SendMessageAsync("", false, Config.Utilities.Embed("User Left", $"{arg} has left the server.", Config.Utilities.DomColorFromURL(arg.GetAvatarUrl()), "", arg.GetAvatarUrl()));
		}

        private async Task HandleCommandAsync(SocketMessage s)
        {
            var msg = s as SocketUserMessage;
            if (msg == null || msg.Author.IsBot) return;

            var context = new SocketCommandContext(_client, msg);

            await Config.RankHandler.TryToGiveUserXP(context, msg.Author);

            int argPos = 0;
            if (msg.HasStringPrefix("!", ref argPos))
            {
                var result = await _service.ExecuteAsync(context, argPos);
                if (result.IsSuccess)
                {
                    //
                }
            }

            string m = msg.Content.ToLower();

            if (m == "!reset trivia")
            {
                if (!UserAccounts.GetAccount((SocketGuildUser)msg.Author).superadmin)
				{
                    await Config.Utilities.PrintError(context, $"You do not have permission to do that command, {msg.Author.Mention}.");
                    return;
                }
                await msg.Channel.SendMessageAsync($"{context.User.Mention} has reset Trivia.");
                Config.MinigameHandler.Trivia.ResetTrivia();
                return;
            }

            if (m == "!reset rr")
            {
                if (!UserAccounts.GetAccount((SocketGuildUser)msg.Author).superadmin)
				{
                    await Config.Utilities.PrintError(context, $"You do not have permission to do that command, {msg.Author.Mention}.");
                    return;
                }
                await msg.Channel.SendMessageAsync($"{context.User.Mention} has reset Russian Roulette.");
                Config.MinigameHandler.RR.Reset();
                return;
            }

            if (m == "!reset ttt")
            {
                if (!UserAccounts.GetAccount((SocketGuildUser)msg.Author).superadmin)
                {
                    await Config.Utilities.PrintError(context, $"You do not have permission to do that command, {msg.Author.Mention}.");
                    return;
                }
                await msg.Channel.SendMessageAsync($"{msg.Author.Mention} has reset Tic-Tac-Toe.");
				Config.ResetTTT();
                return;
            }

            if (m == "!reset ng")
            {
                if (!UserAccounts.GetAccount((SocketGuildUser)msg.Author).superadmin)
				{
                    await Config.Utilities.PrintError(context, $"You do not have permission to do that command, {msg.Author.Mention}.");
                    return;
                }
                Config.MinigameHandler.NG.Reset();
                return;
            }






            // Answer trivia
            if (m == "a" || m == "b" || m == "c" || m == "d")
            {
                if(context.Channel.Id == 518846214603669537)
                    await Config.MinigameHandler.Trivia.AnswerTrivia((SocketGuildUser)msg.Author, context, m);
            }

            if (m.Contains("lennyface"))
                await context.Channel.SendMessageAsync("( ͡° ͜ʖ ͡°)");

            string[] spellingMistakes = { "should of", "would of", "wouldnt of", "wouldn't of", "would not of", "couldnt of", "couldn't of", "could not of", "better of", "shouldnt of", "shouldn't of", "should not of", "alot", "could of" };
            string[] spellingFix = { "should have", "would have", "wouldn't have", "wouldn't have", "would not have", "couldn't have", "couldn't have", "could not have", "better have", "shouldn't have", "shouldn't have", "should not have", "a lot", "could have" };

            for (int i = 0; i < spellingMistakes.Length; i++)
            {
                if (m.Contains(spellingMistakes[i]))
                    await msg.Channel.SendMessageAsync(spellingFix[i] + "*");
            }
            
            string tempString = m.Replace("`", "").Replace(" ", "");
            foreach (string x in Config.botResources.bannedWords)
            {
                if(tempString.Contains(x))
                {
                    var messages = await context.Channel.GetMessagesAsync(1).Flatten();
                    await context.Channel.DeleteMessagesAsync(messages);
                    return;
                }
            }

            if (s.Channel.Name.StartsWith("@"))
                Console.WriteLine($" ----------\n DIRECT MESSAGE\n From: {s.Channel}\n {s}\n ----------");
        }
    }
}