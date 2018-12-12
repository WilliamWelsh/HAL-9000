using Discord;
using System.Linq;
using System.Timers;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Gideon.Handlers
{
    class RankHandler
    {
        private List<ulong> UsersGivenXPInLastMinute;

        public void Start()
        {
            UsersGivenXPInLastMinute = new List<ulong>();
            Timer timer = new Timer()
            {
                Interval = (1000 * 60),
                AutoReset = true,
                Enabled = true,
            };
            timer.Elapsed += Reset;
        }

        // Empty the list of users that got xp in the last minute
        private void Reset(object sender, ElapsedEventArgs e) => UsersGivenXPInLastMinute = new List<ulong>();

        // Give a user XP when they talk (15-25 xp once a minute) and then check if they can level up
        public async Task TryToGiveUserXP(SocketCommandContext context, SocketUser user)
        {
            if (UsersGivenXPInLastMinute.Contains(user.Id)) return;
            GiveUserXP(user, Config.Utilities.GetRandomNumber(15, 25));
            UsersGivenXPInLastMinute.Add(user.Id);
            await CheckXP(context, user);
        }

        public void GiveUserXP(SocketUser user, int xp)
        {
            UserAccounts.GetAccount(user).xp += xp;
            UserAccounts.SaveAccounts();
        }

        private async Task CompareAndSet(SocketCommandContext context, UserAccount account, uint level, uint targetXP)
        {
            if (account.xp >= targetXP && account.level < level)
            {
                account.level = level;
                UserAccounts.SaveAccounts();
                await Rankup(context, level);
            }
        }

        private async Task AddRole(SocketCommandContext context, string oldRole, string newRole)
        {
            await (context.User as IGuildUser).AddRoleAsync(context.Guild.Roles.FirstOrDefault(x => x.Name == newRole));
            await (context.User as IGuildUser).RemoveRoleAsync(context.Guild.Roles.FirstOrDefault(x => x.Name == oldRole));
        }

        private async Task Rankup(SocketCommandContext context, uint level)
        {
            string desc = $"{context.User.Mention} has leveled up to {level}.";
            if (level == 1)
            {
                desc += $" {context.User.Mention} is now a Noob.";
                await (context.User as IGuildUser).AddRoleAsync(context.Guild.Roles.FirstOrDefault(x => x.Name == "Noob"));
            }
            else if (level == 6)
            {
                desc += $" {context.User.Mention} is now a Speedster.";
                await AddRole(context, "Noob", "Speedster");
            }
            else if (level == 11)
            {
                desc += $" {context.User.Mention} is now a Kaiju Slayer.";
                await AddRole(context, "Speedster", "Kaiju Slayer");
            }
            else if (level == 16)
            {
                desc += $" {context.User.Mention} is now a Avenger.";
                await AddRole(context, "Kaiju Slayer", "Avenger");
            }
            await context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("Level Up", desc, Config.Utilities.DomColorFromURL(context.User.GetAvatarUrl()), "", context.User.GetAvatarUrl()));
        }

        uint[] xpLevel = { 0, 100, 255, 475, 770, 1150, 1625, 2205, 2900, 3720, 4675, 5775, 7030, 8450, 10045, 11825, 13800, 15900, 18375, 20995, 23850 };

        private async Task CheckXP(SocketCommandContext context, SocketUser user)
        {
            UserAccount account = UserAccounts.GetAccount(user);
            for (uint i = 0; i < xpLevel.Length; i++)
                await CompareAndSet(context, account, i, xpLevel[i]);
        }

        public async Task DisplayLevelAndXP(SocketCommandContext context, SocketUser user)
        {
            UserAccount account = UserAccounts.GetAccount(user);
            string name = (user as SocketGuildUser).Nickname != null ? (user as SocketGuildUser).Nickname : user.Username;
            string desc = $"Level: {account.level}\nTotal XP: {account.xp}\nXP until next level: {account.xp}/{xpLevel[account.level + 1]}";
            await context.Channel.SendMessageAsync("", false, Config.Utilities.Embed($"{name}'s Level", desc, Config.Utilities.DomColorFromURL(user.GetAvatarUrl()), "", user.GetAvatarUrl()));
        }
    }
}