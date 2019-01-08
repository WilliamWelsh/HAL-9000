using Gideon.Minigames;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Gideon.Handlers
{
    class MinigameHandler
    {
        public static Trivia Trivia = new Trivia();
        public static TicTacToe TTT = new TicTacToe();
        public static WhoSaidIt WSI = new WhoSaidIt();
        public static NumberGuess NG = new NumberGuess();
        public static RussianRoulette RR = new RussianRoulette();
        public static RockPaperScissors RPS = new RockPaperScissors();
        public static UnbeatableTicTacToe UnbeatableTTT = new UnbeatableTicTacToe();

        public static async Task DisplayGames(SocketCommandContext context)
        {
            if (!await Utilities.CheckForChannel(context, 518846214603669537, context.User)) return;
            await Utilities.SendEmbed(context.Channel, "MiniGames", "Trivia\n`!trivia`\n\nTic-Tac-Toe\n`!ttt`\n\nNumber Guess\n`!play ng`\n\nRussian Roulette\n`!rr`\n\n8-Ball\n`!8ball`", Colors.Green, "", "");
        }

        public async Task TryToStartTrivia(SocketCommandContext context, string input)
        {
            if (input == "all")
                await Trivia.TryToStartTrivia((SocketGuildUser)context.User, context, "all");
        }

        // Have to call this from TicTacToe(s)
        public static void ResetTTT() => TTT = new TicTacToe();
        public static void ResetUTTT() => UnbeatableTTT = new UnbeatableTicTacToe();

        // Start Unbeatable Tic-Tac-Toe
        public static async Task StartUTTT(SocketCommandContext Context)
        {
            ResetUTTT();
            await UnbeatableTTT.StartGame(Context);
        }

        // Reset a game
        public static async Task ResetGame(SocketCommandContext context, string game)
        {
            if (!await Utilities.CheckForSuperadmin(context, context.User)) return;
            else if (game == "trivia")
            {
                await Utilities.SendEmbed(context.Channel, "MiniGames", $"{context.User.Mention} has reset Trivia.", Colors.Green, "", "");
                Trivia.ResetTrivia();
            }
            else if (game == "rr")
            {
                await Utilities.SendEmbed(context.Channel, "MiniGames", $"{context.User.Mention} has reset Russian Roulette.", Colors.Green, "", "");
                RR.Reset();
            }
            else if (game == "ttt")
            {
                await Utilities.SendEmbed(context.Channel, "MiniGames", $"{context.User.Mention} has reset Tic-Tac-Toe.", Colors.Green, "", "");
                ResetTTT();
            }
            else if (game == "ng")
            {
                await Utilities.SendEmbed(context.Channel, "MiniGames", $"{context.User.Mention} has reset the Number Guess game.", Colors.Green, "", "");
                NG.Reset();
            }
            else if (game == "wsi")
            {
                await Utilities.SendEmbed(context.Channel, "MiniGames", $"{context.User.Mention} has reset the Who Said It.", Colors.Green, "", "");
                WSI.Reset();
            }
            else if (game == "")
                await Utilities.PrintError(context.Channel, "Please specify a game to reset.");
            else
                await Utilities.PrintError(context.Channel, $"I was unable to find the `{game}` game.\n\nAvailable games to reset:\nTrivia\n`!trivia`\n\nTic-Tac-Toe\n`!ttt`\n\nNumber Guess\n`!play ng`\n\nRussian Roulette\n`!rr`");
        }
    }
}