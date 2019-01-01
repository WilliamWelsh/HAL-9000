using System;
using Discord;
using Gideon.Handlers;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Gideon
{
    class Program
    {
        public static DiscordSocketClient _client;
        EventHandler _handler;

        static void Main(string[] args) => new Program().StartAsync().GetAwaiter().GetResult();

        public async Task StartAsync()
        {
            if (Config.bot.DisordBotToken == "" || Config.bot.DisordBotToken == null) return;
            _client = new DiscordSocketClient(new DiscordSocketConfig { LogLevel = LogSeverity.Verbose });
            _client.Log += Log;
			_client.ReactionAdded += OnReactionAdded;
			await _client.LoginAsync(TokenType.Bot, Config.bot.DisordBotToken);
            await _client.StartAsync();
            _handler = new EventHandler();
            await _handler.InitializeAsync(_client);
            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.Message);
            return Task.CompletedTask;
        }

        // If someone adds a reaction, check to see if it's for a minigame that's being played
        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
		{
			if (MinigameHandler.RPS.MessageID == reaction.MessageId)
				await MinigameHandler.RPS.ViewPlay(reaction.Emote.ToString(), channel, reaction.User);
			else
				await MinigameHandler.TTT.Play(reaction, channel, reaction.User);
		}
	}
}