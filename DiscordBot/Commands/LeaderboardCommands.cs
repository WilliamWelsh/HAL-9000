using System;
using System.Linq;
using System.Text;
using Discord.Commands;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Gideon.Handlers
{
    [RequireContext(ContextType.Guild)]
    public class LeaderboardCommands : ModuleBase<SocketCommandContext>
    {
        // Leaderboard of: Who has the most coins
        [Command("lb coins")]
        public async Task CoinsLBShortcut() => await CoinsHandler.PrintCoinsLeaderboard(Context);

        // Leaderboard of: Who joined the earliest
        [Command("lb joined")]
        public async Task JoinedLB()
        {
            await Utilities.SendEmbed(Context.Channel, "Top 10 People Who Joined The Earliest", First10UsersByJoinDate(MakeListAndOrderIt("joined")), Colors.Blue, "", "");
        }

        // Leaderboard of: Who has the most coins
        [Command("lb new")]
        public async Task NewAccountLB()
        {
            List<DateTime> dates = MakeListAndOrderIt("created");
            dates.Reverse();
            await Utilities.SendEmbed(Context.Channel, "Top 10 Newest Accounts", First10UsersByCreationDate(dates), Colors.Blue, "", "");
        }

        // Leaderboard of: Who has the oldest account
        [Command("lb created")]
        [Alias("lb old")]
        public async Task CreatedLB()
        {
            await Utilities.SendEmbed(Context.Channel, "Top 10 People With The Oldest Accounts", First10UsersByCreationDate(MakeListAndOrderIt("created")), Colors.Blue, "", "");
        }

        // Make a string of the first 10 users in a list, compared by date
        // Format: 1. UserName, Date
        private string First10UsersByJoinDate(List<DateTime> list)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < 10; i++)
                foreach (var user in Context.Guild.Users)
                    if (list.ElementAt(i) == ((DateTimeOffset)user.JoinedAt).DateTime)
                        result.AppendLine($"`{i + 1}.` **{user.Username}**, `{list.ElementAt(i).ToString("MMMM dd, yyy")}`");
            return result.ToString();
        }

        // Same as the function above, but compares to the created date
        private string First10UsersByCreationDate(List<DateTime> list)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < 10; i++)
                foreach (var user in Context.Guild.Users)
                    if (list.ElementAt(i) == user.CreatedAt.DateTime)
                        result.AppendLine($"`{i + 1}.` **{user.Username}**, `{list.ElementAt(i).ToString("MMMM dd, yyy")}`");
            return result.ToString();
        }

        // Makes a list of times and sorts them by earliest to latest
        private List<DateTime> MakeListAndOrderIt(string whatToAdd)
        {
            List<DateTime> dates = new List<DateTime>();
            foreach (var user in Context.Guild.Users)
            {
                if (whatToAdd == "joined")
                    dates.Add(((DateTimeOffset)user.JoinedAt).DateTime);
                else
                    dates.Add(user.CreatedAt.DateTime);
            }
            dates = dates.OrderByDescending(x => x.Date).ToList();
            dates.Reverse();
            return dates;
        }

        // Leaderboard of: Who has the most coins
        [Command("lb level")]
        public async Task LevelLB()
        {
            List<uint> levels = new List<uint>();
            foreach (var user in Context.Guild.Users)
                levels.Add(UserAccounts.GetAccount(user).level);
            levels.Sort();
            levels.Reverse();
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < 10; i++)
                foreach (var user in Context.Guild.Users)
                    if (levels.ElementAt(i) == UserAccounts.GetAccount(user).level && !result.ToString().Contains(user.Username))
                    {
                        result.AppendLine($"`{i + 1}.` **{user.Username}**, `Level {levels.ElementAt(i)}`");
                        break;
                    }
            await Utilities.SendEmbed(Context.Channel, "Top 10 Users With The Highest Level", result.ToString(), Colors.Blue, "", "");
        }

        // View Leaderboards
        [Command("lb")]
        public async Task Leaderboards()
        {
            StringBuilder description = new StringBuilder();
            description.AppendLine("`!lb coins` People with the most coins.").AppendLine();
            description.AppendLine("`!lb joined` First people that joined the server.").AppendLine();
            description.AppendLine("`!lb created` or `!lb old` People with the oldest accounts.").AppendLine();
            description.AppendLine("`!lb level` People with the highest level").AppendLine();
            await Utilities.SendEmbed(Context.Channel, "Leaderboards", description.ToString(), Colors.Green, "", "");
        }
    }
}