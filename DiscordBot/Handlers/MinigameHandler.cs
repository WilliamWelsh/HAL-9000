using System;
using System.IO;
using Newtonsoft.Json;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using DiscordBot.Minigames;
using System.Threading.Tasks;
using System.Collections.Generic;
using Gideon.Minigames;

namespace DiscordBot.Handlers
{
    class MinigameHandler
    {
        public static Trivia Trivia = new Trivia();
        public static TriviaQuestions TriviaQuestions;

        public static TicTacToe TTT = new TicTacToe();
        public static FloodIt FloodIt = new FloodIt();
        public static NumberGuess NG = new NumberGuess();
        public static RussianRoulette RR = new RussianRoulette();
        public static RockPaperScissors RPS = new RockPaperScissors();
        public static UnbeatableTicTacToe UnbeatableTTT = new UnbeatableTicTacToe();

        private static readonly List<AITTTPlayer> AITTTPlayers = new List<AITTTPlayer>();

        // Set up Trivia Questions
        public static void SetUpMinigames()
        {
            using (StreamReader sr = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Gideon.Minigames.Resources.trivia_questions.json")))
                TriviaQuestions = JsonConvert.DeserializeObject<TriviaQuestions>(sr.ReadToEnd());
        }

        // Display available minigames
        public static async Task DisplayGames(SocketCommandContext context)
        {
            await Utilities.SendEmbed(context.Channel, "MiniGames", "Trivia\n`!trivia`\n\nTic-Tac-Toe\n`!ttt`\n\nNumber Guess\n`!play ng`\n\nRussian Roulette\n`!rr`\n\n8-Ball\n`!8ball`\n\nRock-Paper-Scissors\n`!rps`", Utilities.Green, "", "");
        }

        public async Task TryToStartTrivia(SocketCommandContext context, string input)
        {
            if (input == "all")
                await Trivia.TryToStartTrivia((SocketGuildUser)context.User, context, "all");
        }

        // Have to call this from TicTacToe(s)
        public static void ResetTTT() => TTT = new TicTacToe();
        public static void ResetUTTT() => UnbeatableTTT = new UnbeatableTicTacToe();

        // Start Unbeatable Tic-Tac-Toe
        public static async Task StartAITicTacToe(SocketCommandContext Context)
        {
            for (int i = 0; i < AITTTPlayers.Count; i++)
            {
                if (AITTTPlayers[i].User == Context.User)
                {
                    AITTTPlayers[i] = new AITTTPlayer(Context.User, new UnbeatableTicTacToe());
                    await AITTTPlayers[i].Game.StartGame(Context).ConfigureAwait(false);
                    return;
                }
            }

            var newPlayer = new AITTTPlayer(Context.User, new UnbeatableTicTacToe());
            await newPlayer.Game.StartGame(Context).ConfigureAwait(false);
            AITTTPlayers.Add(newPlayer);
        }

        // Unbeatable TTT Reaction Handler
        public static async Task ReactToAITicTacToe(ulong UserID, SocketReaction Reaction)
        {
            for (int i = 0; i < AITTTPlayers.Count; i++)
            {
                if (AITTTPlayers[i].User.Id == UserID)
                {
                    await AITTTPlayers[i].Game.Play(Reaction).ConfigureAwait(false);
                    return;
                }
            }
        }

        // Print TTT option
        public static async Task PrintTTTOptions(ISocketMessageChannel Channel)
        {
            await Utilities.SendEmbed(Channel, "Tic-Tac-Toe Options", "`!ttt 2` - 2 Player\n\n`!join ttt` - Join 2 player TTT\n\n`!ttt ai` - Play with Gideon", Utilities.ClearColor, "", "");
        }

        // Reset a game
        public static async Task ResetGame(SocketCommandContext context, string game)
        {
            if (game == "trivia")
            {
                await Utilities.SendEmbed(context.Channel, "MiniGames", $"{context.User.Mention} has reset Trivia.", Utilities.ClearColor, "", "");
                Trivia.ResetTrivia();
            }
            else if (game == "rr")
            {
                await Utilities.SendEmbed(context.Channel, "MiniGames", $"{context.User.Mention} has reset Russian Roulette.", Utilities.ClearColor, "", "");
                RR.Reset();
            }
            else if (game == "ttt")
            {
                await Utilities.SendEmbed(context.Channel, "MiniGames", $"{context.User.Mention} has reset Tic-Tac-Toe.", Utilities.ClearColor, "", "");
                ResetTTT();
            }
            else if (game == "ng")
            {
                await Utilities.SendEmbed(context.Channel, "MiniGames", $"{context.User.Mention} has reset the Number Guess game.", Utilities.ClearColor, "", "");
                NG.Reset();
            }
            else if (game == "")
                await Utilities.PrintError(context.Channel, "Please specify a game to reset.");
            else
                await Utilities.PrintError(context.Channel, $"I was unable to find the `{game}` game.\n\nAvailable games to reset:\nTrivia\n`!trivia`\n\nTic-Tac-Toe\n`!ttt`\n\nNumber Guess\n`!play ng`\n\nRussian Roulette\n`!rr`");
        }
    }

    //  Player data for an instance of Unbeatable Tic Tac Toe
    public class AITTTPlayer : IEquatable<AITTTPlayer>
    {
        public SocketUser User { get; }
        public UnbeatableTicTacToe Game { get; }

        public AITTTPlayer(SocketUser user, UnbeatableTicTacToe game)
        {
            User = user;
            Game = game;
        }

        public bool Equals(AITTTPlayer other) => User.Id == other.User.Id;

        public override bool Equals(object obj) => Equals(obj as AITTTPlayer);

        public override int GetHashCode() => 0; // Sorry
    }

    // Trivia Questions
    public class TriviaQuestions
    {
        [JsonProperty("Questions")]
        public TriviaQuestion[] Questions { get; set; }
    }

    public class TriviaQuestion
    {
        [JsonProperty("Question")]
        public string QuestionQuestion { get; set; }

        [JsonProperty("Answer")]
        public string Answer { get; set; }

        [JsonProperty("IncorrectAnswers")]
        public string[] IncorrectAnswers { get; set; }
    }
}
