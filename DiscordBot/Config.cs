using System.IO;
using Gideon.Handlers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;

namespace Gideon
{
    class Config
    {
        private const string configFolder = "Resources";
        private const string configFile = "config.json";
        private const string resourcesFile = "resources.json";
        private const string questionsFile = "questions.json";
        private const string triviaQuestionsFile = "trivia_questions.json";

        public static BotConfig bot;
        public static BotResources botResources;
        public static BotQuestions botQuestions;
        public static TriviaQuestions triviaQuestions;

        public static QuestionHandler QuestionHandler = new QuestionHandler();
        public static ImageFetcher ImageFetcher = new ImageFetcher();
        public static Utilities Utilities = new Utilities();
        public static WarnHandler WarnHandler = new WarnHandler();
        public static StatsHandler StatsHandler = new StatsHandler();
        public static TecosHandler TH = new TecosHandler();
        public static MinigameHandler MinigameHandler = new MinigameHandler();

        static Config()
        {
            if (!Directory.Exists(configFolder))
                Directory.CreateDirectory(configFolder);

            if (!File.Exists(configFolder + "/" + configFile))
            {
                string json = JsonConvert.SerializeObject(bot, Formatting.Indented);
                File.WriteAllText(configFolder + "/" + configFile, json);
            }
            else
            {
                string json = File.ReadAllText(configFolder + "/" + configFile);
                bot = JsonConvert.DeserializeObject<BotConfig>(json);
            }

            if (!File.Exists(configFolder + "/" + resourcesFile))
            {
                string json = JsonConvert.SerializeObject(botResources, Formatting.Indented);
                File.WriteAllText(configFolder + "/" + resourcesFile, json);
            }
            else
            {
                string json = File.ReadAllText(configFolder + "/" + resourcesFile);
                botResources = JsonConvert.DeserializeObject<BotResources>(json);
            }

            if (!File.Exists(configFolder + "/" + questionsFile))
            {
                string json = JsonConvert.SerializeObject(botQuestions, Formatting.Indented);
                File.WriteAllText(configFolder + "/" + questionsFile, json);
            }
            else
            {
                string json = File.ReadAllText(configFolder + "/" + questionsFile);
                botQuestions = JsonConvert.DeserializeObject<BotQuestions>(json);
            }

            if (!File.Exists(configFolder + "/" + triviaQuestionsFile))
            {
                string json = JsonConvert.SerializeObject(triviaQuestions, Formatting.Indented);
                File.WriteAllText(configFolder + "/" + triviaQuestionsFile, json);
            }
            else
            {
                string json = File.ReadAllText(configFolder + "/" + triviaQuestionsFile);
                triviaQuestions = JsonConvert.DeserializeObject<TriviaQuestions>(json);
            }
        }

        public static void ModifyChannelWhitelist(string channel, bool isAdding)
        {
            if(isAdding)
                botResources.allowedChannels.Add(channel);
            else
                botResources.allowedChannels.Remove(channel);
            string json = JsonConvert.SerializeObject(botResources, Formatting.Indented);
            File.WriteAllText(configFolder + "/" + resourcesFile, json);
        }

        public static void ModifyNextVideoDate(string input)
        {
            botResources.nextVideoDate = input;
            string json = JsonConvert.SerializeObject(botResources, Formatting.Indented);
            File.WriteAllText(configFolder + "/" + resourcesFile, json);
        }
    }

    public struct BotConfig
    {
        public string DisordBotToken;
        public string MovieTVAPIKey;
        public string YouTubeAPIKey;
    }

    public struct BotResources
    {
        public string nextVideoDate;
        public List<string> bannedWords;
        public List<string> allowedChannels;
    }

    public struct BotQuestions
    {
        public List<string> WhenIsNextVideo;
        public List<string> HowMuchDoesGameCost;
        public List<string> WhichPlatform;
        public List<string> IsBlackLightningInGame;
        public List<string> IsBatmanInGame;
        public List<string> IsBatwomanInGame;
        public List<string> WhereIsDownload;
        public List<string> WhenDoesGameComeOut;
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
}