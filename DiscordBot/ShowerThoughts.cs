using Discord;
using System.Net;
using Newtonsoft.Json;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Gideon
{
    static class ShowerThoughts
    {
        // Print a random Shower Thought from Reddit
        public static async Task PrintRandomThought(ISocketMessageChannel Channel)
        {
            dynamic stuff = null;
            using (WebClient client = new WebClient())
                stuff = JsonConvert.DeserializeObject(client.DownloadString("https://www.reddit.com/r/showerthoughts/top.json?sort=top&t=week&limit=100"));

            stuff = stuff.data.children[Utilities.GetRandomNumber(0, 100)].data;

            await Channel.SendMessageAsync(null, false, new EmbedBuilder()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithIconUrl("https://styles.redditmedia.com/t5_2szyo/styles/communityIcon_z7dkyeif8kzz.png")
                    .WithName("r/Showerthoughts")
                    .WithUrl($"https://www.reddit.com{stuff.permalink}"))
                .WithColor(Utilities.ClearColor)
                .WithDescription(stuff.title.ToString())
                .Build());
        }
    }
}
