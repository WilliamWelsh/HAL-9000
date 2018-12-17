using Discord;
using Gideon.Minigames;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Gideon.Handlers
{
    class MinigameHandler
    {
        private static readonly Color color = new Color(31, 139, 76);

        public _8ball _8ball = new _8ball();
        public Trivia Trivia = new Trivia();
        public TicTacToe TTT = new TicTacToe();
        public NumberGuess NG = new NumberGuess();
        public RussianRoulette RR = new RussianRoulette();
        public RockPaperScissors RPS = new RockPaperScissors();

        public async Task DisplayGames(SocketCommandContext context)
        {
            if (!await Config.Utilities.CheckForChannel(context, 518846214603669537, context.User)) return;
            await context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("MiniGames", "Trivia\n`!trivia`\n\nTic-Tac-Toe\n`!ttt`\n\nNumber Guess\n`!play ng`\n\nRussian Roulette\n`!rr`\n\n8-Ball\n`!8ball`", new Color(31, 139, 76), "", ""));
        }

        public async Task TryToStartTrivia(SocketCommandContext context, string input)
        {
            if (input == "all")
                await Trivia.TryToStartTrivia((SocketGuildUser)context.User, context, "all");
        }

        // Have to call this from TicTacToe
        public void ResetTTT() => TTT = new TicTacToe();

        // Reset a game
        public async Task ResetGame(SocketCommandContext context, string game)
        {
            if (!await Config.Utilities.CheckForSuperadmin(context, context.User)) return;
            else if (game == "trivia")
            {
                await context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("MiniGames", $"{context.User.Mention} has reset Trivia.", color, "", ""));
                Trivia.ResetTrivia();
            }
            else if (game == "rr")
            {
                await context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("MiniGames", $"{context.User.Mention} has reset Russian Roulette.", color, "", ""));
                RR.Reset();
            }
            else if (game == "ttt")
            {
                await context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("MiniGames", $"{context.User.Mention} has reset Tic-Tac-Toe.", color, "", ""));
                ResetTTT();
            }
            else if (game == "ng")
            {
                await context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("MiniGames", $"{context.User.Mention} has reset the Number Guess game.", color, "", ""));
                NG.Reset();
            }
            else if (game == "")
                await Config.Utilities.PrintError(context.Channel, "Please specify a game to reset.");
            else
                await Config.Utilities.PrintError(context.Channel, $"I was unable to find the `{game}` game.\n\nAvailable games to reset:\nTrivia\n`!trivia`\n\nTic-Tac-Toe\n`!ttt`\n\nNumber Guess\n`!play ng`\n\nRussian Roulette\n`!rr`");
        }
    }
}