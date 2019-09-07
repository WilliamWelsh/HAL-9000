using Discord.Commands;
using System.Threading.Tasks;

namespace DiscordBot.Minigames
{
    static class _8ball
    {
        private static string[] answers = { "https://i.imgur.com/sof0Iqn.png", "https://i.imgur.com/cRRRWM8.png", "https://i.imgur.com/3yhp7Fd.png",
            "https://i.imgur.com/PXeTy9m.png", "https://i.imgur.com/cVxDhiT.png", "https://i.imgur.com/KgGNoft.png", "https://i.imgur.com/XiXcw6H.png",
            "https://i.imgur.com/Zg7aaIO.png", "https://i.imgur.com/7xk7zEj.png", "https://i.imgur.com/R3FgCed.png" };

        // Display the 8-Ball menu
        public static async Task Greet8Ball(SocketCommandContext context)
        {
            await Utilities.SendEmbed(context.Channel, "8-Ball", "Welcome to 8-Ball! Ask me anything.\n\nExample:\n`!8ball am I cool?`", Utilities.ClearColor, "", "");
        }

        // Play 8-Ball
        public static async Task Play8Ball(SocketCommandContext context)
        {
            await Utilities.SendEmbed(context.Channel, "8-Ball", context.User.Mention, Utilities.ClearColor, "", answers[Utilities.GetRandomNumber(0, answers.Length)]);
        }
    }
}
