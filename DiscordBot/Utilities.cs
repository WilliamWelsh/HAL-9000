using System;
using Discord;
using System.IO;
using System.Net;
using System.Linq;
using System.Drawing;
using ColorThiefDotNet;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Gideon
{
    class Utilities
    {
        // Get a true random number
        private static readonly Random getrandom = new Random();
        public int GetRandomNumber(int min, int max)
        {
            lock (getrandom) { return getrandom.Next(min, max); }
        }

        // Generic Embed template
        public Embed Embed(string t, string d, Discord.Color c, string f, string thURL)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle(t);
            embed.WithDescription(d);
            embed.WithColor(c);
            embed.WithFooter(f);
            embed.WithThumbnailUrl(thURL);
            return embed;
        }

        // Print an error
        public async Task PrintError(SocketCommandContext context, string description) => await context.Channel.SendMessageAsync("", false, Embed("Error", description, new Discord.Color(227, 37, 39), "", ""));

        // Get a dominant color from an image (url)
        public Discord.Color DomColorFromURL(string url)
        {
            var colorThief = new ColorThief();
            WebClient client = new WebClient();

            byte[] bytes = client.DownloadData(url);
            MemoryStream ms = new MemoryStream(bytes);
            Bitmap bitmap = new Bitmap(System.Drawing.Image.FromStream(ms));

            // Get the hexadecimal and remove the '#'
            string color = colorThief.GetColor(bitmap).Color.ToString().Substring(1);

            // First two values of the hex
            int r = int.Parse(color.Substring(0, color.Length - 4), System.Globalization.NumberStyles.AllowHexSpecifier);

            // Get the middle two values of the hex
            int g = int.Parse(color.Substring(2, 2), System.Globalization.NumberStyles.AllowHexSpecifier);

            // Final two values
            int b = int.Parse(color.Substring(4), System.Globalization.NumberStyles.AllowHexSpecifier);

            return new Discord.Color(r, g, b);
        }

		// Convert a hexidecimal to an RGB value (input does not include the '#')
		public Discord.Color HexToRGB(string hex)
		{
			// First two values of the hex
			int r = int.Parse(hex.Substring(0, hex.Length - 4), System.Globalization.NumberStyles.AllowHexSpecifier);

			// Get the middle two values of the hex
			int g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.AllowHexSpecifier);

			// Final two values
			int b = int.Parse(hex.Substring(4), System.Globalization.NumberStyles.AllowHexSpecifier);

			return new Discord.Color(r, g, b);
		}
	}
}