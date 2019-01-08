using System;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Gideon.Handlers
{
    static class StatsHandler
    {
        public static string GetCreatedDate(IUser user) => user.CreatedAt.ToString("MMMM dd, yyy");
        public static string GetJoinedDate(SocketGuildUser user) => ((DateTimeOffset)user.JoinedAt).ToString("MMMM dd, yyy");

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

        // Display a User's country
        public static async Task DisplayCountry(SocketCommandContext context, SocketGuildUser user)
        {
            string country = UserAccounts.GetAccount(user).country;
            string flagEmoji = GetFlag(country);
            await Utilities.SendEmbed(context.Channel, $"{user.Nickname ?? user.Username}'s Country", $"{flagEmoji} {country} {flagEmoji}", Utilities.DomColorFromURL(user.GetAvatarUrl()), "", user.GetAvatarUrl());
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
            else if (country == "Czech Republic")
                return ":flag_cz:";
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

            string roles = string.Join(", ", user.Roles);

            // If the user's role description is just "@everyone, " then they have no role, otherwise replace @everyone because that's not a role
            embed.AddField($"{(user.Roles.Count <= 2 ? "Role" : "Roles")}", roles == "@everyone, " ? "none" : roles.Substring(0, roles.Length - 2).Replace("@everyone, ", ""));

            var account = UserAccounts.GetAccount(user);
            embed.AddField("Coins", account.coins.ToString("#,##0"))
                .AddField("Country", $"{GetFlag(account.country)} {account.country}")
                .AddField("Level", account.level.ToString())
                .AddField("XP", account.xp.ToString("#,##0"))
                .AddField("Local Time", GetTime(user))
                .WithColor(Utilities.DomColorFromURL(user.GetAvatarUrl()))
                .WithThumbnailUrl(user.GetAvatarUrl());
            await context.Channel.SendMessageAsync(null, false, embed.Build());
        }
    }
}