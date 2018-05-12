using System.IO;
using Gideon.Modules;
using Newtonsoft.Json;
using Gideon.Minigames;
using System.Collections.Generic;

namespace Gideon
{
    class Config
    {
        private const string configFolder = "Resources";
        private const string configFile = "config.json";

        public static BotConfig bot;

        public static string[] Staff = { "admin#0001", "Gannon#7623", "Cottage#5796", "Tecosaurus#6343",
                                        "Ralph282#9943", "Professor Zoom#6274", "Galva_#0939", "NoahCody#0214",
                                        "Renz#3903", "KaraZor#8671", "Jack_Hartley97#6754", "Jack_Hartley97#7912",
                                        "Jonathan TRG#2932" };

        public static TecosHandler TH = new TecosHandler();
        public static RussianRoulette RR = new RussianRoulette();

        static Config()
        {
            if (!Directory.Exists(configFolder))
                Directory.CreateDirectory(configFolder);

            if (!File.Exists(configFolder + "/" + configFile))
            {
                bot = new BotConfig { allowedChannels = new List<string>() };
                string json = JsonConvert.SerializeObject(bot, Formatting.Indented);
                File.WriteAllText(configFolder + "/" + configFile, json);
            }
            else
            {
                string json = File.ReadAllText(configFolder + "/" + configFile);
                bot = JsonConvert.DeserializeObject<BotConfig>(json);
            }
        }

        public static void ModifyChannelWhitelist(string channel, bool isAdding)
        {
            if(isAdding)
                bot.allowedChannels.Add(channel);
            else
                bot.allowedChannels.Remove(channel);
            string json = JsonConvert.SerializeObject(bot, Formatting.Indented);
            File.WriteAllText(configFolder + "/" + configFile, json);
        }
    }

    public struct BotConfig
    {
        public string DisordBotToken;
        public string MovieTVAPIKey;
        public string YouTubeAPIKey;
        public string cmdPrefix;
        public List<string> allowedChannels;
    }
}