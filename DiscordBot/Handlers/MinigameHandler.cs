using Discord;
using Discord.Commands;
using Gideon.Minigames;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Gideon.Handlers
{
    class MinigameHandler
    {
        CoinsHandler TH = new CoinsHandler();

        public _8ball _8ball = new _8ball();
        public Trivia Trivia = new Trivia();
        //public TicTacToe TTT = new TicTacToe();
        public NumberGuess NG = new NumberGuess();
        public RussianRoulette RR = new RussianRoulette();
        public RockPaperScissors RPS = new RockPaperScissors();

        public async Task DisplayGames(SocketCommandContext context)
        {
            if (context.Channel.Id != 518846214603669537)
            {
                await Config.Utilities.PrintError(context, $"Please use the {context.Guild.GetTextChannel(518846214603669537).Mention} chat for that, {context.User.Mention}.");
                return;
            }
            await context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("MiniGames", "Trivia\n`!trivia`\n\nTic-Tac-Toe\n`!ttt`\n\nNumber Guess\n`!play ng`\n\nRussian Roulette\n`!rr`\n\n8-Ball\n`!8ball`", new Color(31, 139, 76), "", ""));
        }

        public async Task TryToStartTrivia(SocketCommandContext context, string input)
        {
            if(input == "all")
                await Trivia.TryToStartTrivia((SocketGuildUser)context.User, context, "all");
        }
    }
}