using Gideon.Handlers;
using Gideon.Minigames;
using System;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Gideon.Handlers
{
    class MinigameHandler
    {
        TecosHandler TH = new TecosHandler();

        public _8ball _8ball = new _8ball();
        public Trivia Trivia = new Trivia();
        public TicTacToe TTT = new TicTacToe();
        public NumberGuess NG = new NumberGuess();
        public RussianRoulette RR = new RussianRoulette();

        public async Task DisplayGames(SocketCommandContext context)
        {
            if (context.Channel.Id != 443205778656985089)
            {
                await Config.Utilities.PrintError(context, $"Please use the {context.Guild.GetTextChannel(443205778656985089).Mention} chat for that, {context.User.Mention}.");
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