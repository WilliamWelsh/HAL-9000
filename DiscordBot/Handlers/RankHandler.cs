using Discord;
using System.Text;
using System.Linq;
using System.Timers;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Gideon.Handlers
{
    static class RankHandler
    {
        // A list of user IDs that have received xp within the last minute
        private static List<ulong> UsersGivenXPInLastMinute = new List<ulong>();

        // Set up the timer
        public static void Start()
        {
            Timer timer = new Timer
            {
                Interval = 6000, // 1 minute (6000 milliseconds)
                AutoReset = true,
                Enabled = true,
            };
            timer.Elapsed += Reset;
        }

        // Every minute, empty the list of users that got xp in the last minute
        private static void Reset(object sender, ElapsedEventArgs e) => UsersGivenXPInLastMinute.Clear();

        // Give a user XP when they talk (15-25 xp once a minute) and then check if they can level up
        public static async Task TryToGiveUserXP(SocketCommandContext context, SocketUser user)
        {
            if (UsersGivenXPInLastMinute.Contains(user.Id)) return;
            GiveUserXP(user, Utilities.GetRandomNumber(15, 25));
            UsersGivenXPInLastMinute.Add(user.Id);
            await CheckXP(context, user).ConfigureAwait(false);
        }

        // Give a user XP
        public static void GiveUserXP(SocketUser user, int xp)
        {
            UserAccounts.GetAccount(user).xp += xp;
            UserAccounts.SaveAccounts();
        }

        // Try to level up a user
        private static async Task CompareAndSet(SocketCommandContext context, SocketUser user, UserAccount account, uint level, uint targetXP)
        {
            if (account.xp >= targetXP && account.level < level)
            {
                account.level = level;
                UserAccounts.SaveAccounts();
                await Rankup(context, user, level).ConfigureAwait(false);
            }
        }

        // Give a user a role, and take away their old role
        private static async Task AddRole(SocketCommandContext context, SocketUser user, string oldRole, string newRole)
        {
            await (user as IGuildUser).AddRoleAsync(context.Guild.Roles.FirstOrDefault(x => x.Name == newRole));
            await (user as IGuildUser).RemoveRoleAsync(context.Guild.Roles.FirstOrDefault(x => x.Name == oldRole));
        }

        // Rank up a user
        private static async Task Rankup(SocketCommandContext context, SocketUser user, uint level)
        {
            StringBuilder description = new StringBuilder().Append($"{user.Mention} has leveled up to {level}.");
            if (level == 1)
            {
                description.Append($" {user.Mention} is now a Noob.");
                await (user as IGuildUser).AddRoleAsync(context.Guild.Roles.FirstOrDefault(x => x.Name == "Noob"));
            }
            else if (level == 6)
            {
                description.Append($" {user.Mention} is now a Symbiote.");
                await AddRole(context, user, "Noob", "Symbiote").ConfigureAwait(false);
            }
            else if (level == 11)
            {
                description.Append($" {user.Mention} is now a Speedster.");
                await AddRole(context, user, "Symbiote", "Speedster").ConfigureAwait(false);
            }
            else if (level == 16)
            {
                description.Append($" {user.Mention} is now a Kaiju Slayer.");
                await AddRole(context, user, "Speedster", "Kaiju Slayer").ConfigureAwait(false);
            }
            else if (level == 21)
            {
                description.Append($" {user.Mention} is now part of the Watchmen.");
                await AddRole(context, user, "Kaiju Slayer", "Watchmen").ConfigureAwait(false);
            }
            else if (level == 26)
            {
                description.Append($" {user.Mention} is now an Avenger.");
                await AddRole(context, user, "Watchmen", "Avenger").ConfigureAwait(false);
            }
            await Utilities.SendEmbed(context.Channel, "Level Up", description.ToString(), Utilities.DomColorFromURL(user.GetAvatarUrl()), "", user.GetAvatarUrl());
        }

        // I stole the MEE6 bot's XP amounts for their level system because it's pretty good
        // https://mee6.github.io/Mee6-documentation/levelxp/
        static uint[] xpLevel = { 0, 100, 255, 475, 770, 1150, 1625, 2205, 2900, 3720, 4675, 5775, 7030, 8450, 10045, 11825, 13800, 15900, 18375, 20995, 23850,
        26950, 30305, 33925, 37820, 42000, 46475, 51255, 56350, 61770, 67525, 73625, 80080, 86900, 94095, 101675, 109650, 118030, 126825, 136045, 145700};

        // Check how much xp a user has
        public static async Task CheckXP(SocketCommandContext context, SocketUser user)
        {
            UserAccount account = UserAccounts.GetAccount(user);
            for (uint i = 0; i < xpLevel.Length; i++)
                await CompareAndSet(context, user, account, i, xpLevel[i]).ConfigureAwait(false);
        }

        // Show a user's level, xp, and how much xp they need to go to the next level
        public static async Task DisplayLevelAndXP(SocketCommandContext context, SocketUser user)
        {
            UserAccount account = UserAccounts.GetAccount(user);
            StringBuilder description = new StringBuilder()
                .AppendLine($"Rank: {LevelToRank(account.level)}").AppendLine()
                .AppendLine($"Level: {account.level}").AppendLine()
                .AppendLine($"Total XP: {account.xp.ToString("#,##0")}").AppendLine()
                .AppendLine($"XP until next level: {(account.xp - xpLevel[account.level]).ToString("#,##0")}/{(xpLevel[account.level + 1] - xpLevel[account.level]).ToString("#,##0")}").AppendLine();
            Color rankColor = context.Guild.Roles.FirstOrDefault(x => x.Name == LevelToRank(account.level)).Color;
            await Utilities.SendEmbed(context.Channel, $"{(user as SocketGuildUser).Nickname ?? user.Username}'s Level", description.ToString(), rankColor, "", user.GetAvatarUrl());
        }

        // Convert a level to a rank name
        public static string LevelToRank(uint level)
        {
            if (level <= 5)
                return "Noob";
            else if (level > 5 && level <= 10)
                return "Symbiote";
            else if (level > 10 && level <= 15)
                return "Speedster";
            else if (level > 15 && level <= 20)
                return "Kaiju Slayer";
            else if (level > 20 && level <= 25)
                return "Watchmen";
            else
                return "Avenger";
        }

        /*
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
        */
    }
}