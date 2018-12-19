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
        
        public static _8ball _8ball = new _8ball();
        public static Trivia Trivia = new Trivia();
        public static TicTacToe TTT = new TicTacToe();
        public static WhoSaidIt WSI = new WhoSaidIt();
        public static NumberGuess NG = new NumberGuess();
        public static RussianRoulette RR = new RussianRoulette();
        public static RockPaperScissors RPS = new RockPaperScissors();

        public static async Task DisplayGames(SocketCommandContext context)
        {
            if (!await Utilities.CheckForChannel(context, 518846214603669537, context.User)) return;
            await context.Channel.SendMessageAsync("", false, Utilities.Embed("MiniGames", "Trivia\n`!trivia`\n\nTic-Tac-Toe\n`!ttt`\n\nNumber Guess\n`!play ng`\n\nRussian Roulette\n`!rr`\n\n8-Ball\n`!8ball`", new Color(31, 139, 76), "", ""));
        }

        public async Task TryToStartTrivia(SocketCommandContext context, string input)
        {
            if (input == "all")
                await Trivia.TryToStartTrivia((SocketGuildUser)context.User, context, "all");
        }

        // Have to call this from TicTacToe
        public static void ResetTTT() => MinigameHandler.TTT = new TicTacToe();

        // Reset a game
        public static async Task ResetGame(SocketCommandContext context, string game)
        {
            if (!await Utilities.CheckForSuperadmin(context, context.User)) return;
            else if (game == "trivia")
            {
                await context.Channel.SendMessageAsync("", false, Utilities.Embed("MiniGames", $"{context.User.Mention} has reset Trivia.", color, "", ""));
                Trivia.ResetTrivia();
            }
            else if (game == "rr")
            {
                await context.Channel.SendMessageAsync("", false, Utilities.Embed("MiniGames", $"{context.User.Mention} has reset Russian Roulette.", color, "", ""));
                RR.Reset();
            }
            else if (game == "ttt")
            {
                await context.Channel.SendMessageAsync("", false, Utilities.Embed("MiniGames", $"{context.User.Mention} has reset Tic-Tac-Toe.", color, "", ""));
                ResetTTT();
            }
            else if (game == "ng")
            {
                await context.Channel.SendMessageAsync("", false, Utilities.Embed("MiniGames", $"{context.User.Mention} has reset the Number Guess game.", color, "", ""));
                NG.Reset();
            }
            else if (game == "wsi")
            {
                await context.Channel.SendMessageAsync("", false, Utilities.Embed("MiniGames", $"{context.User.Mention} has reset the Who Said It.", color, "", ""));
                WSI.Reset();
            }
            else if (game == "")
                await Utilities.PrintError(context.Channel, "Please specify a game to reset.");
            else
                await Utilities.PrintError(context.Channel, $"I was unable to find the `{game}` game.\n\nAvailable games to reset:\nTrivia\n`!trivia`\n\nTic-Tac-Toe\n`!ttt`\n\nNumber Guess\n`!play ng`\n\nRussian Roulette\n`!rr`");
        }
    }
}