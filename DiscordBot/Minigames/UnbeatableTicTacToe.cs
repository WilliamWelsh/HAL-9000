using Discord;
using Discord.Rest;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Handlers;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DiscordBot.Minigames
{
    public class UnbeatableTicTacToe
    {
        readonly int[] Board = new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        readonly int[][] PossibleWins = new[] {
                new[] { 0, 1, 2 },
                new[] { 3, 4, 5 },
                new[] { 6, 7, 8 },
                new[] { 0, 3, 6 },
                new[] { 1, 4, 7 },
                new[] { 2, 5, 8 },
                new[] { 0, 4, 8 },
                new[] { 2, 4, 6 } };

        string PrintBoard => $"{NumberToEmoji(Board[0])}{NumberToEmoji(Board[1])}{NumberToEmoji(Board[2])}\n{NumberToEmoji(Board[3])}{NumberToEmoji(Board[4])}{NumberToEmoji(Board[5])}\n{NumberToEmoji(Board[6])}{NumberToEmoji(Board[7])}{NumberToEmoji(Board[8])}";

        RestUserMessage GameMessage;
        private bool isGameGoing;
        bool HasValidMove = true;
        string PlayerName;
        readonly List<string> Emojis = new List<string>(new[] { "↖", "⬆", "↗", "⬅", "⏺", "➡", "↙", "⬇", "↘" });

        // Modify the game message
        public async Task ModifyMessage (string Description)
        {
            await GameMessage.ModifyAsync(m => { m.Embed = new EmbedBuilder()
                .WithTitle("Tic-Tac-Toe")
                .WithColor(Utilities.ClearColor)
                .WithDescription(Description)
                .WithFooter($"Playing with {PlayerName}.")
                .Build(); ;});
        }

        // 0 = Blank, 1 = X, -1 = O
        string NumberToEmoji (int num)
        {
            if (num == 0)
                return ":white_large_square:";
            else if (num == 1)
                return ":x:";
            return ":o:";
        }

        // Returns 0 if the board input is not a winning board for the human
        int WinningBoard (int[] board)
        {
            for (int i = 0; i < 8; ++i)
		        if (board[PossibleWins[i][0]] != 0 && board[PossibleWins[i][0]] == board[PossibleWins[i][1]] && board[PossibleWins[i][1]] == board[PossibleWins[i][2]])
			        return board[PossibleWins[i][2]];
	        return 0;
        }

        // https://en.wikipedia.org/wiki/Minimax
        int Minimax (int[] board, int player)
        {
            int winner = WinningBoard(board);

            if (winner != 0)
                return winner * player;

            int move = -1, score = -2;

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
            int move = -1, score = -2;

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
            var Player = (SocketGuildUser)context.User;
            PlayerName = Player.Nickname ?? Player.Username;
            isGameGoing = true;

            GameMessage = await context.Channel.SendMessageAsync(null, false, new EmbedBuilder()
                .WithTitle("Tic-Tac-Toe")
                .WithColor(Utilities.ClearColor)
                .WithDescription("Please wait for the game to load...")
                .WithFooter($"Playing with {PlayerName}.")
                .Build());

            foreach (string Emoji in Emojis)
                await GameMessage.AddReactionAsync(new Emoji(Emoji));

            await ModifyMessage($"{PlayerName}'s turn.\n\n{PrintBoard}").ConfigureAwait(false);
        }

        // Make sure the slot the user is trying to play is blank
        private bool IsValidSlot (int slot)
        {
            if (Board[slot] == 0)
            {
                HasValidMove = true;
                return true;
            }
            HasValidMove = false;
            return false;
        }

        // Play a slot
        public async Task Play (SocketReaction Reaction)
        {
            if (!isGameGoing) return;
            string emote = Reaction.Emote.ToString();

            // Loop through all the available emoji reactions
            for (int i = 0; i < Emojis.Count; i ++)
            {
                // If the slot is valid, then set the player's move to that slot on the board
                if (emote == Emojis[i] && IsValidSlot(i))
                    Board[i] = -1;
            }

            await GameMessage.RemoveReactionAsync(Reaction.Emote, Reaction.User.Value);

            if (HasValidMove)
            {
                await ModifyMessage($"My turn.\n\n{PrintBoard}").ConfigureAwait(false);

                await CheckForDraw().ConfigureAwait(false);

                if (isGameGoing)
                {
                    // Have Gideon make a move and set their slot move to 1 (X)
                    Board[GideonsMove(Board)] = 1;

                    await Task.Delay(2000).ConfigureAwait(false); // This is to prevent spam, and seem more friendly
                    await ModifyMessage($"{PlayerName}'s turn.\n\n{PrintBoard}").ConfigureAwait(false);

                    // Check if Gideon won
                    if (WinningBoard(Board) != 0)
                        await DeclareWinner().ConfigureAwait(false);
                    await CheckForDraw().ConfigureAwait(false);
                }
            }
        }

        // Declare that Gideon has won
        private async Task DeclareWinner()
        {
            // No point in checking if the player won (sorry, human)
            await ModifyMessage($"I won.\n\n{PrintBoard}").ConfigureAwait(false);
            await Reset().ConfigureAwait(false);
        }

        // Check for a draw
        private async Task CheckForDraw()
        {
            foreach (var s in Board)
                if (s == 0) return; // 0 = blank slot, if there is a single blank spot then a draw isn't possible
            await ModifyMessage($"It's a draw!\n\n{PrintBoard}").ConfigureAwait(false);
            await Reset().ConfigureAwait(false);
        }

        private async Task Reset()
        {
            await GameMessage.RemoveAllReactionsAsync().ConfigureAwait(false);
            isGameGoing = false;
            MinigameHandler.ResetUTTT();
        }
    }
}
