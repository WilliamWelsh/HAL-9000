using System;
using Discord;
using System.IO;
using Discord.Rest;
using Gideon.Handlers;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Gideon
{
    class Program
    {
        static void Main(string[] args) => new Program().StartAsync().GetAwaiter().GetResult();

        public async Task StartAsync()
        {
            if (string.IsNullOrEmpty(File.ReadAllText("Resources/token.txt"))) return;

            Config.Setup();

            DiscordSocketClient _client = new DiscordSocketClient(new DiscordSocketConfig { LogLevel = LogSeverity.Verbose });
            _client.Log += Log;
			_client.ReactionAdded += OnReactionAdded;
			await _client.LoginAsync(TokenType.Bot, File.ReadAllText("Resources/token.txt"));
            await _client.StartAsync();
            await _client.SetGameAsync("users", null, ActivityType.Listening);
            EventHandler _handler = new EventHandler();
            await _handler.InitializeAsync(_client);

            await Task.Delay(3000).ConfigureAwait(false); // Allowing time to log in
            await new PewdsVsTSeriesWatcher().SetUp(await _client.GetGuild(294699220743618561).GetTextChannel(548356800995524618).GetMessageAsync(548356888996216832) as RestUserMessage);

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
            await MinigameHandler.ReactToAITicTacToe(Reaction.UserId, Reaction);

            // Rock-Paper-Scissors
            if (MinigameHandler.RPS.MessageID == Reaction.MessageId && MinigameHandler.RPS.Player.Id == Reaction.UserId)
                await MinigameHandler.RPS.ViewPlay(Reaction.Emote.ToString(), Channel);

            // Tic-Tac-Toe
            if (MinigameHandler.TTT.GameMessage.Id == Reaction.MessageId)
                await MinigameHandler.TTT.Play(Reaction, Reaction.User);
        }
	}
}