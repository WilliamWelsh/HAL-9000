using System.Collections.Generic;
using System.Diagnostics;

namespace HAL9000
{
    public static class Bots
    {
        // All of my bots
        public static List<ulong> List = new List<ulong> {
            477287091798278145, // Rotten Tomatoes
            529569000028373002, // Time Bot
            708136238980399195, // TradeStation Bot
            710949840833740881, // TOSOption Bot
            811803089853874226, // ARK Invest Bot
        };

        // Restart a bot
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
            else if (botID == 708136238980399195) // TradeStation Bot
            {
                path = @"C:\Users\Administrator\Desktop\TradeStationBot";
                fileName = @"C:\Users\Administrator\Desktop\TradeStationBot\TradeStationBot.exe";
            }
            else if (botID == 710949840833740881) // TOSOption Bot
            {
                path = @"C:\Users\Administrator\Desktop\TOSOptionBot";
                fileName = @"C:\Users\Administrator\Desktop\TOSOptionBot\TOSOptionBot.exe";
            }
            else if (botID == 710949840833740881) // ARK Invest Bot
            {
                path = @"C:\Users\Administrator\Desktop\ARK Bot";
                fileName = @"C:\Users\Administrator\Desktop\ARK Bot\ARK-Invest-Bot.exe";
            }

            // Find the existing bot process and kill it
            var procceses = Process.GetProcessesByName(fileName.Substring(fileName.LastIndexOf("\\") + 1).Replace(".exe", ""));
            if (procceses != null)
                foreach (var process in procceses)
                    process.Kill();

            // Start the bot
            Process.Start(new ProcessStartInfo { FileName = fileName, WorkingDirectory = path, UseShellExecute = true });
        }
    }
}