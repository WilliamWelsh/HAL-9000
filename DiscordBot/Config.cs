using System.IO;
using Newtonsoft.Json;
using Gideon.Handlers;
using System.Collections.Generic;

namespace Gideon
{
    static class Config
    {
        public static readonly BotConfig bot;
        public static readonly TriviaQuestions triviaQuestions;
        public static readonly whoSaidItResources whoSaidItResources;

        static Config()
        {
            RankHandler.Start();

            if (!Directory.Exists("Resources"))
                Directory.CreateDirectory("Resources");

            // If the file doesn't exist, WriteAllText with the json
            // If it exists, deserialize the json into the corresponding object

            // config.json
            if (!File.Exists("Resources/config.json"))
                File.WriteAllText("Resources/config.json", JsonConvert.SerializeObject(bot, Formatting.Indented));
            else
                bot = JsonConvert.DeserializeObject<BotConfig>(File.ReadAllText("Resources/config.json"));

            // trivia_questions.json
            if (!File.Exists("Resources/trivia_questions.json"))
                File.WriteAllText("Resources/trivia_questions.json", JsonConvert.SerializeObject(triviaQuestions, Formatting.Indented));
            else
                triviaQuestions = JsonConvert.DeserializeObject<TriviaQuestions>(File.ReadAllText("Resources/trivia_questions.json"));

            // whoSaidIt.json
            if (!File.Exists("Resources/whoSaidIt.json"))
                File.WriteAllText("Resources/whoSaidIt.json", JsonConvert.SerializeObject(whoSaidItResources, Formatting.Indented));
            else
                whoSaidItResources = JsonConvert.DeserializeObject<whoSaidItResources>(File.ReadAllText("Resources/whoSaidIt.json"));
        }
    }

    public struct BotConfig
    {
        public string DisordBotToken;
        public string MovieTVAPIKey;
        public string YouTubeAPIKey;
        public List<string> Rs;
        public List<string> alaniPics;
    }

    public struct TriviaQuestion
    {
        public string Question;
        public string Answer;
        public List<string> IncorrectAnswers;
    }

    public struct TriviaQuestions
    {
        public List<TriviaQuestion> Questions;
    }

    public struct whoSaidItQuote
    {
        public string Quote;
        public string Speaker;
    }
    public struct whoSaidItResources
    {
        public List<whoSaidItQuote> Quotes;
        public List<string> Options;
    }
}