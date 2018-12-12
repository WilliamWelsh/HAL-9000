using System;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace Gideon
{
    class Program
    {
        DiscordSocketClient _client;
        CommandHandler _handler;

        static void Main(string[] args)
        => new Program().StartAsync().GetAwaiter().GetResult();

        public async Task StartAsync()
        {
            if (Config.bot.DisordBotToken == "" || Config.bot.DisordBotToken == null) return;
            _client = new DiscordSocketClient(new DiscordSocketConfig { LogLevel = LogSeverity.Verbose });
            _client.Log += Log;
			_client.ReactionAdded += OnReactionAdded;
			await _client.LoginAsync(TokenType.Bot, Config.bot.DisordBotToken);
            await _client.StartAsync();
            _handler = new CommandHandler();
            await _handler.InitializeAsync(_client);
            //ConsoleInput();
            await Task.Delay(-1);
        }

        //private async Task ConsoleInput()
        //{
        //    var input = string.Empty;
        //    while (input.Trim().ToLower() != "block")
        //    {
        //        input = Console.ReadLine();
        //        if (input.Trim().ToLower() == "message")
        //            ConsoleSendMessage();
        //    }
        //}

        //private async void ConsoleSendMessage()
        //{
        //    Console.WriteLine("Select the guild:");
        //    var guild = GetSelectedGuild(_client.Guilds);
        //    var textChannel = GetSelectedTextChannel(guild.TextChannels);
        //    var msg = string.Empty;
        //    while (msg.Trim() == string.Empty)
        //    {
        //        Console.WriteLine("Your message:");
        //        msg = Console.ReadLine();
        //    }

        //    await textChannel.SendMessageAsync(msg);
        //}

        //private SocketTextChannel GetSelectedTextChannel(IEnumerable<SocketTextChannel> channels)
        //{
        //    var textChannels = channels.ToList();
        //    var maxIndex = textChannels.Count - 1;
        //    for (var i = 0; i <= maxIndex; i++)
        //    {
        //        Console.WriteLine($"{i} - {textChannels[i].Name}");
        //    }

        //    var selectedIndex = -1;
        //    while (selectedIndex < 0 || selectedIndex > maxIndex)
        //    {
        //        var success = int.TryParse(Console.ReadLine().Trim(), out selectedIndex);
        //        if (!success)
        //        {
        //            Console.WriteLine("That was an invalid index, try again.");
        //            selectedIndex = -1;
        //        }
        //    }

        //    return textChannels[selectedIndex];
        //}

        //private SocketGuild GetSelectedGuild(IEnumerable<SocketGuild> guilds)
        //{
        //    var socketGuilds = guilds.ToList();
        //    var maxIndex = socketGuilds.Count - 1;
        //    for (var i = 0; i <= maxIndex; i++)
        //    {
        //        Console.WriteLine($"{i} - {socketGuilds[i].Name}");
        //    }

        //    var selectedIndex = -1;
        //    while (selectedIndex < 0 || selectedIndex > maxIndex)
        //    {
        //        var success = int.TryParse(Console.ReadLine().Trim(), out selectedIndex);
        //        if (!success)
        //        {
        //            Console.WriteLine("That was an invalid index, try again.");
        //            selectedIndex = -1;
        //        }
        //    }

        //    return socketGuilds[selectedIndex];
        //}

        //private async Task ConsoleSendMessage(string input)
        //{
        //    var guilds = _client.Guilds.ToList();
        //    SocketGuild CoinsServer = null;
        //    foreach (SocketGuild g in guilds)
        //        if (g.Id == 333843634606702602)
        //            CoinsServer = g;
        //    await CoinsServer.GetTextChannel(339887750683688965).SendMessageAsync("");
        //}

        private async Task Log(LogMessage msg) => Console.WriteLine(msg.Message);

		private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
		{
			if (Config.MinigameHandler.RPS.MessageID == reaction.MessageId)
			{
				await Config.MinigameHandler.RPS.ViewPlay(reaction.Emote.ToString(), channel, reaction.User);
			}
			else
				await Config.TTT.Play(reaction, channel, reaction.User);
		}
	}
}