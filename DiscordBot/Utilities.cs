using System;
using Discord;
using System.IO;
using System.Net;
using System.Linq;
using System.Drawing;
using ColorThiefDotNet;
using Discord.Commands;
using Discord.WebSocket;

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

        // Return whether a user is Respected or higher
        public bool isRespectedPlus(SocketCommandContext c, SocketGuildUser u)
        {
            var Respected = c.Guild.Roles.FirstOrDefault(x => x.Name == "Respected");
            var Helper = c.Guild.Roles.FirstOrDefault(x => x.Name == "Helpers");
            var Developer = c.Guild.Roles.FirstOrDefault(x => x.Name == "Developer");
            var Lead = c.Guild.Roles.FirstOrDefault(x => x.Name == "Lead");
            var Director = c.Guild.Roles.FirstOrDefault(x => x.Name == "Director");
            if (u.Roles.Contains(Respected) ||
                u.Roles.Contains(Helper) ||
                u.Roles.Contains(Developer) ||
                u.Roles.Contains(Lead) ||
                u.Roles.Contains(Director))
                return true;
            return false;
        }

        // Return whether a user is Helper or higher
        public bool isHelperPlus(SocketCommandContext c, SocketGuildUser u)
        {
            var Helper = c.Guild.Roles.FirstOrDefault(x => x.Name == "Helpers");
            var Developer = c.Guild.Roles.FirstOrDefault(x => x.Name == "Developer");
            var Lead = c.Guild.Roles.FirstOrDefault(x => x.Name == "Lead");
            var Director = c.Guild.Roles.FirstOrDefault(x => x.Name == "Director");
            if (u.Roles.Contains(Helper) ||
                u.Roles.Contains(Developer) ||
                u.Roles.Contains(Lead) ||
                u.Roles.Contains(Director))
                return true;
            return false;
        }
    }
}