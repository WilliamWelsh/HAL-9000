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

        // Every minute, empty the list of users that got xp in the last minute
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

        private async Task CompareAndSet(SocketCommandContext context, SocketUser user, UserAccount account, uint level, uint targetXP)
        {
            if (account.xp >= targetXP && account.level < level)
            {
                account.level = level;
                UserAccounts.SaveAccounts();
                await Rankup(context, user, level);
            }
        }

        private async Task AddRole(SocketCommandContext context, SocketUser user, string oldRole, string newRole)
        {
            await (user as IGuildUser).AddRoleAsync(context.Guild.Roles.FirstOrDefault(x => x.Name == newRole));
            await (user as IGuildUser).RemoveRoleAsync(context.Guild.Roles.FirstOrDefault(x => x.Name == oldRole));
        }

        private async Task Rankup(SocketCommandContext context, SocketUser user, uint level)
        {
            string desc = $"{user.Mention} has leveled up to {level}.";
            if (level == 1)
            {
                desc += $" {user.Mention} is now a Noob.";
                await (user as IGuildUser).AddRoleAsync(context.Guild.Roles.FirstOrDefault(x => x.Name == "Noob"));
            }
            else if (level == 6)
            {
                desc += $" {user.Mention} is now a Symbiote.";
                await AddRole(context, user, "Noob", "Symbiote");
            }
            else if (level == 11)
            {
                desc += $" {user.Mention} is now a Speedster.";
                await AddRole(context, user, "Symbiote", "Speedster");
            }
            else if (level == 16)
            {
                desc += $" {user.Mention} is now a Kaiju Slayer.";
                await AddRole(context, user, "Speedster", "Kaiju Slayer");
            }
            else if (level == 21)
            {
                desc += $" {user.Mention} is now an Avenger.";
                await AddRole(context, user, "Kaiju Slayer", "Avenger");
            }
            await context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("Level Up", desc, Config.Utilities.DomColorFromURL(user.GetAvatarUrl()), "", user.GetAvatarUrl()));
        }

        // I stole the MEE6 bot's XP amounts for their level system because it's pretty good
        // https://mee6.github.io/Mee6-documentation/levelxp/
        uint[] xpLevel = { 0, 100, 255, 475, 770, 1150, 1625, 2205, 2900, 3720, 4675, 5775, 7030, 8450, 10045, 11825, 13800, 15900, 18375, 20995, 23850,
        26950, 30305, 33925, 37820, 42000, 46475, 51255, 56350, 61770, 67525};

        public async Task CheckXP(SocketCommandContext context, SocketUser user)
        {
            UserAccount account = UserAccounts.GetAccount(user);
            for (uint i = 0; i < xpLevel.Length; i++)
                await CompareAndSet(context, user, account, i, xpLevel[i]);
        }

        public async Task DisplayLevelAndXP(SocketCommandContext context, SocketUser user)
        {
            UserAccount account = UserAccounts.GetAccount(user);
            string name = (user as SocketGuildUser).Nickname ?? user.Username;
            string desc = $"Rank: {LevelToRank(account.level)}\n\nLevel: {account.level}\n\nTotal XP: {account.xp.ToString("#,##0")}\n\nXP until next level: {(account.xp- xpLevel[account.level]).ToString("#,##0")}/{(xpLevel[account.level + 1]-xpLevel[account.level]).ToString("#,##0")}";
            await context.Channel.SendMessageAsync("", false, Config.Utilities.Embed($"{name}'s Level", desc, context.Guild.Roles.FirstOrDefault(x => x.Name == LevelToRank(account.level)).Color, "", user.GetAvatarUrl()));
        }

        public string LevelToRank(uint level)
        {
            if (level <= 5)
                return "Noob";
            else if (level > 5 && level <= 10)
                return "Symbiote";
            else if (level > 10 && level <= 15)
                return "Speedster";
            else if (level > 15 && level <= 20)
                return "Kaiju Slayer";
            else
                return "Avenger";
        }
    }
}