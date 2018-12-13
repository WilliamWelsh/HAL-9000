using System;
using Discord;
using System.IO;
using System.Linq;
using System.Text;
using System.Net.Http;
using Discord.Commands;
using Gideon.Minigames;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
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

		Trivia Trivia = Config.MinigameHandler.Trivia;
		NumberGuess NG = Config.MinigameHandler.NG;
		RussianRoulette RR = Config.MinigameHandler.RR;
        TicTacToe TTT = Config.MinigameHandler.TTT;

		[Command("rolecolors")]
		public async Task DisplayRoleColors()
		{
			string s = "";
			foreach (var x in Context.Guild.Roles)
				s += $"{x.Name}, {x.Color}\n";
			await Context.Channel.SendMessageAsync(s);
		}

		[Command("dicksize")]
		public async Task DickSize() => await Context.Channel.SendMessageAsync($"8{new string('=', new Random().Next(1, 13))}D");

        // Used to turn text upside down
        [Command("australia")]
        public async Task AussieText([Remainder]string message) => await Context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("Australian Translator", ToAussie(message), new Color(255, 140, 0), "", ""));

        // Display a list of MiniGames
        [Command("games")]
		public async Task DisplayGames() => await Config.MinigameHandler.DisplayGames(Context);

		#region Tic-Tac-Toe Commands
		// Tic-Tac-Toe Menu/Start Game
		[Command("ttt")]
		public async Task TTTMenu() => await TTT.StartGame(Context);

		// Join Tic-Tac-Toe
		[Command("ttt join")]
		public async Task JoinTTT() => await TTT.JoinGame(Context);
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

		// Pull the trigger in RR
		[Command("pt")]
		public async Task RRPullTrigger() => await RR.PullTrigger(Context);
		#endregion

		#region Trivia Commands
		// Trivia Menu
		[Command("trivia")]
		public async Task TryToStartTrivia() => await Trivia.TryToStartTrivia((SocketGuildUser)Context.User, Context, "trivia");

		// Start Trivia
		[Command("trivia")]
		public async Task TryToStartTrivia(string input) => await Trivia.TryToStartTrivia((SocketGuildUser)Context.User, Context, input);
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
		public async Task Play8Ball() => await Config.MinigameHandler._8ball.Greet8Ball(Context);

		// Play 8-Ball
		[Command("8ball")]
		public async Task Play8Ball([Remainder]string question) => await Config.MinigameHandler._8ball.Play8Ball(Context);

		// Start Rock, Paper, Scissors
		[Command("rps")]
		public async Task StartRPS() => await Config.MinigameHandler.RPS.StartRPS(Context);

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
			char[] letters = message.ToCharArray();
			for (int n = 0; n < letters.Length; n += 2)
				letters[n] = char.ToUpper(letters[n]);
			string new_s = new string(letters);
			string name = ((SocketGuildUser)Context.User).Nickname ?? Context.User.Username;

			await Context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("", new_s, new Color(247, 250, 58), $"Mocked by {name}", "http://i0.kym-cdn.com/photos/images/masonry/001/255/479/85b.png"));
		}

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

		[Command("onlinecount")]
		public async Task CountUsersOnline() => await Context.Channel.SendMessageAsync($"There are currently {Context.Guild.Users.ToArray().Length} members online.");

		[Command("joined")]
		public async Task JoinedAt() => await JoinedAt((SocketGuildUser)Context.User);

		[Command("joined")]
		public async Task JoinedAt(SocketGuildUser user) => await Context.Channel.SendMessageAsync(Config.StatsHandler.GetJoinedDate(user));

		[Command("created")]
		public async Task Created() => await Created((SocketGuildUser)Context.User);

		[Command("created")]
		public async Task Created([Remainder]SocketGuildUser user) => await Context.Channel.SendMessageAsync(Config.StatsHandler.GetCreatedDate(user));

		[Command("avatar")]
		public async Task GetAvatar() => await GetAvatar((SocketGuildUser)Context.User);

		[Command("avatar")]
		public async Task GetAvatar(SocketGuildUser user)
		{
			var embed = new EmbedBuilder();
			embed.WithColor(Config.Utilities.DomColorFromURL(user.GetAvatarUrl()));
			embed.WithImageUrl(user.GetAvatarUrl());
			await Context.Channel.SendMessageAsync("", false, embed);
		}

		#region Coins Related Commands

		// Pickpocket a user
		[Command("pickpocket")]
		public async Task PickPocketCoins(SocketGuildUser user) => await Config.CoinHandler.PickPocket(Context, user);

		// Start a Coins Lottery
		[Command("coins lottery start")]
		public async Task StartCoinsLottery(int amount, int cost) => await Config.CoinHandler.StartCoinsLottery(Context, amount, cost);

		// Join the Coins Lottery
		[Command("coins lottery join")]
		public async Task JoinCoinsLottery() => await Config.CoinHandler.JoinCoinsLottery(Context);

		// Draw the Coins Lottery
		[Command("coins lottery draw")]
		public async Task DrawCoinsLottery() => await Config.CoinHandler.DrawLottery(Context);

		// Reset Coins Lottery
		[Command("coins lottery reset")]
		public async Task ResetCoinsLottery() => await Config.CoinHandler.ResetCoinsLottery(Context, true);

		// Spawn Coins for a user
		[Command("coins spawn")]
		public async Task SpawnCoins(SocketGuildUser user, [Remainder]int amount)
		{
			if (354458973572956160 != Context.User.Id) return;
			await Config.CoinHandler.SpawnCoins(Context, user, amount);
		}

		// Remove Coins for a user
		[Command("coins remove")]
		public async Task RemoveCoins(SocketGuildUser user, [Remainder]int amount)
		{
			if (354458973572956160 != Context.User.Id) return;
			await Config.CoinHandler.RemoveCoins(Context, user, amount);
		}

		// See how many Coins you have
		[Command("coins")]
		public async Task SeeCoins() => await Config.CoinHandler.DisplayCoins(Context, (SocketGuildUser)Context.User, Context.Channel);

		// (Overloaded) See how many Coins another user has
		[Command("coins")]
		public async Task SeeCoins([Remainder]SocketGuildUser user) => await Config.CoinHandler.DisplayCoins(Context, user, Context.Channel);

		// Give Coins to another user (not spawning them)
		[Command("coins give")]
		public async Task GiveCoins(SocketGuildUser user, [Remainder]int amount) => await Config.CoinHandler.GiveCoins(Context, (SocketGuildUser)Context.User, user, amount);

		// Coins Store
		[Command("coins store")]
		public async Task CoinsStore() => await Config.CoinHandler.DisplayCoinsStore(Context, (SocketGuildUser)Context.User, Context.Channel);

		// Leaderboard Shortcut
		[Command("lb coins")]
		public async Task CoinsLBShortcut() => await CoinsLeaderboard();

		// Print the Coins leaderboard
		[Command("leaderboard coins")]
		public async Task CoinsLeaderboard() => await Config.CoinHandler.PrintCoinsLeaderboard(Context);

		#endregion

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
			for (int i = 0; i < Context.Guild.Emotes.Count / 2; i++)
				EmoteString += $":flash: ";
			await Context.Channel.SendMessageAsync(EmoteString);
            //EmoteString = "";
            //for (int i = Context.Guild.Emotes.Count / 2; i < Context.Guild.Emotes.Count; i++)
            //    EmoteString += Context.Guild.Emotes.ElementAt(i);
            //await Context.Channel.SendMessageAsync(EmoteString);
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
			targetAccount.country = name;
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
				if (media.Ratings[i].Source == "Rotten Tomatoes") RTScore = media.Ratings[i].Value;

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

		[Command("sourcecode")]
		public async Task GetSourceCode2() => await Context.Channel.SendMessageAsync("https://github.com/WilliamWelsh/GideonBot");

		// Display available commands
		[Command("help")]
		public async Task Help() => await Context.Channel.SendMessageAsync("Coming soon.");

        // Makes an emoji big, code from my friend Elias // @Elias#6666 -- 208409824818364426 (discord ID)
        [Command("jumbo")]
		public async Task Jumbo(string emoji)
		{
			string emojiUrl = null;

			if (Emote.TryParse(emoji, out Emote found))
				emojiUrl = found.Url;
			else
			{
				int codepoint = char.ConvertToUtf32(emoji, 0);
				string codepointHex = codepoint.ToString("X").ToLower();
				emojiUrl = "https://raw.githubusercontent.com/twitter/twemoji/gh-pages/2/72x72/{codepointHex}.png";
			}

			try
			{
				HttpClient client = new HttpClient();
				var req = await client.GetStreamAsync(emojiUrl);
				await Context.Channel.SendFileAsync(req, Path.GetFileName(emojiUrl));
			}
			catch (HttpRequestException) { } { }
		}

		// Convert a hexadecimal to an RGB value
		[Command("rgb")]
		public async Task HexToRGB(string input)
		{
			input = input.Replace("#", "");

			if (!new Regex("^[a-zA-Z0-9]*$").IsMatch(input) || input.Length != 6)
			{
				await Context.Channel.SendMessageAsync("Please enter a valid hexadecimal.");
				return;
			}

			var RGB = Config.Utilities.HexToRGB(input);
			string result = $"`#{input}` = `{RGB.R}, {RGB.G}, {RGB.B}`\n\n" +
				$"`Red: {RGB.R}`\n" +
				$"`Green: {RGB.G}`\n" +
				$"`Blue: {RGB.B}`\n";
			await Context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("Hexadecimal to RGB", result, RGB, "", ""));
		}

		// Convert an RGB value to a hexadecimal
		[Command("hex")]
		public async Task RGBToHex(int R, int G, int B)
		{
			if (R < 0 || R > 255)
			{
				await Context.Channel.SendMessageAsync("Please enter a valid Red number.");
				return;
			}

			else if (G < 0 || G > 255)
			{
				await Context.Channel.SendMessageAsync("Please enter a valid Green number.");
				return;
			}

			else if (B < 0 || B > 255)
			{
				await Context.Channel.SendMessageAsync("Please enter a valid Blue number.");
				return;
			}
			await Context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("RGB to Hexadecimal", $"`{R}, {G}, {B}` = `#{R:X2}{G:X2}{B:X2}`", new Color(R, G, B), "", ""));
		}

        // Send a random picture of Alani
        [Command("alani")]
        public async Task RandomAlaniPic()
        {
            string pic = Config.ImageFetcher.GetRandomPic();
            var embedW = new EmbedBuilder();
            embedW.WithTitle("Alani :heart_eyes:");
            embedW.WithColor(Config.Utilities.DomColorFromURL(pic));
            embedW.WithImageUrl(pic);
            await Context.Channel.SendMessageAsync("", false, embedW);
        }

        [Command("xp add")]
        public async Task AddXP(SocketUser user, int xp)
        {
            if (!(UserAccounts.GetAccount(Context.User).superadmin)) return;
            Config.RankHandler.GiveUserXP(user, xp);
            await Config.RankHandler.CheckXP(Context, user);
            await Context.Channel.SendMessageAsync("Updated user.");
        }

        [Command("level")]
        public async Task ViewLevel() => await Config.RankHandler.DisplayLevelAndXP(Context, Context.User);

        [Command("level")]
        public async Task ViewLevel(SocketUser user) => await Config.RankHandler.DisplayLevelAndXP(Context, user);

        [Command("xp")]
        public async Task ViewXP() => await Config.RankHandler.DisplayLevelAndXP(Context, Context.User);

        [Command("xp")]
        public async Task ViewXP(SocketUser user) => await Config.RankHandler.DisplayLevelAndXP(Context, user);

        [Command("levels")]
        public async Task ViewLevels() => await ViewRanks();

        [Command("ranks")]
        public async Task ViewRanks()
        {
            await Context.Channel.SendMessageAsync("Level 0-5 Noob\n" +
                "Level 6-10 Symbiote\n" +
                "Level 11-15 Speedster\n" +
                "Level 16-20 Kaiju Slayer\n" +
                "Level 21-25 Avenger");
        }

        [Command("playing")]
        public async Task ViewRanks(SocketUser user) => await Context.Channel.SendMessageAsync($"{(user.Game.ToString() == "" ? "Not currently playing anything." : user.Game.ToString())}");

        [Command("lb joined")]
        public async Task JoinedLB()
        {
            List<DateTime> dates = MakeListAndOrderIt("joined");
            string result = "";
            for (int i = 0; i < 50; i++)
                foreach (var user in Context.Guild.Users)
                    if (dates.ElementAt(i) == ((DateTimeOffset)user.JoinedAt).DateTime)
                        result += $"{i+1}. {user.Username}, `{dates.ElementAt(i).ToString("MMMM dd, yyy")}`\n";
            await Context.Channel.SendMessageAsync(result);
        }

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

        private async Task DisplayPlayerIndexInList(List<DateTime> list, SocketGuildUser target, DateTime time, string text)
        {
            for (int i = 0; i < list.Count; i++)
                foreach (var user in Context.Guild.Users)
                    if (list.ElementAt(i) == time)
                    {
                        string description = $"{target.Mention} is #{i + 1} {text} on `{list.ElementAt(i).ToString("MMMM dd, yyy")}`";
                        await Context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("Leaderboard", description, Config.Utilities.DomColorFromURL(target.GetAvatarUrl()), "", target.GetAvatarUrl()));
                        return;
                    }
        }

        [Command("lb joined")]
        public async Task JoinedLB(SocketGuildUser target) => await DisplayPlayerIndexInList(MakeListAndOrderIt("joined"), target, ((DateTimeOffset)target.JoinedAt).DateTime, " who joined ");

        [Command("lb created")]
        public async Task CreatedLB()
        {
            List<DateTime> dates = MakeListAndOrderIt("created");
            string result = "";
            for (int i = 0; i < 50; i++)
                foreach (var user in Context.Guild.Users)
                    if (dates.ElementAt(i) == user.CreatedAt.DateTime)
                        result += $"{i + 1}. {user.Username}, `{dates.ElementAt(i).ToString("MMMM dd, yyy")}`\n";
            await Context.Channel.SendMessageAsync(result);
        }

        [Command("lb created")]
        public async Task CreatedLB(SocketGuildUser target) => await DisplayPlayerIndexInList(MakeListAndOrderIt("created"), target, target.CreatedAt.DateTime, " with the oldest account ");

        // Convert ASCII to Binary
        [Command("binary")]
        public async Task ASCIIToBinary([Remainder]string ascii)
        {
            string binary = "";
            foreach (char c in ascii)
                binary += Convert.ToString(c, 2).PadLeft(8, '0');
            await Context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("ASCII to Binary Converter", binary, new Color(34, 139, 34), "", ""));
        }

        // Convert Binary to ASCII
        [Command("ascii")]
        public async Task BinaryToASCII([Remainder]string input)
        {
            var list = new List<byte>();
            for (int i = 0; i < input.Length; i += 8)
            {
                string t = input.Substring(i, 8);
                list.Add(Convert.ToByte(t, 2));
            }

            await Context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("Binary to ASCII Converter", Encoding.ASCII.GetString(list.ToArray()), new Color(34, 139, 34), "", ""));
        }

        // Show everyone on the server who is a fan of Shawn Mendes
        [Command("mendesarmy")]
        public async Task ShowMendesArmy()
        {
            string users = "";
            int amount = 0;
            var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == "Shawn Mendes Fan");
            foreach (var user in Context.Guild.Users)
                if (user.Roles.Contains(role))
                {
                    users += user.Mention + "\n";
                    amount++;
                }
            await Context.Channel.SendMessageAsync("", false, Config.Utilities.Embed($"Mendes Army ({amount})", users, new Color(208, 185, 179), "", "https://cdn.discordapp.com/avatars/519261973737635842/55e95c3bd26751828c96802292897a41.png?size=128"));
        }

        // Display All Bans
        [Command("bans")]
        public async Task ShowBans()
        {
            string bans = "";
            var banArray = Context.Guild.GetBansAsync().Result.ToArray();
            foreach (var ban in banArray)
                bans += $"{ban.User} for \"{ban.Reason}\"\n";
            await Context.Channel.SendMessageAsync("", false, Config.Utilities.Embed($"Server Bans ({banArray.Length})", bans, new Color(227, 37, 39), "", ""));
        }

        // Easter Egg command for Josef -- 361678050255044619
        [Command("gae")]
        public async Task Gae() => await Context.Channel.SendMessageAsync("https://i.imgur.com/iLpCs7K.png");

        // View Leaderboards
        [Command("lb")]
        public async Task Leaderboards() => await Context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("Leaderboards", "`!lb coins` People with the most coins.\n`!lb joined` First people that joined the server.\n`!lb created` People with the oldest accounts.", new Color(0, 173, 0), "", ""));

        private string FindPeopleWithRoles(string role)
        {
            var rolea = Context.Guild.Roles.FirstOrDefault(x => x.Name == role);
            string desc = "";
            foreach (SocketGuildUser user in Context.Guild.Users.ToArray())
            {
                if (user.Roles.Contains(rolea))
                {
                    desc += user.Mention + ", Level " + UserAccounts.GetAccount(user).level + "\n";
                }
            }
            return desc;
        }

        [Command("roles")]
        public async Task FindPeopleInRoles([Remainder]string role)
        {
            await Context.Channel.SendMessageAsync("", false, Config.Utilities.Embed("", FindPeopleWithRoles(role), new Color(0,0,0), "", ""));
        }
    }
}