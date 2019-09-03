using System;
using Discord;
using System.Text;
using System.Linq;
using Discord.Commands;
using Discord.WebSocket;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Net;
using System.Drawing;
using System.IO;
using System.Collections.Generic;

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
            var messages = await channel.GetMessagesAsync(++amount).Flatten().ToList();
            await channel.DeleteMessagesAsync(messages);
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
		public async Task GetSourceCode1() => await Context.Channel.SendMessageAsync("https://github.com/WilliamWelsh/GideonBot");

		// Display available commands
		[Command("help")]
		public async Task Help() => await Context.Channel.SendMessageAsync($"View available commands here:\nhttps://github.com/WilliamWelsh/GideonBot/blob/master/README.md");

        // Makes an emoji big
        [Command("image")]
        public async Task EmojisToImage([Remainder]string input)
        {
            // Split emojis into an array
            var rows = input.Split('\n');
            var allEmojis = new List<string[]>(rows.Length);
            foreach (var row in rows)
            {
                var emojis = row.Split('>');
                allEmojis.Add(emojis);
            }

            int imageRows = rows.Length;
            int imageColumns = 1;

            // Get the max amount of columns
            foreach (var row in allEmojis)
                if (row.Length > imageColumns)
                    imageColumns = row.Length;
            imageColumns--;

            int imageWidth = 128 * imageColumns;
            int imageHeight = 128 * imageRows;

            Bitmap image = new Bitmap(imageWidth, imageHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics graphics = Graphics.FromImage(image))
            {
                // Some settings to help with resizing
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                for (int row = 0; row < allEmojis.Count; row++)
                {
                    for (int emoji = 0; emoji < allEmojis[row].Length; emoji++)
                    {
                        if (allEmojis[row][emoji].Length > 1)
                        {
                            System.Drawing.Image emojiImage = Utilities.DownloadImage(GetEmojiImageURL(allEmojis[row][emoji] + ">"));
                            graphics.DrawImage(emojiImage,
                                new Rectangle(new Point(emoji * 128, row * 128), new Size(128, 128)),
                                new Rectangle(new Point(), emojiImage.Size),
                                GraphicsUnit.Pixel);
                        }
                    }
                }
            }

            image.Save("hi.png");

            await Context.Channel.SendFileAsync("hi.png");
        }

        // Makes an emoji big
        [Command("url")]
        [Alias("jumbo")]
        public async Task PrintEmojis([Remainder]string input)
        {
            // Split emojis into an array
            var emojis = input.Split('>');

            // If there's just one emoji then print it
            if (emojis.Length == 2) // 2 because there's a leftover ">"
                await PrintEmoji(emojis[0] + ">").ConfigureAwait(false);
            else // Print the emojis in one big picture
                await PrintBigEmojiPicture(emojis, Context);
        }

        private async Task PrintEmoji(string emoji)
        {
            string url = GetEmojiImageURL(emoji);
            await Context.Channel.SendMessageAsync("", false, Utilities.ImageEmbed("", "", Utilities.DomColorFromURL(url), "", url));
        }

        private string GetEmojiImageURL(string emoji) => $"https://cdn.discordapp.com/emojis/{Regex.Replace(emoji, "[^0-9.]", "")}.{(emoji.StartsWith("<a:") ? "gif" : "png")}";

        private async Task PrintBigEmojiPicture(string[] emojis, SocketCommandContext Context)
        {
            // The rows and columns is the square root of the amount of emojis rounded up
            int rows = Convert.ToInt32(Math.Ceiling(Math.Sqrt(emojis.Length - 1)));
            int columns = rows;

            var emojiImages = new List<System.Drawing.Image>();

            foreach (var emoji in emojis)
            {
                if (emoji.Length > 1)
                {
                    // Add ">" to the emoji cause it got removed when we split the emojis into an array
                    // Download the image and add it to the list
                    emojiImages.Add(Utilities.DownloadImage(GetEmojiImageURL($"{emoji}>")));
                }
                
            }

            int imageWidth = 128 * rows;
            int imageHeight = 128 * columns;

            Bitmap image = new Bitmap(imageWidth, imageHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics graphics = Graphics.FromImage(image))
            {
                // Some settings to help with resizing
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                // Draw the rest of the images
                int i = 0;
                for (int row = 0; row < rows; row++)
                {
                    for (int column = 0; column < columns; column++)
                    {
                        i++;
                        if (i > emojiImages.Count)
                            break; // If we run out of images then stop
                        graphics.DrawImage(emojiImages[i-1],
                            new Rectangle(new Point(column * 128, row * 128), new Size(128, 128)),
                            new Rectangle(new Point(), emojiImages[i-1].Size),
                            GraphicsUnit.Pixel);
                    }
                }
            }

            image.Save("hi.png");

            await Context.Channel.SendFileAsync("hi.png");
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

        //[Command("printmessage")]
        //[RequireOwner]
        //public async Task Say()
        //{
        //    var description = new StringBuilder()
        //        .AppendLine("React to one of the numbers if you want a colored name, or 9 to remove your color.")
        //        .AppendLine()
        //        .AppendLine("1 - Red")
        //        .AppendLine("2 - Blue")
        //        .AppendLine("3 - Pink")
        //        .AppendLine("4 - Teal")
        //        .AppendLine("5 - Green")
        //        .AppendLine("6 - Purple")
        //        .AppendLine("7 - Yellow")
        //        .AppendLine("8 - Orange")
        //        .AppendLine("9 - [Remove Color]")
        //        .ToString();

        //    var msg = await Context.Channel.SendMessageAsync(null, false, new EmbedBuilder()
        //        .WithDescription(description)
        //        .WithColor(Colors.Orange)
        //        .Build());

        //    await msg.AddReactionAsync(new Emoji("1\u20e3"));
        //    await msg.AddReactionAsync(new Emoji("2\u20e3"));
        //    await msg.AddReactionAsync(new Emoji("3\u20e3"));
        //    await msg.AddReactionAsync(new Emoji("4\u20e3"));
        //    await msg.AddReactionAsync(new Emoji("5\u20e3"));
        //    await msg.AddReactionAsync(new Emoji("6\u20e3"));
        //    await msg.AddReactionAsync(new Emoji("7\u20e3"));
        //    await msg.AddReactionAsync(new Emoji("8\u20e3"));
        //    await msg.AddReactionAsync(new Emoji("9\u20e3"));
        //}
    }
}
