using System;
using Discord;
using System.Linq;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Gideon.Minigames
{
    class Trivia
    {
        private bool isTriviaBeingPlayed = false;
        private SocketGuildUser userPlaying = null;
        private string correctAnswer, triviaMode;
        private DateTime StartTime;
        private List<SocketGuildUser> PlayersAnswered = new List<SocketGuildUser>();

        private Embed Embed(string Description, string Footer)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle("Trivia");
            embed.WithDescription(Description);
            embed.WithColor(new Color(31, 139, 76));
            embed.WithFooter(Footer);
            return embed;
        }

        private string GetName(SocketGuildUser user) => user.Nickname ?? user.Username;

        public async Task TryToStartTrivia(SocketGuildUser user, SocketCommandContext context, string input)
        {
            if (context.Channel.Id != 518846214603669537)
            {
                await Config.Utilities.PrintError(context, $"Please use the {context.Guild.GetTextChannel(518846214603669537).Mention} chat for that, {user.Mention}.");
                return;
            }
            if (isTriviaBeingPlayed && (DateTime.Now - StartTime).TotalSeconds < 60)
            {
                await context.Channel.SendMessageAsync("", false, Embed($"Sorry, {userPlaying.Mention} is currently playing.\nYou can request a Respected+ to `!reset trivia` if there is an issue.", ""));
                return;
            }
            if(input == "trivia")
            {
                await context.Channel.SendMessageAsync("", false, Embed("Please select a mode.\n\n`!trivia solo` - Play alone.\n\n`!trivia all` - First to answer wins.", ""));
                return;
            }
            if (isTriviaBeingPlayed && (DateTime.Now - StartTime).TotalSeconds > 60)
                await CancelGame(userPlaying, context);
            await StartTrivia(user, context, input.Replace("trivia ", ""));
        }

        private async Task CancelGame(SocketGuildUser user, SocketCommandContext context)
        {
            Config.CoinHandler.AdjustCoins(user, -1);
            await context.Channel.SendMessageAsync("", false, Embed($"{user.Mention} took too long to answer and lost 1 coin.", ""));
        }

        private async Task StartTrivia(SocketGuildUser user, SocketCommandContext context, string mode)
        {
            userPlaying = user;
            triviaMode = mode;
            isTriviaBeingPlayed = true;
            StartTime = DateTime.Now;
            int QuestionNum = Config.Utilities.GetRandomNumber(0, Config.triviaQuestions.Questions.Count);

            string[] Fakes = {"","","",""};

            Fakes[0] = Config.triviaQuestions.Questions.ElementAt(QuestionNum).IncorrectAnswers.ElementAt(0);
            Fakes[1] = Config.triviaQuestions.Questions.ElementAt(QuestionNum).IncorrectAnswers.ElementAt(1);
            Fakes[2] = Config.triviaQuestions.Questions.ElementAt(QuestionNum).IncorrectAnswers.ElementAt(2);
            Fakes[3] = Config.triviaQuestions.Questions.ElementAt(QuestionNum).Answer;

            Random rdn = new Random();
            string[] RandomFakes = Fakes.OrderBy(x => rdn.Next()).ToArray();

            for (int n = 0; n < RandomFakes.Length; n++)
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
            if (user == userPlaying && triviaMode == "solo")
            {
                string name = user.Nickname != null ? user.Nickname : user.ToString();
                if (input == correctAnswer)
                {
                    await context.Channel.SendMessageAsync("", false, Embed("Correct.", $"{GetName(user)} has been awarded 1 Coin."));
                    Config.CoinHandler.AdjustCoins(user, 1);
                    ResetTrivia();
                    return;
                }
                await context.Channel.SendMessageAsync("", false, Embed($"Wrong, it is {correctAnswer.ToUpper()}.", $"{GetName(user)} lost 1 Coin."));
                Config.CoinHandler.AdjustCoins(user, -1);
                ResetTrivia();
                return;
            }
            if (triviaMode == "all" && isTriviaBeingPlayed)
            {
                for (int i = 0; i < PlayersAnswered.Count; i++)
                {
                    if (PlayersAnswered.ElementAt(i) == user)
                    {
                        await context.Channel.SendMessageAsync("", false, Embed($"You already answered, {user.Mention}.", ""));
                        return;
                    }
                }

                PlayersAnswered.Add(user);
                if (input == correctAnswer)
                {
                    await context.Channel.SendMessageAsync("", false, Embed($"Correct, {user.Mention} won!", $"{GetName(user)} has been awarded 1 Coin."));
                    Config.CoinHandler.AdjustCoins(user, 1);
                    ResetTrivia();
                    return;
                }
            }
        }

        public void ResetTrivia()
        {
            userPlaying = null;
            isTriviaBeingPlayed = false;
            PlayersAnswered.Clear();
        }
    }
}