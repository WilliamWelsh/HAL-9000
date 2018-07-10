using System;
using Discord;
using System.IO;
using System.Linq;
using System.Net.Http;
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

        _8ball _8ball = Config.MinigameHandler._8ball;
        TecosHandler TH = Config.TH;
        Trivia Trivia = Config.MinigameHandler.Trivia;
        NumberGuess NG = Config.MinigameHandler.NG;
        TicTacToe TTT = Config.MinigameHandler.TTT;
        RussianRoulette RR = Config.MinigameHandler.RR;

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

        // For debugging
        [Command("rolecolors")]
        public async Task DisplayRoleColors()
        {
            string s = "";
            foreach (var x in Context.Guild.Roles)
            {
                s += $"{x.Name}, {x.Color}\n";
            }
            await Context.Channel.SendMessageAsync(s);
        }

        [Command("addallowedchannel")]
        public async Task AddAllowedChannel([Remainder]string channel)
        {
            if (Context.User.Id != 354458973572956160) return;
            Config.ModifyChannelWhitelist(channel, true);
            await Context.Channel.SendMessageAsync($"#{channel} has been added to the whitelist.");
        }

        [Command("removeallowedchannel")]
        public async Task RemoveAllowedChannel([Remainder]string channel)
        {
            if (Context.User.Id != 354458973572956160) return;
            Config.ModifyChannelWhitelist(channel, false);
            await Context.Channel.SendMessageAsync($"#{channel} has been removed to the whitelist.");
        }

        // Used to turn text upside down
        [Command("australia")]
        public async Task AussieText([Remainder]string message)
        {
            string name = ((SocketGuildUser)Context.User).Nickname ?? Context.User.Username;
            await Context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("Australian Translator", ToAussie(message), new Color(255, 140, 0), $"Translated for {name}.", ""));
        }

        // Print names of available Teco Animations
        [Command("teco anim")]
        public async Task PrintAnimationNames() => await PostAnimation();

        // Print names of available Teco Animations
        [Command("teco anims")]
        public async Task PostAnimation()
        {
            if (!UserAccounts.GetAccount(Context.User).superadmin) return;
            await Context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("Available Teco Anims", "getoutofhere\nholdthathougt\nilikeitbutno\nnofacereveal\nfanfiction\nstfu\nshutupidc\nsuicide\nthefuckisthis", new Color(31, 139, 76), "", ""));
        }

        // Let Teco post animations he made (often as reactions or replies)
        [Command("teco anim")]
        public async Task PostAnimation(string name)
        {
            if (!UserAccounts.GetAccount(Context.User).superadmin) return;
            if (name == "getoutofhere")
                await Context.Channel.SendMessageAsync("https://cdn.discordapp.com/attachments/354460278479650816/466021333910683648/Get_out_of_Here.mp4");
            else if (name == "holdthathougt")
                await Context.Channel.SendMessageAsync("https://cdn.discordapp.com/attachments/354460278479650816/466021399966908447/Hold_that_thought.mp4");
            else if (name == "ilikeitbutno")
                await Context.Channel.SendMessageAsync("https://cdn.discordapp.com/attachments/354460278479650816/466021477024792576/I_like_it_but_no.mp4");
            else if (name == "nofacereveal")
                await Context.Channel.SendMessageAsync("https://cdn.discordapp.com/attachments/354460278479650816/466021541339987979/I_will_not_show_my_face.mp4");
            else if (name == "fanfiction")
                await Context.Channel.SendMessageAsync("https://cdn.discordapp.com/attachments/354460278479650816/466021604573446144/Keep_it_in_your_fan_fiction.mp4");
            else if (name == "stfu")
                await Context.Channel.SendMessageAsync("https://cdn.discordapp.com/attachments/354460278479650816/466021647116271616/Shut_the_fuck_up.mp4");
            else if (name == "shutupidc")
                await Context.Channel.SendMessageAsync("https://cdn.discordapp.com/attachments/354460278479650816/466021716242726934/Shut_up_I_dont_care.mp4");
            else if (name == "suicide")
                await Context.Channel.SendMessageAsync("https://cdn.discordapp.com/attachments/354460278479650816/466021927623065630/Teco_kills_himself.mp4");
            else if (name == "thefuckisthis")
                await Context.Channel.SendMessageAsync("https://cdn.discordapp.com/attachments/354460278479650816/466021977845530624/The_fuck_is_this_shit.mp4");
            else await PostAnimation();
        }

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
        public async Task PlayNG(int input) => await NG.TryToStartGame(Config.Utilities.GetRandomNumber(1, 100), (SocketGuildUser)Context.User, Context, input);

        // Join NG
        [Command("join ng")]
        public async Task JoinNG() => await NG.JoinGame((SocketGuildUser)Context.User, Context);

        // Guess the number in NG
        [Command("g")]
        public async Task GuessNG(int input) => await NG.TryToGuess((SocketGuildUser)Context.User, Context, input);
        #endregion

        // Display 8-Ball instructions
        [Command("8ball")]
        public async Task Play8Ball() => await _8ball.Greet8Ball(Context);

        // Play 8-Ball
        [Command("8ball")]
        public async Task Play8Ball([Remainder]string question) => await _8ball.Play8Ball(Context);

        // Make Gideon say something
        [Command("say")]
        public async Task Say([Remainder]string message)
        {
            if (!UserAccounts.GetAccount(Context.User).superadmin) return;
            var messages = await Context.Channel.GetMessagesAsync(1).Flatten();
            await Context.Channel.DeleteMessagesAsync(messages);
            await Context.Channel.SendMessageAsync(message);
        }

        // Make Gideon DM someone something
        [Command("dm")]
        public async Task DM(SocketGuildUser target, [Remainder]string message)
        {
            if (!UserAccounts.GetAccount(Context.User).superadmin) return;
            var messages = await Context.Channel.GetMessagesAsync(1).Flatten();
            await Context.Channel.DeleteMessagesAsync(messages);
            await target.SendMessageAsync(message);
        }

        // Spongebob Mock Meme
        [Command("mock")]
        public async Task Mock([Remainder]string message)
        {
            if (Config.botResources.allowedChannels.Contains(Context.Channel.Name))
            {
                var messages = await Context.Channel.GetMessagesAsync(1).Flatten();
                await Context.Channel.DeleteMessagesAsync(messages);

                char[] letters = message.ToCharArray();
                for (int n = 0; n < letters.Length; n += 2)
                {
                    letters[n] = char.ToUpper(letters[n]);
                }
                string new_s = new string(letters);
                string name = ((SocketGuildUser)Context.User).Nickname ?? Context.User.Username;

                await Context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("", new_s, new Color(255, 255, 0), $"Mocked by {name}", "http://i0.kym-cdn.com/photos/images/masonry/001/255/479/85b.png"));
                return;
            }
            
            await Context.Channel.SendMessageAsync($"You can only do this command in {Context.Guild.GetTextChannel(339887750683688965).Mention}.");
        }

        // Display the voice acting audtion website
        [Command("audition")]
        public async Task Audition() => await Context.Channel.SendMessageAsync("Audition here: http://www.behindthevoiceactors.com/members/Tecosaurus/casting-call/Crisis-on-Earth-One-An-Arrowverse-Fan-Game/");

        // Display a random person on the server
        [Command("someone")]
        public async Task GetRandomPerson()
        {
            SocketGuildUser[] s = Context.Guild.Users.ToArray();
            SocketGuildUser randomUser = s[Config.Utilities.GetRandomNumber(0, s.Length)];
            string footer = $"{((1.0m / Context.Guild.MemberCount) * 100).ToString("N3")}% chance of selecting this user.";
            await Context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("Random User", randomUser.ToString(), Config.Utilities.DomColorFromURL(randomUser.GetAvatarUrl()), footer, randomUser.GetAvatarUrl()));
        }
        
        // See how many tries it would take to randomly get a specific user
        [Command("someone")]
        public async Task GetRandomPerson(SocketGuildUser user)
        {
            SocketGuildUser[] s = Context.Guild.Users.ToArray();
            uint count = 0;
            bool hasFound = false;
            do
            {
                count++;
                SocketGuildUser randomUser = s[Config.Utilities.GetRandomNumber(0, s.Length)];
                if (randomUser == user) hasFound = true;
            } while (!hasFound);

            string footer = $"{((1.0m / Context.Guild.MemberCount) * 100).ToString("N3")}% chance of selecting this user.";
            await Context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("Random User", $"It took {count} tries to find {user.Mention}.", Config.Utilities.DomColorFromURL(user.GetAvatarUrl()), footer, user.GetAvatarUrl()));
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
        public async Task Respected() => await Context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("Respected", FindPeopleWithRoles("Respected"), new Color(233, 143, 255), "", ""));

        [Command("assisted")]
        public async Task Interns() => await Context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("Assisted", FindPeopleWithRoles("Assisted"), new Color(113, 54, 138), "", ""));

        [Command("helpers")]
        public async Task Helpers() => await Context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("Helpers", FindPeopleWithRoles("Helpers"), new Color(241, 196, 15), "", ""));

        [Command("director")]
        public async Task Director() => await Context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("Director", FindPeopleWithRoles("Director"), new Color(31, 139, 76), "", ""));

        [Command("devs")]
        public async Task ShowDevsShortcut() => await Context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("Developers", FindPeopleWithRoles("Developer"), new Color(231, 76, 60), "", ""));

        [Command("developers")]
        public async Task ShowDevs() => await Context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("Developers", FindPeopleWithRoles("Developer"), new Color(231, 76, 6), "", ""));

        [Command("pz")]
        public async Task ViewPZ() => await Context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("Phantom Zoned People", FindPeopleWithRoles("Phantom Zone"), new Color(84, 110, 122), "", ""));

        [Command("phantomzone")]
        public async Task PhantomZone() => await Context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("Phantom Zoned People", FindPeopleWithRoles("Phantom Zone"), new Color(84, 110, 122), "", ""));

        [Command("leads")]
        public async Task Leads() => await Context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("Leads", FindPeopleWithRoles("Lead"), new Color(230, 126, 34), "", ""));

        [Command("motd")]
        public async Task Motd() => await Context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("Mongoo Of The Day", FindPeopleWithRoles("Mongoo Of The Day"), new Color(105, 255, 234), "", ""));
        #endregion

        [Command("joined")]
        public async Task JoinedAt() => await JoinedAt((SocketGuildUser)Context.User);

        [Command("joined")]
        public async Task JoinedAt(SocketGuildUser user) => await Context.Channel.SendMessageAsync(Config.StatsHandler.GetJoinedDate(user));

        [Command("created")]
        public async Task Created() => await Created((SocketGuildUser)Context.User);

        [Command("created")]
        public async Task Created([Remainder]SocketGuildUser user) => await Context.Channel.SendMessageAsync(Config.StatsHandler.GetCreatedDate(user));

        [Command("avatar")]
        public async Task Avatar() => await Avatar((SocketGuildUser)Context.User);

        [Command("avatar")]
        public async Task Avatar([Remainder]SocketGuildUser user) => await Context.Channel.SendMessageAsync(user.GetAvatarUrl());

        #region Tecos Related Commands

        // Spawn Tecos for a user
        [Command("tecos spawn")]
        public async Task SpawnTecos(SocketGuildUser user, [Remainder]int amount)
        {
            if (354458973572956160 != Context.User.Id) return;
            await Context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("Tecos", Context.User.Mention + " " + TH.SpawnTecos(user, amount), new Color(215, 154, 14), "", ""));
        }

        // Remove Tecos for a user
        [Command("tecos remove")]
        public async Task RemoveTecos(SocketGuildUser user, [Remainder]int amount)
        {
            if (354458973572956160 != Context.User.Id) return;
            await Context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("Tecos", TH.RemoveTecos(user, amount), new Color(215, 154, 14), "", ""));
        }

        // See how many Tecos you have
        [Command("tecos")]
        public async Task SeeTecos() => await TH.DisplayTecos((SocketGuildUser)Context.User, Context.Channel);

        // (Overloaded) See how many Tecos another user has
        [Command("tecos")]
        public async Task SeeTecos([Remainder]SocketGuildUser user) => await TH.DisplayTecos(user, Context.Channel);

        // Give Tecos to another user (not spawning them)
        [Command("tecos give")]
        public async Task GiveTecos(SocketGuildUser user, [Remainder]int amount) => await Context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("Tecos", TH.GiveTecos((SocketGuildUser)Context.User, user, amount), new Color(215, 154, 14), "", ""));

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

        // Set the release date for the next update video for when people ask
        [Command("updatevideo")]
        public async Task SetVideoDate([Remainder]string input)
        {
            if (!UserAccounts.GetAccount(Context.User).superadmin) return;
            Config.ModifyNextVideoDate(input);
            if(input == "n/a")
                await Context.Channel.SendMessageAsync("There is no set date for the next update video.");
            else
                await Context.Channel.SendMessageAsync($"Date updated. To remove, set it to `n/a`. Preview:\nThe next video comes out { Config.botResources.nextVideoDate}.");
        }

        // View local time for yourself (not sure why)
        [Command("time")]
        public async Task ViewTime() => await Config.StatsHandler.DisplayTime(Context, (SocketGuildUser)Context.User);

        // View local time for a user
        [Command("time")]
        public async Task ViewTime(SocketGuildUser user) => await Config.StatsHandler.DisplayTime(Context, user);

        // View your country (not sure why)
        [Command("country")]
        public async Task ViewCountry() => await Config.StatsHandler.DisplayCountry(Context, (SocketGuildUser)Context.User);

        // View a User's country
        [Command("country")]
        public async Task ViewCountry(SocketGuildUser user) => await Config.StatsHandler.DisplayCountry(Context, user);

        // See stats for yourself
        [Command("stats")]
        public async Task DisplayUserStats() => await DisplayUserStats((SocketGuildUser)Context.User);

        // See stats for a certain user
        [Command("stats")]
        public async Task DisplayUserStats([Remainder]SocketGuildUser user) => await Config.StatsHandler.DisplayUserStats(Context, user);

        // View custom server emotes
        [Command("emotes")]
        public async Task ViewServerEmotes()
        {
            string EmoteString = $"{Context.Guild.Emotes.Count} total emotes\n";
            for (int i = 0; i < Context.Guild.Emotes.Count; i++)
            {
                EmoteString += Context.Guild.Emotes.ElementAt(i);
            }
            await Context.Channel.SendMessageAsync(EmoteString);
        }

        // View server stats
        [Command("serverstats")]
        public async Task ServerStats() => await Config.StatsHandler.DisplayServerStats(Context);

        // Set the time for a user
        [Command("time set")]
        public async Task UserTimeSet(SocketGuildUser target, int offset)
        {
            if (!UserAccounts.GetAccount(Context.User).superadmin) return;
            UserAccount targetAccount = UserAccounts.GetAccount(target);
            targetAccount.localTime = offset;
            UserAccounts.SaveAccounts();
            await Context.Channel.SendMessageAsync("User updated.");
        }

        // Set the nationality for a user
        [Command("country set")]
        public async Task UserSetCountry(SocketGuildUser target, [Remainder]string name)
        {
            if (!UserAccounts.GetAccount(Context.User).superadmin) return;
            UserAccount targetAccount = UserAccounts.GetAccount(target);
            targetAccount.Country = name;
            UserAccounts.SaveAccounts();
            await Context.Channel.SendMessageAsync("User updated.");
        }

        // Delete a certain amount of messages
        [Command("delete")]
        public async Task DeleteMessage([Remainder]string amount)
        {
            if (!UserAccounts.GetAccount(Context.User).superadmin) return;
            var messages = await Context.Channel.GetMessagesAsync(Int32.Parse(amount) + 1).Flatten();
            await Context.Channel.DeleteMessagesAsync(messages);
        }

        // Nickname a user
        [Command("nick")]
        public async Task NicknameUser(SocketGuildUser user, [Remainder]string input)
        {
            if (!UserAccounts.GetAccount(Context.User).superadmin) return;
            await user.ModifyAsync(x => { x.Nickname = input; });
            await Context.Channel.SendMessageAsync("User updated.");
        }

        #region Warn Related Commands

        [Command("warn")]
        public async Task Warn(SocketGuildUser user, [Remainder]string reason)
        {
            if(Config.Utilities.isRespectedPlus(Context, (SocketGuildUser)Context.User))
            {
                if (Config.Utilities.isRespectedPlus(Context, user))
                {
                    await Config.WarnHandler.WarnFail(Context.Channel);
                    return;
                }
                await Config.WarnHandler.WarnUser(Context, user, reason);
            }
        }

        [Command("warns")]
        public async Task ViewWarns() => await ViewWarns((SocketGuildUser)Context.User);

        [Command("warns")]
        public async Task ViewWarns(SocketGuildUser user) => await Context.Channel.SendMessageAsync("", false, Config.WarnHandler.AllWarnsEmbed(user, UserAccounts.GetAccount(user)));

        [Command("warns remove")]
        public async Task RemoveWarn(SocketGuildUser user, int number)
        {
            if (!Config.Utilities.isHelperPlus(Context, (SocketGuildUser)Context.User)) return;
            await Config.WarnHandler.RemoveWarn(Context, user, number);
        }

        [Command("warns clear")]
        public async Task ClearWarns(SocketGuildUser user)
        {
            if (!Config.Utilities.isHelperPlus(Context, (SocketGuildUser)Context.User)) return;
            await Config.WarnHandler.ClearWarns(Context, user);
        }
        #endregion

        [Command("gideon")]
        public async Task GideonGreet() => await Context.Channel.SendMessageAsync($"Greetings. How may I be of service, {Context.User.Mention}?\n`!help`");

        // View Stats for a movie
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
            embed.WithColor(Config.Utilities.DomColorFromURL(media.Poster));

            await Context.Channel.SendMessageAsync("", false, embed);
        }

        // View stats for a TV show
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
            embed.WithColor(Config.Utilities.DomColorFromURL(media.Poster));

            await Context.Channel.SendMessageAsync("", false, embed);
        }

        // Print a link to Gideon's sourcecode
        [Command("source")]
        public async Task GetSourceCode1() => await Context.Channel.SendMessageAsync("https://github.com/WilliamWelsh/GideonBot");

        [Command("sourcode")]
        public async Task GetSourceCode2() => await Context.Channel.SendMessageAsync("https://github.com/WilliamWelsh/GideonBot");

        // Display available commands
        [Command("help")]
        public async Task Help() => await Context.Channel.SendMessageAsync("View available commands here:\nhttps://github.com/WilliamWelsh/GideonBot/blob/master/README.md#commands");

        // Makes an emoji big, code from my friend Craig // @Craig#6666 -- 208409824818364426
        [Command("jumbo")]
        public async Task Jumbo(string emoji)
        {
            string emojiUrl = null;

            if (Emote.TryParse(emoji, out Emote found))
                emojiUrl = found.Url;
            else
            {
                int codepoint = Char.ConvertToUtf32(emoji, 0);
                string codepointHex = codepoint.ToString("X").ToLower();

                emojiUrl = "https://raw.githubusercontent.com/twitter/twemoji/gh-pages/2/72x72/{codepointHex}.png";
            }

            try
            {
                HttpClient client = new HttpClient();
                var req = await client.GetStreamAsync(emojiUrl);
                await Context.Channel.SendFileAsync(req, Path.GetFileName(emojiUrl));
            }

            catch (HttpRequestException) {} {}
        }
    }
}