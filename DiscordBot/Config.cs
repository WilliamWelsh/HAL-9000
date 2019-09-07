using System.IO;
using System.Reflection;
using System.Diagnostics;
using DiscordBot.Handlers;
using System.Collections.Generic;

namespace DiscordBot
{
    static class Config
    {
        public static List<string> AlaniPictures;
        public const ulong MiniGamesChannel = 618549807623045132;

        public static readonly List<ulong> MyBots = new List<ulong> {
            477287091798278145, // Rotten Tomatoes
            529569000028373002, // Time Bot
        };

        public static void Setup()
        {
            // Set up trivia questions & who said it? questions
            MinigameHandler.SetUpMinigames();

            // Create my resources folder if it doesn't exist
            if (!Directory.Exists("Resources"))
                Directory.CreateDirectory("Resources");

            // Set the AlaniPictures list up
            AlaniPictures = new List<string>();
            using (StreamReader sr = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Gideon.AlaniPictures.txt")))
                while (sr.Peek() >= 0)
                    AlaniPictures.Add(sr.ReadLine());
        }

        public static void RestartBot(ulong botID)
        {
            string path = "";
            string fileName = "";

            if (botID == 477287091798278145) // Rotten Tomatoes
            {
                path = @"C:\Users\Administrator\Desktop\RTBot";
                fileName = @"C:\Users\Administrator\Desktop\RTBot\RottenTomatoes.exe";
            }
            else if (botID == 529569000028373002) // Time Bot
            {
                path = @"C:\Users\Administrator\Desktop\TimeBot";
                fileName = @"C:\Users\Administrator\Desktop\TimeBot\TimeBot.exe";
            }

            // Find the existing bot process and kill it
            var procceses = Process.GetProcessesByName(fileName.Substring(fileName.LastIndexOf("\\") + 1).Replace(".exe", ""));
            if (procceses != null)
                foreach (var process in procceses)
                    process.Kill();

            // Start the bot
            Process.Start(new ProcessStartInfo { FileName = fileName, WorkingDirectory = path });
        }
    }
}