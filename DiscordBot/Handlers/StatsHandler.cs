using System;
using Discord;
using System.Text;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Gideon.Handlers
{
    static class StatsHandler
    {
        public static string GetCreatedDate(IUser user) => user.CreatedAt.ToString("MMMM dd, yyy");
        public static string GetJoinedDate(SocketGuildUser user) => ((DateTimeOffset)user.JoinedAt).ToString("MMMM dd, yyy");

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
        
        // Display Stats for a user
        public static async Task DisplayUserStats(SocketCommandContext context, SocketGuildUser user)
        {
            var embed = new EmbedBuilder()
                .WithTitle("Stats for " + user.ToString())
                .AddField("Created", GetCreatedDate(user))
                .AddField("Joined", GetJoinedDate(user))
                .AddField("Nickname", user.Nickname ?? "none");

            StringBuilder roles = new StringBuilder();
            foreach (var role in user.Roles)
                roles.Append($", {role.Mention}");

            // If the user's role description is just "@everyone, " then they have no role, otherwise replace @everyone because that's not a role
            string r = roles.ToString();
            embed.AddField($"{(user.Roles.Count <= 2 ? "Role" : "Roles")}", r == ", @everyone" ? "none" : r.Substring(2, r.Length - 2).Replace("@everyone, ", ""));

            var account = UserAccounts.GetAccount(user);
            embed.AddField("Coins", account.coins.ToString("#,##0"))
                .AddField("Level", account.level.ToString())
                .AddField("XP", account.xp.ToString("#,##0"))
                .WithColor(Utilities.DomColorFromURL(user.GetAvatarUrl()))
                .WithThumbnailUrl(user.GetAvatarUrl());
            await context.Channel.SendMessageAsync(null, false, embed.Build());
        }
    }
}