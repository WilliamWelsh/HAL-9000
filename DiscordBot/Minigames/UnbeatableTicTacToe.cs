using Discord;
using Discord.Rest;
using Gideon.Handlers;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Gideon.Minigames
{
    class UnbeatableTicTacToe
    {
        int[] Board = new int[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        int[][] PossibleWins = new[] {
                new int[] { 0, 1, 2 },
                new int[] { 3, 4, 5 },
                new int[] { 6, 7, 8 },
                new int[] { 0, 3, 6 },
                new int[] { 1, 4, 7 },
                new int[] { 2, 5, 8 },
                new int[] { 0, 4, 8 },
                new int[] { 2, 4, 6 } };

        private string PrintBoard => $"{NumberToEmoji(Board[0])}{NumberToEmoji(Board[1])}{NumberToEmoji(Board[2])}\n{NumberToEmoji(Board[3])}{NumberToEmoji(Board[4])}{NumberToEmoji(Board[5])}\n{NumberToEmoji(Board[6])}{NumberToEmoji(Board[7])}{NumberToEmoji(Board[8])}";

        private RestUserMessage GameMessage;
        private SocketGuildUser Player;
        private bool isGameGoing = false, canPlaySlot = true;
        private readonly List<string> Emojis = new List<string>(new string[] { "↖", "⬆", "↗", "⬅", "⏺", "➡", "↙", "⬇", "↘" });

        //Embed GamEmbed = new EmbedBuilder()
        //    .WithTitle("Unbeatable Tic-Tac_Toe");

        string NumberToEmoji (int num)
        {
            if (num == 0)
                return ":white_large_square:";
            else if (num == 1)
                return ":x:";
            else if (num == -1)
                return ":o:";
            return "ERROR!!!";
        }

        int win (int[] board)
        {
            for (int i = 0; i < 8; ++i)
		        if (board[PossibleWins[i][0]] != 0 && board[PossibleWins[i][0]] == board[PossibleWins[i][1]] && board[PossibleWins[i][1]] == board[PossibleWins[i][2]])
			        return board[PossibleWins[i][2]];
	        return 0;
        }

        // https://en.wikipedia.org/wiki/Minimax
        int Minimax (int[] board, int player)
        {
            int winner = win(board);

            if (winner != 0)
                return winner * player;

            int move = -1;
            int score = -2;

            for (int i = 0; i < 9; i++)
            {
                if (board[i] == 0)
                {

                    board[i] = player;
                    int thisScore = -Minimax(board, player * -1);

                    if (thisScore > score)
                    {
                        score = thisScore;
                        move = i;
                    }

                    board[i] = 0;
                }
            }

            if (move == -1)
                return 0;

            return score;
        }

        // The move Gideon makes (based on minimax)
        int GideonsMove (int[] board)
        {
            int move = -1;
            int score = -2;

            for (int i = 0; i < 9; ++i)
            {

                if (board[i] == 0)
                {
                    board[i] = 1;
                    int tempScore = -Minimax(board, -1);
                    board[i] = 0;

                    if (tempScore > score)
                    {
                        score = tempScore;
                        move = i;
                    }
                }
            }

            return move;
        }

        // Start the game (!uttt)
        public async Task StartGame(SocketCommandContext context)
        {
            Player = (SocketGuildUser)context.User;
            isGameGoing = true;

            GameMessage = await context.Channel.SendMessageAsync("Please wait for the game to load...");

            foreach (string Emoji in Emojis)
                await GameMessage.AddReactionAsync(new Emoji(Emoji));

            await GameMessage.ModifyAsync(m => { m.Content = $"It is {Player.Mention}'s turn.\n\n{PrintBoard}"; });
        }

        // Make sure the slot the user is trying to play is blank
        private bool IsValidSlot (int slot)
        {
            if (Board[slot] == 0 && Board[slot] != 1 && Board[slot] != -1)
            {
                canPlaySlot = true;
                return true;
            }
            canPlaySlot = false;
            return false;
        }

        // Play a slot
        public async Task Play (SocketReaction reaction, ISocketMessageChannel channel, Optional<IUser> user)
        {
            if (user.ToString() == "Gideon#8386" || !isGameGoing) return;
            if (Player.ToString() != user.ToString()) return;

            string emote = reaction.Emote.ToString();
        
            if (emote == "↖" && IsValidSlot(0))
                Board[0] = -1;

            if (emote == "⬆" && IsValidSlot(1))
                Board[1] = -1;

            if (emote == "↗" && IsValidSlot(2))
                Board[2] = -1;

            if (emote == "⬅" && IsValidSlot(3))
                Board[3] = -1;

            if (emote == "⏺" && IsValidSlot(4))
                Board[4] = -1;

            if (emote == "➡" && IsValidSlot(5))
                Board[5] = -1;

            if (emote == "↙" && IsValidSlot(6))
                Board[6] = -1;

            if (emote == "⬇" && IsValidSlot(7))
                Board[7] = -1;

            if (emote == "↘" && IsValidSlot(8))
                Board[8] = -1;

            if (canPlaySlot)
            {
                await CheckForWin(-1); // Check if the human won
                await CheckForDraw();

                if (isGameGoing)
                {
                    int k = GideonsMove(Board);
                    Board[k] = 1;

                    await GameMessage.ModifyAsync(m => { m.Content = $"{PrintBoard}"; });

                    await CheckForWin(1); // Check if Gideon won
                    await CheckForDraw();
                }
            }

            await GameMessage.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
        }

        private async Task CheckForWin(int number)
        {
                // Columns
            if (Board[0] == number && Board[3] == number && Board[6] == number ||
                Board[1] == number && Board[4] == number && Board[7] == number ||
                Board[2] == number && Board[5] == number && Board[8] == number ||

                // Rows
                Board[0] == number && Board[1] == number && Board[2] == number ||
                Board[3] == number && Board[4] == number && Board[5] == number ||
                Board[6] == number && Board[7] == number && Board[8] == number ||

                // Diagonal
                Board[0] == number && Board[4] == number && Board[8] == number ||
                Board[6] == number && Board[4] == number && Board[2] == number)

                await DeclareWinner(number);
        }

        private async Task DeclareWinner(int number)
        {
            if (number == 1)
                await GameMessage.ModifyAsync(m => { m.Content = $"I won!!!!\n\n{PrintBoard}"; });
            else
                await GameMessage.ModifyAsync(m => { m.Content = $"You have won!\n\n{PrintBoard}"; });
            MinigameHandler.ResetUTTT();
        }

        private async Task CheckForDraw()
        {
            foreach (var s in Board)
                if (s == 0) return; // 0 = blank slot, if there is a single blank spot then a draw isn't possible
            await GameMessage.ModifyAsync(m => { m.Content = $"It's a draw!\n\n{PrintBoard}"; });
            MinigameHandler.ResetUTTT();
        }
    }
}
