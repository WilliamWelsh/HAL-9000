using System.IO;
using Gideon.Handlers;
using System.Reflection;
using System.Collections.Generic;

namespace Gideon
{
    static class Config
    {
        public static List<string> AlaniPictures;

        public static readonly List<ulong> MyBots = new List<ulong> {
            436780808745910282, // Alani
            477287091798278145, // Rotten Tomatoes
            529569000028373002 // Time Bot
        };

        public static void Setup()
        {
            // Start giving people xp for their messages
            RankHandler.Start();

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
    }
}