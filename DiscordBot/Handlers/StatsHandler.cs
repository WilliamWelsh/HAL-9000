using System;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Gideon.Handlers
{
    class StatsHandler
    {
        public static string GetCreatedDate(SocketGuildUser user)
        {
            try
            {
                return user.CreatedAt.ToString("MMMM dd, yyy");
            }
            catch (Exception)
            {
                return "Error finding date.";
            }
        }

        public static string GetJoinedDate(SocketGuildUser user)
        {
            try
            {
                return ((DateTimeOffset)user.JoinedAt).ToString("MMMM dd, yyy");
            }
            catch (Exception)
            {
                return "Error finding date.";
            }
        }

        private static readonly Color timeColor = new Color(127, 166, 208);
        private static string GetTime(SocketGuildUser user)
        {
            if (UserAccounts.GetAccount(user).localTime == 999)
                return $"Not set.\nContact Reverse to set it up.";
            DateTime localTime = DateTime.Now.AddHours(UserAccounts.GetAccount(user).localTime);
            return $"It's {localTime.ToString("h:mm tt")} for {user.Nickname ?? user.Username}.\n{localTime.ToString("dddd, MMMM d.")}";
        }

        // Print server stats
        public static async Task DisplayServerStats(SocketCommandContext context) => await context.Channel.SendMessageAsync("", false, new EmbedBuilder()
            .WithTitle(context.Guild.Name)
            .AddField("Created", context.Guild.CreatedAt.ToString("dddd, MMMM d, yyyy"))
            .AddField("Owner", context.Guild.Owner.Mention)
            .AddField("Emotes", context.Guild.Emotes.Count)
            .AddField("Members", context.Guild.MemberCount.ToString("#,##0"))
            .WithColor(Utilities.DomColorFromURL(context.Guild.IconUrl))
            .WithThumbnailUrl(context.Guild.IconUrl)
            .Build());

        // Display a User's local time
        public static async Task DisplayTime(SocketCommandContext context, SocketGuildUser user) => await context.Channel.SendMessageAsync("", false, Utilities.Embed("Time Teller", GetTime(user), timeColor, UserAccounts.GetAccount(user).country, ""));

        // Display a User's country
        public static async Task DisplayCountry(SocketCommandContext context, SocketGuildUser user)
        {
            string country = UserAccounts.GetAccount(user).country;
            string flagEmoji = GetFlag(country);
            await context.Channel.SendMessageAsync("", false, Utilities.Embed($"{user.Nickname ?? user.Username}'s Country", $"{flagEmoji} {country} {flagEmoji}", Utilities.DomColorFromURL(user.GetAvatarUrl()), "", user.GetAvatarUrl()));
        }

        // Get Discord flag emoji for a country
        private static string GetFlag(string country)
        {
			if (country == "United States")
				return ":flag_us:";
			else if (country == "Australia")
				return ":flag_au:";
			else if (country == "Sweden")
				return ":flag_se:";
			else if (country == "Spain")
				return ":flag_ea:";
			else if (country == "United Kingdom")
				return ":flag_gb:";
			else if (country == "France")
				return ":flag_fr:";
			else if (country == "Bosnia and Herzegovina")
				return ":flag_ba:";
			else if (country == "New Zealand")
				return ":flag_nz:";
			else if (country == "Philippines")
				return ":flag_ph:";
			else if (country == "Canada")
				return ":flag_ca:";
			else if (country == "China")
				return ":flag_cn:";
			else if (country == "Israel")
				return ":flag_il:";
			else if (country == "Indonesia")
				return ":flag_id:";
			else if (country == "Scotland")
				return "<:flag_scotland:518880178999525426>"; // Custom emoji, Discord doesn't seem to have a Scotland flag (unless I'm blind)
			else return "";
        }

        // Display Stats for a user
        public static async Task DisplayUserStats(SocketCommandContext context, SocketGuildUser user)
        {
            var embed = new EmbedBuilder()
                .WithTitle("Stats for " + user.ToString())
                .AddField("Created", GetCreatedDate(user))
                .AddField("Joined", GetJoinedDate(user))
                .AddField("Nickname", user.Nickname ?? "none");

            string roles = "";
            foreach (SocketRole r in user.Roles) roles += $"{r.Mention}, ";
            if (roles == "<@&333843634606702602>, ")
                roles = "none";
            else
            {
                roles = roles.Substring(23, roles.Length - 23); // Remove the @everyone role
                roles = roles.Substring(0, roles.Length - 2);
            }
            if(user.Roles.Count <= 2)
                embed.AddField("Role", roles);
            else
                embed.AddField("Roles", roles);

            var account = UserAccounts.GetAccount(user);
            embed.AddField("Coins", account.coins.ToString("#,##0"));
            embed.AddField("Country", $"{GetFlag(account.country)} {account.country}");
            embed.AddField("Level", account.level.ToString());
            embed.AddField("XP", account.xp.ToString("#,##0"));
            embed.AddField("Local Time", GetTime(user));
            embed.WithColor(Utilities.DomColorFromURL(user.GetAvatarUrl()));
            embed.WithThumbnailUrl(user.GetAvatarUrl());
            await context.Channel.SendMessageAsync("", false, embed.Build());
        }
    }
}