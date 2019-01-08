using System;
using Discord;
using Gideon.Handlers;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Gideon
{
    class Program
    {
        DiscordSocketClient _client;

        static void Main(string[] args) => new Program().StartAsync().GetAwaiter().GetResult();

        public async Task StartAsync()
        {
            if (Config.bot.DisordBotToken == "" || Config.bot.DisordBotToken == null) return;
            _client = new DiscordSocketClient(new DiscordSocketConfig { LogLevel = LogSeverity.Verbose });
            _client.Log += Log;
			_client.ReactionAdded += OnReactionAdded;
			await _client.LoginAsync(TokenType.Bot, Config.bot.DisordBotToken);
            await _client.StartAsync();
            await _client.SetGameAsync("users", null, ActivityType.Listening);
            EventHandler _handler = new EventHandler();
            await _handler.InitializeAsync(_client);
            await Task.Delay(-1).ConfigureAwait(false);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.Message);
            return Task.CompletedTask;
        }

        // If someone adds a reaction, check to see if it's for a minigame that's being played
        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel Channel, SocketReaction Reaction)
		{
            if (((SocketUser)Reaction.User).IsBot) return;

            // If Unbeatable TTT is being played, and the person that added the reaction is the player, then send it
            if (MinigameHandler.UnbeatableTTT.isGameGoing && MinigameHandler.UnbeatableTTT.Player.Id == Reaction.UserId)
                await MinigameHandler.UnbeatableTTT.Play(Reaction);

            // Rock-Paper-Scissors
            if (MinigameHandler.RPS.MessageID == Reaction.MessageId && MinigameHandler.RPS.Player.Id == Reaction.UserId)
                await MinigameHandler.RPS.ViewPlay(Reaction.Emote.ToString(), Channel);

            // Tic-Tac-Toe
            if (MinigameHandler.TTT.GameMessage.Id == Reaction.MessageId)
                await MinigameHandler.TTT.Play(Reaction, Reaction.User);
        }
	}
}