using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace Gideon.Minigames
{
    class _8ball
    {
        private static readonly Color color = new Color(75, 0, 130);

        private string[] answers = { "https://i.imgur.com/sof0Iqn.png", "https://i.imgur.com/cRRRWM8.png", "https://i.imgur.com/3yhp7Fd.png",
            "https://i.imgur.com/PXeTy9m.png", "https://i.imgur.com/cVxDhiT.png", "https://i.imgur.com/KgGNoft.png", "https://i.imgur.com/XiXcw6H.png",
            "https://i.imgur.com/Zg7aaIO.png", "https://i.imgur.com/7xk7zEj.png", "https://i.imgur.com/R3FgCed.png" };

        public async Task Play8Ball(SocketCommandContext context) => await context.Channel.SendMessageAsync("", false, Utilities.ImageEmbed("8-Ball", context.User.Mention, color, "", answers[Utilities.GetRandomNumber(0, answers.Length)]));

        public async Task Greet8Ball(SocketCommandContext context) => await context.Channel.SendMessageAsync("", false, Utilities.Embed("8-Ball", "Welcome to 8-Ball! Ask me anything.\n\nExample:\n`!8ball am I cool?`", color, "", ""));
    }
}