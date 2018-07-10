using Gideon.Handlers;
using Gideon.Minigames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord.Commands;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;

namespace Gideon.Handlers
{
    class MinigameHandler
    {
        TecosHandler TH = new TecosHandler();

        struct TriviaInstance { public SocketGuildUser player; public Trivia Instance; }
        List<TriviaInstance> TriviaInstances = new List<TriviaInstance>();

        public _8ball _8ball = new _8ball();
        public Trivia Trivia = new Trivia();
        public TicTacToe TTT = new TicTacToe();
        public NumberGuess NG = new NumberGuess();
        public RussianRoulette RR = new RussianRoulette();

        public async Task DisplayGames(ISocketMessageChannel channel) => await channel.SendMessageAsync("", false, Config.Utilities.Embed("MiniGames", "Trivia\n`!trivia`\n\nTic-Tac-Toe\n`!ttt`\n\nNumber Guess\n`!play ng`\n\nRussian Roulette\n`!rr`\n\n8-Ball\n`!8ball`", new Color(31, 139, 76), "", ""));

        void CreateInstance(SocketCommandContext context)
        {
            TriviaInstance newInstance = new TriviaInstance()
            {
                player = (SocketGuildUser)context.User,
                Instance = new Trivia()
            };
        }

        public async Task TryToStartTrivia(SocketCommandContext context, string input)
        {
            if(input == "all")
            {
                await Trivia.TryToStartTrivia((SocketGuildUser)context.User, context, "all");
            }
        }
    }
}