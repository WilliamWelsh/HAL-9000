using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Gideon.Handlers
{
    class WarnHandler
    {
        // Get the warning thread as a variable
        private ISocketMessageChannel warningThread(SocketCommandContext context) => (ISocketMessageChannel)context.Guild.GetChannel(345391332962861058);

        // Get our server owner, Teco, as a variable
        private SocketGuildUser Teco(SocketCommandContext context) => context.Guild.GetUser(279787337766928394);

        // Update warn information to user_data.json (adding)
        private void AddAndUpdateWarnDB(UserAccount account, string reason, string warner)
        {
            account.Warns++;
            account.Warners.Add(warner);
            account.warnReasons.Add(reason);
            UserAccounts.SaveAccounts();
        }

        // Update warn information to user_data.json (removing)
        private void RemoveAndUpdateWarnDB(UserAccount account, int number)
        {
            number--;
            account.Warns--;
            account.Warners.RemoveAt(number);
            account.warnReasons.RemoveAt(number);
            UserAccounts.SaveAccounts();
        }

        // Warn Embed Template
        private Embed warnEmbed(string title, string description, string ThumbnailURL)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle(title);
            embed.WithDescription(description);
            embed.WithColor(new Color(227, 37, 39));
            embed.WithThumbnailUrl(ThumbnailURL);
            return embed;
        }

        // Check if user needs to be kicked or banned
        private async Task CheckWarnCount(UserAccount account, SocketCommandContext context, SocketGuildUser offender)
        {
            switch(account.Warns)
            {
                case 3:
                    await KickAndNotify(context, offender, account, warningThread(context));
                    return;
                case 5:
                    await BanAndNotify(context, offender, account, warningThread(context));
                    return;
            }
        }

        // Print all warnings for a user
        public Embed AllWarnsEmbed(SocketGuildUser offender, UserAccount account)
        {
            var embed = new EmbedBuilder();

            embed.WithTitle($"{offender}'s Warnings");
            embed.WithColor(new Color(227, 37, 39));
            embed.AddField("Total Warns", account.Warns);
            embed.ThumbnailUrl = offender.GetAvatarUrl();

            for (int i = 0; i < account.warnReasons.Count; i++)
            {
                embed.AddField("Warning " + (i + 1), "Warned by " + account.Warners[i] + " for " + account.warnReasons[i]);
            }

            return embed;
        }

        // Remove all warnings for a user
        public async Task ClearWarns(SocketCommandContext context, SocketGuildUser user)
        {
            var account = UserAccounts.GetAccount(user);
            account.Warns = 0;
            account.Warners.Clear();
            account.warnReasons.Clear();
            UserAccounts.SaveAccounts();
            await context.Channel.SendMessageAsync("", false, warnEmbed("Warning", $"{context.User.Mention} cleared all of {user.Mention}'s warnings.", user.GetAvatarUrl()));
            await warningThread(context).SendMessageAsync("", false, warnEmbed("Warning", $"{context.User.Mention} cleared all of {user.Mention}'s warnings.", user.GetAvatarUrl()));
        }

        // Remove a warning for a user
        public async Task RemoveWarn(SocketCommandContext context, SocketGuildUser user, int number)
        {
            RemoveAndUpdateWarnDB(UserAccounts.GetAccount(user), number);
            await warningThread(context).SendMessageAsync("", false, warnEmbed("Warning Removal", $"Warning {number} has been removed from {user.Mention} by {context.User.Mention}.", user.GetAvatarUrl()));
            await context.Channel.SendMessageAsync("", false, warnEmbed("Warning Removal", $"Warning {number} has been removed from {user.Mention} by {context.User.Mention}.", user.GetAvatarUrl()));
        }

        // Used to automatically warn people
        public async Task AutoWarn(SocketCommandContext context, SocketGuildUser offender, string reason)
        {
            var warnThread = warningThread(context);

            var embed = warnEmbed("Warning", $"{offender.Mention} was warned by Gideon for {reason}.", offender.GetAvatarUrl());
            await warnThread.SendMessageAsync("", false, embed);
            await context.Channel.SendMessageAsync("", false, embed);

            await offender.SendMessageAsync("", false, warnEmbed("Warning", $"You were automatically warned for {reason}.\nPlease do not do this again.", offender.GetAvatarUrl()));

            var account = UserAccounts.GetAccount(offender);
            AddAndUpdateWarnDB(account, $"{offender.Mention} was warned by Gideon for {reason}.", "Gideon");
            await CheckWarnCount(account, context, offender);
        }

        // Warn people with the !warn command
        public async Task WarnUser(SocketCommandContext context, SocketGuildUser offender, string reason)
        {
            var account = UserAccounts.GetAccount(offender);
            AddAndUpdateWarnDB(account, reason, context.User.ToString());

            var embed = warnEmbed("Warning", $"{offender.Mention} was warned by {context.User.Mention} for {reason}.", offender.GetAvatarUrl());

            await context.Channel.SendMessageAsync("", false, embed);

            await warningThread(context).SendMessageAsync("", false, embed);

            await offender.SendMessageAsync("", false, warnEmbed("Warning", $"You were warned by {context.User.Mention} for {reason}.\n\nThis is warning {account.Warns}.\nThree warns is a kick, five is a ban.", context.User.GetAvatarUrl()));

            await CheckWarnCount(account, context, offender);
        }

        // Kick a user for reaching three warnings and notify Teco (our server owner)
        public async Task KickAndNotify(SocketCommandContext context, SocketGuildUser offender, UserAccount account, ISocketMessageChannel warningThread)
        {
            var embed = AllWarnsEmbed(offender, account);
            await offender.SendMessageAsync("", false, embed);
            await offender.SendMessageAsync("You were kicked for reaching 3 warnings. 2 more warnings (5), and you will be banned.\nPlease obey the rules.");
            await Teco(context).SendMessageAsync("", false, embed);
            await Teco(context).SendMessageAsync("[KICK] " + offender.ToString() + " has been kicked for reaching 3 warnings.");
            await warningThread.SendMessageAsync("[KICK] " + offender.ToString() + " has been kicked for reaching 3 warnings.");
            await context.Channel.SendMessageAsync("[KICK] " + offender.ToString() + " has been kicked for reaching 3 warnings.");
            await offender.KickAsync("You were kicked for reaching 3 warnings. 2 more warnings (5), and you will be banned. Please obey the rules.");
            return;
        }

        // Ban a user for reaching five warnings and notify Teco (our server owner)
        public async Task BanAndNotify(SocketCommandContext context, SocketGuildUser offender, UserAccount account, ISocketMessageChannel warningThread)
        {
            var embed = AllWarnsEmbed(offender, account);
            await offender.SendMessageAsync("", false, embed);
            await offender.SendMessageAsync("You have been banned from the Crisis on Earth One Discord. You can appeal your ban on the Tecoverse Discord.\nhttps://discord.gg/yD7Rxnu");
            await Teco(context).SendMessageAsync("", false, embed);
            await Teco(context).SendMessageAsync("[BAN] " + offender.ToString() + " has been banned for reaching 5 warnings.");
            await warningThread.SendMessageAsync("", false, embed);
            await warningThread.SendMessageAsync("[BAN] " + offender.ToString() + " has been banned for reaching 5 warnings.");
            await context.Channel.SendMessageAsync("[BAN] " + offender.ToString() + " has been banned for reaching 5 warnings.");
            await offender.Guild.AddBanAsync(offender, 0);
            return;
        }
    }
}