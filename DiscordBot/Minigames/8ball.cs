using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace Gideon.Minigames
{
    class _8ball
    {
        private Embed Embed(string URL, string desc)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle("8-Ball");
            embed.WithDescription(desc);
            embed.WithImageUrl(URL);
            embed.WithColor(new Color(75, 0, 130));
            return embed;
        }

        private string[] answers = { "https://i.imgur.com/sof0Iqn.png", "https://i.imgur.com/cRRRWM8.png", "https://i.imgur.com/3yhp7Fd.png",
            "https://i.imgur.com/PXeTy9m.png", "https://i.imgur.com/cVxDhiT.png", "https://i.imgur.com/KgGNoft.png", "https://i.imgur.com/XiXcw6H.png",
            "https://i.imgur.com/Zg7aaIO.png", "https://i.imgur.com/7xk7zEj.png", "https://i.imgur.com/R3FgCed.png" };

        public async Task Play8Ball(SocketCommandContext context)
        {
            int number = Config.Utilities.GetRandomNumber(0, 10);
            await context.Channel.SendMessageAsync("", false, Embed(answers[number], context.User.Mention));
        }

        public async Task Greet8Ball(SocketCommandContext context) => await context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("8-Ball", "Welcome to 8-Ball! Ask me anything.\n\nExample:\n`!8ball am I cool?`", new Color(75, 0, 130), "", ""));
    }
}