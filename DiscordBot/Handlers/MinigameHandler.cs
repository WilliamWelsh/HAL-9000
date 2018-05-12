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

        public Trivia Trivia = new Trivia();
        public NumberGuess NG = new NumberGuess();
        public TicTacToe TTT = new TicTacToe();
        public RussianRoulette RR = new RussianRoulette();

        public async Task DisplayGames(ISocketMessageChannel channel) => await channel.SendMessageAsync("", false, Utilities.Embed("MiniGames", "Trivia\n`!trivia`\n\nTic-Tac-Toe\n`!ttt`\n\nNumber Guess\n`!play ng`\n\nRussian Roulette\n`!rr`", new Color(0, 173, 0), "", ""));
    }
}