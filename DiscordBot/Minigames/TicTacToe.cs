using Discord;
using Discord.Rest;
using Gideon.Handlers;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Gideon.Minigames
{
	class TicTacToe
	{
		private readonly string[] boardSlots = { ":white_large_square:", ":white_large_square:", ":white_large_square:",
		":white_large_square:", ":white_large_square:", ":white_large_square:",
		":white_large_square:", ":white_large_square:", ":white_large_square:" };

		private string writeBoard => $"{boardSlots[0]}{boardSlots[1]}{boardSlots[2]}\n{boardSlots[3]}{boardSlots[4]}{boardSlots[5]}\n{boardSlots[6]}{boardSlots[7]}{boardSlots[8]}";

        private readonly List<string> Emojis = new List<string>(new[] { "↖", "⬆", "↗", "⬅", "⏺", "➡", "↙", "⬇", "↘" });

        public RestUserMessage GameMessage;

        private SocketGuildUser Player1, Player2, currentTurnUser;

        private bool isGameGoing, hasGameStarted, canPlaySlot = true;

		private async Task IncrementTurn()
		{
			if (!isGameGoing) return;
			currentTurnUser = currentTurnUser == Player1 ? Player2 : Player1;
			await ModifyMessage($"It is {currentTurnUser.Mention}'s turn.\n\n{writeBoard}").ConfigureAwait(false);
		}

        private async Task ModifyMessage (string Description)
        {
            await GameMessage.ModifyAsync(m => { m.Embed = new EmbedBuilder()
                .WithTitle("Tic-Tac-Toe")
                .WithColor(Colors.White)
                .WithDescription(Description)
                .Build(); ;});
        }

		public async Task StartGame(SocketCommandContext context)
		{
			if (!isGameGoing)
			{
				Player1 = (SocketGuildUser)context.User;
				isGameGoing = true;
			}
			else return;
			await Utilities.SendEmbed(context.Channel, "Tic-Tac-Toe", $"{Player1.Mention} has started a game of Tic-Tac-Toe! Type `!ttt join` to play.", Colors.White, "", "");
		}

		public async Task JoinGame(SocketCommandContext context)
		{
			if (isGameGoing && !hasGameStarted && Player1 != (SocketGuildUser)context.User)
				Player2 = context.User as SocketGuildUser;
			else return;

            hasGameStarted = true;
            GameMessage = await context.Channel.SendMessageAsync(null, false, new EmbedBuilder()
                .WithTitle("Tic-Tac-Toe")
                .WithColor(Colors.Red)
                .WithDescription("Please wait for the game to load...")
                .Build());

            foreach (string Emoji in Emojis)
                await GameMessage.AddReactionAsync(new Emoji(Emoji));

			await ModifyMessage($"It is {Player1.Mention}'s turn.\n\n{writeBoard}").ConfigureAwait(false);
			currentTurnUser = Player1;
		}

		private string EmojiToPlace(int slot)
		{
			if (boardSlots[slot] == ":white_large_square:" && boardSlots[slot] != ":x:" && boardSlots[slot] != ":o:")
			{
				canPlaySlot = true;
				return currentTurnUser == Player1 ? ":x:" : ":o:";
			}
			canPlaySlot = false;
			return ":white_large_square:";
		}

		public async Task Play(SocketReaction reaction, Optional<IUser> user)
		{
			if (currentTurnUser.ToString() != user.ToString())
			{
				await GameMessage.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
				return;
			}

			string emote = reaction.Emote.ToString();

            // Loop through all the available responses and place the emoji
            for (int i = 0; i < Emojis.Count; i++)
                if (emote == Emojis[i])
                    boardSlots[i] = EmojiToPlace(0);

			if (canPlaySlot)
			{
				await CheckForWin(":x:").ConfigureAwait(false);
				await CheckForWin(":o:").ConfigureAwait(false);
				await CheckForDraw().ConfigureAwait(false);
				await IncrementTurn().ConfigureAwait(false);
			}

			await GameMessage.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
		}

		private async Task CheckForWin(string letter)
		{
			// Columns
			if (boardSlots[0] == letter && boardSlots[3] == letter && boardSlots[6] == letter ||
				boardSlots[1] == letter && boardSlots[4] == letter && boardSlots[7] == letter ||
				boardSlots[2] == letter && boardSlots[5] == letter && boardSlots[8] == letter ||

				// Rows
				boardSlots[0] == letter && boardSlots[1] == letter && boardSlots[2] == letter ||
				boardSlots[3] == letter && boardSlots[4] == letter && boardSlots[5] == letter ||
				boardSlots[6] == letter && boardSlots[7] == letter && boardSlots[8] == letter ||

				// Diagonal
				boardSlots[0] == letter && boardSlots[4] == letter && boardSlots[8] == letter ||
				boardSlots[6] == letter && boardSlots[4] == letter && boardSlots[2] == letter)

				await DeclareWinner(letter).ConfigureAwait(false);
		}

		private async Task DeclareWinner(string letter)
		{
			SocketGuildUser winner = letter == ":x:" ? Player1 : Player2;
			await ModifyMessage($"{winner.Mention} has won!\n\n{writeBoard}").ConfigureAwait(false);
            MinigameHandler.ResetTTT();
		}

		private async Task CheckForDraw()
		{
			foreach (var s in boardSlots)
				if (s == ":white_large_square:") return;
			await ModifyMessage($"It's a draw!\n\n{writeBoard}").ConfigureAwait(false);
            MinigameHandler.ResetTTT();
		}
	}
}
