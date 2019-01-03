using System;
using Discord;
using System.Linq;
using Gideon.Handlers;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Gideon.Minigames
{
    class Trivia
    {
        private static readonly Color color = Colors.Green;
        private bool isTriviaBeingPlayed = false;
        private SocketGuildUser Player = null;
        private string correctAnswer, triviaMode;
        private DateTime StartTime;
        private List<SocketGuildUser> PlayersAnswered = new List<SocketGuildUser>();
        private static Random rdn = new Random();

        private Embed Embed(string description, string footer) => Utilities.Embed("Trivia", description, color, footer, "");

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
                await CancelGame(Player, context);
            await StartTrivia(user, context, input.Replace("trivia ", ""));
        }

        private async Task CancelGame(SocketGuildUser user, SocketCommandContext context)
        {
            CoinsHandler.AdjustCoins(user, -1);
            await context.Channel.SendMessageAsync("", false, Embed($"{user.Mention} took too long to answer and lost 1 coin.", ""));
        }

        private async Task StartTrivia(SocketGuildUser user, SocketCommandContext context, string mode)
        {
            Player = user;
            triviaMode = mode;
            isTriviaBeingPlayed = true;
            StartTime = DateTime.Now;
            int QuestionNum = Utilities.GetRandomNumber(0, Config.triviaQuestions.Questions.Count);

            string[] Fakes = {"","","",""};

            Fakes[0] = Config.triviaQuestions.Questions.ElementAt(QuestionNum).IncorrectAnswers.ElementAt(0);
            Fakes[1] = Config.triviaQuestions.Questions.ElementAt(QuestionNum).IncorrectAnswers.ElementAt(1);
            Fakes[2] = Config.triviaQuestions.Questions.ElementAt(QuestionNum).IncorrectAnswers.ElementAt(2);
            Fakes[3] = Config.triviaQuestions.Questions.ElementAt(QuestionNum).Answer;

            string[] RandomFakes = Fakes.OrderBy(x => rdn.Next()).ToArray();

            for (int n = 0; n < 4; n++)
            {
                if (RandomFakes[n] == Config.triviaQuestions.Questions.ElementAt(QuestionNum).Answer)
                {
                    switch (n)
                    {
                        case 0:
                            correctAnswer = "a";
                            break;
                        case 1:
                            correctAnswer = "b";
                            break;
                        case 2:
                            correctAnswer = "c";
                            break;
                        case 3:
                            correctAnswer = "d";
                            break;
                    }
                    break;
                }
            }

            string Description = $"\n{Config.triviaQuestions.Questions.ElementAt(QuestionNum).Question}?\n" +
                $"a) {RandomFakes[0]}\n" +
                $"b) {RandomFakes[1]}\n" +
                $"c) {RandomFakes[2]}\n" +
                $"d) {RandomFakes[3]}";
            string Footer = mode == "solo" ? $"Only {GetName(user)} can answer." : "First to answer wins!";
            await context.Channel.SendMessageAsync("", false, Embed(Description, Footer));
        }

        public async Task AnswerTrivia(SocketGuildUser user, SocketCommandContext context, string input)
        {
            if (user == Player && triviaMode == "solo")
            {
                string name = user.Nickname != null ? user.Nickname : user.ToString();
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
                    return;
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