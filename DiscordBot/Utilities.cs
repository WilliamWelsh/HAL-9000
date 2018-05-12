// Contains 
using ColorThiefDotNet;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Gideon
{
    class Utilities
    {
        // Generic Embed template
        public static Embed Embed(string t, string d, Discord.Color c, string f, string thURL)
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

        // Get the warning thread as a variable
        private ISocketMessageChannel warningThread(SocketCommandContext context)
        {
            var channels = context.Guild.Channels;
            SocketGuildChannel c = null;
            foreach (SocketGuildChannel s in channels)
            {
                if (s.Name == "warning-thread") c = s;
            }
            return c as ISocketMessageChannel;
        }

        // Update warn information to user_data.json
        private void UpdateWarnDB(UserAccount account, string reason, string warner)
        {
            account.Warns++;
            account.warnReasons.Add(reason);
            account.Warners.Add(warner);
            UserAccounts.SaveAccounts();
        }

        // Check if user needs to be kicked or banned
        private async Task CheckWarnCount(UserAccount account, SocketCommandContext context, SocketGuildUser offender)
        {
            if (account.Warns == 3)
            {
                await KickAndNotify(context, offender, account, warningThread(context));
                return;
            }

            if (account.Warns == 5)
            {
                await BanAndNotify(context, offender, account, warningThread(context));
                return;
            }
        }

        private Embed warnEmbed(string title, string description, string ThumbnailURL)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle(title);
            embed.WithDescription(description);
            embed.WithColor(new Discord.Color(227, 37, 39));
            embed.WithThumbnailUrl(ThumbnailURL);
            return embed;
        }

        public Embed AllWarnsEmbed(SocketGuildUser offender, UserAccount account)
        {
            var embed = new EmbedBuilder();

            embed.WithTitle($"{offender}'s Warnings");
            embed.WithColor(new Discord.Color(227, 37, 39));
            embed.AddField("Total Warns", account.Warns);
            embed.ThumbnailUrl = offender.GetAvatarUrl();

            for (int i = 0; i < account.warnReasons.Count; i++)
            {
                embed.AddField("Warning " + (i + 1), "Warned by " + account.Warners[i] + " for " + account.warnReasons[i]);
            }

            return embed;
        }

        // Used to automatically warn people (@everyone, jukebox outside off topic)
        public async Task AutoWarn(SocketCommandContext context, SocketGuildUser offender, string reason)
        {
            var warnThread = warningThread(context);

            var embed = warnEmbed("Warning", $"{offender.Mention} was warned by Gideon for {reason}.", offender.GetAvatarUrl());
            await warnThread.SendMessageAsync("", false, embed);
            await context.Channel.SendMessageAsync("", false, embed);

            await offender.SendMessageAsync("", false, warnEmbed("Warning", $"You were automatically warned for {reason}.\nPlease do not do this again.", offender.GetAvatarUrl()));

            var account = UserAccounts.GetAccount(offender);
            UpdateWarnDB(account, $"{offender.Mention} was warned by Gideon for {reason}.", "Gideon");
            await CheckWarnCount(account, context, offender);
        }

        // Warn people with the !warn command
        public async Task WarnUser(SocketCommandContext context, SocketGuildUser offender, string reason)
        {
            var account = UserAccounts.GetAccount(offender);
            UpdateWarnDB(account, reason, context.User.ToString());

            var embed = warnEmbed("Warning", $"{offender.Mention} was warned by {context.User.Mention} for {reason}.", offender.GetAvatarUrl());

            await context.Channel.SendMessageAsync("", false, embed);

            ISocketMessageChannel warnThread = warningThread(context);
            await warnThread.SendMessageAsync("", false, embed);

            await offender.SendMessageAsync("", false, warnEmbed("Warning", $"You were warned by {context.User.Mention} for {reason}.\n\nThis is warning {account.Warns}.\nThree warns is a kick, five is a ban.", context.User.GetAvatarUrl()));

            await CheckWarnCount(account, context, offender);
        }

        public async Task KickAndNotify(SocketCommandContext context, SocketGuildUser offender, UserAccount account, ISocketMessageChannel warningThread)
        {
            var Director = context.Guild.Roles.FirstOrDefault(x => x.Name == "Director");
            foreach (SocketGuildUser u in context.Guild.Users.ToArray())
            {
                if (u.Roles.Contains(Director))
                {
                    var embed = AllWarnsEmbed(offender, account);

                    await offender.SendMessageAsync("", false, embed);
                    await offender.SendMessageAsync("You were kicked for reaching 3 warnings. 2 more warnings (5), and you will be banned.\nPlease obey the rules.");
                    await offender.KickAsync("You were kicked for reaching 3 warnings. 2 more warnings (5), and you will be banned. Please obey the rules.");
                    await u.SendMessageAsync("", false, embed);
                    await u.SendMessageAsync("[KICK] " + offender.ToString() + " has been kicked for reaching 3 warnings.");
                    await warningThread.SendMessageAsync("[KICK] " + offender.ToString() + " has been kicked for reaching 3 warnings.");
                    await context.Channel.SendMessageAsync("[KICK] " + offender.ToString() + " has been kicked for reaching 3 warnings.");
                    return;
                }
            }
        }

        public async Task BanAndNotify(SocketCommandContext context, SocketGuildUser offender, UserAccount account, ISocketMessageChannel warningThread)
        {
            var Director = context.Guild.Roles.FirstOrDefault(x => x.Name == "Director");
            foreach (SocketGuildUser u in context.Guild.Users.ToArray())
            {
                if (u.Roles.Contains(Director))
                {
                    var embed = AllWarnsEmbed(offender, account);

                    await offender.SendMessageAsync("", false, embed);
                    await offender.SendMessageAsync("You have been banned from the Crisis on Earth One Discord. You can appeal your ban on the Tecoverse Discord.\nhttps://discord.gg/yD7Rxnu");
                    await offender.Guild.AddBanAsync(offender, 0);
                    await u.SendMessageAsync("", false, embed);
                    await u.SendMessageAsync("[BAN] " + offender.ToString() + " has been banned for reaching 5 warnings.");
                    await warningThread.SendMessageAsync("", false, embed);
                    await warningThread.SendMessageAsync("[BAN] " + offender.ToString() + " has been banned for reaching 5 warnings.");
                    await context.Channel.SendMessageAsync("[BAN] " + offender.ToString() + " has been banned for reaching 5 warnings.");
                    return;
                }
            }
        }
    }
}
