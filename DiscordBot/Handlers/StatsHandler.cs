using System;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Gideon.Handlers
{
    class StatsHandler
    {
        public string GetCreatedDate(SocketGuildUser user)
        {
            string date = "";
            try
            {
                date = user.CreatedAt.ToString("MMMM dd, yyy");
            }
            catch (Exception)
            {
                date = "Error finding date.";
            }
            return date;
        }

        public string GetJoinedDate(SocketGuildUser user)
        {
            string date = "";
            try
            {
                date = ((DateTimeOffset)user.JoinedAt).ToString("MMMM dd, yyy");
            }
            catch (Exception)
            {
                date = "Error finding date.";
            }
            return date;
        }

        // Print server stats
        public async Task DisplayServerStats(SocketCommandContext context)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle(context.Guild.Name);
            embed.AddField("Created", context.Guild.CreatedAt.ToString("dddd, MMMM d, yyyy"));
            embed.AddField("Owner", context.Guild.Owner.Mention);
            embed.AddField("Emotes", context.Guild.Emotes.Count);
            embed.AddField("Members", context.Guild.MemberCount.ToString("#,##0"));
            embed.WithColor(Config.Utilities.DomColorFromURL(context.Guild.IconUrl));
            embed.WithThumbnailUrl(context.Guild.IconUrl);

            await context.Channel.SendMessageAsync("", false, embed);
        }

        // Display a User's local time
        public async Task DisplayTime(SocketCommandContext context, SocketGuildUser user)
        {
            var account = UserAccounts.GetAccount(user);
            string text = "";
            string name = user.Nickname ?? user.Username;
            switch (account.localTime)
            {
                case 999:
                    text = $"Not set.\nContact {context.Guild.GetUser(354458973572956160).Mention} to set it up.";
                    break;
                default:
                    DateTime t = DateTime.Now.AddHours(account.localTime);
                    text = $"It's {t.ToString("h:mm tt")} for {name}.\n{t.ToString("dddd, MMMM d.")}";
                    break;
            }
            string footer = UserAccounts.GetAccount(user).country != "Not set." ? UserAccounts.GetAccount(user).country : "No country set.";
            await context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("Time Teller", text, new Color(127, 166, 208), footer, ""));
        }

        // Display a User's country
        public async Task DisplayCountry(SocketCommandContext context, SocketGuildUser user)
        {
            string name = user.Nickname ?? user.Username;
            string country = UserAccounts.GetAccount(user).country;
            string flagEmoji = GetFlag(country);
            await context.Channel.SendMessageAsync("", false, Config.Utilities.Embed($"{name}'s Country", $"{flagEmoji} {country} {flagEmoji}", Config.Utilities.DomColorFromURL(user.GetAvatarUrl()), "", user.GetAvatarUrl()));
        }

        // Get Discord flag emoji for a country
        private string GetFlag(string country)
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
				return "<:flag_scotland:518880178999525426>";
			else return "";
        }

        // Display Stats for a user
        public async Task DisplayUserStats(SocketCommandContext context, SocketGuildUser user)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle("Stats for " + user.ToString());
            embed.AddField("Created", GetCreatedDate(user));
            embed.AddField("Joined", GetJoinedDate(user));

            string nick = user.Nickname ?? "none";
            embed.AddField("Nickname", nick);
            string roles = "";
            foreach (SocketRole r in user.Roles) roles += r.Mention + ", ";
            if (roles == "<@&333843634606702602>, ")
                roles = "none";
            else
            {
                roles = roles.Substring(23, roles.Length - 23);
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

            switch (account.localTime)
            {
                case 999:
                    embed.AddField("Local Time", "Not set.");
                    break;
                default:
                    DateTime t = DateTime.Now.AddHours(account.localTime);
                    embed.AddField("Local Time", $"{t.ToString("h:mm tt, dddd, MMMM d")}");
                    break;
            }

            embed.WithColor(Config.Utilities.DomColorFromURL(user.GetAvatarUrl()));
            embed.WithThumbnailUrl(user.GetAvatarUrl());
            await context.Channel.SendMessageAsync("", false, embed);
        }
    }
}