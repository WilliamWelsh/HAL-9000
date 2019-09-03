using System;
using Discord;
using System.IO;
using Discord.WebSocket;
using DiscordBot.Handlers;
using System.Threading.Tasks;

namespace DiscordBot
{
    class Program
    {
        DiscordSocketClient client;

        static void Main(string[] args) => new Program().StartAsync().GetAwaiter().GetResult();

        public async Task StartAsync()
        {
            if (string.IsNullOrEmpty(File.ReadAllText("Resources/token.txt"))) return;

            Config.Setup();

            client = new DiscordSocketClient(new DiscordSocketConfig { LogLevel = LogSeverity.Verbose });
            client.Log += Log;
            client.ReactionAdded += OnReactionAdded;

			await client.LoginAsync(TokenType.Bot, File.ReadAllText("Resources/token.txt"));
            await client.StartAsync();
            await client.SetGameAsync("on Mars", null, ActivityType.Playing);

            EventHandler _handler = new EventHandler();
            await _handler.InitializeAsync(client);

            await Task.Delay(-1).ConfigureAwait(false);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.Message);
            return Task.CompletedTask;
        }

        // If someone adds a reaction, check to see if it's for a minigame that's being played
        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel Channel, SocketReaction Reaction)
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