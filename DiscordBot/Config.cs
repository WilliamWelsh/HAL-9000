using System.IO;
using Gideon.Handlers;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Gideon
{
    static class Config
    {
        public static readonly BotConfig bot;
        public static readonly whoSaidItResources whoSaidItResources;

        static Config()
        {
            RankHandler.Start();
            MinigameHandler.InitialTriviaSetup();

            if (!Directory.Exists("Resources"))
                Directory.CreateDirectory("Resources");

            // If the file doesn't exist, WriteAllText with the json
            // If it exists, deserialize the json into the corresponding object

            // config.json
            if (!File.Exists("Resources/config.json"))
                File.WriteAllText("Resources/config.json", JsonConvert.SerializeObject(bot, Formatting.Indented));
            else
                bot = JsonConvert.DeserializeObject<BotConfig>(File.ReadAllText("Resources/config.json"));

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
        public List<string> Rs;
        public List<string> alaniPics;
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