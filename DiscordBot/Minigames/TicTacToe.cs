using System;
using Discord;
using System.Linq;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Gideon.Minigames
{
    class TicTacToe
    {
        public string[] SlotValues = { " ", " ", " ", " ", " ", " ", " ", " ", " " };

        bool isGameGoing = false;
        int currentTurn = 0;
        SocketGuildUser host = null;
        struct Player { public SocketGuildUser user; public string letter; };
        List<Player> Players = new List<Player>();

        Embed Embed(string Description, string Footer)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle("Tic-Tac-Toe");
            embed.WithDescription(Description);
            embed.WithColor(new Color(50, 50, 50));
            embed.WithFooter(Footer);
            return embed;
        }

        public async Task TryToStartGame(SocketCommandContext context, string input)
        {
            if (context.Channel.Name != "minigames")
            {
                await context.Channel.SendMessageAsync($"Please use the #minigames chat for that, {context.User.Mention}.");
                return;
            }
            if (isGameGoing)
            {
                await context.Channel.SendMessageAsync("", false, Embed("Sorry, but a game has already started.\nYou can request a Respected+ to `!reset ttt` if there is an issue.", ""));
                return;
            }
            if (input == "!ttt")
            {
                await context.Channel.SendMessageAsync("", false, Embed("Please select a letter to start a game.\n\n`!ttt X`\n`!ttt O`", ""));
                return;
            }

            string letter = input.Replace("!ttt ", "");
            if(letter != "x" && letter != "o")
            {
                await context.Channel.SendMessageAsync("", false, Embed("You may only use X and O.\n\n`!ttt X`\n`!ttt O`", ""));
                return;
            }

            host = (SocketGuildUser)context.User;
            AddPlayer(host, letter.ToUpper());
            isGameGoing = true;
            await context.Channel.SendMessageAsync("", false, Embed($"{host.Mention} has started a game!\n\nType `!join ttt` to join!", "Waiting for someone to join..."));
        }

        private void AddPlayer(SocketGuildUser user, string letter)
        {
            Player newPlayer = new Player();
            newPlayer.user = user;
            newPlayer.letter = letter;
            Players.Add(newPlayer);
        }

        public async Task TryToJoinGame(SocketCommandContext context)
        {
            if (context.Channel.Name != "minigames")
            {
                await context.Channel.SendMessageAsync($"Please use the #minigames chat for that, {context.User.Mention}.");
                return;
            }
            if (!isGameGoing)
            {
                await context.Channel.SendMessageAsync("", false, Embed("There is no game going.\n\nType `!ttt` to start one.", ""));
                return;
            }
            if (Players.Count == 2) return;

            SocketGuildUser newPlayer = (SocketGuildUser)context.User;
            string letter = Players.ElementAt(0).letter == "X" ? "O" : "X";
            AddPlayer(newPlayer, letter);

            await context.Channel.SendMessageAsync("", false, Embed($"{newPlayer.Mention} has joined as \"{letter}\"!\n\nWaiting for {Players.ElementAt(0).user.Mention} to play...\n\n`!put #` - Put your letter at whichever slot number # you choose.\n\n{WriteBoard()}", ""));
        }

        public string WriteBoard() =>
            // Has to be indented this way to comply with Discord's code tags/Markdown
            $@"```
|{SlotValues[0]}|{SlotValues[1]}|{SlotValues[2]}|
|{SlotValues[3]}|{SlotValues[4]}|{SlotValues[5]}|
|{SlotValues[6]}|{SlotValues[7]}|{SlotValues[8]}|```";

        public async Task PutLetter(SocketCommandContext context, string input)
        {
            if (context.Channel.Name != "minigames")
            {
                await context.Channel.SendMessageAsync($"Please use the #minigames chat for that, {context.User.Mention}.");
                return;
            }
            if (!isGameGoing)
            {
                await context.Channel.SendMessageAsync("", false, Embed("There is no game going.\n\nType `!ttt` to start one.", ""));
                return;
            }

            SocketGuildUser player = (SocketGuildUser)context.User;
            if((player != Players.ElementAt(0).user && player != Players.ElementAt(1).user) || player != Players.ElementAt(currentTurn).user) return;

            int Slot;
            Int32.TryParse(input.Replace("!put ", ""), out Slot);

            if(Slot < 1)
            {
                await context.Channel.SendMessageAsync("", false, Embed($"1 is the first slot, {player.Mention}.", ""));
                return;
            }
            else if (Slot > 9)
            {
                await context.Channel.SendMessageAsync("", false, Embed($"9 is the last slot, {player.Mention}.", ""));
                return;
            }

            Slot--;
            if (SlotValues[Slot] != " ")
            {
                await context.Channel.SendMessageAsync("", false, Embed("That slot has already been filled.", ""));
                return;
            }

            SlotValues[Slot] = Players.ElementAt(currentTurn).letter;
            await CheckForWin(context, "X");
            await CheckForWin(context, "O");
            await CheckForDraw(context);
            currentTurn = currentTurn == 0 ? 1 : 0;

            await context.Channel.SendMessageAsync("", false, Embed($"{WriteBoard()}\n{Players.ElementAt(currentTurn).user.Mention}'s turn.", ""));
        }

        private async Task CheckForWin(SocketCommandContext context, string letter)
        {
            // Columns
            if(SlotValues[0] == letter && SlotValues[3] == letter && SlotValues[6] == letter ||
                SlotValues[1] == letter && SlotValues[4] == letter && SlotValues[7] == letter ||
                SlotValues[2] == letter && SlotValues[5] == letter && SlotValues[8] == letter ||

            // Rows
                SlotValues[0] == letter && SlotValues[1] == letter && SlotValues[2] == letter ||
                SlotValues[3] == letter && SlotValues[4] == letter && SlotValues[5] == letter ||
                SlotValues[6] == letter && SlotValues[7] == letter && SlotValues[8] == letter ||

            // Diagonal
                SlotValues[0] == letter && SlotValues[4] == letter && SlotValues[8] == letter ||
                SlotValues[6] == letter && SlotValues[4] == letter && SlotValues[2] == letter)
            {
                await DeclareWinner(context, letter);
            }
        }

        private async Task CheckForDraw(SocketCommandContext context)
        {
            foreach(string s in SlotValues)
            {
                if (s == " ") return;
            }
            await context.Channel.SendMessageAsync("", false, Embed($"{WriteBoard()}\nIt's a draw!", "Nobody loses any Tecos."));
            Reset();
            return;
        }

        private async Task DeclareWinner(SocketCommandContext context, string winningLetter)
        {
            SocketGuildUser winner, loser;
            if(Players.ElementAt(0).letter == winningLetter)
            {
                winner = Players.ElementAt(0).user;
                loser = Players.ElementAt(1).user;
            }
            else
            {
                winner = Players.ElementAt(1).user;
                loser = Players.ElementAt(0).user;
            }

            await context.Channel.SendMessageAsync("", false, Embed($"{WriteBoard()}\n{winner.Mention} has won 5 Tecos!\n\n{loser.Mention} has lost 5 Tecos.", ""));
            Config.TH.AdjustTecos(winner, 5);
            Config.TH.AdjustTecos(loser, -5);
            Reset();
            return;
        }
        
        public void Reset()
        {
            currentTurn = 0;
            Players.Clear();
            isGameGoing = false;
            host = null;
            for(int i = 0; i < SlotValues.Length; i++)
            {
                SlotValues[i] = " ";
            }
        }
    }
}