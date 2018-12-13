using Discord;
using Discord.Rest;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Gideon.Minigames
{
	class TicTacToe
	{
		private string[] boardSlots = { ":white_large_square:", ":white_large_square:", ":white_large_square:",
		":white_large_square:", ":white_large_square:", ":white_large_square:",
		":white_large_square:", ":white_large_square:", ":white_large_square:" };

		private string writeBoard => $"{boardSlots[0]}{boardSlots[1]}{boardSlots[2]}\n{boardSlots[3]}{boardSlots[4]}{boardSlots[5]}\n{boardSlots[6]}{boardSlots[7]}{boardSlots[8]}";

		private RestUserMessage m;

        private SocketGuildUser Player1, Player2, currentTurnUser;

        private bool isGameGoing = false, hasGameStarted = false, canPlaySlot = true;

		public async Task IncrementTurn()
		{
			if (!isGameGoing) return;
			currentTurnUser = currentTurnUser == Player1 ? Player2 : Player1;
			await m.ModifyAsync(m => { m.Content = $"It is {currentTurnUser.Mention}'s turn.\n\n{writeBoard}"; });
		}

		public async Task StartGame(SocketCommandContext context)
		{
			if (!isGameGoing)
			{
				Player1 = (SocketGuildUser)context.User;
				isGameGoing = true;
			}
			else return;
			await context.Channel.SendMessageAsync($"{Player1.Mention} has started a game of Tic-Tac-Toe! Type `!ttt join` to play.");
		}

		public async Task JoinGame(SocketCommandContext context)
		{
			if (isGameGoing && !hasGameStarted && Player1 != (SocketGuildUser)context.User)
				Player2 = context.User as SocketGuildUser;
			else return;

			m = await context.Channel.SendMessageAsync("Please wait for the game to load...");
			await m.RemoveAllReactionsAsync();
			await m.AddReactionAsync(new Emoji("↖"));
			await m.AddReactionAsync(new Emoji("⬆"));
			await m.AddReactionAsync(new Emoji("↗"));
			await m.AddReactionAsync(new Emoji("⬅"));
			await m.AddReactionAsync(new Emoji("⏺"));
			await m.AddReactionAsync(new Emoji("➡"));
			await m.AddReactionAsync(new Emoji("↙"));
			await m.AddReactionAsync(new Emoji("⬇"));
			await m.AddReactionAsync(new Emoji("↘"));
			await m.ModifyAsync(m => { m.Content = $"It is {Player1.Mention}'s turn.\n\n{writeBoard}"; });
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

		public async Task Play(SocketReaction reaction, ISocketMessageChannel channel, Optional<IUser> user)
		{
			if (user.ToString() == "Gideon#8386") return;
			if (currentTurnUser.ToString() != user.ToString())
			{
				await m.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
				return;
			}

			string emote = reaction.Emote.ToString();
			if (emote == "↖")
				boardSlots[0] = EmojiToPlace(0);
			else if (emote == "⬆")
				boardSlots[1] = EmojiToPlace(1);
			else if (emote == "↗")
				boardSlots[2] = EmojiToPlace(2);
			else if (emote == "⬅")
				boardSlots[3] = EmojiToPlace(3);
			else if (emote == "⏺")
				boardSlots[4] = EmojiToPlace(4);
			else if (emote == "➡")
				boardSlots[5] = EmojiToPlace(5);
			else if (emote == "↙")
				boardSlots[6] = EmojiToPlace(6);
			else if (emote == "⬇")
				boardSlots[7] = EmojiToPlace(7);
			else if (emote == "↘")
				boardSlots[8] = EmojiToPlace(8);

			if (canPlaySlot)
			{
				await CheckForWin(channel, ":x:");
				await CheckForWin(channel, ":o:");
				await CheckForDraw();
				await IncrementTurn();
			}

			await m.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
		}

		private async Task CheckForWin(ISocketMessageChannel channel, string letter)
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

				await DeclareWinner(channel, letter);
		}

		private async Task DeclareWinner(ISocketMessageChannel channel, string letter)
		{
			SocketGuildUser winner = letter == ":x:" ? Player1 : Player2;
			await m.ModifyAsync(m => { m.Content = $"{winner.Mention} has won!\n\n{writeBoard}"; });
			Reset();
		}

		private async Task CheckForDraw()
		{
			foreach (var s in boardSlots)
				if (s == ":white_large_square:") return;
			await m.ModifyAsync(m => { m.Content = $"It's a draw!\n\n{writeBoard}"; });
			Reset();
		}

        private void Reset() => Config.MinigameHandler.ResetTTT();
	}
}
