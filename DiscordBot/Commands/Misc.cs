using System;
using Discord;
using System.Text;
using Discord.Commands;
using Discord.WebSocket;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;

namespace DiscordBot.Handlers
{
    [RequireContext(ContextType.Guild)]
	public class Misc : ModuleBase<SocketCommandContext>
	{
        // Say something
        [IsOwner]
        [Command("say")]
        public async Task Say([Remainder]string message)
		{
            await Context.Channel.DeleteMessageAsync(Context.Message.Id);
			await Context.Channel.SendMessageAsync(message);
		}

        // Convert epoch time to readable time
        [Command("epoch")]
        public async Task EpochToTime(long seconds)
        {
            DateTimeOffset date = DateTimeOffset.FromUnixTimeSeconds(seconds);
            await Context.Channel.SendMessageAsync(date.ToString("MMM dd, yyyy HH:mm:ss"));
        }

        [Command("lights on")]
        public async Task LightsOn()
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://maker.ifttt.com/trigger/Turn_Lights_On/with/key/i2RVUwTaXnGhu1wYgPcmXwHT4mMYTYrHbM2h-AQ4s5W");
            request.Method = "GET";
            String test = String.Empty;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                test = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
            }
        }

        [Command("lights off")]
        public async Task LightsOff()
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://maker.ifttt.com/trigger/Turn_Lights_Off/with/key/i2RVUwTaXnGhu1wYgPcmXwHT4mMYTYrHbM2h-AQ4s5W");
            request.Method = "GET";
            String test = String.Empty;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                test = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
            }
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
        public async Task ViewServerEmotes() => await Utilities.SendEmbed(Context.Channel, $"Server Emotes ({Context.Guild.Emotes.Count})", string.Join("", Context.Guild.Emotes), Utilities.ClearColor, "", "");

        // View server stats
        [Command("serverstats")]
		public async Task ServerStats() => await StatsHandler.DisplayServerStats(Context);

		// Delete a certain amount of messages
        [IsOwner]
		[Command("delete")]
		public async Task DeleteMessage(int amount)
		{
            var channel = (ITextChannel)Context.Channel;
            //var messages = await channel.GetMessagesAsync(++amount).Flatten().ToList();
            //await channel.DeleteMessagesAsync(messages);
		}

        // Nickname a user
        [Command("nick")]
        [RequireRole("root")]
		public async Task NicknameUser(SocketGuildUser user, [Remainder]string input)
		{
            await user.ModifyAsync(x => { x.Nickname = input; });
            await Utilities.PrintSuccess(Context.Channel, $"Set {user.Mention}'s nickname to `{input}`.");
		}

		// Print a link to Gideon's sourcecode
		[Command("source")]
        [Alias("sourcecode", "github", "code")]
		public async Task GetSourceCode1() => await Context.Channel.SendMessageAsync("https://github.com/WilliamWelsh/DiscordBot");

		// Display available commands
		[Command("help")]
		public async Task Help() => await Context.Channel.SendMessageAsync($"View available commands here:\nhttps://github.com/WilliamWelsh/DiscordBot/blob/master/README.md");

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
			await Utilities.SendEmbed(Context.Channel, "RGB to Hexadecimal", $"`{R}, {G}, {B}` = `#{R:X2}{G:X2}{B:X2}`", new Discord.Color(R, G, B), "", "");
		}

        // Send a random picture of Alani
        [Command("alani")]
        public async Task RandomAlaniPic()
        {
            string pic = Config.AlaniPictures[Utilities.GetRandomNumber(0, Config.AlaniPictures.Count)];
            await Context.Channel.SendMessageAsync("", false, Utilities.ImageEmbed("", "", Utilities.DomColorFromURL(pic), "", pic));
        }

        // Get the dominant color of an image
        [Command("color")]
        public async Task GetDomColor(string url)
        {
            var color = Utilities.DomColorFromURL(url);
            await Utilities.SendEmbed(Context.Channel, "Dominant Color", $"The dominant color for the image is:\n\nHexadecimal:\n`#{color.R:X2}{color.G:X2}{color.B:X2}`\n\nRGB:\n`Red: {color.R}`\n`Green: {color.G}`\n`Blue: {color.B}`", color, "", url);
        }

        // View the dominant color of your avatar
        [Command("mycolor")]
        public async Task MyColor(SocketUser target = null) => await GetDomColor(target == null ? Context.User.GetAvatarUrl() : target.GetAvatarUrl());

        [Command("ping")]
        public async Task Pong() => await Context.Channel.SendMessageAsync("pong!");

        [Command("userdata")]
        public async Task DisplayData(SocketUser user = null)
        {
            var account = UserAccounts.GetAccount(user ?? Context.User);
            StringBuilder data = new StringBuilder()
                .AppendLine("```json\n  {")
                .AppendLine($"    \"UserID\": {(user == null ? Context.User.Id : user.Id)},")
                .AppendLine($"    \"Coins\": {account.Coins},")
                .AppendLine("  }```");
            await Context.Channel.SendMessageAsync(data.ToString());
        }

        // Display how long the bot has been online
        [Command("uptime")]
        public async Task DisplayUptime()
        {
            var time = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
            string uptime = $"{time.Hours} hours, {time.Minutes}m {time.Seconds}s";
            await Utilities.SendEmbed(Context.Channel, "", uptime, Utilities.ClearColor, "", "");
        }

        // Display a random shower thought from the subreddit
        [Command("showerthought")]
        [Alias("shower thought", "st", "showerthoughts", "shower thoughts")]
        public async Task DisplayShowerThought() => await ShowerThoughts.PrintRandomThought(Context.Channel);

        // Restart one of my bots (in case of an error or crash)
        [Command("restart")]
        [RequireRole("root")]
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

            Config.RestartBot(Bot.Id);

            await Utilities.SendEmbed(Context.Channel, "Back Online", $"{Bot.Mention} has been restarted by {Context.User.Mention}.", Utilities.ClearColor, "", Bot.GetAvatarUrl());
        }

        [Command("test")]
        public async Task Test()
        {

        }
    }
}
