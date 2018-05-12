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
        TecosHandler TH = new TecosHandler();

        string[] Questions = { "What is Beebo the God of",
        "What is name of the CCPD Captain",
        "Which Earth is Accelerated Man on",
        "When did the first episode of The Flash air",
        "In which episode did Barry first time travel",
        "Which character does Teddy Sears play in The Flash",
        "Who is the time travelling protagonist from the future in Legends of Tomorrow",
        "What is the date of the newspaper inside of the Time Vault",
        "How many Totems of Zambesi exist",
        "Which character in the Arrowverse was first to be 100% CGI",
        "What is the time travelling organisation Rip Hunter founded in Legends of Tomorrow",
        "How many times has Mark Hamill appeared on the Flash as The Trickster",
        "How many Actors/Actresses who were on The Flash (1990) have appeared on The Flash (2014)",
        "Which actor/actress has played the most characters",
        "Which character is Jewish",
        "Which company does Arrow's Visual Effects",
        "Which episode did we see Barry run the fastest",
        "Who made a guest appearance at the end of the Flash Season 1 finale",
        "How many bus metas are there in The Flash season 4",
        "What is the enchancement drug from Arrow season 2 called",
        "When does Stephen Amells Arrow contract end",
        "Who has not travelled through time",
        "Which Earth is The Ray as featured in the crossover from",
        "How many animated shows from CW Seed are set in the Arrowverse",
        "What Network was Supergirl on during Season 1",
        "How many TV shows set in the Arrowverse",
        "How many Earths in the multiverse",
        "Which of these characters don't have their own game",
        "What is the name of Barry's pet Turtle",
        "Who played Sara Lance in the pilot episode of Arrow",
        "Who is the showrunner for Supergirl",
        "Who directed the Arrow and The Flash pilot",
        "Who voices Prometheus in Arrow",
        "How many episodes in Legends of Tomorrow Season 2",
        "Who plays Vandal Savage"};

        string[] Answers = { "God of War", "David Singh", "Earth-19", "October 7, 2014", "\"Out of Time\"", "Hunter Zolomon",
        "Rip Hunter", "Thursday April 25 2024", "6", "Invunche", "Time Bureau", "3", "4", "Tom Cavanagh", "Felicity Smoak", "Zoic Studios", "\"Enter Flashtime\"", "Kendra Saunders", "12", "Mirakuru",
        "2019", "Oliver", "Earth-1", "2", "CBS", "5", "53", "Green Arrow", "McSnurtle", "Jacqueline Macinnes Wood", "Alison Adler", "David Nutter", "Michael Dorn", "17", "Casper Crump"};

        string[] WrongAnswers =
        {
            "God of Mischief",
            "God of Thunder",
            "God of Chaos",

            "Joe West",
            "Julian Albert",
            "Sam Scudder",

            "Earth-17",
            "Earth-39",
            "Earth-24",

            "October 9, 2014",
            "October 7, 2015",
            "October 4, 2014",

            "\"Fast Enough\"",
            "\"Flash Back\"",
            "\"Legends of Today\"",

            "Jay Garrick",
            "Black Flash",
            "Firestorm",

            "Marty McFly",
            "Phillip J Fry",
            "Vandal Savage",

            "Thursday May 24 2025",
            "Tuesday April 25 2026",
            "Tuesday April 23 2024",

            "4",
            "5",
            "8",

            "Parasite",
            "Gorilla Grodd",
            "King Shark",

            "Time Travellers",
            "Time Masters",
            "The Legends",

            "4",
            "1",
            "2",

            "0",
            "2",
            "6",

            "Grant Gustin",
            "Katie Cassidy",
            "Teddy Sears",

            "John Constantine",
            "Kara Danvers",
            "Ray Palmer",

            "Encore VFX",
            "Luma Pictures",
            "Industrial Light and Magic",

            "\"Fast Enough\"",
            "\"Enter Zoom\"",
            "\"Wrath of Savitar\"",

            "Zoom",
            "Jay Garrick",
            "Captain Cold",

            "6",
            "9",
            "14",

            "Vertigo",
            "Drug X",
            "Velocity-9",

            "2021",
            "2020",
            "2018",

            "Felicity",
            "Constantine",
            "Cisco",

            "Earth-2",
            "Earth-X",
            "Earth-10",

            "1",
            "3",
            "4",

            "The CW",
            "NBC",
            "FOX",

            "3",
            "4",
            "6",

            "52",
            "51",
            "Infinite",

            "Constantine",
            "Superman",
            "The Flash",

            "Turtle",
            "Bart",
            "Cosmos",

            "Caity Lotz",
            "Kathleen Gati",
            "Wendy Mericle",

            "Wendy Mericle",
            "Beth Schwartz",
            "Andrew Kreisberg",

            "Greg Berlanti",
            "JJ Abrams",
            "Andrew Kreisberg",

            "Josh Segerra",
            "John Barrowman",
            "Tony Todd",

            "16",
            "22",
            "23",

            "Falk Hentschel",
            "Glen Winter",
            "Neal McDonough"

        };

        bool isTriviaBeingPlayed = false;
        SocketGuildUser userPlaying = null;
        string correctAnswer;
        string triviaMode;

        List<SocketGuildUser> PlayersAnswered = new List<SocketGuildUser>();

        Embed Embed(string Description, string Footer)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle("Trivia");
            embed.WithDescription(Description);
            embed.WithColor(new Color(0, 172, 0));
            embed.WithFooter(Footer);
            return embed;
        }

        private static readonly Random getrandom = new Random();

        public static int GetRandomNumber(int min, int max)
        {
            lock (getrandom) { return getrandom.Next(min, max); }
        }

        public async Task TryToStartTrivia(SocketGuildUser user, SocketCommandContext context, string input)
        {
            if (isTriviaBeingPlayed)
            {
                await context.Channel.SendMessageAsync("", false, Embed($"Sorry, {userPlaying.Mention} is currently playing.\nYou can request a Respected+ to `!reset trivia` if there is an issue.", ""));
                return;
            }
            if(input == "trivia")
            {
                await context.Channel.SendMessageAsync("", false, Embed("Please select a mode.\n\n`!trivia solo` - Play alone\n\n`!trivia all` - First to answer wins.", ""));
                return;
            }
            await StartTrivia(user, context, input.Replace("trivia ", ""));
        }

        private async Task StartTrivia(SocketGuildUser user, SocketCommandContext context, string mode)
        {
            userPlaying = user;
            triviaMode = mode;
            isTriviaBeingPlayed = true;
            int QuestionNum = GetRandomNumber(0, Questions.Length);

            string[] Fakes = {"","","",""};

            Fakes[0] = WrongAnswers[QuestionNum * 3];
            Fakes[1] = WrongAnswers[(QuestionNum * 3) + 1];
            Fakes[2] = WrongAnswers[(QuestionNum * 3) + 2];
            Fakes[3] = Answers[QuestionNum];

            Random rdn = new Random();
            string[] RandomFakes = Fakes.OrderBy(x => rdn.Next()).ToArray();

            for (int n = 0; n < RandomFakes.Length; n++)
            {
                if (RandomFakes[n] == Answers[QuestionNum])
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

            string Description = $"\n{Questions[QuestionNum]}?\n" +
                $"a) {RandomFakes[0]}\n" +
                $"b) {RandomFakes[1]}\n" +
                $"c) {RandomFakes[2]}\n" +
                $"d) {RandomFakes[3]}";
            string name = userPlaying.Nickname != null ? userPlaying.Nickname : userPlaying.ToString();
            string Footer = mode == "solo" ? $"Only {name} can answer." : "First to answer wins!";
            await context.Channel.SendMessageAsync("", false, Embed(Description, Footer));
        }

        public async Task AnswerTrivia(SocketGuildUser user, SocketCommandContext context, string input)
        {
            if (user == userPlaying && triviaMode == "solo")
            {
                string name = user.Nickname != null ? user.Nickname : user.ToString();
                if (input == correctAnswer)
                {
                    await context.Channel.SendMessageAsync("", false, Embed("Correct.", $"{name} has been awarded 1 Teco."));
                    TH.AdjustTecos(user, 1);
                    ResetTrivia();
                    return;
                }
                await context.Channel.SendMessageAsync("", false, Embed($"Wrong, it is {correctAnswer.ToUpper()}.", $"{name} lost 1 Teco."));
                TH.AdjustTecos(user, -1);
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
                    string name = user.Nickname != null ? user.Nickname : user.ToString();
                    await context.Channel.SendMessageAsync("", false, Embed($"Correct, {user.Mention} won!", $"{name} has been awarded 1 Teco."));
                    TH.AdjustTecos(user, 1);
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