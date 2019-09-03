using System;
using Discord;
using System.IO;
using System.Linq;
using Gideon.Handlers;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Gideon
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
            //client.Ready += OnReadyAsync;
            client.ReactionAdded += OnReactionAdded;

			await client.LoginAsync(TokenType.Bot, File.ReadAllText("Resources/token.txt"));
            await client.StartAsync();
            //await client.SetGameAsync("users", null, ActivityType.Listening);

            EventHandler _handler = new EventHandler();
            await _handler.InitializeAsync(client);

            await Task.Delay(-1).ConfigureAwait(false);
        }

        // Color Roles
        //private IRole Red, Blue, Pink, Teal, Green, Purple, Yellow, Orange;

        //private Task OnReadyAsync()
        //{
        //    // Set up the colored role variables
        //    var roles = client.Guilds.ElementAt(0).Roles;
        //    Red = roles.FirstOrDefault(x => x.Name == "Red");
        //    Blue = roles.FirstOrDefault(x => x.Name == "Blue");
        //    Pink = roles.FirstOrDefault(x => x.Name == "Pink");
        //    Teal = roles.FirstOrDefault(x => x.Name == "Teal");
        //    Green = roles.FirstOrDefault(x => x.Name == "Green");
        //    Purple = roles.FirstOrDefault(x => x.Name == "Purple");
        //    Yellow = roles.FirstOrDefault(x => x.Name == "Yellow");
        //    Orange = roles.FirstOrDefault(x => x.Name == "Orange");
        //    Console.WriteLine("Set up colored role variables.");
        //    return Task.CompletedTask;
        //}

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.Message);
            return Task.CompletedTask;
        }

        private readonly ulong ColorRoleMessageID = 575743637745303584;

        // If someone adds a reaction, check to see if it's for a minigame that's being played
        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel Channel, SocketReaction Reaction)
		{
            if (((SocketUser)Reaction.User).IsBot) return;

            // If Unbeatable TTT is being played, and the person that added the reaction is the player, then send it
            await MinigameHandler.ReactToAITicTacToe(Reaction.UserId, Reaction);

            // Rock-Paper-Scissors
            if (MinigameHandler.RPS.MessageID == Reaction.MessageId && MinigameHandler.RPS.Player.Id == Reaction.UserId)
                await MinigameHandler.RPS.ViewPlay(Reaction.Emote.ToString(), Channel);

            //if (arg1.Id == ColorRoleMessageID)
            //{
            //    string emote = Reaction.Emote.Name;
            //    var user = (SocketGuildUser)Reaction.User;
            //    if (emote == "1\u20e3")
            //        await user.AddRoleAsync(Red);
            //    else if (emote == "2\u20e3")
            //        await user.AddRoleAsync(Blue);
            //    else if (emote == "3\u20e3")
            //        await user.AddRoleAsync(Pink);
            //    else if (emote == "4\u20e3")
            //        await user.AddRoleAsync(Teal);
            //    else if (emote == "5\u20e3")
            //        await user.AddRoleAsync(Green);
            //    else if (emote == "6\u20e3")
            //        await user.AddRoleAsync(Purple);
            //    else if (emote == "7\u20e3")
            //        await user.AddRoleAsync(Yellow);
            //    else if (emote == "8\u20e3")
            //        await user.AddRoleAsync(Orange);
            //    else if (emote == "9\u20e3")
            //    {
            //        await user.RemoveRolesAsync(user.Roles);
            //    }
            //}

            // Tic-Tac-Toe
            if (MinigameHandler.TTT.GameMessage.Id == Reaction.MessageId)
                await MinigameHandler.TTT.Play(Reaction, Reaction.User);
        }
	}
}