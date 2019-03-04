using System;
using Discord;
using System.Text;
using System.Linq;
using System.Net.Http;
using Discord.Commands;
using Discord.WebSocket;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Gideon.Handlers
{
    [RequireContext(ContextType.Guild)]
	public class Misc : ModuleBase<SocketCommandContext>
	{
        [Command("rolecolors")]
		public async Task DisplayRoleColors()
		{
            StringBuilder text = new StringBuilder();
			foreach (var x in Context.Guild.Roles)
				text.AppendLine($"{x.Name}, {x.Color}");
			await Context.Channel.SendMessageAsync(text.ToString());
		}

        // Make Gideon say something
        [IsOwner]
        [Command("say")]
        public async Task Say([Remainder]string message)
		{
            await Context.Channel.DeleteMessageAsync(Context.Message.Id);
			await Context.Channel.SendMessageAsync(message);
		}

        // Make Gideon DM someone something
        [IsOwner]
        [Command("dm")]
		public async Task DM(SocketGuildUser target, [Remainder]string message)
		{
            await Context.Channel.DeleteMessageAsync(Context.Message.Id);
			await target.SendMessageAsync(message);
		}

        // Show when a user joined the server
		[Command("joined")]
        public async Task JoinedAt(SocketGuildUser user = null)
        {
            var target = user ?? (SocketGuildUser)Context.User;
            await Utilities.SendDomColorEmbed(Context.Channel, "Joined Date", $"{target.Mention} joined the server on {StatsHandler.GetJoinedDate(target)}.", target.GetAvatarUrl());
        }

        // Show when a user's account was created
        [Command("created")]
        public async Task Created(SocketGuildUser user = null)
        {
            var target = user ?? Context.User;
            await Utilities.SendDomColorEmbed(Context.Channel, "Creation Date", $"{target.Mention} created their account on {StatsHandler.GetCreatedDate(target)}.", target.GetAvatarUrl());
        }

        // Show a user's avatar
        [Command("avatar")]
        public async Task GetAvatar(SocketGuildUser user = null) => await Context.Channel.SendMessageAsync("", false, Utilities.ImageEmbed("", "", Utilities.DomColorFromURL((user ?? Context.User).GetAvatarUrl()), "", (user ?? Context.User).GetAvatarUrl().Replace("?size=128", "?size=512")));

        // See stats for a certain user
		[Command("stats")]
		public async Task DisplayUserStats(SocketGuildUser user = null) => await StatsHandler.DisplayUserStats(Context, user ?? (SocketGuildUser)Context.User);

        // View custom server emotes
        [Command("emotes")]
        [Alias("emojis")]
        public async Task ViewServerEmotes() => await Utilities.SendEmbed(Context.Channel, $"Server Emotes ({Context.Guild.Emotes.Count})", string.Join("", Context.Guild.Emotes), Colors.LightBlue, "", "");

        // View server stats
        [Command("serverstats")]
		public async Task ServerStats() => await StatsHandler.DisplayServerStats(Context);

		// Delete a certain amount of messages
        [IsOwner]
		[Command("delete")]
		public async Task DeleteMessage(int amount)
		{
            var channel = (ITextChannel)Context.Channel;
            var messages = await channel.GetMessagesAsync(++amount).Flatten().ToList();
            await channel.DeleteMessagesAsync(messages);
		}

        // Nickname a user
        [Command("nick")]
        [RequireRole("Administrator")]
		public async Task NicknameUser(SocketGuildUser user, [Remainder]string input)
		{
            await user.ModifyAsync(x => { x.Nickname = input; });
            await Utilities.PrintSuccess(Context.Channel, $"Set {user.Mention}'s nickname to `{input}`.");
		}

		[Command("gideon")]
		public async Task GideonGreet() => await Context.Channel.SendMessageAsync($"Greetings. How may I be of service, {Context.User.Mention}?\n`!help`");

		// Print a link to Gideon's sourcecode
		[Command("source")]
        [Alias("sourcecode", "github", "code")]
		public async Task GetSourceCode1() => await Context.Channel.SendMessageAsync("https://github.com/WilliamWelsh/GideonBot");

		// Display available commands
		[Command("help")]
		public async Task Help() => await Context.Channel.SendMessageAsync($"View available commands here:\nhttps://github.com/WilliamWelsh/GideonBot/blob/master/README.md");

        // Makes an emoji big, @Elias WE FINALLY DID IT
        [Command("url")]
        [Alias("jumbo")]
        public async Task PrintEmojis([Remainder]string input)
        {
            var emojis = input.Split('>');
            foreach (var s in emojis)
                await PrintEmoji(s + ">").ConfigureAwait(false);
        }

        private async Task PrintEmoji(string emoji)
        {
            string url = $"https://cdn.discordapp.com/emojis/{Regex.Replace(emoji, "[^0-9.]", "")}.{(emoji.StartsWith("<a:") ? "gif" : "png")}";
            await Context.Channel.SendMessageAsync("", false, Utilities.ImageEmbed("", "", Utilities.DomColorFromURL(url), "", url));
        }

        // Convert a hexadecimal to an RGB value
        [Command("rgb")]
		public async Task HexToRGB(string input)
		{
			var hex = input.Replace("#", "");

			if (!new Regex("^[a-zA-Z0-9]*$").IsMatch(hex) || hex.Length != 6)
			{
                await Utilities.PrintError(Context.Channel, $"Please enter a valid hexadecimal, {Context.User.Mention}.");
				return;
			}

			var RGB = Utilities.HexToRGB(hex);
            StringBuilder description = new StringBuilder()
                .AppendLine($"`#{hex}` = `{RGB.R}, {RGB.G}, {RGB.B}`").AppendLine()
                .AppendLine($"`Red: {RGB.R}`")
                .AppendLine($"`Green: {RGB.G}`")
                .AppendLine($"`Blue: {RGB.B}`");
			await Utilities.SendEmbed(Context.Channel, "Hexadecimal to RGB", description.ToString(), RGB, "", "");
		}

		// Convert an RGB value to a hexadecimal
		[Command("hex")]
		public async Task RGBToHex(int R, int G, int B)
		{
			if (R < 0 || R > 255)
			{
                await Utilities.PrintError(Context.Channel, $"Please enter a valid red value, {Context.User.Mention}.");
                return;
			}

			else if (G < 0 || G > 255)
			{
                await Utilities.PrintError(Context.Channel, $"Please enter a valid green value, {Context.User.Mention}.");
                return;
			}

			else if (B < 0 || B > 255)
			{
                await Utilities.PrintError(Context.Channel, $"Please enter a valid blue value, {Context.User.Mention}.");
                return;
			}
			await Utilities.SendEmbed(Context.Channel, "RGB to Hexadecimal", $"`{R}, {G}, {B}` = `#{R:X2}{G:X2}{B:X2}`", new Color(R, G, B), "", "");
		}

        // Send a random picture of Alani
        [Command("alani")]
        public async Task RandomAlaniPic()
        {
            string pic = Config.AlaniPictures[Utilities.GetRandomNumber(0, Config.AlaniPictures.Count)];
            await Context.Channel.SendMessageAsync("", false, Utilities.ImageEmbed("", "", Utilities.DomColorFromURL(pic), "", pic));
        }

        // Give xp to a user
        [IsOwner]
        [Command("xp add")]
        public async Task AddXP(SocketUser user, int xp)
        {
            RankHandler.GiveUserXP(user, xp);
            await RankHandler.CheckXP(Context, user);
            await Utilities.PrintSuccess(Context.Channel, $"Gave {user.Mention} {xp} xp.");
        }

        [Command("xp")]
        [Alias("level", "rank")]
        public async Task ViewXP(SocketUser user = null) => await RankHandler.DisplayLevelAndXP(Context, user ?? Context.User);

        [Command("ranks")]
        [Alias("levels")]
        public async Task ViewRanks()
        {
            StringBuilder ranks = new StringBuilder()
                .AppendLine("Level 0-5 Noob")
                .AppendLine("Level 6-10 Symbiote")
                .AppendLine("Level 11-15 Speedster")
                .AppendLine("Level 16-20 Kaiju Slayer")
                .AppendLine("Level 21-25 Watchmen")
                .AppendLine("Level 26+ Avenger");
            await Utilities.SendEmbed(Context.Channel, "Ranks", ranks.ToString(), Colors.LightBlue, "You get 15-25 xp for sending a message, but only once a minute.", "");
        }

        private string FindPeopleWithRoles(string targetRole)
        {
            SocketRole role = Context.Guild.Roles.FirstOrDefault(x => x.Name == targetRole);
            StringBuilder description = new StringBuilder();
            foreach (SocketGuildUser user in Context.Guild.Users.ToArray())
                if (user.Roles.Contains(role))
                    description.AppendLine($"{user.Mention}, Level {UserAccounts.GetAccount(user).level}");
            return description.ToString();
        }

        // Find the users that are in a certain role.
        [Command("roles")]
        public async Task FindPeopleInRoles([Remainder]string role) => await Utilities.SendEmbed(Context.Channel, "", FindPeopleWithRoles(role), Context.Guild.Roles.FirstOrDefault(x => x.Name == role).Color, "", "");

        // Get the dominant color of an image
        [Command("color")]
        public async Task GetDomColor(string url)
        {
            var color = Utilities.DomColorFromURL(url);
            await Utilities.SendEmbed(Context.Channel, "Dominant Color", $"The dominant color for the image is:\n\nHexadecimal:\n`#{color.R:X2}{color.G:X2}{color.B:X2}`\n\nRGB:\n`Red: {color.R}`\n`Green: {color.G}`\n`Blue: {color.B}`", color, "", url);
        }

        [Command("ping")]
        public async Task Pong() => await Context.Channel.SendMessageAsync("pong!");

        [Command("userdata")]
        public async Task DisplayData(SocketUser user = null)
        {
            var account = UserAccounts.GetAccount(user ?? Context.User);
            StringBuilder data = new StringBuilder()
                .AppendLine("```json\n  {")
                .AppendLine($"    \"userID\": {(user == null ? Context.User.Id : user.Id)},")
                .AppendLine($"    \"coins\": {account.coins},")
                .AppendLine($"    \"xp\": {account.xp},")
                .AppendLine($"    \"level\": {account.level}")
                .AppendLine("  }```");
            await Context.Channel.SendMessageAsync(data.ToString());
        }

        [Command("create emote")]
        public async Task MakEmote(string url, string name)
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(new Uri(url));
                var emote = await Context.Guild.CreateEmoteAsync(name, new Image(await response.Content.ReadAsStreamAsync()));
                await Utilities.PrintSuccess(Context.Channel, $"Created: {emote}");
            }
        }

        // Display how long the bot has been online
        [Command("uptime")]
        public async Task DisplayUptime()
        {
            var time = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
            string uptime = $"{time.Hours} hours, {time.Minutes}m {time.Seconds}s";
            await Utilities.SendEmbed(Context.Channel, "", uptime, Colors.Blue, "", "");
        }

        // Display a random shower thought from the subreddit
        [Command("showerthought")]
        [Alias("shower thought", "st", "showerthoughts", "shower thoughts")]
        public async Task DisplayShowerThought() => await ShowerThoughts.PrintRandomThought(Context.Channel);

        // Restart one of my bots (in case of an error or crash)
        [Command("restart")]
        [RequireRole("Administrator")]
        public async Task RestartBot(SocketGuildUser Bot)
        {
            if (!Bot.IsBot)
            {
                await Utilities.PrintError(Context.Channel, "That user is not a bot.");
                return;
            }
            else if (!Config.MyBots.Contains(Bot.Id))
            {
                await Utilities.PrintError(Context.Channel, "That bot cannot be restarted.");
                return;
            }

            string path = "";
            string fileName = "";

            if (Bot.Id == 436780808745910282) // Alani
            {
                path = @"C:\Users\Administrator\Desktop\AlaniBot";
                fileName = @"C:\Users\Administrator\Desktop\AlaniBot\AlaniBot.exe";
            }
            else if (Bot.Id == 477287091798278145) // Rotten Tomatoes
            {
                path = @"C:\Users\Administrator\Desktop\RTBot";
                fileName = @"C:\Users\Administrator\Desktop\RTBot\RottenTomatoes.exe";
            }
            else if (Bot.Id == 529569000028373002) // Time Bot
            {
                path = @"C:\Users\Administrator\Desktop\TimeBot";
                fileName = @"C:\Users\Administrator\Desktop\TimeBot\TimeBot.exe";
            }

            var procceses = Process.GetProcessesByName(fileName.Substring(fileName.LastIndexOf("\\") + 1).Replace(".exe", "")); // one less variable ok??
            if (procceses != null)
                foreach (var process in procceses)
                    process.Kill();

            Process.Start(new ProcessStartInfo { FileName = fileName, WorkingDirectory = path });
            await Utilities.SendEmbed(Context.Channel, "Back Online", $"{Bot.Mention} has been restarted by {Context.User.Mention}.", Colors.Green, "", Bot.GetAvatarUrl());
        }
    }
}
