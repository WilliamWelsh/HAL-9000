using System;
using Discord;
using System.Linq;
using Discord.Commands;
using Gideon.Minigames;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Gideon.Handlers
{
    [RequireContext(ContextType.Guild)]
    public class Misc : ModuleBase<SocketCommandContext>
    {
        private string ToAussie(string s)
        {
            char[] X = @"¿/˙'\‾¡zʎxʍʌnʇsɹbdouɯlʞɾıɥƃɟǝpɔqɐ".ToCharArray();
            string V = @"?\.,/_!zyxwvutsrqponmlkjihgfedcba";
            return new string((from char obj in s.ToCharArray()
                               select (V.IndexOf(obj) != -1) ? X[V.IndexOf(obj)] : obj).Reverse().ToArray());
        }

        TecosHandler TH = Config.TH;
        Trivia Trivia = Config.MinigameHandler.Trivia;
        NumberGuess NG = Config.MinigameHandler.NG;
        TicTacToe TTT = Config.MinigameHandler.TTT;
        RussianRoulette RR = Config.MinigameHandler.RR;

        private static readonly Random getrandom = new Random();

        public static int GetRandomNumber(int min, int max)
        {
            lock (getrandom) { return getrandom.Next(min, max); }
        }

        private string FindPeopleWithRoles(string role)
        {
            var rolea = Context.Guild.Roles.FirstOrDefault(x => x.Name == role);
            string desc = "";
            foreach (SocketGuildUser user in Context.Guild.Users.ToArray())
            {
                if (user.Roles.Contains(rolea))
                {
                    desc += user.Mention + "\n";
                }
            }
            return desc;
        }

        Embed Embed(string t, string d, Color c, string f, string thURL)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle(t);
            embed.WithDescription(d);
            embed.WithColor(c);
            embed.WithFooter(f);
            embed.WithThumbnailUrl(thURL);
            return embed;
        }
        
        [Command("addallowedchannel")]
        public async Task AddAllowedChannel([Remainder]string channel)
        {
            if (Context.User.ToString() != "admin#0001")
                return;
            Config.ModifyChannelWhitelist(channel, true);
            await Context.Channel.SendMessageAsync($"#{channel} has been added to the whitelist.");
        }

        [Command("removeallowedchannel")]
        public async Task RemoveAllowedChannel([Remainder]string channel)
        {
            if (Context.User.ToString() != "admin#0001")
                return;
            Config.ModifyChannelWhitelist(channel, false);
            await Context.Channel.SendMessageAsync($"#{channel} has been removed to the whitelist.");
        }

        [Command("australia")]
        public async Task AussieText([Remainder]string message) => await Context.Channel.SendMessageAsync("", false, Embed("Australian Translator", ToAussie(message), new Color(255, 140, 0), "Requested by " + Context.User, ""));

        // Display a list of MiniGames
        [Command("games")]
        public async Task DisplayGames() => await Config.MinigameHandler.DisplayGames(Context.Channel);

        #region Tic-Tac-Toe Commands
        // Tic-Tac-Toe Menu/Start Game
        [Command("ttt")]
        public async Task TTTMenu(string input) => await TTT.TryToStartGame(Context, input);

        // Join Tic-Tac-Toe
        [Command("join ttt")]
        public async Task JoinTTT() => await TTT.TryToJoinGame(Context);

        // Write X or O in Tic-Tac-Toe
        [Command("put")]
        public async Task PutTTTLetter(string letter) => await TTT.PutLetter(Context, letter);
        #endregion

        #region Russian Roulette Commands
        // RR Menu
        [Command("rr")]
        public async Task RRMenu() => await RR.TryToStartGame(Context, "");

        // Try to start a game of RR
        [Command("rr")]
        public async Task RRTryToStart(string input) => await RR.TryToStartGame(Context, input);

        // Join RR
        [Command("join rr")]
        public async Task RRJoin() => await RR.TryToJoin(Context);

        // Bet Tecos on an RR Game
        [Command("rr bet")]
        public async Task RussianRouletteBet(SocketGuildUser UserBeingBetOn, [Remainder]int amount) => await RR.TryToPlaceBet(UserBeingBetOn, Context, amount);

        // Pull the trigger in RR
        [Command("pt")]
        public async Task RRPullTrigger() => await RR.PullTrigger(Context);
        #endregion

        #region Trivia Commands
        // Trivia Menu
        [Command("trivia")]
        public async Task TroToStartTrivia() => await Trivia.TryToStartTrivia((SocketGuildUser)Context.User, Context, "trivia");

        // Start Trivia
        [Command("trivia")]
        public async Task TroToStartTrivia(string input) => await Trivia.TryToStartTrivia((SocketGuildUser)Context.User, Context, input);
        #endregion

        #region Number Guess Game Commands
        // Play NG (Solo)
        [Command("play ng")]
        public async Task PlayNG() => await PlayNG(0);

        // Play NG (2+ players)
        [Command("play ng")]
        public async Task PlayNG(int input) => await NG.TryToStartGame(GetRandomNumber(1, 100), (SocketGuildUser)Context.User, Context, input);

        // Join NG
        [Command("join ng")]
        public async Task JoinNG() => await NG.JoinGame((SocketGuildUser)Context.User, Context);

        // Guess the number in NG
        [Command("g")]
        public async Task GuessNG(int input) => await NG.TryToGuess((SocketGuildUser)Context.User, Context, input);
        #endregion

        #region Thanos Related Commands
        [Command("thanos")]
        public async Task Thanos([Remainder] SocketGuildUser user)
        {
            var account = UserAccounts.GetAccount(user);
            string u = user.Nickname != null ? user.Nickname : user.Username;
            if (account.hasDoneThanosCommand)
            {
                switch (account.isKilledByThanos)
                {
                    case true:
                        account.isKilledByThanos = true;
                        await Context.Channel.SendMessageAsync("", false, Embed("Thanos' Mercy", $"{u} was slain by Thanos, for the good of the Universe.", new Color(120, 102, 140), "", "https://cdn.discordapp.com/attachments/339887750683688965/441112531788890114/image.jpg"));
                        return;
                    case false:
                        account.isKilledByThanos = false;
                        await Context.Channel.SendMessageAsync("", false, Embed("Thanos' Mercy", $"{user} was spared by Thanos.", new Color(120, 102, 140), "", "https://cdn.discordapp.com/attachments/339887750683688965/441112531788890114/image.jpg"));
                        return;
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync("", false, Embed("Thanos' Mercy", $"{u} has not yet done the `!thanos` command.", new Color(120, 102, 140), "", "https://cdn.discordapp.com/attachments/339887750683688965/441112531788890114/image.jpg"));
            }
        }

        [Command("thanoslist")]
        public async Task ThanosList()
        {
            string dead = "";
            foreach(SocketGuildUser user in Context.Guild.Users)
            {
                var account = UserAccounts.GetAccount(user);
                if (account.isKilledByThanos)
                    dead += user.ToString() + "\n";
            }
            await Context.Channel.SendMessageAsync("", false, Embed("People Killed", dead, new Color(120, 102, 140), "", "https://cdn.discordapp.com/attachments/339887750683688965/441112531788890114/image.jpg"));
        }

        [Command("thanos")]
        public async Task Thanos()
        {
            var account = UserAccounts.GetAccount(Context.User as SocketGuildUser);
            string user = (Context.User as SocketGuildUser).Nickname != null ? (Context.User as SocketGuildUser).Nickname : Context.User.Username;
            if (!account.hasDoneThanosCommand)
            {
                account.hasDoneThanosCommand = true;

                int i = GetRandomNumber(0, 100);
                if (i <= 50)
                {
                    account.isKilledByThanos = true;
                }
                else
                {
                    account.isKilledByThanos = false;
                }
            }
            UserAccounts.SaveAccounts();
            switch (account.isKilledByThanos)
            {
                case true:
                    account.isKilledByThanos = true;
                    await Context.Channel.SendMessageAsync("", false, Embed("Thanos' Mercy", $"{user} was slain by Thanos, for the good of the Universe.", new Color(120, 102, 140), "", "https://cdn.discordapp.com/attachments/339887750683688965/441112531788890114/image.jpg"));
                    return;
                case false:
                    account.isKilledByThanos = false;
                    await Context.Channel.SendMessageAsync("", false, Embed("Thanos' Mercy", $"{user} was spared by Thanos.", new Color(120, 102, 140), "", "https://cdn.discordapp.com/attachments/339887750683688965/441112531788890114/image.jpg"));
                    return;
            }
        }
        #endregion

        [Command("dev")]
        public async Task MakeDev([Remainder] SocketGuildUser user)
        {
            if (Context.User.ToString() != "admin#0001")
                return;
            var rolea = Context.Guild.Roles.FirstOrDefault(x => x.Name == "Developer");
            await user.AddRoleAsync(rolea);
        }

        [Command("undev")]
        public async Task UnDev([Remainder] SocketGuildUser user)
        {
            if (Context.User.ToString() != "admin#0001")
                return;
            var rolea = Context.Guild.Roles.FirstOrDefault(x => x.Name == "Developer");
            await user.RemoveRoleAsync(rolea);
        }

        [Command("say")]
        public async Task say([Remainder]string message)
        {
            if (!(Context.User.ToString() == "admin#0001"))
                return;

            var messages = await Context.Channel.GetMessagesAsync(1).Flatten();
            await Context.Channel.DeleteMessagesAsync(messages);
            await Context.Channel.SendMessageAsync(message);
        }

        [Command("mock")]
        public async Task Mock([Remainder]string message)
        {
            if (Config.bot.allowedChannels.Contains(Context.Channel.Name))
            {
                var messages = await Context.Channel.GetMessagesAsync(1).Flatten();
                await Context.Channel.DeleteMessagesAsync(messages);

                char[] letters = message.ToCharArray();
                for (int n = 0; n < letters.Length; n += 2)
                {
                    letters[n] = char.ToUpper(letters[n]);
                }
                string new_s = new string(letters);

                var embed = new EmbedBuilder();
                embed.WithDescription(new_s);
                embed.WithColor(new Color(255, 255, 0));
                embed.WithThumbnailUrl("http://i0.kym-cdn.com/photos/images/masonry/001/255/479/85b.png");
                embed.WithFooter("Mocked by " + Context.User);

                await Context.Channel.SendMessageAsync("", false, embed);
                return;
            }
            
            await Context.Channel.SendMessageAsync("You can only do this command in #off-topic.");
        }

        [Command("audition")]
        public async Task Audition() => await Context.Channel.SendMessageAsync("Audition here: http://www.behindthevoiceactors.com/members/Tecosaurus/casting-call/Crisis-on-Earth-One-An-Arrowverse-Fan-Game/");

        [Command("someone")]
        public async Task GetRandomPerson()
        {
            var users = Context.Guild.Users;
            SocketGuildUser[] s = users.ToArray();
            string p = s[GetRandomNumber(0, s.Length)].ToString();
            await Context.Channel.SendMessageAsync(p);
        }

        [Command("onlinestaff")]
        public async Task OnlineStaff()
        {
            var roleHelper = Context.Guild.Roles.FirstOrDefault(x => x.Name == "Helpers");
            var roleDev = Context.Guild.Roles.FirstOrDefault(x => x.Name == "Developer");
            var roleLead = Context.Guild.Roles.FirstOrDefault(x => x.Name == "Lead");
            var roleDir = Context.Guild.Roles.FirstOrDefault(x => x.Name == "Director");
            string onlineHelpers = "";
            string onlineDevs = "";
            string onlineLeads = "";
            string onlineDir = "";

            foreach (SocketGuildUser user in Context.Guild.Users.ToArray())
            {
                if (user.Roles.Contains(roleHelper) && user.Status.ToString() == "Online") onlineHelpers += user.ToString() + "\n";
                if (user.Roles.Contains(roleDev) && user.Status.ToString() == "Online") onlineDevs += user.ToString() + "\n";
                if (user.Roles.Contains(roleLead) && user.Status.ToString() == "Online") onlineLeads += user.ToString() + "\n";
                if (user.Roles.Contains(roleDir) && user.Status.ToString() == "Online") onlineDir = "Tecosaurus";
            }

            var embed = new EmbedBuilder();
            embed.WithTitle("Online Staff Members");
            if(onlineDir != "")
                embed.AddField("--- Director ---", onlineDir);
            if (onlineLeads != "")
                embed.AddField("--- Leads ---", onlineLeads);
            if (onlineDevs != "")
                embed.AddField("--- Developers ---", onlineDevs);
            if (onlineHelpers != "")
                embed.AddField("--- Helpers ---", onlineHelpers);
            if (onlineDir == "" && onlineLeads == "" && onlineDevs == "" && onlineHelpers == "")
                embed.WithDescription("Sorry, there are no staff members online right now.");
            embed.WithColor(new Color(0, 154, 0));


            await Context.Channel.SendMessageAsync("", false, embed);
        }

        [Command("onlinecount")]
        public async Task CountUsersOnline()
        {
            string n = Context.Guild.Users.ToArray().Length.ToString();
            await Context.Channel.SendMessageAsync("There are currently " + n + " members online.");
        }
        [Command("tecoverse")]
        public async Task Tecoverse() => await Context.Channel.SendMessageAsync("https://discord.gg/yD7Rxnu");

        #region View People in Rank Commands
        [Command("respected")]
        public async Task Respected() => await Context.Channel.SendMessageAsync("", false, Embed("Respected", FindPeopleWithRoles("Respected"), new Color(255, 105, 180), "", ""));

        [Command("assisted")]
        public async Task Interns() => await Context.Channel.SendMessageAsync("", false, Embed("Assisted", FindPeopleWithRoles("Assisted"), new Color(102, 0, 204), "", ""));

        [Command("helpers")]
        public async Task Helpers() => await Context.Channel.SendMessageAsync("", false, Embed("Helpers", FindPeopleWithRoles("Helpers"), new Color(255, 255, 0), "", ""));

        [Command("director")]
        public async Task Director() => await Context.Channel.SendMessageAsync("", false, Embed("Director", FindPeopleWithRoles("Director"), new Color(0, 153, 0), "", ""));

        [Command("devs")]
        public async Task ShowDevsShortcut() => await Context.Channel.SendMessageAsync("", false, Embed("Developers", FindPeopleWithRoles("Developer"), new Color(255, 0, 0), "", ""));

        [Command("developers")]
        public async Task ShowDevs() => await Context.Channel.SendMessageAsync("", false, Embed("Developers", FindPeopleWithRoles("Developer"), new Color(255, 0, 0), "", ""));

        [Command("phantomzone")]
        public async Task PhantomZone() => await Context.Channel.SendMessageAsync("", false, Embed("Phantom Zoned People", FindPeopleWithRoles("Phantom Zone"), new Color(84, 110, 122), "", ""));

        [Command("leads")]
        public async Task Leads() => await Context.Channel.SendMessageAsync("", false, Embed("Leads", FindPeopleWithRoles("Lead"), new Color(255, 165, 0), "", ""));
        #endregion

        [Command("joined")]
        public async Task JoinedAt() => await JoinedAt((SocketGuildUser)Context.User);

        [Command("joined")]
        public async Task JoinedAt([Remainder]SocketGuildUser user) => await Context.Channel.SendMessageAsync(((DateTimeOffset)user.JoinedAt).ToString("MMMM dd, yyyy"));

        [Command("created")]
        public async Task Created() => await Created((SocketGuildUser)Context.User);

        [Command("created")]
        public async Task Created([Remainder]SocketGuildUser user) => await Context.Channel.SendMessageAsync(user.CreatedAt.ToString("MMMM dd, yyy"));

        [Command("avatar")]
        public async Task Avatar() => await Avatar((SocketGuildUser)Context.User);

        [Command("avatar")]
        public async Task Avatar([Remainder]SocketGuildUser user) => await Context.Channel.SendMessageAsync(user.GetAvatarUrl());

        #region Tecos Related Commands

        // Spawn Tecos for a user
        [Command("tecos spawn")]
        public async Task SpawnTecos(SocketGuildUser user, [Remainder]int amount)
        {
            if (Context.User.ToString() != "admin#0001") return;
            await Context.Channel.SendMessageAsync("", false, Embed("Tecos", Context.User.Mention + " " + TH.SpawnTecos(user, amount), new Color(215, 154, 14), "", ""));
        }

        // Remove Tecos for a user
        [Command("tecos remove")]
        public async Task RemoveTecos(SocketGuildUser user, [Remainder]int amount)
        {
            if (Context.User.ToString() != "admin#0001") return;
            await Context.Channel.SendMessageAsync("", false, Embed("Tecos", TH.RemoveTecos(user, amount), new Color(215, 154, 14), "", ""));
        }

        // See how many Tecos you have
        [Command("tecos")]
        public async Task SeeTecos() => await TH.DisplayTecos((SocketGuildUser)Context.User, Context.Channel);

        // (Overloaded) See how many Tecos another user has
        [Command("tecos")]
        public async Task SeeTecos([Remainder]SocketGuildUser user) => await TH.DisplayTecos(user, Context.Channel);

        // Give Tecos to another user (not spawning them)
        [Command("tecos give")]
        public async Task GiveTecos(SocketGuildUser user, [Remainder]int amount) => await Context.Channel.SendMessageAsync("", false, Embed("Tecos", TH.GiveTecos((SocketGuildUser)Context.User, user, amount), new Color(215, 154, 14), "", ""));

        // Tecos Store
        [Command("tecos store")]
        public async Task TecosStore() => await TH.DisplayTecosStore((SocketGuildUser)Context.User, Context.Channel);

        // Leaderboard Shortcut
        [Command("lb tecos")]
        public async Task TecosLBShortcut() => await TecosLeaderboard();

        [Command("leaderboard tecos")]
        public async Task TecosLeaderboard()
        {
            List<int> list = new List<int>();
            for (int i = 0; i < Context.Guild.Users.Count; i++)
            {
                list.Add(UserAccounts.GetAccount(Context.Guild.Users.ElementAt(i)).Tecos);
            }

            int[] MostTecosArray = new int[5];
            int indexMin = 0;
            var IntArray = list.ToArray();
            MostTecosArray[indexMin] = IntArray[0];
            int min = MostTecosArray[indexMin];

            for (int i = 1; i < IntArray.Length; i++)
            {
                if (i < 5)
                {
                    MostTecosArray[i] = IntArray[i];
                    if (MostTecosArray[i] < min)
                    {
                        min = MostTecosArray[i];
                        indexMin = i;
                    }
                }
                else if (IntArray[i] > min)
                {
                    min = IntArray[i];
                    MostTecosArray[indexMin] = min;
                    for (int r = 0; r < 5; r++)
                    {
                        if (MostTecosArray[r] < min)
                        {
                            min = MostTecosArray[r];
                            indexMin = r;
                        }
                    }
                }
            }

            var embed = new EmbedBuilder();
            embed.WithTitle("Tecos Leaderboard");
            embed.WithColor(new Color(215, 154, 14));

            Array.Sort(MostTecosArray);
            Array.Reverse(MostTecosArray);
            List<SocketGuildUser> PeopleOnLB = new List<SocketGuildUser>();
            for (int i = 0; i < 5; i++)
            {
                for (int n = 0; n < Context.Guild.Users.Count; n++)
                {
                    if (UserAccounts.GetAccount(Context.Guild.Users.ElementAt(n)).Tecos == MostTecosArray[i] && !PeopleOnLB.Contains(Context.Guild.Users.ElementAt(n)))
                    {
                        string name = Context.Guild.Users.ElementAt(n).Nickname != null ? Context.Guild.Users.ElementAt(n).Nickname : Context.Guild.Users.ElementAt(n).Username;
                        embed.AddField($"{i + 1} - {name}", MostTecosArray[i] + " Tecos");
                        PeopleOnLB.Add(Context.Guild.Users.ElementAt(n));
                        break;
                    }
                }
            }

            await Context.Channel.SendMessageAsync("", false, embed);
        }

        #endregion

        // See stats for yourself
        [Command("stats")]
        public async Task Stats() => await Stats((SocketGuildUser)Context.User);

        // See stats for a certain user
        [Command("stats")]
        public async Task Stats([Remainder]SocketGuildUser user)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle("Stats for " + user.ToString());
            embed.AddField("Created", user.CreatedAt.ToString("MMMM dd, yyy"));
            embed.AddField("Joined", ((DateTimeOffset)user.JoinedAt).ToString("MMMM dd, yyyy"));

            string nick = user.Nickname ?? "none";
            embed.AddField("Nickname", nick);

            string roles = "";
            foreach (SocketRole r in user.Roles) roles += r.ToString() + ", ";
            if (roles == "@everyone, ")
            {
                roles = "none";
            }
            else
            {
                roles = roles.Substring(11, roles.Length - 11);
                roles = roles.Substring(0, roles.Length - 2);
            }
            embed.AddField("Roles", roles);

            var account = UserAccounts.GetAccount(user);
            embed.AddField("Tecos", account.Tecos);
            embed.AddField("Warnings", account.Warns);

            embed.WithColor(new Utilities().DomColorFromURL(user.GetAvatarUrl()));
            embed.WithThumbnailUrl(user.GetAvatarUrl());

            await Context.Channel.SendMessageAsync("", false, embed);
        }

        [Command("delete")]
        public async Task DeleteMessage([Remainder]string amount)
        {
            if (Context.User.ToString() == "Tecosaurus#6343" || Context.User.ToString() == "admin#0001")
            {
                var messages = await Context.Channel.GetMessagesAsync(Int32.Parse(amount) + 1).Flatten();
                await Context.Channel.DeleteMessagesAsync(messages);
            }
        }

        [Command("warn")]
        public async Task Warn(SocketGuildUser user, [Remainder]string reason)
        {
            if (Config.Staff.Contains(Context.User.ToString()))
            {
                await new Utilities().WarnUser(Context, user, reason);
            }
        }

        [Command("warns")]
        public async Task ViewWarns(SocketGuildUser user) => await Context.Channel.SendMessageAsync("", false, new Utilities().AllWarnsEmbed(user, UserAccounts.GetAccount(user)));

        [Command("warns remove")]
        public async Task RemoveWarn(SocketGuildUser user, [Remainder]string number)
        {
            if (!Config.Staff.Contains(Context.User.ToString())) return;

            var account = UserAccounts.GetAccount(user);

            string result = $"Warning {number} has been removed from {user.Mention} by {Context.User.Mention}.";

            account.Warners.RemoveAt(Int32.Parse(number) - 1);
            account.warnReasons.RemoveAt(Int32.Parse(number) - 1);
            account.Warns--;
            UserAccounts.SaveAccounts();

            var channels = Context.Guild.Channels;
            SocketGuildChannel c = null;
            foreach (SocketGuildChannel s in channels)
            {
                if (s.Name == "warning-thread") c = s;
            }

            await (c as ISocketMessageChannel).SendMessageAsync("", false, Embed("Warning Removal", result, new Color(255, 0, 0), "", user.GetAvatarUrl()));
            await Context.Channel.SendMessageAsync("", false, Embed("Warning Removal", result, new Color(255, 0, 0), "", user.GetAvatarUrl()));
        }

        [Command("gideon")]
        public async Task GideonGreet() => await Context.Channel.SendMessageAsync($"Greetings. How may I be of service, {Context.User.Mention}?\n!help");

        [Command("movie")]
        public async Task SearchMovie([Remainder]string search)
        {
            MediaFetchHandler mediaFH = new MediaFetchHandler();
            MediaFetchHandler.Movie media;
            media = mediaFH.FetchMovie(search);

            string RTScore = "N/A";
            string IMDBScore;

            for (int i = 0; i < media.Ratings.Length; i++)
            {
                if (media.Ratings[i].Source == "Rotten Tomatoes") RTScore = media.Ratings[i].Value;
            }

            IMDBScore = media.imdbRating == "N/A" ? "N/A" : $"{media.imdbRating}/10";

            var embed = new EmbedBuilder();
            embed.WithTitle($":film_frames: {media.Title} ({media.Year})");
            embed.WithThumbnailUrl(media.Poster);
            embed.WithDescription(media.Plot);
            embed.AddField("Director", media.Director);
            embed.AddField("Runtime", media.Runtime);
            embed.AddField("Box Office", media.BoxOffice);
            embed.AddField("IMDB Score", IMDBScore);
            embed.AddField("Rotten Tomatoes", RTScore);
            embed.WithColor(new Utilities().DomColorFromURL(media.Poster));

            await Context.Channel.SendMessageAsync("", false, embed);
        }

        [Command("tv")]
        public async Task SearchShows([Remainder]string search)
        {
            MediaFetchHandler mediaFH = new MediaFetchHandler();
            MediaFetchHandler.Movie media;
            media = mediaFH.FetchMovie(search);

            string IMDBScore;

            IMDBScore = media.imdbRating == "N/A" ? "N/A" : $"{media.imdbRating}/10";
            media.Year = media.Year.Replace("â€“", "-");
            var embed = new EmbedBuilder();
            embed.WithTitle($":film_frames: {media.Title} ({media.Year})");
            embed.WithThumbnailUrl(media.Poster);
            embed.WithDescription(media.Plot);
            embed.AddField("Runtime", media.Runtime);
            embed.AddField("IMDB Score", IMDBScore);
            embed.WithColor(new Utilities().DomColorFromURL(media.Poster));

            await Context.Channel.SendMessageAsync("", false, embed);
        }

        [Command("source")]
        public async Task GetSourceCode1() => await Context.Channel.SendMessageAsync("https://github.com/WilliamWelsh/GideonBot");

        [Command("sourcode")]
        public async Task GetSourceCode2() => await Context.Channel.SendMessageAsync("https://github.com/WilliamWelsh/GideonBot");

        [Command("help")]
        public async Task Help()
        {
            await Context.User.SendMessageAsync(
                "Hello! I am Gideon.\nI am programmed by Reverse (admin#0001), so if you encounter an issue. Please DM him.\n\n" +
                "Here are a list of commands:\n" +
                "`!audition` Get a link to audition for a voice role in the game.\n" +
                "`!avatar @MentionedUser` Get someone's profile picture.\n" +
                "`!created @MentionedUser` Get someone's account creation date.\n" +
                "`!stats @MentionedUser` Get someone's stats.\n" +
                "`!joined @MentionedUser` Get someone's join date (to the COEO discord).\n" +
                "`!tecoverse` Get a link to the Tecoverse Discord.\n" +
                "`!yt` View Teco's YouTube stats.\n" +
                "`!warns @MentionedUser` View someone's warnings.\n" +


                "`!onlinestaff` View online staff members.\n" +
                "`!leads` View the leaders.\n" +
                "`!devs` View the developers.\n" +
                "`!helpers` View the helpers.\n" +
                "`!assisted` View the assisted.\n" +
                "`!respected` View those with the respected role.\n" +
                "`!phantomzone` View those banished to the Phantom Zone.\n" +


                "\n\n\n`MESSAGE FROM REVERSE: THERE ARE LIKE 20-30 MORE COMMANDS THAT I HAVE NOT LISTED HERE YET`"
            );
        }
    }
}