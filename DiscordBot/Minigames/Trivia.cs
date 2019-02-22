using System;
using Discord;
using System.Linq;
using System.Text;
using Gideon.Handlers;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Gideon.Minigames
{
    class Trivia
    {
        private bool isTriviaBeingPlayed;
        private SocketGuildUser Player;
        private string correctAnswer, triviaMode;
        private DateTime StartTime;
        private readonly List<SocketGuildUser> PlayersAnswered = new List<SocketGuildUser>();
        private static readonly Random rdn = new Random();

        private Embed Embed(string description, string footer) => Utilities.Embed("Trivia", description, Colors.Green, footer, "");

        private string GetName(SocketGuildUser user) => user.Nickname ?? user.Username;

        public async Task TryToStartTrivia(SocketGuildUser user, SocketCommandContext context, string input)
        {
            if (!await Utilities.CheckForChannel(context, 518846214603669537, context.User)) return;
            if (isTriviaBeingPlayed && (DateTime.Now - StartTime).TotalSeconds < 60)
            {
                await Utilities.PrintError(context.Channel, $"Sorry, {Player.Mention} is currently playing.\nYou can ask an admin to `!reset trivia` if there is an issue.");
                return;
            }
            if (input == "trivia")
            {
                await context.Channel.SendMessageAsync("", false, Embed("Please select a mode.\n\n`!trivia solo` - Play alone.\n\n`!trivia all` - First to answer wins.", ""));
                return;
            }
            if (isTriviaBeingPlayed && (DateTime.Now - StartTime).TotalSeconds > 60)
                await CancelGame(Player, context).ConfigureAwait(false);
            await StartTrivia(user, context, input.Replace("trivia ", "")).ConfigureAwait(false);
        }

        private async Task CancelGame(SocketGuildUser user, SocketCommandContext Context)
        {
            CoinsHandler.AdjustCoins(user, -1);
            await Context.Channel.SendMessageAsync("", false, Embed($"{user.Mention} took too long to answer and lost 1 coin.", ""));
        }

        private async Task StartTrivia(SocketGuildUser user, SocketCommandContext Context, string mode)
        {
            Player = user;
            triviaMode = mode;
            isTriviaBeingPlayed = true;
            StartTime = DateTime.Now;
            var Question = MinigameHandler.TriviaQuestions.Questions[Utilities.GetRandomNumber(0, MinigameHandler.TriviaQuestions.Questions.Count())];
            
            string[] Answers = {"","","",""};

            Answers[0] = Question.IncorrectAnswers[0];
            Answers[1] = Question.IncorrectAnswers[1];
            Answers[2] = Question.IncorrectAnswers[2];
            Answers[3] = Question.Answer;

            Answers = Answers.OrderBy(x => rdn.Next()).ToArray();

            for (int n = 0; n < 4; n++)
            {
                if (Answers[n] == Question.Answer)
                {
                    if (n == 0)
                        correctAnswer = "a";
                    else if (n == 1)
                        correctAnswer = "b";
                    else if (n == 2)
                        correctAnswer = "c";
                    else
                        correctAnswer = "d";
                }
            }

            StringBuilder Description = new StringBuilder()
                .AppendLine($"{Question.QuestionQuestion}?\n")
                .AppendLine($"a) {Answers[0]}\n")
                .AppendLine($"b) {Answers[1]}\n")
                .AppendLine($"c) {Answers[2]}\n")
                .AppendLine($"d) {Answers[3]}");
            string Footer = mode == "solo" ? $"Only {GetName(user)} can answer." : "First to answer wins!";
            await Context.Channel.SendMessageAsync("", false, Embed(Description.ToString(), Footer));
        }

        public async Task AnswerTrivia(SocketGuildUser user, SocketCommandContext context, string input)
        {
            // Solo Mode Answer
            if (user == Player && triviaMode == "solo")
            {
                if (input == correctAnswer)
                {
                    await context.Channel.SendMessageAsync("", false, Embed("Correct.", $"{GetName(user)} has been awarded 1 coin."));
                    CoinsHandler.AdjustCoins(user, 1);
                    ResetTrivia();
                    return;
                }
                await context.Channel.SendMessageAsync("", false, Embed($"Wrong, it is {correctAnswer.ToUpper()}.", $"{GetName(user)} lost 1 coin."));
                CoinsHandler.AdjustCoins(user, -1);
                ResetTrivia();
                return;
            }

            // All Mode answer
            if (triviaMode == "all" && isTriviaBeingPlayed)
            {
                for (int i = 0; i < PlayersAnswered.Count; i++)
                    if (PlayersAnswered.ElementAt(i) == user)
                    {
                        await Utilities.PrintError(context.Channel, $"You already answered, {user.Mention}.");
                        return;
                    }

                PlayersAnswered.Add(user);
                if (input == correctAnswer)
                {
                    await context.Channel.SendMessageAsync("", false, Embed($"Correct, {user.Mention} won!", $"{GetName(user)} has been awarded 1 coin."));
                    CoinsHandler.AdjustCoins(user, 1);
                    ResetTrivia();
                }
            }
        }

        public void ResetTrivia()
        {
            Player = null;
            isTriviaBeingPlayed = false;
            PlayersAnswered.Clear();
        }
    }
}