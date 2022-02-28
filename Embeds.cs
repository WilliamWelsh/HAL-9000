using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace HAL9000
{
    public static class Embeds
    {
        public static Color Red = new Color(231, 76, 60);
        public static Color Green = new Color(59, 165, 93);

        /// <summary>
        /// Creates an embed with a red background and a message
        /// </summary>
        public static async Task PrintError(this SocketInteraction interaction, string message = null)
        {
            await interaction.RespondAsync(embed: new EmbedBuilder()
                .WithColor(Red)
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithName("Error"))
                .WithDescription(message == null ? "I'm sorry. I'm afraid I can't let you do that." : message)
                .Build(), ephemeral: true);
        }
    }
}